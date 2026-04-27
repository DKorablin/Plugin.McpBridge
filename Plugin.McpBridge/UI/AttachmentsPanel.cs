namespace Plugin.McpBridge.UI;

/// <summary>Horizontal strip that displays pasted image attachments with individual remove buttons.</summary>
internal sealed class AttachmentsPanel : FlowLayoutPanel
{
	private readonly List<(Image Image, Panel AttachPanel)> _attachments = new List<(Image Image, Panel AttachPanel)>();
	private readonly ToolTip _toolTip = new ToolTip();

	public AttachmentsPanel()
	{
		this.AutoScroll = true;
		this.Dock = DockStyle.Bottom;
		this.FlowDirection = FlowDirection.LeftToRight;
		this.Height = 62;
		this.Visible = false;
		this.WrapContents = false;
	}

	/// <summary>Adds an image as a thumbnail attachment with an inline remove button.</summary>
	public void AddImageAttachment(Image image)
	{
		Panel attachPanel = new Panel()
		{
			Size = new Size(56, 56),
			BorderStyle = BorderStyle.FixedSingle,
			Margin = new Padding(2),
		};
		PictureBox pb = new PictureBox()
		{
			Image = image,
			SizeMode = PictureBoxSizeMode.Zoom,
			Dock = DockStyle.Fill,
		};
		Button btnRemove = new Button()
		{
			Text = "✕",
			Size = new Size(17, 17),
			Location = new Point(39, 0),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(180, 60, 60),
			ForeColor = Color.White,
			Padding = new Padding(0),
			Font = new Font(SystemFonts.DefaultFont.FontFamily, 6f),
			TabStop = false,
		};
		btnRemove.Click += (Object? s, EventArgs e) => this.RemoveAttachment(attachPanel, image);
		this._toolTip.SetToolTip(btnRemove, "Remove attachment");

		attachPanel.Controls.Add(pb);
		attachPanel.Controls.Add(btnRemove);
		btnRemove.BringToFront();
		this.Controls.Add(attachPanel);

		this._attachments.Add((image, attachPanel));
		if(this._attachments.Count == 1)
			this.Visible = true;
	}

	/// <summary>Removes and disposes a single attachment.</summary>
	private void RemoveAttachment(Panel attachPanel, Image image)
	{
		this._attachments.RemoveAll(a => a.AttachPanel == attachPanel);
		this.Controls.Remove(attachPanel);
		attachPanel.Dispose();
		image.Dispose();
		if(this._attachments.Count == 0)
			this.Visible = false;
	}

	/// <summary>Returns a snapshot array of all currently attached images.</summary>
	public IEnumerable<Image> GetAttachments()
		=> this._attachments.Select(a => a.Image);

	/// <summary>Removes and disposes all attachments.</summary>
	public void ClearAttachments()
	{
		foreach((Image img, Panel panel) in this._attachments)
		{
			panel.Dispose();
			img.Dispose();
		}
		this._attachments.Clear();
		this.Controls.Clear();
		this.Visible = false;
	}
}
