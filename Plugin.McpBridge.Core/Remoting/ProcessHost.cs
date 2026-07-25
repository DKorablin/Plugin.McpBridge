using System.Diagnostics;
using System.Runtime.Serialization.Json;

namespace Plugin.McpBridge.Core.Remoting;

/// <summary>Launches and manages the DevUI executable as a child process for local agent diagnostics.</summary>
internal sealed class ProcessHost : IDisposable
{
	private const String ExeNameArgs1 = "Plugin.McpBridge.{0}.exe";
	private String ConfigName => $"McpBridge.{_exeType}.{Guid.NewGuid():N}.json";

	private readonly ProcessType _exeType;
	private readonly IMcpTrace _trace;
	private readonly Action<ProcessType, Int32>? _onProcessFailed;
	private Process? _process;

	public ProcessHost(ProcessType type, IMcpTrace trace, Action<ProcessType, Int32>? onProcessFailed = null)
	{
		this._exeType = type;
		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._onProcessFailed = onProcessFailed;
	}

	/// <summary>Serializes the current settings into a temp config file and launches the DevUI process.</summary>
	public Task StartAsync(Settings settings, String instructions, CancellationToken cancellationToken = default)
	{
		if(this._process != null)
			this.Stop();

		String exePath = this.GetProcessPath();
		String configPath = Path.Combine(Path.GetTempPath(), this.ConfigName);

		SettingsDto config = this.BuildConfig(settings, instructions);
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
			Task.Run(() => this.WaitForProcessExitAsync(cancellationToken));
		}
		return Task.CompletedTask;
	}

	/// <summary>Kills the DevUI child process if it is still running.</summary>
	public Task StopAsync(CancellationToken cancellationToken = default)
	{
		this.Stop();
		return Task.CompletedTask;
	}

	/// <summary>Waits for the process to exit and invokes the failure callback if it exits with non-zero code.</summary>
	private async Task WaitForProcessExitAsync(CancellationToken cancellationToken)
	{
		if(this._process == null)
			return;

		try
		{
			var process = this._process;
			await process.WaitForExitAsync(cancellationToken);
			if(this._process == null)
				return;// Process was stopped while waiting

			Int32 exitCode = process.ExitCode;
			if(exitCode != 0)
			{
				this._trace.TraceEvent(TraceEventType.Warning, 0, $"Process {this._exeType} exited with code {exitCode:X8}");
				this._onProcessFailed?.Invoke(this._exeType, exitCode);
			}
		}
		catch(OperationCanceledException)
		{
			// Process was cancelled, not a failure
		}
		catch(Exception ex)
		{
			this._trace.TraceEvent(TraceEventType.Error, 0, $"Error monitoring process {this._exeType}: {ex.Message}");
			this._onProcessFailed?.Invoke(this._exeType, -1);
		}
	}

	public void Dispose()
		=> this.Stop();

	public static String? GetProcessPath(ProcessType type, Boolean throwException = false)
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

		var process = this._process;
		Interlocked.Exchange(ref this._process, null);
		try
		{
			if(!process.HasExited)
				process.Kill(entireProcessTree: true);
		}
		finally
		{
			process.Dispose();
		}
	}

	private void OnProcessOutputReceived(Object sender, DataReceivedEventArgs e)
	{
		if(e.Data != null)
			this._trace.TraceEvent(TraceEventType.Information, 0, e.Data);
	}

	private void OnProcessErrorReceived(Object sender, DataReceivedEventArgs e)
	{
		this._trace.TraceEvent(TraceEventType.Warning, 0, $"Error received from process {this._exeType}: {e.Data ?? "<null>"}");
	}

	private String GetProcessPath()
		=> GetProcessPath(this._exeType, true)!;

	private SettingsDto BuildConfig(Settings settings, String instructions)
	{
		String serverUrl = this._exeType switch
		{
			ProcessType.DevUI => settings.DevUIServerUrl,
			ProcessType.AgUI => settings.AgUIServerUrl,
			ProcessType.RAG => String.Empty,
			ProcessType.DTS => settings.DtsEmulatorEndpoint,
			_ => throw new InvalidOperationException($"Unsupported executable: {this._exeType}"),
		};

		return new SettingsDto(serverUrl, settings, instructions);
	}
}