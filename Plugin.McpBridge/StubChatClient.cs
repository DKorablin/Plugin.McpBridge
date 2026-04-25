using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge
{
	/// <summary>Scripted IChatClient for UI testing — returns predefined responses with no network or credentials.</summary>
	/// <remarks>
	/// Trigger phrases in the user message (case-insensitive):
	/// <list type="bullet">
	///   <item><c>help</c> — markdown response with headers, lists and code.</item>
	///   <item><c>long</c> — multi-paragraph response to exercise scrolling.</item>
	///   <item><c>invoke</c> — simulates a MethodsInvoke tool call; triggers the confirmation panel.</item>
	///   <item><c>events</c> — simulates a real plugin method call to get events; triggers the confirmation panel.</item>
	///   <item><c>settings</c> — simulates a SettingsSet tool call; triggers the confirmation panel.</item>
	///   <item>anything else — short default markdown response.</item>
	/// </list>
	/// </remarks>
	internal sealed class StubChatClient : IChatClient
	{
		private static readonly String DefaultResponse = @"## Stub AI

I am a **stub** assistant for UI testing.

Try typing:
- `help` — formatted markdown
- `long` — long response
- `invoke` — confirmation panel (MethodsInvoke)
- `events` - real test plugin method call (MethodsInvoke)
- `settings` — confirmation panel (SettingsSet)";

		private static readonly String HelpResponse = @"# Markdown Test

## Headers work

**Bold**, *italic*, and `inline code` are rendered.

### Lists

- First item
- Second item
+ Third item

### Code block

```
var x = 42;
Console.WriteLine(x);
```

*End of markdown test.*";

		private static readonly String LongResponse = "## Long Response\n\n" +
			String.Join("\n\n", Enumerable.Range(1, 50).Select(i => $@"**Paragraph {i:N0}:** 
Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."));

		public Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			ChatMessage[] allMessages = messages as ChatMessage[] ?? messages.ToArray();
			String userText = allMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? String.Empty;
			String lower = userText.ToLowerInvariant();

			if(allMessages[allMessages.Length - 1].Role == ChatRole.Tool)
				return Task.FromResult(BuildText("The tool was executed. Here is a **summary** of the result."));

			if(lower.Contains("invoke"))
				return Task.FromResult(BuildToolCall(nameof(AssistantAgent.MethodsInvoke), new Dictionary<String, Object?>
				{
					{ "pluginId", "stub-plugin" },
					{ "methodName", "StubMethod" },
					{ "argsJson", "{}" },
				}));

			if(lower.Contains("events"))
				return Task.FromResult(BuildToolCall(nameof(AssistantAgent.MethodsInvoke), new Dictionary<String, Object?>
				{
					{ "pluginId", "535b6be7-847b-45ab-bdaa-68e1e52be508" },
					{ "methodName", "GetEvents" },
					{ "argsJson", "{\"timeStart\":\"2026-04-25T00:00:00\", \"timeEnd\":\"2026-04-25T23:59:59\", \"eventLogEntryTypes\": [\"Error\",\"Warning\",\"Information\"]}" },
				}));

			if(lower.Contains("settings"))
				return Task.FromResult(BuildToolCall(nameof(AssistantAgent.SettingsSet), new Dictionary<String, Object?>
				{
					{ "pluginId", "stub-plugin" },
					{ "settingName", "StubSetting" },
					{ "valueJson", "\"stub-value\"" },
				}));

			if(lower.Contains("help"))
				return Task.FromResult(BuildText(HelpResponse));

			if(lower.Contains("long"))
				return Task.FromResult(BuildText(LongResponse));

			return Task.FromResult(BuildText(DefaultResponse));
		}

		public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException("Stub client does not support streaming.");

		public Object? GetService(Type serviceType, Object? serviceKey = null) => null;

		public void Dispose() { }

		private static ChatResponse BuildText(String text)
			=> new ChatResponse(new ChatMessage(ChatRole.Assistant, text));

		private static ChatResponse BuildToolCall(String functionName, Dictionary<String, Object?> args)
		{
			FunctionCallContent call = new FunctionCallContent("stub-call-1", functionName, args);
			ChatMessage message = new ChatMessage(ChatRole.Assistant, [call]);
			return new ChatResponse(message);
		}
	}
}
