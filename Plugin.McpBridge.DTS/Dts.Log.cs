using McpBridge.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Plugin.McpBridge.DTS;

internal static partial class DtsLog
{
	private const Int32 LogId = LogAppIDs.Dts;

	[LoggerMessage(EventId = LogId, Level = LogLevel.Information, Message = "DTS Emulator starting...")]
	public static partial void EmulatorStarting(this ILogger logger);

	[LoggerMessage(EventId = LogId + 1, Level = LogLevel.Information, Message = "DTS Emulator started successfully at {endpoint} (dashboard: {dashboard})")]
	public static partial void EmulatorStarted(this ILogger logger, String endpoint, String dashboard);

	[LoggerMessage(EventId = LogId + 2, Level = LogLevel.Error, Message = "Failed to start DTS Emulator. Check Docker installation and configuration.")]
	public static partial void EmulatorStartFailed(this ILogger logger);

	[LoggerMessage(EventId = LogId + 3, Level = LogLevel.Information, Message = "DTS Emulator stopping...")]
	public static partial void EmulatorStopping(this ILogger logger);

	[LoggerMessage(EventId = LogId + 4, Level = LogLevel.Information, Message = "DTS Emulator stopped")]
	public static partial void EmulatorStopped(this ILogger logger);

	[LoggerMessage(EventId = LogId + 5, Level = LogLevel.Warning, Message = "Failed to stop existing container, continuing anyway.")]
	public static partial void ContainerFailedToStop(this ILogger logger);

	[LoggerMessage(EventId = LogId + 6, Level = LogLevel.Information, Message = "DTS Emulator container started successfully ({image}:{tag})")]
	public static partial void ContainerStarted(this ILogger logger, String image, String tag);

	[LoggerMessage(EventId = LogId + 7, Level = LogLevel.Information, Message = "DTS Emulator container stopped and removed")]
	public static partial void ContainerStoppedAndRemoved(this ILogger logger);

	[LoggerMessage(EventId = LogId + 8, Level = LogLevel.Error, Message = "Failed to connect to Docker daemon at {endpoint} within timeout. Ensure Docker is running and accessible.")]
	public static partial void ContainerConnectionFailed(this ILogger logger, Uri? endpoint, Exception exc);

	[LoggerMessage(EventId = LogId + 9, Level = LogLevel.Information, Message = "Docker client initialized successfully. Endpoint: {endpoint}")]
	public static partial void ContainerConnectedSucessfully(this ILogger logger, Uri? endpoint);

	[LoggerMessage(EventId = LogId + 10, Level = LogLevel.Information, Message = "DTS Emulator image already available: {imageName}")]
	public static partial void DtsImageAlreadyAvailable(this ILogger logger, String imageName);

	[LoggerMessage(EventId = LogId + 11, Level = LogLevel.Information, Message = "Pulling DTS Emulator image: {imageName}")]
	public static partial void DtsEmulatorPulling(this ILogger logger, String imageName);

	[LoggerMessage(EventId = LogId + 12, Level = LogLevel.Information, Message = "DTS Emulator pulling status: {status}")]
	public static partial void DtsEmulatorPullingStatus(this ILogger logger, String status);

	[LoggerMessage(EventId = LogId + 13, Level = LogLevel.Information, Message = "DTS Emulator image pulled successfully")]
	public static partial void DtsEmulatorPulledSucessfully(this ILogger logger);

	[LoggerMessage(EventId = LogId + 14, Level = LogLevel.Information, Message = "Stopping existing container: {containerId}")]
	public static partial void ContainerStopping(this ILogger logger, String containerId);

	[LoggerMessage(EventId = LogId + 15, Level = LogLevel.Information, Message = "Starting DTS emulator container for ports gRPC={grpcPort}, Dashboard={dashboardPort}")]
	public static partial void DtsEmulatorStarting(this ILogger logger, Int32 grpcPort, Int32 dashboardPort);

	[LoggerMessage(EventId = LogId + 16, Level = LogLevel.Information, Message = "Container created: {containerId}")]
	public static partial void ContainerCreated(this ILogger logger, String containerId);

	[LoggerMessage(EventId = LogId + 17, Level = LogLevel.Information, Message = "Container started with ports gRPC={grpcPort}, Dashboard={dashboardPort}")]
	public static partial void DtsEmulatorStarted(this ILogger logger, Int32 grpcPort, Int32 dashboardPort);
}