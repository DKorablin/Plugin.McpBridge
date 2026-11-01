using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using McpBridge.Core;
using McpBridge.Core.Logging;
using McpBridge.Core.Mcp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Plugin.McpBridge.Hosting;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Logging
{
	public class CorrelationTests
	{
		#region CorrelationContext

		[Fact]
		public void BeginRequest_Disabled_DoesNotSetCurrent()
		{
			using IDisposable? scope = CorrelationContext.BeginRequest(false);

			scope.Should().BeNull();
			CorrelationContext.Current.Should().BeNull();
		}

		[Fact]
		public void BeginRequest_Enabled_GeneratesDifferentIdForEachRequest()
		{
			String? first;
			using(CorrelationContext.BeginRequest(true))
				first = CorrelationContext.Current;
			String? second;
			using(CorrelationContext.BeginRequest(true))
				second = CorrelationContext.Current;

			CorrelationContext.IsValid(first).Should().BeTrue();
			second.Should().NotBe(first);
			CorrelationContext.Current.Should().BeNull();
		}

		[Fact]
		public void BeginRequest_Nested_RestoresPreviousId()
		{
			using(CorrelationContext.BeginRequest(true))
			{
				String? outer = CorrelationContext.Current;
				using(CorrelationContext.BeginRequest(true))
					CorrelationContext.Current.Should().NotBe(outer);

				CorrelationContext.Current.Should().Be(outer);
			}
		}

		[Fact]
		public void BeginRequest_ValidExistingId_ContinuesIt()
		{
			String existingId = Guid.NewGuid().ToString("N");

			using(CorrelationContext.BeginRequest(true, existingId))
				CorrelationContext.Current.Should().Be(existingId);
		}

		[Theory]
		[InlineData("not-a-guid")]
		[InlineData("forged\r\nlog line")]
		[InlineData("")]
		public void BeginRequest_InvalidExistingId_GeneratesNewId(String existingId)
		{
			using(CorrelationContext.BeginRequest(true, existingId))
			{
				CorrelationContext.Current.Should().NotBe(existingId);
				CorrelationContext.IsValid(CorrelationContext.Current).Should().BeTrue();
			}
		}

		[Fact]
		public async Task BeginRequest_FlowsAcrossAwaitAndTaskRun()
		{
			using(CorrelationContext.BeginRequest(true))
			{
				String? expected = CorrelationContext.Current;

				await Task.Yield();
				String? afterAwait = CorrelationContext.Current;
				String? insideTaskRun = await Task.Run(() => CorrelationContext.Current);

				afterAwait.Should().Be(expected);
				insideTaskRun.Should().Be(expected);
			}
		}

		#endregion

		#region TraceSourceLogger

		[Fact]
		public void TraceSourceLogger_NoRequest_WritesMessageUnchanged()
		{
			Mock<ITraceSource> trace = new Mock<ITraceSource>();
			ILogger sut = new TraceSourceLogger(trace.Object);

			sut.LogInformation("hello");

			trace.Verify(x => x.TraceEvent(TraceEventType.Information, It.IsAny<Int32>(), "hello"), Times.Once);
		}

		[Fact]
		public void TraceSourceLogger_InsideRequest_PrefixesMessageWithId()
		{
			Mock<ITraceSource> trace = new Mock<ITraceSource>();
			ILogger sut = new TraceSourceLogger(trace.Object);

			using(CorrelationContext.BeginRequest(true))
			{
				sut.LogInformation("hello");

				trace.Verify(x => x.TraceEvent(TraceEventType.Information, It.IsAny<Int32>(), $"[{CorrelationContext.Current}] hello"), Times.Once);
			}
		}

		#endregion

		#region CorrelationIdHandler

		[Fact]
		public async Task CorrelationIdHandler_InsideRequest_SendsHeader()
		{
			CapturingHandler inner = new CapturingHandler();
			using HttpClient http = new HttpClient(new CorrelationIdHandler(inner));

			using(CorrelationContext.BeginRequest(true))
			{
				await http.GetAsync("http://localhost/");

				inner.LastRequest!.Headers.GetValues(CorrelationContext.HeaderName).Should().ContainSingle()
					.Which.Should().Be(CorrelationContext.Current);
			}
		}

		[Fact]
		public async Task CorrelationIdHandler_NoRequest_DoesNotSendHeader()
		{
			CapturingHandler inner = new CapturingHandler();
			using HttpClient http = new HttpClient(new CorrelationIdHandler(inner));

			await http.GetAsync("http://localhost/");

			inner.LastRequest!.Headers.Contains(CorrelationContext.HeaderName).Should().BeFalse();
		}

		#endregion

		#region McpServer

		[Fact]
		public async Task McpServer_ToolCallFromRequest_ContinuesCallerId()
		{
			using(CorrelationContext.BeginRequest(true))
			{
				String? expected = CorrelationContext.Current;

				String? actual = await CallProbeToolAsync(true, new CorrelationIdHandler(new HttpClientHandler()));

				actual.Should().Be(expected);
			}
		}

		[Fact]
		public async Task McpServer_InvalidHeader_GeneratesNewId()
		{
			String? actual = await CallProbeToolAsync(true, new FixedHeaderHandler("not-a-guid"));

			actual.Should().NotBe("not-a-guid");
			CorrelationContext.IsValid(actual).Should().BeTrue();
		}

		[Fact]
		public async Task McpServer_Disabled_IgnoresHeader()
		{
			String? actual = await CallProbeToolAsync(false, new FixedHeaderHandler(Guid.NewGuid().ToString("N")));

			actual.Should().BeNull();
		}

		/// <summary>Calls a tool on a real server through <see cref="McpSession"/> and returns the correlation ID the tool saw.</summary>
		private static async Task<String?> CallProbeToolAsync(Boolean enabled, HttpMessageHandler handler)
		{
			String? seenId = null;
			AIFunction probe = AIFunctionFactory.Create(() => { seenId = CorrelationContext.Current; return "ok"; }, "probe");
			Settings settings = new Settings { McpServerUrl = $"http://localhost:{GetFreePort()}", CorrelationIdEnabled = enabled };

			using McpServer server = new McpServer(TestUtils.Logger, settings.McpServerUrl, settings.CorrelationIdEnabled, [probe]);
			server.Start();

			using HttpClient http = new HttpClient(handler) { BaseAddress = new Uri(settings.McpServerUrl) };
			using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			using McpSession session = new McpSession(http);
			await session.ConnectAsync(timeout.Token);

			Int32 id = session.NextId();
			await session.SendAsync(
				id,
				new JsonObject
				{
					["jsonrpc"] = "2.0",
					["id"] = id,
					["method"] = "tools/call",
					["params"] = new JsonObject { ["name"] = "probe", ["arguments"] = new JsonObject() },
				},
				timeout.Token);

			return seenId;
		}

		private static Int32 GetFreePort()
		{
			TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			Int32 port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		#endregion

		#region Nested types

		private sealed class CapturingHandler : HttpMessageHandler
		{
			public HttpRequestMessage? LastRequest { get; private set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				this.LastRequest = request;
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
			}
		}

		private sealed class FixedHeaderHandler : DelegatingHandler
		{
			private readonly String _value;

			public FixedHeaderHandler(String value)
				: base(new HttpClientHandler())
				=> this._value = value;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				request.Headers.TryAddWithoutValidation(CorrelationContext.HeaderName, this._value);
				return base.SendAsync(request, cancellationToken);
			}
		}

		#endregion
	}
}
