using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
	{
		this.InitializeComponent();
	}

	protected override void OnCreateControl()
	{
		this.Window.Caption = "OpenAI Chat";
		base.OnCreateControl();
	}

	private void bnSend_Click(Object sender, EventArgs e)
	{
		var request = txtRequest.Text;
		if(String.IsNullOrWhiteSpace(request))
			return;

		txtRequest.Clear();
		txtResponse.AppendText($"> {request}");
		txtResponse.AppendText(Environment.NewLine);
		foreach(var response in this.Plugin.InvokeMessage(request))
		{
			txtResponse.AppendText($"< {response}");
			txtResponse.AppendText(Environment.NewLine);
		}
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter && !e.Shift)
			this.bnSend_Click(sender, e);
	}
}