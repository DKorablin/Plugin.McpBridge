using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class ToolFacadeTests
	{
		#region Constructor

		[Fact]
		public void Ctor_TraceIsNull_ThrowsArgumentNullException()
		{
			Action act = () => _ = new ToolFacade(null!, (Func<Task<String>>)(() => Task.FromResult(String.Empty)));

			act.Should().Throw<ArgumentNullException>().WithParameterName("trace");
		}

		#endregion

		#region Invocation

		[Fact]
		public async Task NoConfirmationSubscriber_InvokesInnerFunctionDirectly()
		{
			Boolean invoked = false;
			ToolFacade wrapper = new ToolFacade(TestUtils.Trace, (Func<Task<String>>)(() => { invoked = true; return Task.FromResult("ok"); }));

			await wrapper.InvokeAsync();

			invoked.Should().BeTrue();
		}

		#endregion

		#region Exception handling

		[Fact]
		public async Task InnerFunction_ThrowsGenericException_ReturnsToolErrorMessage()
		{
			ToolFacade wrapper = new ToolFacade(TestUtils.Trace, (Func<Task<String>>)(() => throw new InvalidOperationException("unexpected")));

			Func<Task> act = async () => await wrapper.InvokeAsync();

			await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("unexpected");
		}

		#endregion

		#region Result pass-through

		[Fact]
		public async Task InnerFunction_ReturnsValue_PassesThroughResult()
		{
			ToolFacade wrapper = new ToolFacade(TestUtils.Trace, (Func<Task<String>>)(() => Task.FromResult("expected")));

			Object? result = await wrapper.InvokeAsync();

			result.Should().BeOfType<System.Text.Json.JsonElement>()
				.Which.GetString().Should().Be("expected");
		}

		#endregion

		#region Metadata

		[Fact]
		public void Name_ReflectsWrappedMethodName()
		{
			ToolFacade wrapper = new ToolFacade(TestUtils.Trace, (Func<Task<String>>)NamedMethod);

			wrapper.Name.Should().Be(nameof(NamedMethod));
		}

		private static Task<String> NamedMethod() => Task.FromResult(String.Empty);

		#endregion
	}
}
