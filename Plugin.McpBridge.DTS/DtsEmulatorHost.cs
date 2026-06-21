using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plugin.McpBridge.Tests;

namespace Plugin.McpBridge.DTS;

/// <summary>Hosted service that manages DTS Emulator container lifecycle.</summary>
internal sealed class DtsEmulatorHost : IHostedService
{
	private readonly SettingsDto _settings;
	private readonly DockerContainerManager _containerManager;
	private readonly ILogger<DtsEmulatorHost> _logger;

	public DtsEmulatorHost(SettingsDto settings, DockerContainerManager containerManager, ILogger<DtsEmulatorHost> logger)
	{
		this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
		this._containerManager = containerManager ?? throw new ArgumentNullException(nameof(containerManager));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("DTS Emulator starting...");
		Boolean success = await this._containerManager.StartContainerAsync(this._settings.DtsEmulatorEndpoint, cancellationToken);
		
		if(success)
			this._logger.LogInformation("DTS Emulator started successfully at {Endpoint}", this._settings.DtsEmulatorEndpoint);
		else
			this._logger.LogError("Failed to start DTS Emulator. Check Docker installation and configuration.");
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("DTS Emulator stopping...");
		await this._containerManager.StopContainerAsync(cancellationToken);
		this._logger.LogInformation("DTS Emulator stopped");
	}
}