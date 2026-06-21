using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge.DTS;

/// <summary>Manages Docker container lifecycle for DTS Emulator.</summary>
internal sealed class DockerContainerManager
{
	private const String DtsImageName = "mcpbridge/dts-emulator";
	private const String DtsImageTag = "latest";
	private const String DtsContainerName = "mcpbridge-dts-emulator";

	private readonly ILogger<DockerContainerManager> _logger;
	private DockerClient? _dockerClient;
	private String? _containerId;

	public DockerContainerManager(ILogger<DockerContainerManager> logger)
	{
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>Ensures Docker is available and image is present, then starts the container.</summary>
	public async Task<Boolean> StartContainerAsync(String dtsEndpoint, CancellationToken cancellationToken = default)
	{
		if(!this.TryInitializeDockerClient())
			return false;

		if(!await this.EnsureImageAvailableAsync(cancellationToken))
			return false;

		if(!await this.StopExistingContainerAsync(cancellationToken))
			this._logger.LogWarning("Failed to stop existing container, continuing anyway.");

		if(!await this.CreateAndStartContainerAsync(dtsEndpoint, cancellationToken))
			return false;

		this._logger.LogInformation("DTS Emulator container started successfully");
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

	private Boolean TryInitializeDockerClient()
	{
		DockerClientConfiguration config = new DockerClientConfiguration();
		this._dockerClient = config.CreateClient();

		this._logger.LogInformation("Docker client initialized successfully");
		return true;
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

	private async Task<Boolean> CreateAndStartContainerAsync(String dtsEndpoint, CancellationToken cancellationToken)
	{
		if(this._dockerClient == null)
			return false;

		Int32 port = this.ExtractPort(dtsEndpoint);
		String imageName = $"{DtsImageName}:{DtsImageTag}";

		CreateContainerResponse container = await this._dockerClient.Containers.CreateContainerAsync(
			new CreateContainerParameters
			{
				Image = imageName,
				Name = DtsContainerName,
				Hostname = "dts-emulator",
				ExposedPorts = new Dictionary<String, EmptyStruct>
				{
						{ $"{port}/tcp", default }
				},
				HostConfig = new HostConfig
				{
					PortBindings = new Dictionary<String, IList<PortBinding>>
					{
						{ $"{port}/tcp", new List<PortBinding> { new PortBinding { HostPort = port.ToString() } } }
					}
				}
			}, cancellationToken);

		this._containerId = container.ID;
		this._logger.LogInformation("Container created: {ContainerId}", this._containerId);

		await this._dockerClient.Containers.StartContainerAsync(this._containerId,
			new ContainerStartParameters(), cancellationToken);

		this._logger.LogInformation("Container started on port {Port}", port);
		return true;
	}

	private Int32 ExtractPort(String dtsEndpoint)
	{
		if(Uri.TryCreate(dtsEndpoint, UriKind.Absolute, out Uri? uri) && uri != null)
			return uri.Port > 0 ? uri.Port : 4001;
		
		return Int32.TryParse(dtsEndpoint, out Int32 port) ? port : 4001;
	}
}