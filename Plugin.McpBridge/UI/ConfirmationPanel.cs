using System.Drawing;
using System.Windows.Forms;
using Plugin.McpBridge.Events;

namespace Plugin.McpBridge.UI;

/// <summary>Warning bar that presents an AI action for user confirmation.</summary>
internal sealed class ConfirmationPanel : Panel
{
	private readonly Label _lblText;
	private AgentConfirmationEventArgs? _pending;

	/// <summary>Raised after the user allows or denies, or after <see cref="Dismiss"/> is called.</summary>
	public event EventHandler? ConfirmationHandled;

	public ConfirmationPanel()
	{
		this.Dock = DockStyle.Bottom;
		this.Height = 30;
		this.Visible = false;
		this.Padding = new Padding(2);
		this.BackColor = Color.FromArgb(255, 243, 187);

		Label lblIcon = new Label
		{
			Text = "⚠",
			Dock = DockStyle.Left,
			Width = 24,
			TextAlign = ContentAlignment.MiddleCenter,
			ForeColor = Color.FromArgb(133, 79, 0),
			Font = new Font(this.Font, FontStyle.Bold),
		};
		this._lblText = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			ForeColor = Color.FromArgb(133, 79, 0),
			Font = new Font(this.Font, FontStyle.Bold),
		};
		Button bnAllow = new Button { Text = "Allow", Dock = DockStyle.Right, Width = 75, UseVisualStyleBackColor = true };
		Button bnDeny  = new Button { Text = "Deny",  Dock = DockStyle.Right, Width = 75, UseVisualStyleBackColor = true };
		bnAllow.Click += (Object? s, EventArgs e) => this.Handle(true);
		bnDeny.Click  += (Object? s, EventArgs e) => this.Handle(false);

		this.Controls.Add(this._lblText);
		this.Controls.Add(lblIcon);
		this.Controls.Add(bnAllow);
		this.Controls.Add(bnDeny);
	}

	/// <summary>Shows the bar and stores the pending confirmation.</summary>
	public void Request(AgentConfirmationEventArgs confirmation)
	{
		this._pending = confirmation;
		this._lblText.Text = confirmation.ActionDescription;
		this.Visible = true;
	}

	/// <summary>Cancels any pending confirmation and hides the bar without user interaction.</summary>
	public void Dismiss()
	{
		AgentConfirmationEventArgs? pending = this._pending;
		if(pending == null)
			return;

		this._pending = null;
		this.Visible = false;
		pending.Cancel();
		this.ConfirmationHandled?.Invoke(this, EventArgs.Empty);
	}

	private void Handle(Boolean allowed)
	{
		AgentConfirmationEventArgs? pending = this._pending;
		if(pending == null)
			return;

		this._pending = null;
		this.Visible = false;
		pending.Confirm(allowed);
		this.ConfirmationHandled?.Invoke(this, EventArgs.Empty);
	}
}
