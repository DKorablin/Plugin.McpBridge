using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using Microsoft.Extensions.AI;
using Plugin.McpBridge;
using Plugin.McpBridge.Mcp;

namespace McpBridge.Core.Remoting;

/// <summary>Serializable snapshot of the settings needed to start the UI process.</summary>
public sealed class SettingsDto : Settings
{
	public static String AssemblyName => Assembly.GetEntryAssembly()?.GetName().Name ?? "Plugin.McpBridge.Undefined";

	/// <summary>Gets or sets the URL of the user interface server.</summary>
	public String UiServerUrl { get; set; } = String.Empty;

	/// <summary>Gets or sets the instructions associated with this instance.</summary>
	public String Instructions { get; set; } = String.Empty;

	public SettingsDto() { }//Used for serialization purposes

	internal SettingsDto(String uiServerUrl, Settings settings, String systemInstructions)
	{
		this.UiServerUrl = uiServerUrl;
		this.Instructions = systemInstructions;

		foreach(PropertyInfo prop in typeof(Settings).GetProperties())
			if(prop.CanRead && prop.CanWrite)
				prop.SetValue(this, prop.GetValue(settings));
	}

	public static SettingsDto CreateSettingsFromArgs(ref String[] args, CancellationTokenSource cts)
	{
		SettingsDto settings = SettingsDto.LoadSettingsFromJson(args);

		Int32 parentPidIndex = Array.IndexOf(args, "--parent-pid");
		if(parentPidIndex >= 0 && parentPidIndex + 1 < args.Length && Int32.TryParse(args[parentPidIndex + 1], out Int32 parentPid))
			_ = WatchParentAsync(parentPid, cts);

		args = parentPidIndex >= 0
			? args[1..parentPidIndex].Concat(args[(parentPidIndex + 2)..]).ToArray()
			: args[1..];

		return settings;
	}

	private static SettingsDto LoadSettingsFromJson(String[] args)
	{
		if(args.Length == 0)
			throw new InvalidOperationException($"No configuration file provided. Usage: {SettingsDto.AssemblyName} <config-file-path>");

		String configPath = args[0];
		if(!File.Exists(configPath))
			throw new InvalidOperationException($"Config file not found: {configPath}");

		SettingsDto config;
		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsDto));
		using(FileStream stream = File.OpenRead(configPath))
			config = (SettingsDto)serializer.ReadObject(stream)!;
		File.Delete(configPath);
		return config;
	}

	public async Task<AIFunction[]> FetchBridgeToolsAsync()
	{
		if(String.IsNullOrEmpty(this.McpServerUrl))
			return Array.Empty<AIFunction>();

		AIFunction[] tools;
		using(CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
		{
			HttpClient bridgeHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10), BaseAddress = new Uri(this.McpServerUrl) };
			tools = await McpClient.FetchAllAsync(SettingsDto.AssemblyName, bridgeHttp, timeoutCts.Token);
		}
		return tools;
	}

	private static async Task WatchParentAsync(Int32 parentPid, CancellationTokenSource cts)
	{
		try
		{
			Process parent = Process.GetProcessById(parentPid);
			await parent.WaitForExitAsync(cts.Token);
			Console.WriteLine($"Parent process {parentPid} exited. Shutting down.");
		} catch(ArgumentException)
		{
			Console.WriteLine($"Parent process {parentPid} not found. Shutting down.");
		} finally
		{
			await cts.CancelAsync();
		}
	}
}