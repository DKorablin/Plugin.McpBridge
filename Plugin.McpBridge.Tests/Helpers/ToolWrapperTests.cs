using System;
using System.Threading.Tasks;
using FluentAssertions;
using Plugin.McpBridge.Helpers;
using Xunit;

namespace Plugin.McpBridge.Tests.Helpers
{
	public class ToolWrapperTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new ToolWrapper(null!, (Func<Task<String>>)(() => Task.FromResult(String.Empty)));

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		#endregion

		#region Confirmation

		[Fact]
		public async Task ConfirmationDeclined_ReturnsDeclinedMessage()
		{
			AssistantAgent sut = TestHelpers.CreateSut();
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<String, String, String, Task<Object>>)sut.MethodsInvoke);
			wrapper.ConfirmationRequired += (s, e) => e.Confirm(false);

			Object? result = await wrapper.InvokeAsync();

			result.Should().Be("Operation declined by user.");
		}

		[Fact]
		public async Task ConfirmationApproved_InvokesInnerFunction()
		{
			Boolean invoked = false;
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => { invoked = true; return Task.FromResult("ok"); }));
			wrapper.ConfirmationRequired += (s, e) => e.Confirm(true);

			await wrapper.InvokeAsync();

			invoked.Should().BeTrue();
		}

		[Fact]
		public async Task NoConfirmationSubscriber_InvokesInnerFunctionDirectly()
		{
			Boolean invoked = false;
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => { invoked = true; return Task.FromResult("ok"); }));

			await wrapper.InvokeAsync();

			invoked.Should().BeTrue();
		}

		[Fact]
		public async Task ConfirmationRequired_EventIsFired()
		{
			Boolean eventFired = false;
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => Task.FromResult("ok")));
			wrapper.ConfirmationRequired += (s, e) => { eventFired = true; e.Confirm(true); };

			await wrapper.InvokeAsync();

			eventFired.Should().BeTrue();
		}

		#endregion

		#region Exception handling

		[Fact]
		public async Task InnerFunction_ThrowsArgumentException_ReturnsMessage()
		{
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => throw new ArgumentException("bad input")));

			Object? result = await wrapper.InvokeAsync();

			result.Should().Be("bad input");
		}

		[Fact]
		public async Task InnerFunction_ThrowsGenericException_ReturnsToolErrorMessage()
		{
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => throw new InvalidOperationException("unexpected")));

			Object? result = await wrapper.InvokeAsync();

			result.Should().Be("[Tool error] unexpected");
		}

		#endregion

		#region Result pass-through

		[Fact]
		public async Task InnerFunction_ReturnsValue_PassesThroughResult()
		{
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => Task.FromResult("expected")));

			Object? result = await wrapper.InvokeAsync();

			result.Should().BeOfType<System.Text.Json.JsonElement>()
				.Which.GetString().Should().Be("expected");
		}

		[Fact]
		public async Task ConfirmationApproved_PassesThroughInnerReturnValue()
		{
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)(() => Task.FromResult("payload")));
			wrapper.ConfirmationRequired += (s, e) => e.Confirm(true);

			Object? result = await wrapper.InvokeAsync();

			result.Should().BeOfType<System.Text.Json.JsonElement>()
				.Which.GetString().Should().Be("payload");
		}

		#endregion

		#region Metadata

		[Fact]
		public void Name_ReflectsWrappedMethodName()
		{
			ToolWrapper wrapper = new ToolWrapper(TestHelpers.Trace, (Func<Task<String>>)NamedMethod);

			wrapper.Name.Should().Be(nameof(NamedMethod));
		}

		private static Task<String> NamedMethod() => Task.FromResult(String.Empty);

		#endregion
	}
}
