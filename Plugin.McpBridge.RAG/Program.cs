using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plugin.McpBridge.Tests;

namespace Plugin.McpBridge.RAGHost;

internal static class Program
{
	private static async Task<Int32> Main(String[] args)
	{
		using(CancellationTokenSource lifetimeCts = new CancellationTokenSource())
		{
			SettingsDto settings = SettingsDto.CreateSettingsFromArgs(ref args, lifetimeCts);

			HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
			builder.Logging.ClearProviders();
			builder.Logging.AddSimpleConsole(options =>
			{
				options.SingleLine = true;
				options.TimestampFormat = "HH:mm:ss ";
			});

			builder.Services.AddSingleton(settings);
			builder.Services.AddSingleton(new RagSyncOptions { DebounceInterval = TimeSpan.FromSeconds(1), });
			builder.Services.AddSingleton<RagFileFingerprintService>();
			builder.Services.AddSingleton<RagSyncMetadataStoreFactory>();
			builder.Services.AddSingleton<RagIndexSyncService>();
			builder.Services.AddSingleton<RagFileSyncCoordinator>();
			builder.Services.AddHostedService<RagSyncHostedService>();

			using IHost host = builder.Build();
			await host.RunAsync(lifetimeCts.Token);
		}
		return 0;
	}
}