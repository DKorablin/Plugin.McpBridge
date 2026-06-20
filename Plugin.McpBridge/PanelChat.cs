using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Agents;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;
using Plugin.McpBridge.UI;
using Plugin.McpBridge.Workflows;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private AIAgent? _workflowAgent;
	private AgentSession? _workflowSession;
	private WorkflowFactory? _workflowFactory;
	private String? _selectedWorkflowName;
	private Boolean _streamingActive;
	private CancellationTokenSource? _cts;
	private String _conversationId = String.Empty;
	private Boolean _updatingSessionList;

	private sealed class SessionListItem
	{
		public String ConversationId { get; }

		public DateTime? LastWriteTimeUtc { get; }

		public SessionListItem(String conversationId, DateTime? lastWriteTimeUtc)
		{
			this.ConversationId = conversationId;
			this.LastWriteTimeUtc = lastWriteTimeUtc;
		}

		public override String ToString()
			=> this.LastWriteTimeUtc.HasValue
				? this.ConversationId + " (" + this.LastWriteTimeUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + ")"
				: this.ConversationId + " (new)";
	}

	private Plugin Plugin => (Plugin)this.Window.Plugin.Instance;

	private IWindow Window => (IWindow)base.Parent!;

	private WorkflowFactory WorkflowFactory => this._workflowFactory ??= new WorkflowFactory(this.Plugin.Host, this.Plugin.Settings, this.Plugin.Trace);

	private Boolean IsWorkflowSelected => this._selectedWorkflowName != null;

	private WorkflowFactoryItem? SelectedWorkflow
	{
		get
		{
			String? selectedWorkflowName = this._selectedWorkflowName;
			if(selectedWorkflowName == null)
				return null;

			return this.WorkflowFactory.GetWorkflow(selectedWorkflowName);
		}
	}

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

		this._conversationId = this.Plugin.Settings.LastConversationId ?? Guid.NewGuid().ToString();
		this.ReloadWorkflows();
		this.UpdateSessionComboWidth();
		this.RefreshSessionList();
		this.LoadConversationHistory(this._conversationId);
		this.UpdateUiState();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		this.UpdateSessionComboWidth();
	}

	private void UpdateSessionComboWidth()
	{
		if(!this.IsHandleCreated)
			return;

		Int32 availableWidth = this.bnRemoveSession.Bounds.Left - this.cbSessions.Bounds.Left - this.cbSessions.Margin.Right;
		this.cbSessions.Width = Math.Max(1, availableWidth);
	}

	private void LoadConversationHistory(String conversationId)
	{
		String? sessionStorageDir = this.Plugin.Settings.SessionStorageDirectory;
		if(sessionStorageDir == null)
			return;
		if(this._conversationId != conversationId)
			return;

		Task.Run(async () =>
		{
			var store = new FileSystemAgentSessionStore(sessionStorageDir);
			await foreach(ChatMessage message in store.ReadSessionAsync(this.GetSessionScopeName(), conversationId))
				mdResponse.AppendMessage(message);
		});
	}

	private void RefreshSessionList()
	{
		if(this.InvokeRequired)
		{
			this.Invoke(this.RefreshSessionList);
			return;
		}

		this._updatingSessionList = true;
		try
		{
			this.cbSessions.Items.Clear();

			List<SessionListItem> sessions = new List<SessionListItem>();
			String? sessionStorageDir = this.Plugin.Settings.SessionStorageDirectory;
			if(sessionStorageDir != null && Directory.Exists(sessionStorageDir))
			{
				var store = new FileSystemAgentSessionStore(sessionStorageDir);
				String agentName = this.GetSessionScopeName();
				String agentDirectoryPath = Path.Combine(sessionStorageDir, agentName);
				foreach(String sessionId in store.ListSessions(agentName))
				{
					String filePath = Path.Combine(agentDirectoryPath, sessionId + ".json");
					DateTime? lastWriteTimeUtc = File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : null;
					sessions.Add(new SessionListItem(sessionId, lastWriteTimeUtc));
				}
			}

			if(!sessions.Any(s => s.ConversationId == this._conversationId))
				sessions.Add(new SessionListItem(this._conversationId, null));

			this.cbSessions.Items.AddRange(sessions.ToArray());

			SessionListItem? selected = sessions.FirstOrDefault(s => s.ConversationId == this._conversationId);
			if(selected != null)
				this.cbSessions.SelectedItem = selected;

			this.cbSessions.Enabled = this.cbSessions.Items.Count > 0;
			this.bnRemoveSession.Enabled = this.cbSessions.Enabled && this.cbSessions.SelectedItem != null;
		} finally
		{
			this._updatingSessionList = false;
		}
	}

	private void SetConversation(String conversationId, Boolean loadHistory)
	{
		this._conversationId = conversationId;
		this.Plugin.Settings.LastConversationId = conversationId;
		this.ResetAgent();
		if(loadHistory)
			this.LoadConversationHistory(conversationId);
		this.RefreshSessionList();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		pnlConfirmation.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
		this.DisposeWorkflows();
	}

	private void PnlConfirmation_ConfirmationHandled(Object sender, EventArgs e)
		=> this.Invoke(this.UpdateUiState);

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName == nameof(this.Plugin.Settings.LastConversationId))
			return;

		this.ResetAgent();
		this.ReloadWorkflows();
		this.LoadConversationHistory(this._conversationId);
		this.RefreshSessionList();
		this.UpdateUiState();
	}

	private void ResetAgent()
	{
		mdResponse.Clear();

		if(this._agent != null)
		{
			this._agent.AiResponseReceived -= this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired -= this.Agent_ConfirmationRequired;
		}
		this._agent = null;
		this._workflowAgent = null;
		this._workflowSession = null;

		this._cts?.Cancel();
		this._cts?.Dispose();
		this._cts = null;

		pnlConfirmation.Dismiss();
		this._streamingActive = false;
		this.UpdateUiState();
	}

	private String GetSessionScopeName()
	{
		if(this._selectedWorkflowName != null)
			return this._selectedWorkflowName;

		return this._agent?.AgentName ?? FileSystemAgentSessionStore.DefaultAgentName;
	}

	private void DisposeWorkflows()
	{
		this._workflowFactory?.Dispose();
		this._workflowFactory = null;
	}

	private void ReloadWorkflows()
	{
		this.WorkflowFactory.Reload();
		if(this._selectedWorkflowName != null
			&& this.WorkflowFactory.GetWorkflow(this._selectedWorkflowName) == null)
			this._selectedWorkflowName = null;
	}

	private async Task<AssistantAgent> GetAgent()
	{
		if(this._agent == null)
		{
			if(this.CurrentProvider == null)
				throw new InvalidOperationException("No AI provider configured.");

			this._agent = await this.Plugin.InitializeAgent(this.CurrentProvider, this._conversationId.ToString());
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
			if(this.IsWorkflowSelected)
				try {
					await this.InvokeWorkflowMessage(message, attachments, token);
				}catch(Exception exc)
				{//TODO: Centralize this catch inside AssistantAgent class
					this.Plugin.Trace.TraceData(TraceEventType.Error, 0, exc);
					throw;
				}
			else
			{
				AssistantAgent agent = await this.GetAgent();
				await agent.InvokeMessageAsync(message, attachments, token);
			}
		} catch(Exception ex)
		{
			mdResponse.AppendMessage(ex.Message, MarkdownTextBox.MessageKind.Error);
		} finally
		{
			this._cts?.Dispose();
			this._cts = null;

			this.UpdateUiState();
		}
	}

	private async Task InvokeWorkflowMessage(String message, DataContent[] attachments, CancellationToken token)
	{
		WorkflowFactoryItem workflow = this.SelectedWorkflow
			?? throw new InvalidOperationException("Selected workflow was not found.");
		WorkflowHandle workflowHandle = await this.WorkflowFactory.GetHandleAsync(workflow, token);

		if(this._workflowAgent == null || this._workflowAgent.Name != workflow.Name)
		{
			this._workflowAgent = workflowHandle.Workflow.AsAIAgent(name: workflow.Name);
			this._workflowSession = null;
		}

		if(this._workflowSession == null)
		{
			String? storageDir = this.Plugin.Settings.SessionStorageDirectory;
			if(storageDir != null)
			{
				var store = new FileSystemAgentSessionStore(storageDir);
				this._workflowSession = await store.GetSessionAsync(this._workflowAgent, this._conversationId, token);
			} else
				this._workflowSession = await this._workflowAgent.CreateSessionAsync(token);
		}

		ChatMessage input = PanelChat.BuildUserMessage(message, attachments);
		AgentResponse response = await this._workflowAgent.RunAsync(input, this._workflowSession, null, token);
		while(response.FinishReason == ChatFinishReason.ToolCalls)
		{
			ToolApprovalRequestContent request = (ToolApprovalRequestContent)response.Messages[^1].Contents[0];
			if(request.ToolCall is not FunctionCallContent functionCall)
				break;

			Boolean approved = await this.RequestWorkflowApproval(functionCall);
			ToolApprovalResponseContent approval = request.CreateResponse(approved);
			ChatMessage approvalMessage = new ChatMessage(ChatRole.User, [approval]);
			response = await this._workflowAgent.RunAsync(approvalMessage, this._workflowSession, null, token);
		}

		String? sessionStorageDirectory = this.Plugin.Settings.SessionStorageDirectory;
		if(sessionStorageDirectory != null && this._workflowSession != null)
		{
			var store = new FileSystemAgentSessionStore(sessionStorageDirectory);
			await store.SaveSessionAsync(this._workflowAgent, this._conversationId, this._workflowSession, token);
		}

		this.Invoke(() =>
		{
			mdResponse.AppendMarkdown(response.Text);
			mdResponse.ScrollToCaret();
		});
	}

	private Task<Boolean> RequestWorkflowApproval(FunctionCallContent call)
	{
		AgentConfirmationEventArgs request = new AgentConfirmationEventArgs(call);
		this.BeginInvoke(() =>
		{
			pnlConfirmation.Request(request);
			this.UpdateUiState();
		});

		return request.ConfirmationTask;
	}

	private static ChatMessage BuildUserMessage(String text, DataContent[]? attachments)
	{
		if(attachments == null || attachments.Length == 0)
			return new ChatMessage(ChatRole.User, text);

		List<AIContent> contents = new List<AIContent> { new TextContent(text) };
		foreach(DataContent attachment in attachments)
			contents.Add(attachment);
		return new ChatMessage(ChatRole.User, contents);
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
		=> this.SetConversation(Guid.NewGuid().ToString(), loadHistory: false);

	private void cbSessions_SelectedIndexChanged(Object? sender, EventArgs e)
	{
		this.bnRemoveSession.Enabled = this.cbSessions.SelectedItem != null;

		if(this._updatingSessionList)
			return;

		SessionListItem? selected = this.cbSessions.SelectedItem as SessionListItem;
		if(selected == null || selected.ConversationId == this._conversationId)
			return;

		this.SetConversation(selected.ConversationId, loadHistory: true);
	}

	private void bnRemoveSession_Click(Object? sender, EventArgs e)
	{
		SessionListItem? selected = this.cbSessions.SelectedItem as SessionListItem;
		if(selected == null)
			return;

		if(MessageBox.Show("Are you sure you want to delete this session?", this.Window.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			return;

		SessionListItem[] currentItems = this.cbSessions.Items.Cast<SessionListItem>().ToArray();
		Int32 selectedIndex = this.cbSessions.SelectedIndex;
		SessionListItem? nextItem = selectedIndex switch
		{
			>= 0 when selectedIndex < currentItems.Length - 1 => currentItems[selectedIndex + 1],
			> 0 => currentItems[selectedIndex - 1],
			_ => null
		};

		String? sessionStorageDir = this.Plugin.Settings.SessionStorageDirectory;
		if(sessionStorageDir != null)
		{
			var store = new FileSystemAgentSessionStore(sessionStorageDir);
			String agentName = this.GetSessionScopeName();
			store.DeleteSession(agentName, selected.ConversationId);
		}

		if(nextItem != null)
		{
			this.SetConversation(nextItem.ConversationId, loadHistory: true);
			return;
		}

		this.SetConversation(Guid.NewGuid().ToString(), loadHistory: false);
	}

	private void tsbnSend_DropDownOpening(Object sender, EventArgs e)
	{
		tsbnSend.DropDownItems.Clear();

		Settings settings = this.Plugin.Settings;
		Guid selectedAgentId = settings.SelectedAgent.Id;
		Boolean workflowSelected = this.IsWorkflowSelected;

		ToolStripMenuItem agentsHeader = new ToolStripMenuItem("Agents") { Enabled = false };
		tsbnSend.DropDownItems.Add(agentsHeader);
		for(Int32 i = 0; i < settings.AiAgents.Count; i++)
		{
			AiAgentDto agentDto = settings.AiAgents[i];
			ToolStripMenuItem item = new ToolStripMenuItem(agentDto.Description ?? $"Agent {i + 1}")
			{
				Tag = agentDto.Id,
				Checked = !workflowSelected && agentDto.Id == selectedAgentId,
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
				Checked = !workflowSelected && provider.Id == selectedProviderId,
				Enabled = !workflowSelected,
			};
			item.Click += this.tsbnSend_ProviderItem_Click;
			tsbnSend.DropDownItems.Add(item);
		}

		tsbnSend.DropDownItems.Add(new ToolStripSeparator());

		ToolStripMenuItem workflowsHeader = new ToolStripMenuItem("Workflows") { Enabled = false };
		tsbnSend.DropDownItems.Add(workflowsHeader);
		IReadOnlyList<WorkflowFactoryItem> workflows = this.WorkflowFactory.GetWorkflows();

		if(workflows.Count == 0)
			tsbnSend.DropDownItems.Add(new ToolStripMenuItem("(none)") { Enabled = false });
		else
			foreach(WorkflowFactoryItem workflow in workflows)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(workflow.Name)
				{
					Tag = workflow.Name,
					Checked = workflowSelected && String.Equals(this._selectedWorkflowName, workflow.Name, StringComparison.Ordinal),
				};
				item.Click += this.tsbnSend_WorkflowItem_Click;
				tsbnSend.DropDownItems.Add(item);
			}
	}

	private void tsbnSend_AgentItem_Click(Object? sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender!;
		this._selectedWorkflowName = null;
		this.Plugin.Settings.SelectedAgentId = (Guid)item.Tag!;
		this.ResetAgent();
		this.LoadConversationHistory(this._conversationId);
		this.RefreshSessionList();
		this.UpdateUiState();
	}

	private void tsbnSend_WorkflowItem_Click(Object? sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender!;
		this._selectedWorkflowName = (String)item.Tag!;
		this.ResetAgent();
		this.LoadConversationHistory(this._conversationId);
		this.RefreshSessionList();
		this.UpdateUiState();
	}

	private void tsbnSend_ProviderItem_Click(Object? sender, EventArgs e)
	{
		ToolStripMenuItem item = (ToolStripMenuItem)sender!;
		this.Plugin.Settings.SelectedAgent.SelectedProviderId = (Guid)item.Tag!;
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
		Boolean hasTarget = this.IsWorkflowSelected
			? this.SelectedWorkflow != null
			: this.CurrentProvider != null;

		// tsbnSend Logic
		tsbnSend.Enabled = !needsConfirmation && hasTarget;
		tsbnSend.Text = isProcessing ? "&Cancel" : "&Send";
		tsbnSend.Image = isProcessing ? _imgCancel : _imgSend;

		// Window Caption Logic
		String agentInfo;
		String providerInfo;
		if(this.IsWorkflowSelected)
		{
			WorkflowFactoryItem? workflow = this.SelectedWorkflow;
			agentInfo = workflow != null ? workflow.Name + " | " : String.Empty;
			providerInfo = workflow != null ? "Workflow" : "Undefined";
		} else
		{
			Int32 agentIndex = this.Plugin.Settings.AiAgents.IndexOf(this.Plugin.Settings.SelectedAgent);
			if(this.Plugin.Settings.SelectedAgent.Description != null)
				agentInfo = $"{this.Plugin.Settings.SelectedAgent.Description} | ";
			else if(this.Plugin.Settings.AiAgents.Count > 1)
				agentInfo = $"Agent {agentIndex + 1} | ";
			else
				agentInfo = String.Empty;

			providerInfo = this.CurrentProvider?.ToString() ?? "Undefined";
		}
		String statusIcon = needsConfirmation ? " (!)" : String.Empty;
		this.Window.Caption = agentInfo + providerInfo + statusIcon;

		// Input Logic
		if(!isProcessing && !needsConfirmation && hasTarget)
			txtRequest.Focus();

		this.bnRemoveSession.Enabled = this.cbSessions.SelectedItem != null && !isProcessing;
	}
}