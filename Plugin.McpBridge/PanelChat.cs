using System.ComponentModel;
using Plugin.McpBridge.Helpers;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private McpBridge? _mcpBridge;
	private AssistantAgent? _agent;

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
		base.OnCreateControl();
	}

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
	{
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
		Boolean allowed = false;
		this.Invoke(() =>
		{
			DialogResult result = MessageBox.Show(
				$"The AI assistant wants to perform the following action:\r\n\r\n{e.ActionDescription}\r\n\r\nAllow this action?",
				"Confirm Action",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			allowed = result == DialogResult.Yes;
		});
		e.Confirm(allowed);
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