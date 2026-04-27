using System.ComponentModel;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.UI;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private Boolean _streamingActive;
	private CancellationTokenSource? _cts;
	private const String Caption = "OpenAI Chat";
	private readonly List<(Image Image, Panel AttachPanel)> _attachments = new List<(Image Image, Panel AttachPanel)>();

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
		tsbnSend.Image = this._imgSend;
		tsbnSend.Enabled = true;
		this.ClearAttachments();
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
		tsbnSend.Image = this._imgCancel;

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
						tsbnSend.Image = this._imgSend;
						tsbnSend.Enabled = true;
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
			tsbnSend.Enabled = false;

			return;
		}

		String request = txtRequest.Text.Trim();
		if(String.IsNullOrWhiteSpace(request))
			return;

		txtRequest.Clear();
		rtfResponse.AppendMessage(request, RichEditBoxExtension.MessageKind.User);
		this.ClearAttachments();

		this.InvokeMessage(request);
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.V && e.Control && Clipboard.ContainsImage())
		{
			Image? img = Clipboard.GetImage();
			if(img != null)
			{
				this.AddImageAttachment(img);
				e.SuppressKeyPress = true;
				return;
			}
		}

		if(e.KeyCode == Keys.Enter && !e.Shift)
		{
			this.tsbnSend_Click(sender, e);
			e.SuppressKeyPress = true;
		}
	}

	private void AddImageAttachment(Image image)
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
		btnRemove.Click += (Object? s, EventArgs ev) => this.RemoveAttachment(attachPanel, image);
		attachPanel.Controls.Add(pb);
		attachPanel.Controls.Add(btnRemove);
		btnRemove.BringToFront();
		pnlAttachments.Controls.Add(attachPanel);
		this._attachments.Add((image, attachPanel));
		if(this._attachments.Count == 1)
			this.ExpandAttachmentsPanel();
	}

	private void RemoveAttachment(Panel attachPanel, Image image)
	{
		this._attachments.RemoveAll(a => a.AttachPanel == attachPanel);
		pnlAttachments.Controls.Remove(attachPanel);
		attachPanel.Dispose();
		image.Dispose();
		if(this._attachments.Count == 0)
			this.CollapseAttachmentsPanel();
	}

	private void ClearAttachments()
	{
		foreach((Image img, Panel panel) in this._attachments)
		{
			panel.Dispose();
			img.Dispose();
		}
		this._attachments.Clear();
		pnlAttachments.Controls.Clear();
		this.CollapseAttachmentsPanel();
	}

	private void ExpandAttachmentsPanel()
	{
		if(pnlAttachments.Visible)
			return;
		splitMain.SplitterDistance = Math.Max(splitMain.Panel1MinSize, splitMain.SplitterDistance - pnlAttachments.Height);
		pnlAttachments.Visible = true;
	}

	private void CollapseAttachmentsPanel()
	{
		if(!pnlAttachments.Visible)
			return;
		splitMain.SplitterDistance = Math.Min(splitMain.Height - splitMain.Panel2MinSize - splitMain.SplitterWidth, splitMain.SplitterDistance + pnlAttachments.Height);
		pnlAttachments.Visible = false;
	}
}