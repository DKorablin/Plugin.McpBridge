using System.Diagnostics;
using System.Runtime.Serialization.Json;
using Plugin.McpBridge.Agents;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tests;

/// <summary>Launches and manages the DevUI executable as a child process for local agent diagnostics.</summary>
internal sealed class ProcessHost : IDisposable
{
	private const String ExeNameArgs1 = "Plugin.McpBridge.{0}.exe";
	private String ConfigName => $"McpBridge.{_exeType}.{Guid.NewGuid():N}.json";
	private String TraceName => $"{typeof(ProcessHost).Assembly.GetName().Name}.{this._exeType}";

	private readonly IHost _host;
	private readonly ExeType _exeType;
	private readonly ITraceSource _trace;
	private Process? _process;

	public enum ExeType
	{
		DevUI,
		AgUI,
		RAG,
		DTS,
	}

	public ProcessHost(IHost host, ExeType type)
	{
		this._host = host ?? throw new ArgumentNullException(nameof(host));
		this._exeType = type;
		this._trace = host.Plugins.CreateTraceSource(this.TraceName);
	}

	/// <summary>Serializes the current settings into a temp config file and launches the DevUI process.</summary>
	public Task StartAsync(Settings settings, CancellationToken cancellationToken = default)
	{
		if(this._process != null)
			this.Stop();

		String exePath = this.GetExePath();
		String configPath = Path.Combine(Path.GetTempPath(), this.ConfigName);

		SettingsDto config = this.BuildConfig(settings);
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsDto));
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

	public static String? GetExePath(ExeType type, Boolean throwException = false)
	{
		String exeName = String.Format(ExeNameArgs1, type);
		String? assemblyDir = Path.GetDirectoryName(typeof(ProcessHost).Assembly.Location);
		if(assemblyDir != null)
		{
			String candidate = Path.Combine(assemblyDir, exeName);
			if(File.Exists(candidate))
				return candidate;
		}

		return throwException
			? throw new FileNotFoundException($"{type} executable not found. Expected alongside the plugin assembly.", exeName)
			: null;
	}

	private void Stop()
	{
		if(this._process == null)
			return;

		try
		{
			if(!this._process.HasExited)
				this._process.Kill(entireProcessTree: true);
		}
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

	private String GetExePath()
		=> GetExePath(this._exeType, true)!;

	private SettingsDto BuildConfig(Settings settings)
	{
		String serverUrl = this._exeType switch
		{
			ExeType.DevUI => settings.DevUIServerUrl,
			ExeType.AgUI => settings.AgUIServerUrl,
			ExeType.RAG => String.Empty,
			ExeType.DTS => settings.DtsEmulatorEndpoint,
			_ => throw new InvalidOperationException($"Unsupported executable: {this._exeType}"),
		};

		return new SettingsDto(serverUrl, settings, AgentFactory.BuildSystemInstructions(settings, settings.SelectedAgent, this._host));
	}
}