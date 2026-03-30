using System.ComponentModel;
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
		this._mcpBridge?.Dispose();
		this._mcpBridge = null;
		this._agent = null;
	}

	private void EnsureConnected()
	{
		if(this._mcpBridge == null || this._agent == null)
		{
			try
			{
				PluginSettingsHelper settingsHelper = new PluginSettingsHelper(this.Plugin.Host);
				this._mcpBridge = new McpBridge(this.Plugin.Trace, this.Plugin.Host, settingsHelper);
				this._agent = new AssistantAgent(this.Plugin.Trace, this._mcpBridge, settingsHelper);

				this._agent.Initialize(this.Plugin.Settings);
				this._mcpBridge.Start();
			} catch(Exception)
			{
				this._mcpBridge = null;
				this._agent = null;
				throw;
			}
		}
	}

	private IEnumerable<String> InvokeMessage(String message)
	{
		if(String.IsNullOrWhiteSpace(message))
			return Enumerable.Empty<String>();

		this.EnsureConnected();

		return this._agent.InvokeMessage(message, this.Plugin.Settings);
	}

	private void bnSend_Click(Object sender, EventArgs e)
	{
		var request = txtRequest.Text;

		txtRequest.Clear();
		txtResponse.AppendText($"> {request}");
		txtResponse.AppendText(Environment.NewLine);

		foreach(var response in this.InvokeMessage(request))
		{
			var formattedResponse = response.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine);
			txtResponse.AppendText($"< {formattedResponse}");
			txtResponse.AppendText(Environment.NewLine);
		}
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter && !e.Shift)
			this.bnSend_Click(sender, e);
	}
}