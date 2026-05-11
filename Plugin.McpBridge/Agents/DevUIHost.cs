using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Launches and manages the DevUI executable as a child process for local agent diagnostics.</summary>
internal sealed class DevUIHost : IDisposable
{
	private const String ExeName = "Plugin.McpBridge.DevUI.exe";

	private Process? _process;

	public Int32 Port { get; }

	public DevUIHost(Int32 port = 5050)
		=> this.Port = port;

	/// <summary>Serializes the current settings into a temp config file and launches the DevUI process.</summary>
	public Task StartAsync(Settings settings, AiProviderDto provider, IHost pluginHost, String? bridgeUrl = null, CancellationToken cancellationToken = default)
	{
		if(this._process != null)
			this.Stop();

		String exePath = GetExePath();
		String configPath = Path.Combine(Path.GetTempPath(), $"McpBridge.DevUI.{Guid.NewGuid():N}.json");

		DevUIConfig config = BuildConfig(settings, provider, pluginHost);
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DevUIConfig));
		using(FileStream stream = File.Create(configPath))
			serializer.WriteObject(stream, config);

		ProcessStartInfo psi = new ProcessStartInfo(exePath, configPath)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		this._process = Process.Start(psi);
		return Task.CompletedTask;
	}

	/// <summary>Kills the DevUI child process if it is still running.</summary>
	public Task StopAsync(CancellationToken cancellationToken = default)
	{
		this.Stop();
		return Task.CompletedTask;
	}

	public void Dispose()
		=> this.Stop();

	private void Stop()
	{
		if(this._process == null)
			return;

		try
		{
			if(!this._process.HasExited)
				this._process.Kill(entireProcessTree: true);
		}
		catch(InvalidOperationException) { }
		finally
		{
			this._process.Dispose();
			this._process = null;
		}
	}

	private static String GetExePath()
	{
		String? assemblyDir = Path.GetDirectoryName(typeof(DevUIHost).Assembly.Location);
		if(assemblyDir != null)
		{
			String candidate = Path.Combine(assemblyDir, ExeName);
			if(File.Exists(candidate))
				return candidate;
		}
		throw new FileNotFoundException($"DevUI executable not found. Expected alongside the plugin assembly.", ExeName);
	}

	private DevUIConfig BuildConfig(Settings settings, AiProviderDto provider, IHost pluginHost)
	{
		List<DevUIPluginInfo> plugins = new List<DevUIPluginInfo>();
		Boolean allAllowed = settings.PluginsPermission == null || settings.PluginsPermission.Length == 0;
		foreach(IPluginDescription pluginDescription in pluginHost.Plugins)
		{
			if(!allAllowed && Array.Exists(settings.PluginsPermission!, p => p == pluginDescription.ID))
				continue;
			plugins.Add(new DevUIPluginInfo
			{
				Id = pluginDescription.ID,
				Name = pluginDescription.Name,
				Version = pluginDescription.Version?.ToString(),
				HasSettings = PluginSettingsTools.HasPluginSettings(pluginDescription),
				HasMembers = PluginMethodsTools.HasCallableMembers(pluginDescription),
			});
		}

		return new DevUIConfig
		{
			Port = this.Port,
			SystemPrompt = settings.AssistantSystemPrompt,
			MaxTokens = settings.MaxTokens,
			ToolsPermission = settings.ToolsPermission,
			PluginsPermission = settings.PluginsPermission,
			ConnectionTimeout = settings.ConnectionTimeout,
			Plugins = plugins,
			Provider = new DevUIProviderConfig
			{
				ProviderType = provider.ProviderType.ToString(),
				ModelId = provider.ModelId,
				ApiKey = provider.ApiKey,
				DeploymentName = provider.DeploymentName,
				ModelEndpointUrl = provider.ModelEndpointUrl,
				Temperature = provider.Temperature,
				ReasoningOutput = provider.ReasoningOutput?.ToString(),
				ReasoningEffort = provider.ReasoningEffort?.ToString(),
			},
		};
	}
}

