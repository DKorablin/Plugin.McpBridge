using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Data;
using SAL.Flatbed;

namespace Plugin.McpBridge.Agents;

/// <summary>Launches and manages the DevUI executable as a child process for local agent diagnostics.</summary>
internal sealed class DevUIHost : IDisposable
{
	private const String ExeName = "Plugin.McpBridge.DevUI.exe";

	private readonly IHost _host;
	private readonly ITraceSource _trace;
	private Process? _process;

	public DevUIHost(IHost host)
	{
		this._host = host;
		this._trace = host.Plugins.CreateTraceSource(typeof(DevUIHost).Assembly.GetName().Name + ".DevUI");
	}

	/// <summary>Serializes the current settings into a temp config file and launches the DevUI process.</summary>
	public Task StartAsync(Settings settings, AiProviderDto provider, CancellationToken cancellationToken = default)
	{
		if(this._process != null)
			this.Stop();

		String exePath = GetExePath();
		String configPath = Path.Combine(Path.GetTempPath(), $"McpBridge.DevUI.{Guid.NewGuid():N}.json");

		DevUIConfig config = BuildConfig(settings, provider);
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DevUIConfig));
		using(FileStream stream = File.Create(configPath))
			serializer.WriteObject(stream, config);

		ProcessStartInfo psi = new ProcessStartInfo(exePath, $"{configPath} --parent-pid {Environment.ProcessId}")
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};

		this._process = Process.Start(psi);
		if(this._process != null)
		{
			this._process.OutputDataReceived += this.OnProcessOutputReceived;
			this._process.ErrorDataReceived += this.OnProcessErrorReceived;
			this._process.BeginOutputReadLine();
			this._process.BeginErrorReadLine();
		}
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

	private void OnProcessOutputReceived(Object sender, DataReceivedEventArgs e)
	{
		if(e.Data != null)
			this._trace.TraceEvent(TraceEventType.Information, 0, e.Data);
	}

	private void OnProcessErrorReceived(Object sender, DataReceivedEventArgs e)
	{
		if(e.Data != null)
			this._trace.TraceEvent(TraceEventType.Warning, 0, e.Data);
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

	private DevUIConfig BuildConfig(Settings settings, AiProviderDto provider)
		=> new DevUIConfig
		{
			DevUiServerUrl = settings.DevUIServerUrl,
			Instructions = AgentFactory.BuildSystemInstructions(this._host, settings),
			MaxTokens = settings.MaxTokens,
			ToolsPermission = settings.ToolsPermission,
			PluginsPermission = settings.PluginsPermission,
			ConnectionTimeout = settings.ConnectionTimeout,
			Provider = provider,
			McpServerUrl = settings.McpServerUrl,
		};
}