using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using McpBridge.Core.Remoting;

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
		this._logger.EmulatorStarting();
		Boolean success = await this._containerManager.StartContainerAsync(this._settings.DtsEmulatorPort, this._settings.DtsEmulatorDashboardPort, cancellationToken);
		
		if(success)
			this._logger.EmulatorStarted(this._settings.DtsEmulatorEndpoint, this._settings.DtsEmulatorDashboard);
		else
			this._logger.EmulatorStartFailed();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		this._logger.EmulatorStopping();
		await this._containerManager.StopContainerAsync(cancellationToken);
		this._logger.EmulatorStopped();
	}
}