using System.IO.Pipes;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SAL.Flatbed;
using HostingIHost = Microsoft.Extensions.Hosting.IHost;

namespace Plugin.McpBridge
{
	/// <summary>Manages the in-process MCP server and client over anonymous pipe transport.</summary>
	internal sealed class McpBridge : IDisposable
	{
		private readonly SAL.Flatbed.IHost _host;
		private readonly PluginSettingsHelper _settingsHelper;
		private readonly TraceSource _trace;
		private readonly Object _syncRoot = new Object();
		private HostingIHost? _mcpHost;
		private McpClient? _mcpClient;
		private Stream? _serverInputStream;
		private Stream? _serverOutputStream;
		private Stream? _clientInputStream;
		private Stream? _clientOutputStream;

		public McpBridge(TraceSource trace, SAL.Flatbed.IHost host, PluginSettingsHelper settingsHelper)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
			this._settingsHelper = settingsHelper ?? throw new ArgumentNullException(nameof(settingsHelper));
			this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		}

		public void Start()
		{
			lock(this._syncRoot)
			{
				if(this._mcpClient != null)
					return;

				String pipeSuffix = Guid.NewGuid().ToString("N");
				String pipeToServerName = "PluginMcpBridge_In_" + pipeSuffix;
				String pipeFromServerName = "PluginMcpBridge_Out_" + pipeSuffix;

				NamedPipeServerStream serverInput = new NamedPipeServerStream(pipeToServerName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				NamedPipeServerStream serverOutput = new NamedPipeServerStream(pipeFromServerName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				NamedPipeClientStream clientOutput = new NamedPipeClientStream(".", pipeToServerName, PipeDirection.Out, PipeOptions.Asynchronous);
				NamedPipeClientStream clientInput = new NamedPipeClientStream(".", pipeFromServerName, PipeDirection.In, PipeOptions.Asynchronous);

				clientOutput.Connect();
				clientInput.Connect();
				serverInput.WaitForConnection();
				serverOutput.WaitForConnection();

				this._serverInputStream = new TraceTapStream(serverInput, this._trace, "C->S", true, false);
				this._serverOutputStream = new TraceTapStream(serverOutput, this._trace, "S->C", false, true);
				this._clientInputStream = clientInput;
				this._clientOutputStream = clientOutput;

				IHostBuilder hostBuilder = new HostBuilder();
				hostBuilder.ConfigureServices((hostContext, services) =>
				{
					IMcpServerBuilder mcpServerBuilder = services.AddMcpServer(options =>
					{
						options.ServerInfo = new Implementation() { Name = "Plugin.McpBridge", Version = typeof(McpBridge).Assembly.GetName().Version.ToString() };
						options.ServerInstructions = "Expose loaded SAL plugins as MCP tools.";
					});

					mcpServerBuilder.WithTools(this.CreateMcpTools());
					mcpServerBuilder.WithStreamServerTransport(this._serverInputStream, this._serverOutputStream);
				});

				this._mcpHost = hostBuilder.Build();
				this._mcpHost.StartAsync(CancellationToken.None).GetAwaiter().GetResult();

				IClientTransport clientTransport = new StreamClientTransport(this._clientOutputStream!, this._clientInputStream!, null);
				this._mcpClient = McpClient.CreateAsync(clientTransport, new McpClientOptions(), null, CancellationToken.None).GetAwaiter().GetResult();
			}
		}

		public void Stop()
		{
			lock(this._syncRoot)
			{
				if(this._mcpClient != null)
				{
					this._mcpClient.DisposeAsync().AsTask().GetAwaiter().GetResult();
					this._mcpClient = null;
				}

				if(this._mcpHost != null)
				{
					this._mcpHost.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
					this._mcpHost.Dispose();
					this._mcpHost = null;
				}

				this._clientOutputStream?.Dispose();
				this._clientOutputStream = null;

				this._clientInputStream?.Dispose();
				this._clientInputStream = null;

				this._serverOutputStream?.Dispose();
				this._serverOutputStream = null;

				this._serverInputStream?.Dispose();
				this._serverInputStream = null;
			}
		}

		public void Dispose() => this.Stop();

		public String ListLoadedToolsFromMcpClient()
		{
			if(this._mcpClient == null)
				return "MCP client is not connected.";

			IList<McpClientTool> tools = this._mcpClient.ListToolsAsync(new RequestOptions(), CancellationToken.None).GetAwaiter().GetResult();
			if(tools == null || tools.Count <= 0)
				return "No tools returned by MCP server.";

			StringBuilder builder = new StringBuilder();
			foreach(McpClientTool tool in tools)
			{
				builder.Append("- ");
				builder.Append(tool.ProtocolTool.Name);
				if(!String.IsNullOrWhiteSpace(tool.ProtocolTool.Description))
				{
					builder.Append(" : ");
					builder.Append(tool.ProtocolTool.Description);
				}
				builder.AppendLine();
			}

			return builder.ToString().Trim();
		}

		private IEnumerable<McpServerTool> CreateMcpTools()
		{
			yield return McpServerTool.Create(new Func<String>(this.ListLoadedPluginsFromHost), new McpServerToolCreateOptions() { Description = "Lists all plugins currently loaded in the SAL host, including their ID, name, version, whether they expose settings, and their available methods." });
			yield return McpServerTool.Create(new Func<String, String>(this._settingsHelper.ListPluginSettings), new McpServerToolCreateOptions() { Description = "Lists all settings exposed by a specific SAL plugin, including each setting's name, current value, type, and description. Requires the plugin ID." });
			yield return McpServerTool.Create(new Func<String, String, String>(this._settingsHelper.ReadPluginSetting), new McpServerToolCreateOptions() { Description = "Reads the current value of a single setting from a specific SAL plugin. Requires the plugin ID and the setting name (property name or display name)." });
			yield return McpServerTool.Create(new Func<String, String, String, String>(this._settingsHelper.UpdatePluginSetting), new McpServerToolCreateOptions() { Description = "Updates the value of a single setting on a specific SAL plugin and returns the new value. Requires the plugin ID, the setting name, and the new value as a string." });
			yield return McpServerTool.Create(new Func<String, String, String, String>(this.InvokePluginMethodPlaceholder), new McpServerToolCreateOptions() { Description = "Invokes a named method on a specific SAL plugin with the provided arguments serialized as JSON. Requires the plugin ID, the method name, and a JSON string of arguments." });
		}

		public String ListLoadedPluginsFromHost()
		{
			if(this._host.Plugins.Count <= 0)
				return "No plugins loaded.";

			StringBuilder pluginsText = new StringBuilder();

			foreach(IPluginDescription pluginDescription in this._host.Plugins)
			{
				pluginsText.Append("- ");
				pluginsText.Append(pluginDescription.ID);
				pluginsText.Append(" | ");
				pluginsText.Append(pluginDescription.Name);
				pluginsText.Append(" | ");
				pluginsText.Append(pluginDescription.Version.ToString());
				pluginsText.Append(" | Settings: ");
				pluginsText.Append(PluginSettingsHelper.HasPluginSettings(pluginDescription) ? "yes" : "no");
				pluginsText.AppendLine();

				if(pluginDescription.Type != null && pluginDescription.Type.Members != null)
					foreach(IPluginMemberInfo pluginMember in pluginDescription.Type.Members)
						if(pluginMember != null && pluginMember.MemberType == System.Reflection.MemberTypes.Method)
						{
							pluginsText.Append("  * method: ");
							pluginsText.Append(pluginMember.Name);
							pluginsText.AppendLine();
						}
			}

			return pluginsText.ToString().Trim();
		}

		private String InvokePluginMethodPlaceholder(String pluginId, String methodName, String argumentsJson)
			=> $"Plugin invocation placeholder. Plugin='{pluginId}', Method='{methodName}', Args='{argumentsJson}'";

		private sealed class TraceTapStream : Stream
		{
			private const Int32 MaxTraceChars = 4096;
			private readonly Stream _inner;
			private readonly TraceSource _trace;
			private readonly String _channel;
			private readonly Boolean _traceReads;
			private readonly Boolean _traceWrites;

			public TraceTapStream(Stream inner, TraceSource trace, String channel, Boolean traceReads, Boolean traceWrites)
			{
				this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
				this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
				this._channel = channel ?? throw new ArgumentNullException(nameof(channel));
				this._traceReads = traceReads;
				this._traceWrites = traceWrites;
			}

			public override Boolean CanRead => this._inner.CanRead;
			public override Boolean CanSeek => this._inner.CanSeek;
			public override Boolean CanWrite => this._inner.CanWrite;
			public override Int64 Length => this._inner.Length;
			public override Int64 Position { get => this._inner.Position; set => this._inner.Position = value; }

			public override void Flush() => this._inner.Flush();

			public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
			{
				Int32 bytesRead = this._inner.Read(buffer, offset, count);
				if(this._traceReads && bytesRead > 0)
					this.Trace("READ", buffer, offset, bytesRead);

				return bytesRead;
			}

			public override Int64 Seek(Int64 offset, SeekOrigin origin) => this._inner.Seek(offset, origin);
			public override void SetLength(Int64 value) => this._inner.SetLength(value);

			public override void Write(Byte[] buffer, Int32 offset, Int32 count)
			{
				if(this._traceWrites && count > 0)
					this.Trace("WRITE", buffer, offset, count);

				this._inner.Write(buffer, offset, count);
			}

			/*public override async ValueTask<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
			{
				Int32 bytesRead = await this._inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				if(this._traceReads && bytesRead > 0)
				{
					Byte[] copy = buffer.Slice(0, bytesRead).ToArray();
					this.Trace("READ", copy, 0, copy.Length);
				}

				return bytesRead;
			}

			public override async Task WriteAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
			{
				if(this._traceWrites && count > 0)
					this.Trace("WRITE", buffer, offset, count);

				await this._inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
			}

			public override async ValueTask WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
			{
				if(this._traceWrites && !buffer.IsEmpty)
				{
					Byte[] copy = buffer.ToArray();
					this.Trace("WRITE", copy, 0, copy.Length);
				}

				await this._inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
			}*/

			protected override void Dispose(Boolean disposing)
			{
				if(disposing)
					this._inner.Dispose();

				base.Dispose(disposing);
			}

			/*public override async ValueTask DisposeAsync()
			{
				await this._inner.DisposeAsync().ConfigureAwait(false);
				await base.DisposeAsync().ConfigureAwait(false);
			}*/

			private void Trace(String action, Byte[] buffer, Int32 offset, Int32 count)
			{
				String payload = Encoding.UTF8.GetString(buffer, offset, count);
				if(payload.Length > MaxTraceChars)
					payload = payload.Substring(0, MaxTraceChars) + " ...<truncated>";

				payload = payload.Replace("\r", "\\r").Replace("\n", "\\n");
				this._trace.TraceEvent(TraceEventType.Verbose, 0, $"[MCP {this._channel} {action}] {payload}");
			}
		}
	}
}