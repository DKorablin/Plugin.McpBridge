using System.ComponentModel;
using Plugin.McpBridge.UI;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private Boolean _streamingActive;
	private CancellationTokenSource? _cts;
	private const String Caption = "OpenAI Chat";

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
	{
		this.InitializeComponent();
	}

	protected override void OnCreateControl()
	{
		this.Window.Caption = Caption;
		this.Window.Closed += this.Window_Closed;
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		base.OnCreateControl();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		pnlConfirmation.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void PnlConfirmation_ConfirmationHandled(Object sender, EventArgs e)
		=> this.Invoke(() =>
		{
			tsbnSend.Enabled = true;
			this.Window.Caption = Caption;
		});

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
		=> this.ResetAgent();

	private void ResetAgent()
	{
		rtfResponse.Clear();

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
		tsbnSend.Text = "&Send";
		tsbnSend.Enabled = true;
	}

	private AssistantAgent GetAgent()
	{
		if(this._agent == null)
		{
			this._agent = this.Plugin.InitializeAgent();
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
		}
		return this._agent;
	}

	private void InvokeMessage(String message)
	{
		if(String.IsNullOrWhiteSpace(message))
			return;

		pnlConfirmation.Dismiss();
		this._streamingActive = false;
		this._cts?.Dispose();
		this._cts = new CancellationTokenSource();
		tsbnSend.Text = "&Cancel";

		CancellationToken token = this._cts.Token;
		AssistantAgent agent = this.GetAgent();
		Task.Run(async () =>
		{
			try
			{
				await agent.InvokeMessageAsync(message, token);
			} catch(Exception ex)
			{
				this.Invoke(() => rtfResponse.AppendMessage(ex.Message, RichEditBoxExtension.MessageKind.Error));
			} finally
			{
				this.Invoke(() =>
					{
						tsbnSend.Text = "&Send";
						this._cts?.Dispose();
						this._cts = null;
					});
			}
		});
	}

	private void Agent_AiResponseReceived(Object? sender, AgentResponseEventArgs e)
	{
		this.Invoke(() =>
		{
			if(!this._streamingActive)
				this._streamingActive = true;

			rtfResponse.AppendMarkdown(e.Response);
			rtfResponse.ScrollToCaret();

			if(e.IsFinal)
			{
				this._streamingActive = false;
				this._cts?.Dispose();
				this._cts = null;
			}
		});
	}

	private void Agent_ConfirmationRequired(Object? sender, AgentConfirmationEventArgs e)
	{
		this.BeginInvoke(() =>
		{
			pnlConfirmation.Request(e);
			tsbnSend.Enabled = false;
			this.Window.Caption = Caption + " (!)";
		});
	}

	private void bnNewConversation_Click(Object sender, EventArgs e)
		=> this.ResetAgent();

	private void tsbnSend_Click(Object sender, EventArgs e)
	{
		if(this._cts != null)
		{
			if(!this._cts.IsCancellationRequested)
				this._cts.Cancel();

			return;
		}

		String request = txtRequest.Text.Trim();
		if(String.IsNullOrWhiteSpace(request))
			return;

		txtRequest.Clear();
		rtfResponse.AppendMessage(request, RichEditBoxExtension.MessageKind.User);

		this.InvokeMessage(request);
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter && !e.Shift)
		{
			this.tsbnSend_Click(sender, e);
			e.SuppressKeyPress = true;
		}
	}
}