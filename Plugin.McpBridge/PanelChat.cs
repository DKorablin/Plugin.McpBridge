using System.ComponentModel;
using Plugin.McpBridge.Helpers;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private McpBridge? _mcpBridge;
	private AssistantAgent? _agent;
	private AgentConfirmationEventArgs? _pendingConfirmation;

	private Panel _pnlConfirmation = null!;
	private Label _lblConfirmationText = null!;
	private Button _bnConfirmAllow = null!;
	private Button _bnConfirmDeny = null!;

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
	{
		this.InitializeComponent();
	}

	protected override void OnCreateControl()
	{
		this.Window.Caption = "OpenAI Chat";
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		this.Window.Closed += this.Window_Closed;
		this.InitializeConfirmationPanel();
		base.OnCreateControl();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		this.HandleConfirmation(false);
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void InitializeConfirmationPanel()
	{
		this._lblConfirmationText = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
		};
		this._bnConfirmAllow = new Button { Text = "Allow", Dock = DockStyle.Right, Width = 75 };
		this._bnConfirmDeny = new Button { Text = "Deny", Dock = DockStyle.Right, Width = 75 };
		this._bnConfirmAllow.Click += (Object? s, EventArgs e) => this.HandleConfirmation(true);
		this._bnConfirmDeny.Click += (Object? s, EventArgs e) => this.HandleConfirmation(false);

		this._pnlConfirmation = new Panel { Dock = DockStyle.Bottom, Height = 30, Visible = false, Padding = new Padding(2) };
		this._pnlConfirmation.Controls.Add(this._lblConfirmationText);
		this._pnlConfirmation.Controls.Add(this._bnConfirmAllow);
		this._pnlConfirmation.Controls.Add(this._bnConfirmDeny);
		this.Controls.Add(this._pnlConfirmation);
	}

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
	{
		AgentConfirmationEventArgs? pending = this._pendingConfirmation;
		this._pendingConfirmation = null;
		pending?.Cancel();
		this._pnlConfirmation.Visible = false;
		bnSend.Enabled = true;

		if(this._agent != null)
		{
			this._agent.AiResponseReceived -= this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired -= this.Agent_ConfirmationRequired;
		}
		this._mcpBridge?.Dispose();
		this._mcpBridge = null;
		this._agent = null;
	}

	private void EnsureConnected()
	{
		if(this._mcpBridge == null || this._agent == null)
		{
			this.Plugin.InitializeMcpBridge(out this._mcpBridge, out this._agent);
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
		}
	}

	private void InvokeMessage(String message)
	{
		if(String.IsNullOrWhiteSpace(message))
			return;

		this.EnsureConnected();
		this.HandleConfirmation(false);
		bnSend.Enabled = false;

		AssistantAgent agent = this._agent!;
		Task.Run(async () =>
		{
			try
			{
				await agent.InvokeMessageAsync(message, this.Plugin.Settings, CancellationToken.None);
			} catch(Exception ex)
			{
				this.Invoke(() =>
				{
					txtResponse.AppendText($"< Error: {ex.Message}");
					txtResponse.AppendText(Environment.NewLine);
				});
			} finally
			{
				this.Invoke(() => bnSend.Enabled = true);
			}
		});
	}

	private void Agent_AiResponseReceived(Object? sender, AgentResponseEventArgs e)
	{
		String formattedResponse = e.Response.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine);
		this.Invoke(() =>
		{
			txtResponse.AppendText($"> {formattedResponse}");
			txtResponse.AppendText(Environment.NewLine);
		});
	}

	private void Agent_ConfirmationRequired(Object? sender, AgentConfirmationEventArgs e)
	{
		this.BeginInvoke(() =>
		{
			this._pendingConfirmation = e;
			this._lblConfirmationText.Text = e.ActionDescription;
			this._pnlConfirmation.Visible = true;
			bnSend.Enabled = false;
		});
	}

	private void HandleConfirmation(Boolean allowed)
	{
		AgentConfirmationEventArgs? pending = this._pendingConfirmation;
		if(pending == null)
			return;

		this._pendingConfirmation = null;
		this._pnlConfirmation.Visible = false;
		bnSend.Enabled = true;
		pending?.Confirm(allowed);
	}

	private void bnSend_Click(Object sender, EventArgs e)
	{
		String request = txtRequest.Text.Trim();

		txtRequest.Clear();
		txtResponse.AppendText($"< {request}");
		txtResponse.AppendText(Environment.NewLine);

		this.InvokeMessage(request);
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter && !e.Shift)
			this.bnSend_Click(sender, e);
	}
}