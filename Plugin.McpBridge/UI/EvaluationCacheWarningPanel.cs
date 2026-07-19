namespace Plugin.McpBridge.UI;

/// <summary>Warning bar that indicates Evaluation Cache is active and sessions are disabled.</summary>
internal sealed class EvaluationCacheWarningPanel : Panel
{
	public EvaluationCacheWarningPanel()
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
		Label lblText = new Label
		{
			Text = "Evaluation Cache is enabled — sessions will not work.",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			ForeColor = Color.FromArgb(133, 79, 0),
			Font = new Font(this.Font, FontStyle.Bold),
		};

		this.Controls.Add(lblText);
		this.Controls.Add(lblIcon);
	}
}
