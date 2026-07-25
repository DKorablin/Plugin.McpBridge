using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plugin.McpBridge.Core.Remoting;

namespace Plugin.McpBridge.DTS;

internal static class Program
{
	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
			try
			{
				SettingsDto settings = SettingsDto.CreateSettingsFromArgs(ref args, lifetimeCts);

				HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
				builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
				builder.Logging.ClearProviders();
				builder.Logging.AddSimpleConsole(options =>
				{
					options.SingleLine = true;
					options.TimestampFormat = "HH:mm:ss ";
				});

				builder.Services.AddSingleton(settings);
				builder.Services.AddSingleton<DockerContainerManager>();
				builder.Services.AddHostedService<DtsEmulatorHost>();

				using IHost host = builder.Build();
				await host.RunAsync(lifetimeCts.Token);
			} catch(OperationCanceledException) when(lifetimeCts.IsCancellationRequested)
			{
				return 0;
			}
		return 0;
	}
}