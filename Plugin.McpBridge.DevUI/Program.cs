using System.ClientModel;
using System.ClientModel.Primitives;
using System.Runtime.Serialization.Json;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using Plugin.McpBridge;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.DevUI;

System.Diagnostics.Debugger.Launch();

if(args.Length == 0)
{
	Console.Error.WriteLine("Usage: Plugin.McpBridge.DevUI <config-file>");
	return 1;
}

String configPath = args[0];
if(!File.Exists(configPath))
{
	Console.Error.WriteLine($"Config file not found: {configPath}");
	return 1;
}

DevUIConfig config;
DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DevUIConfig));
using(FileStream stream = File.OpenRead(configPath))
	config = (DevUIConfig)serializer.ReadObject(stream)!;

File.Delete(configPath);

// Build IChatClient from config
HttpClient httpClient = new HttpClient { Timeout = config.ConnectionTimeout };
IChatClient rawClient = BuildChatClient(config.Provider, httpClient);

IChatClient configuredClient = new ChatClientBuilder(rawClient)
	.ConfigureOptions(options =>
	{
		if(config.MaxTokens.HasValue)
			options.MaxOutputTokens = config.MaxTokens.Value;
		if(config.Provider.Temperature.HasValue)
			options.Temperature = (Single)config.Provider.Temperature.Value;
		if(config.Provider.ReasoningOutput != null || config.Provider.ReasoningEffort != null)
			options.Reasoning = new ReasoningOptions
			{
				Output = Enum.TryParse<ReasoningOutput>(config.Provider.ReasoningOutput, out ReasoningOutput ro) ? ro : ReasoningOutput.None,
				Effort = Enum.TryParse<ReasoningEffort>(config.Provider.ReasoningEffort, out ReasoningEffort re) ? re : ReasoningEffort.Medium
			};
	})
	.Build();

String instructions = BuildSystemInstructions(config);

// Fetch bridge tools from the plugin process (if a bridge URL was provided)
IReadOnlyList<BridgeProxyTool> bridgeTools = Array.Empty<BridgeProxyTool>();
if(!String.IsNullOrEmpty(config.BridgeUrl))
{
	HttpClient bridgeHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
	for(Int32 attempt = 0; attempt < 5; attempt++)
	{
		try
		{
			bridgeTools = await BridgeProxyTool.FetchAllAsync(config.BridgeUrl, bridgeHttp);
			Console.WriteLine($"Bridge connected: {bridgeTools.Count} tools loaded from {config.BridgeUrl}");
			break;
		}
		catch(HttpRequestException)
		{
			if(attempt == 4)
				Console.Error.WriteLine($"Warning: could not connect to tool bridge at {config.BridgeUrl}. Tools unavailable.");
			else
				await Task.Delay(500);
		}
	}
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args[1..]);
((IWebHostBuilder)builder.WebHost).UseUrls($"http://localhost:{config.Port}");

IHostedAgentBuilder agentBuilder = builder.AddAIAgent("assistant", instructions, configuredClient);
if(bridgeTools.Count > 0)
	agentBuilder.WithAITools(bridgeTools.Cast<AITool>().ToArray());

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

WebApplication app = builder.Build();
app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();

Console.WriteLine($"DevUI running at http://localhost:{config.Port}/devui");
await app.RunAsync();
return 0;

static IChatClient BuildChatClient(DevUIProviderConfig provider, HttpClient httpClient)
{
	HttpClientPipelineTransport transport = new HttpClientPipelineTransport(httpClient);
	if(provider.ProviderType == nameof(AiProviderType.AzureOpenAI))
		return new AzureOpenAIClient(
			new Uri(provider.ModelEndpointUrl!),
			new ApiKeyCredential(provider.ApiKey!),
			new AzureOpenAIClientOptions { Transport = transport })
			.GetChatClient(provider.DeploymentName ?? provider.ModelId)
			.AsIChatClient();

	OpenAIClientOptions clientOptions = new OpenAIClientOptions { Transport = transport };
	if(provider.ModelEndpointUrl != null)
		clientOptions.Endpoint = new Uri(provider.ModelEndpointUrl);

	return new OpenAIClient(new ApiKeyCredential(provider.ApiKey ?? "local-no-key"), clientOptions)
		.GetChatClient(provider.ModelId)
		.AsIChatClient();
}

static String BuildSystemInstructions(DevUIConfig config)
{
	StringBuilder sb = new StringBuilder(config.SystemPrompt);
	sb.AppendLine();
	sb.AppendLine();

	if(config.Plugins.Count > 0)
	{
		sb.AppendLine("Loaded SAL plugins:");
		foreach(DevUIPluginInfo p in config.Plugins)
		{
			sb.Append("- ");
			sb.Append(p.Id);
			sb.Append(" | ");
			sb.Append(p.Name);
			sb.Append(" | ");
			sb.Append(p.Version);
			sb.Append(" | Settings: ");
			sb.Append(p.HasSettings ? "yes" : "no");
			sb.Append(" | Members: ");
			sb.AppendLine(p.HasMembers ? "yes" : "no");
		}
	} else
		sb.AppendLine("No SAL plugins are available.");

	return sb.ToString().TrimEnd();
}
