using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.UI;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private static class Defaults
	{
		public const String AgentStateFileName = "PanelChat_agentState.json";
	}

	private AssistantAgent? _agent;
	private Boolean _streamingActive;
	private CancellationTokenSource? _cts;

	private Plugin Plugin => (Plugin)this.Window.Plugin.Instance;

	private IWindow Window => (IWindow)base.Parent;

	private AiProviderDto? CurrentProvider
	{
		get
		{
			BindingList<AiProviderDto> providers = this.Plugin.Settings.AiProviders;
			if(providers.Count == 0)
				return null;

			return this.Plugin.Settings.SelectedAgent.GetSelectedProvider(providers);
		}
	}

	public PanelChat()
		=> this.InitializeComponent();

	protected override void OnCreateControl()
	{
		this.Window.Closed += this.Window_Closed;
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		base.OnCreateControl();
		this.UpdateUiState();

		Task.Run(async () =>
		{
			String? sessionJson = this.LoadAgentSession();
			if(sessionJson != null)
				this.LoadSessionHistory(sessionJson);
		});
	}

	private void LoadSessionHistory(String sessionJson)
	{
		JsonElement root = JsonSerializer.Deserialize<JsonElement>(sessionJson);
		if(!root.TryGetProperty("stateBag", out JsonElement stateBag) ||
			!stateBag.TryGetProperty("InMemoryChatHistoryProvider", out JsonElement historyState) ||
			!historyState.TryGetProperty("messages", out JsonElement messagesElement))
		{
			this.Plugin.Trace.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "Failed to load session history: Invalid format.");
			return;
		}

		ChatMessage[]? messages = JsonSerializer.Deserialize<ChatMessage[]>(messagesElement, AIJsonUtilities.DefaultOptions);
		if(messages?.Length > 0)
			this.Invoke(() =>
			{
				foreach(ChatMessage msg in messages)
				{
					if(msg.Role == ChatRole.User)
						mdResponse.AppendMessage(msg.Text, MarkdownTextBox.MessageKind.User);
					else if(msg.Role == ChatRole.Assistant)
						mdResponse.AppendMarkdown(msg.Text);
				}
			});
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		pnlConfirmation.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void PnlConfirmation_ConfirmationHandled(Object sender, EventArgs e)
		=> this.Invoke(this.UpdateUiState);

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		=> this.ResetAgent();

	private void ResetAgent()
	{
		mdResponse.Clear();

		if(this._agent != null)
		{
			this._agent.AiResponseReceived -= this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired -= this.Agent_ConfirmationRequired;
		}
		this._agent = null;

		this._cts?.Cancel();
		this._cts?.Dispose();
		this._cts = null;

		pnlConfirmation.Dismiss();
		this._streamingActive = false;
		this.UpdateUiState();
	}

	private async Task<AssistantAgent> GetAgent()
	{
		if(this._agent == null)
		{
			if(this.CurrentProvider == null)
				throw new InvalidOperationException("No AI provider configured.");

			String? sessionJson = this.LoadAgentSession();
			this._agent = await this.Plugin.InitializeAgent(this.CurrentProvider, sessionJson);
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
			this.UpdateUiState();

		}
		return this._agent;
	}

	private async Task InvokeMessage(String message, DataContent[] attachments)
	{
		pnlConfirmation.Dismiss();
		this._streamingActive = false;

		this._cts?.Dispose();
		this._cts = new CancellationTokenSource();
		CancellationToken token = this._cts.Token;

		this.UpdateUiState();

		try
		{
			AssistantAgent agent = await this.GetAgent();
			await agent.InvokeMessageAsync(message, attachments, token);
		} catch(Exception ex)
		{
			this.Invoke(() => mdResponse.AppendMessage(ex.Message, MarkdownTextBox.MessageKind.Error));
		} finally
		{
			this._cts?.Dispose();
			this._cts = null;

			this.UpdateUiState();
		}
	}

	private void SaveAgentSession(String? sessionJson)
	{
		if(String.IsNullOrWhiteSpace(sessionJson))
			this.Plugin.Host.Plugins.Settings(this.Plugin).RemoveAssemblyBlob(Defaults.AgentStateFileName);
		else
			using(MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sessionJson)))
				this.Plugin.Host.Plugins.Settings(this.Plugin).SaveAssemblyBlob(Defaults.AgentStateFileName, ms);
	}

	private String? LoadAgentSession()
	{
		using(Stream stream = this.Plugin.Host.Plugins.Settings(this.Plugin).LoadAssemblyBlob(Defaults.AgentStateFileName))
			return stream == null
				? null
				: new StreamReader(stream).ReadToEnd();
	}

	private void Agent_AiResponseReceived(Object? sender, AgentResponseEventArgs e)
	{
		this.Invoke(() =>
		{
			if(!this._streamingActive)
				this._streamingActive = true;

			mdResponse.AppendMarkdown(e.Response);
			mdResponse.ScrollToCaret();

			if(e.IsFinal)
			{
				this._streamingActive = false;
				this._cts?.Dispose();
				this._cts = null;
				this.UpdateUiState();
				AssistantAgent? agent = this._agent;
				if(agent != null)
					_ = Task.Run(async () =>
					{
						String? json = await agent.GetSessionState();
						if(json != null)
							this.SaveAgentSession(json);
					});
			}
		});
	}

	private void Agent_ConfirmationRequired(Object? sender, AgentConfirmationEventArgs e)
	{
		this.BeginInvoke(() =>
		{
			pnlConfirmation.Request(e);
			this.UpdateUiState();
		});
	}

	private void bnNewConversation_Click(Object sender, EventArgs e)
	{
		this.SaveAgentSession(null);
		this.ResetAgent();
	}

	private void tsbnSend_DropDownOpening(Object sender, EventArgs e)
	{
		tsbnSend.DropDownItems.Clear();

		Settings settings = this.Plugin.Settings;
		Guid selectedAgentId = settings.SelectedAgent.Id;

		ToolStripMenuItem agentsHeader = new ToolStripMenuItem("Agents") { Enabled = false };
		tsbnSend.DropDownItems.Add(agentsHeader);
		for(Int32 i = 0; i < settings.AiAgents.Count; i++)
		{
			AiAgentDto agentDto = settings.AiAgents[i];
			ToolStripMenuItem item = new ToolStripMenuItem($"Agent {i + 1}")
			{
				Tag = agentDto.Id,
				Checked = agentDto.Id == selectedAgentId,
			};
			item.Click += this.tsbnSend_AgentItem_Click;
			tsbnSend.DropDownItems.Add(item);
		}

		tsbnSend.DropDownItems.Add(new ToolStripSeparator());

		ToolStripMenuItem providersHeader = new ToolStripMenuItem("Providers") { Enabled = false };
		tsbnSend.DropDownItems.Add(providersHeader);
		Guid? selectedProviderId = settings.SelectedAgent.SelectedProviderId
			?? (settings.AiProviders.Count > 0 ? settings.AiProviders[0].Id : (Guid?)null);
		foreach(AiProviderDto provider in settings.AiProviders)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(provider.ToString())
			{
				Tag = provider.Id,
				Checked = provider.Id == selectedProviderId,
			};
			item.Click += this.tsbnSend_ProviderItem_Click;
			tsbnSend.DropDownItems.Add(item);
		}
	}

	private void tsbnSend_AgentItem_Click(Object? sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender!;
		this.Plugin.Settings.SelectedAgentId = (Guid)item.Tag;
	}

	private void tsbnSend_ProviderItem_Click(Object? sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender!;
		this.Plugin.Settings.SelectedAgent.SelectedProviderId = (Guid)item.Tag;
	}

	private void tsbnSend_Click(Object sender, EventArgs e)
	{
		if(this._cts != null)
		{
			if(!this._cts.IsCancellationRequested)
				this._cts.Cancel();
			tsbnSend.Enabled = false;

			return;
		}

		String request = txtRequest.Text.Trim();
		if(String.IsNullOrWhiteSpace(request))
			return;

		txtRequest.Clear();
		mdResponse.AppendMessage(request, MarkdownTextBox.MessageKind.User);
		DataContent[] attachments = pnlAttachments.GetAttachments().ToArray();
		Task.Run(async () => await this.InvokeMessage(request, attachments));
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.V && e.Control && Clipboard.ContainsImage())
		{
			Image? img = Clipboard.GetImage();
			if(img != null)
			{
				pnlAttachments.AddAttachment(img);
				e.SuppressKeyPress = true;
				return;
			}
		}

		if(e.KeyCode == Keys.V && e.Control && Clipboard.ContainsFileDropList())
		{
			System.Collections.Specialized.StringCollection? files = Clipboard.GetFileDropList();
			if(files != null)
				foreach(String? path in files)
					if(path != null)
						pnlAttachments.AddAttachment(path);
			e.SuppressKeyPress = true;
			return;
		}

		if(e.KeyCode == Keys.Enter && !e.Shift)
		{
			this.tsbnSend_Click(sender, e);
			e.SuppressKeyPress = true;
		}
	}

	private void PnlAttachments_VisibleChanged(Object? sender, EventArgs e)
		=> splitMain.SplitterDistance = pnlAttachments.Visible
			? Math.Max(splitMain.Panel1MinSize, splitMain.SplitterDistance - pnlAttachments.Height)
			: Math.Min(splitMain.Height - splitMain.Panel2MinSize - splitMain.SplitterWidth, splitMain.SplitterDistance + pnlAttachments.Height);

	private void UpdateUiState()
	{
		if(this.InvokeRequired)
		{
			this.Invoke(this.UpdateUiState);
			return;
		}

		Boolean isProcessing = _cts != null;
		Boolean needsConfirmation = pnlConfirmation.Visible; // Assuming a property exists
		Boolean hasProvider = this.CurrentProvider != null;

		// tsbnSend Logic
		tsbnSend.Enabled = !needsConfirmation && hasProvider;
		tsbnSend.Text = isProcessing ? "&Cancel" : "&Send";
		tsbnSend.Image = isProcessing ? _imgCancel : _imgSend;

		// Window Caption Logic
		Int32 agentIndex = this.Plugin.Settings.AiAgents.IndexOf(this.Plugin.Settings.SelectedAgent);
		String agentInfo = this.Plugin.Settings.AiAgents.Count > 1 ? $"Agent {agentIndex + 1} | " : String.Empty;
		String providerInfo = this.CurrentProvider?.ToString() ?? "Undefined";
		String statusIcon = needsConfirmation ? " (!)" : String.Empty;
		this.Window.Caption = agentInfo + providerInfo + statusIcon;

		// Input Logic
		if(!isProcessing && !needsConfirmation && hasProvider)
			txtRequest.Focus();
	}
}