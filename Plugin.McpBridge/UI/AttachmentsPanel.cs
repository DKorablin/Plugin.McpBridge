using System.Drawing.Imaging;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge.UI;

/// <summary>Horizontal strip that displays pasted image attachments with individual remove buttons.</summary>
internal sealed class AttachmentsPanel : FlowLayoutPanel
{
	private readonly List<(Object Data, Panel AttachPanel)> _attachments = new List<(Object Data, Panel AttachPanel)>();
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
	public void AddAttachment(Object data)
	{
		Panel attachPanel = new Panel()
		{
			Size = new Size(56, 56),
			BorderStyle = BorderStyle.FixedSingle,
			Margin = new Padding(2),
		};

		Control ctrl;

		if(data is Image img)
			ctrl = new PictureBox()
			{
				Image = img,
				SizeMode = PictureBoxSizeMode.Zoom,
				Dock = DockStyle.Fill,
			};
		else if(data is String str && File.Exists(str))
		{
			ctrl = new Label()
			{
				Text = Path.GetFileName(str),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				AutoEllipsis = true,
				Padding = new Padding(4, 0, 0, 0),
				Font = new Font(SystemFonts.DefaultFont.FontFamily, 7.5f),
			};
		} else
			throw new NotImplementedException($"Unknown attachment type {data}");

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
		btnRemove.Click += (Object? s, EventArgs e) => this.RemoveAttachment(attachPanel, data);
		this._toolTip.SetToolTip(btnRemove, "Remove attachment");

		attachPanel.Controls.Add(ctrl);
		attachPanel.Controls.Add(btnRemove);
		btnRemove.BringToFront();
		this.Controls.Add(attachPanel);

		this._attachments.Add((data, attachPanel));
		if(!this.Visible)
			this.Visible = true;
	}

	/// <summary>Removes and disposes a single attachment.</summary>
	private void RemoveAttachment(Panel attachPanel, Object data)
	{
		this._attachments.RemoveAll(a => a.AttachPanel == attachPanel);
		this.Controls.Remove(attachPanel);
		attachPanel.Dispose();
		if(data is IDisposable disp)
			disp.Dispose();

		if(this._attachments.Count == 0)
			this.Visible = false;
	}

	/// <summary>Removes and disposes all attachments.</summary>
	public void ClearAttachments()
	{
		foreach((Object data, Panel panel) in this._attachments)
		{
			panel.Dispose();
			if(data is IDisposable disp)
				disp.Dispose();
		}
		this._attachments.Clear();

		this.Controls.Clear();
		this.Visible = false;
	}

	public IEnumerable<DataContent> GetAttachments()
	{
		foreach(var attachment in this._attachments)
		{
			if(attachment.Data is Image img)
				yield return ImageToDataContent(img);
			else if(attachment.Data is String str && File.Exists(str))
			{
				Byte[] fileBytes = File.ReadAllBytes(str);
				yield return new DataContent(fileBytes, "application/octet-stream")
				{
					Name = Path.GetFileName(str)
				};
			} else
				throw new NotImplementedException($"Unknown attachment type {attachment.Data}");
		}

		this.ClearAttachments();

		DataContent ImageToDataContent(Image image)
		{
			using(MemoryStream ms = new MemoryStream())
			{
				image.Save(ms, ImageFormat.Png);
				return new DataContent(ms.ToArray(), "image/png");
			}
		}
	}
}