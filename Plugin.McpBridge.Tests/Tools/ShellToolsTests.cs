using System;
using System.Threading.Tasks;
using FluentAssertions;
using Plugin.McpBridge.Tools;
using Xunit;

namespace Plugin.McpBridge.Tests.Tools
{
	public class ShellToolsTests
	{
		#region SystemInformation

		[Fact]
		public async Task SystemInformation_ReturnsCurrentTimeFromProvider()
		{
			DateTimeOffset fixedTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
			FakeTimeProvider timeProvider = new FakeTimeProvider(fixedTime);
			ShellTools sut = new ShellTools(timeProvider);

			String result = await sut.SystemInformation();

			result.Should().Contain(timeProvider.GetLocalNow().ToString());
		}

		[Fact]
		public async Task SystemInformation_ContainsOsVersion()
		{
			ShellTools sut = new ShellTools();

			String result = await sut.SystemInformation();

			result.Should().Contain(Environment.OSVersion.ToString());
		}

		#endregion

		#region Nested types

		private sealed class FakeTimeProvider : TimeProvider
		{
			private readonly DateTimeOffset _utcNow;

			public FakeTimeProvider(DateTimeOffset utcNow) => this._utcNow = utcNow;

			public override DateTimeOffset GetUtcNow() => this._utcNow;
		}

		#endregion
	}
}
