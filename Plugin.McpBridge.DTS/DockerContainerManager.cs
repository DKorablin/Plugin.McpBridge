using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Plugin.McpBridge.DTS;

/// <summary>Manages Docker container lifecycle for DTS Emulator.</summary>
internal sealed class DockerContainerManager
{
	private const String DtsImageName = "mcr.microsoft.com/dts/dts-emulator";
	private const String DtsImageTag = "latest";
	private const String DtsContainerName = "mcpbridge-dts-emulator";

	private readonly ILogger<DockerContainerManager> _logger;
	private DockerClient? _dockerClient;
	private String? _containerId;
	private Uri? _dockerEndpoint;

	public DockerContainerManager(ILogger<DockerContainerManager> logger)
	{
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>Ensures Docker is available and image is present, then starts the container.</summary>
	public async Task<Boolean> StartContainerAsync(Int32 dtsEmulatorPort, Int32 dtsEmulatorDashboardPort, CancellationToken cancellationToken = default)
	{
		if(!await this.TryInitializeDockerClientAsync(cancellationToken))
			return false;

		if(!await this.EnsureImageAvailableAsync(cancellationToken))
			return false;

		if(!await this.StopExistingContainerAsync(cancellationToken))
			this._logger.LogWarning("Failed to stop existing container, continuing anyway.");

		if(!await this.CreateAndStartContainerAsync(dtsEmulatorPort, dtsEmulatorDashboardPort, cancellationToken))
			return false;

		this._logger.LogInformation("DTS Emulator container started successfully ({Image}:{Tag})", DtsImageName, DtsImageTag);
		return true;
	}

	/// <summary>Stops and removes the DTS Emulator container.</summary>
	public async Task StopContainerAsync(CancellationToken cancellationToken = default)
	{
		if(this._dockerClient == null || String.IsNullOrEmpty(this._containerId))
			return;

		await this._dockerClient.Containers.StopContainerAsync(this._containerId,
			new ContainerStopParameters { WaitBeforeKillSeconds = 10 }, cancellationToken);

		await this._dockerClient.Containers.RemoveContainerAsync(this._containerId,
			new ContainerRemoveParameters(), cancellationToken);

		this._logger.LogInformation("DTS Emulator container stopped and removed");
	}

	private async Task<Boolean> TryInitializeDockerClientAsync(CancellationToken cancellationToken)
	{
		System.Diagnostics.Debugger.Launch();
		if(this._dockerClient != null)
			return true;

		this._dockerEndpoint = this.ResolveDockerEndpoint();

		DockerClientConfiguration config = this._dockerEndpoint == null
			? new DockerClientConfiguration()
			: new DockerClientConfiguration(this._dockerEndpoint);

		this._dockerClient = config.CreateClient();

		using CancellationTokenSource connectTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		connectTimeoutCts.CancelAfter(TimeSpan.FromSeconds(8));
		try
		{
			await this._dockerClient.System.PingAsync(connectTimeoutCts.Token);
		} catch(TimeoutException exc)
		{
			this._logger.LogError(exc, "Failed to connect to Docker daemon at {Endpoint} within timeout. Ensure Docker is running and accessible.", this._dockerEndpoint);
			throw;
		}

		this._logger.LogInformation("Docker client initialized successfully. Endpoint: {Endpoint}",
			this._dockerEndpoint?.ToString() ?? "default endpoint");
		return true;
	}

	private Uri? ResolveDockerEndpoint()
	{
		String? dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
		if(!String.IsNullOrWhiteSpace(dockerHost) && Uri.TryCreate(dockerHost, UriKind.Absolute, out Uri? dockerHostUri) && dockerHostUri != null)
			return dockerHostUri;

		if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return new Uri("npipe://./pipe/docker_engine");

		return null;
	}

	private async Task<Boolean> EnsureImageAvailableAsync(CancellationToken cancellationToken)
	{
		if(this._dockerClient == null)
			return false;

		String fullImageName = $"{DtsImageName}:{DtsImageTag}";

		IList<ImagesListResponse> images = await this._dockerClient.Images.ListImagesAsync(
			new ImagesListParameters
			{
				Filters = new Dictionary<String, IDictionary<String, Boolean>>
			{
					{ "reference", new Dictionary<String, Boolean> { { fullImageName, true } } }
			}
			}, cancellationToken);

		if(images.Count > 0)
		{
			this._logger.LogInformation("DTS Emulator image already available: {ImageName}", fullImageName);
			return true;
		}

		this._logger.LogInformation("Pulling DTS Emulator image: {ImageName}", fullImageName);

		await this._dockerClient.Images.CreateImageAsync(
			new ImagesCreateParameters { FromImage = DtsImageName, Tag = DtsImageTag },
			null,
			new Progress<JSONMessage>(msg =>
			{
				if(!String.IsNullOrEmpty(msg.Status))
					this._logger.LogInformation("Image pull: {Status}", msg.Status);
			}),
			cancellationToken);

		this._logger.LogInformation("DTS Emulator image pulled successfully");
		return true;
	}

	private async Task<Boolean> StopExistingContainerAsync(CancellationToken cancellationToken)
	{
		if(this._dockerClient == null)
			return true;

		IList<ContainerListResponse> containers = await this._dockerClient.Containers.ListContainersAsync(
			new ContainersListParameters
			{
				All = true,
				Filters = new Dictionary<String, IDictionary<String, Boolean>>
				{
						{ "name", new Dictionary<String, Boolean> { { DtsContainerName, true } } }
				}
			}, cancellationToken);

		foreach(ContainerListResponse container in containers)
		{
			if(container.State == "running")
			{
				this._logger.LogInformation("Stopping existing container: {ContainerId}", container.ID);
				await this._dockerClient.Containers.StopContainerAsync(container.ID,
					new ContainerStopParameters { WaitBeforeKillSeconds = 10 }, cancellationToken);
			}

			await this._dockerClient.Containers.RemoveContainerAsync(container.ID,
				new ContainerRemoveParameters(), cancellationToken);
		}

		return true;
	}

	private async Task<Boolean> CreateAndStartContainerAsync(Int32 dtsEmulatorPort, Int32 dtsEmulatorDashboardPort, CancellationToken cancellationToken)
	{
		if(this._dockerClient == null)
			return false;

		this._logger.LogInformation("Starting DTS emulator container for ports gRPC={GrpcPort}, Dashboard={DashboardPort}", dtsEmulatorPort, dtsEmulatorDashboardPort);
		String imageName = $"{DtsImageName}:{DtsImageTag}";
		Int32[] dtsPorts = [dtsEmulatorPort, dtsEmulatorDashboardPort];

		Dictionary<String, EmptyStruct> exposedPorts = new Dictionary<String, EmptyStruct>();
		Dictionary<String, IList<PortBinding>> portBindings = new Dictionary<String, IList<PortBinding>>();
		foreach(Int32 port in dtsPorts)
		{
			String containerPort = $"{port}/tcp";
			exposedPorts[containerPort] = default;
			portBindings[containerPort] = [new PortBinding { HostPort = port.ToString() }];
		}

		CreateContainerResponse container = await this._dockerClient.Containers.CreateContainerAsync(
			new CreateContainerParameters
			{
				Image = imageName,
				Name = DtsContainerName,
				Hostname = "dts-emulator",
				ExposedPorts = exposedPorts,
				HostConfig = new HostConfig
				{
					PortBindings = portBindings
				}
			}, cancellationToken);

		this._containerId = container.ID;
		this._logger.LogInformation("Container created: {ContainerId}", this._containerId);

		await this._dockerClient.Containers.StartContainerAsync(this._containerId,
			new ContainerStartParameters(), cancellationToken);

		this._logger.LogInformation("Container started with ports gRPC={GrpcPort}, Dashboard={DashboardPort}", dtsEmulatorPort, dtsEmulatorDashboardPort);
		return true;
	}
}