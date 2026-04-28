using Microsoft.Extensions.AI;
using Plugin.McpBridge.Tools;

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
- `settings` — confirmation panel (SettingsSet)
- `image` - embedded image";

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

		private static readonly String ImageResponse = @"Here is an image:
data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPoAAAD6CAYAAACI7Fo9AAAACXBIWXMAAAsSAAALEgHS3X78AAACGUlEQVR4nO3TMQEAIAzAsIF/z0NGHjQKej07s/OxqwO4y7kA4G8EAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAvwB0vQCBX0T2WQAAAAASUVORK5CYII=

And some extra text";

		public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
		{
			ChatMessage lastMessage = messages.LastOrDefault() ?? new ChatMessage(ChatRole.User, String.Empty);

			if(lastMessage.Role == ChatRole.Tool)
			{
				if(lastMessage.Contents.OfType<FunctionResultContent>().FirstOrDefault() is FunctionResultContent call)
				{
					return Task.FromResult(BuildText($@"Received a tool **{call.CallId}** call with
Exception:
```
{call.Exception}
```
Result:
```
{call.Result}
```"));
				}else
					return Task.FromResult(BuildText($@"The tool was executed. Here is a **summary** of the result:
```
{String.Join(" ", lastMessage.Contents.Select(c => c.RawRepresentation))}
```"));
			} else if(lastMessage.Role == ChatRole.User)
			{
				String lower = lastMessage.Text.ToLowerInvariant();
				switch(lower)
				{
				case "invoke":
					return Task.FromResult(BuildToolCall(nameof(PluginMethodsTools.MethodsInvoke), new Dictionary<String, Object?>
					{
						{ "pluginId", "stub-plugin" },
						{ "methodName", "StubMethod" },
						{ "argumentsJson", "{}" },
					}));
				case "events":
					return Task.FromResult(BuildToolCall(nameof(PluginMethodsTools.MethodsInvoke), new Dictionary<String, Object?>
					{
						{ "pluginId", "535b6be7-847b-45ab-bdaa-68e1e52be508" },
						{ "methodName", "GetEvents" },
						{ "argumentsJson", $"{{\"timeStart\":\"{DateTime.Today.AddDays(-1).ToString("s")}\", \"timeEnd\":\"{DateTime.Today.AddSeconds(-1).ToString("s")}\", \"eventLogEntryTypes\": [\"Error\",\"Warning\",\"Information\"]}}" },
					}));
				case "settings":
					return Task.FromResult(BuildToolCall(nameof(PluginSettingsTools.SettingsSet), new Dictionary<String, Object?>
					{
						{ "pluginId", "stub-plugin" },
						{ "settingName", "StubSetting" },
						{ "valueJson", "\"stub-value\"" },
					}));
				case "help":
					return Task.FromResult(BuildText(HelpResponse));
				case "long":
					return Task.FromResult(BuildText(LongResponse));
				case "image":
					return Task.FromResult(BuildText(ImageResponse));
				default:
					return Task.FromResult(BuildText(DefaultResponse));
				}
			} else
				return Task.FromResult(BuildText($"Received a message with role {lastMessage.Role} and text: {lastMessage.Text}"));
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
