using System.ComponentModel;
using System.Text.RegularExpressions;
using Plugin.McpBridge.UI;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private Boolean _streamingActive;
	private CancellationTokenSource? _cts;
	private const String Caption = "OpenAI Chat";

	private ConfirmationPanel _confirmationPanel = null!;

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
	{
		this.InitializeComponent();
	}

	protected override void OnCreateControl()
	{
		this.Window.Caption = Caption;
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		this.Window.Closed += this.Window_Closed;
		this._confirmationPanel = new ConfirmationPanel();
		this._confirmationPanel.ConfirmationHandled += (Object? s, EventArgs e) => this.Invoke(() =>
		{
			bnSend.Enabled = true;
			this.Window.Caption = Caption;
		});
		this.splitMain.Panel1.Controls.Add(this._confirmationPanel);
		base.OnCreateControl();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		this._confirmationPanel.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

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

		this._confirmationPanel.Dismiss();
		this._streamingActive = false;
		bnSend.Text = "&Send";
		bnSend.Enabled = true;
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

		this._confirmationPanel.Dismiss();
		this._streamingActive = false;
		this._cts?.Dispose();
		this._cts = new CancellationTokenSource();
		bnSend.Text = "&Cancel";

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
						bnSend.Text = "&Send";
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
			this._confirmationPanel.Request(e);
			bnSend.Enabled = false;
			this.Window.Caption = Caption + " (!)";
		});
	}

	private void bnNewConversation_Click(Object sender, EventArgs e)
		=> this.ResetAgent();

	private void bnSend_Click(Object sender, EventArgs e)
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
			this.bnSend_Click(sender, e);
			e.SuppressKeyPress = true;
		}
	}
}