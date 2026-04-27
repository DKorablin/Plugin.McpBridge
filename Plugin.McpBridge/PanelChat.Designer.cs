using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.McpBridge.UI;

namespace Plugin.McpBridge;

partial class PanelChat
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if(disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.rtfResponse = new RichTextBox();
		this.tsBottom = new ToolStrip();
		this.tsbnSend = new ToolStripButton();
		this.txtRequest = new TextBox();
		this.tsTop = new ToolStrip();
		this.bnNewConversation = new ToolStripButton();
		this.splitMain = new SplitContainer();
		this.pnlConfirmation = new ConfirmationPanel();
		this.pnlInput = new Panel();
		this.pnlAttachments = new AttachmentsPanel();
		this.tsBottom.SuspendLayout();
		this.tsTop.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitMain).BeginInit();
		this.splitMain.Panel1.SuspendLayout();
		this.splitMain.Panel2.SuspendLayout();
		this.splitMain.SuspendLayout();
		this.pnlInput.SuspendLayout();
		this.SuspendLayout();
		// 
		// rtfResponse
		// 
		this.rtfResponse.BackColor = SystemColors.Window;
		this.rtfResponse.BorderStyle = BorderStyle.None;
		this.rtfResponse.Dock = DockStyle.Fill;
		this.rtfResponse.Location = new Point(0, 0);
		this.rtfResponse.Name = "rtfResponse";
		this.rtfResponse.ReadOnly = true;
		this.rtfResponse.ScrollBars = RichTextBoxScrollBars.Vertical;
		this.rtfResponse.Size = new Size(175, 70);
		this.rtfResponse.TabIndex = 2;
		this.rtfResponse.Text = "";
		// 
		// tsBottom
		// 
		this.tsBottom.Dock = DockStyle.Bottom;
		this.tsBottom.GripStyle = ToolStripGripStyle.Hidden;
		this.tsBottom.Items.AddRange(new ToolStripItem[] { this.tsbnSend });
		this.tsBottom.Location = new Point(0, 49);
		this.tsBottom.Name = "tsBottom";
		this.tsBottom.Size = new Size(175, 25);
		this.tsBottom.TabIndex = 2;
		// 
		// tsbnSend
		// 
		this.tsbnSend.Alignment = ToolStripItemAlignment.Right;
		this.tsbnSend.DisplayStyle = ToolStripItemDisplayStyle.Image;
		this._imgSend = PanelChat.ImageFromBase64("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACRSURBVDhPY2AYHKD8ugJD/X0OdGHiQfn1Boby6+8ZKq63gw0jGYAMqLj+HwkvZyi/7oCujDBANeQ/Q9nV6wzl1xNI9x66QWR7D90gsr2HaQjMe0S4CF1jxfXn4EAvuSKBrhQBMDWB8HFwgOIFmJr+M1Rcnc9Qds0CXSkqwEwHRDgTGSAMIMKZ2AAoWgg6k0YAAHcZl5HLIgRYAAAAAElFTkSuQmCC");
		this._imgCancel = PanelChat.ImageFromBase64("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFDSURBVDhPY2AY1OCojqTGYW1phyNaUgbocnjBES3ZhCNa0u+PaMv8R2Dp74e1pCvQ1WKAI9rS21E1YuDj++XlOdD1gcERTakMmMLLCaH/jxkpwzWi8dvR9YIByJkgBVfTY/6DwJfrV/6fsjP8/+7wfjD/0dQ+uIH7teQlUDVrSRnAJE/ZG/7/8eQxWNOfL5/B9O9PH/+f9XGAG3BIUyYA1QAk54PwWR+7/3+/fwdr/vfnz/9LsUEoYXFYW7oBxQBwdEElQX6FOfv/379w75yw0EIYoiWbgGIAKGRhkrAwADn7amYs3DvIYQBKIygGgMBhbZnpIElQGDxdNPv/+WB3sOLTbhZgPiIMpLej6wUDiCuknyP7FQNrSb/HiAFksF9fXuCotsx8DI1Qm/FqRgbgaNWUygCF9mFt6YLDGtIW6GqoAgDbISUtJwLyUAAAAABJRU5ErkJggg==");
		this.tsbnSend.Image = this._imgSend;
		this.tsbnSend.ImageTransparentColor = Color.Magenta;
		this.tsbnSend.Name = "tsbnSend";
		this.tsbnSend.Size = new Size(23, 22);
		this.tsbnSend.Text = "&Send";
		this.tsbnSend.ToolTipText = "Send message to LLM";
		this.tsbnSend.Click += this.tsbnSend_Click;
		// 
		// txtRequest
		// 
		this.txtRequest.Dock = DockStyle.Fill;
		this.txtRequest.Multiline = true;
		this.txtRequest.Name = "txtRequest";
		this.txtRequest.TabIndex = 0;
		this.txtRequest.KeyDown += this.txtRequest_KeyDown;
		// 
		// pnlAttachments
		// 
		this.pnlAttachments.Name = "pnlAttachments";
		this.pnlAttachments.VisibleChanged += this.PnlAttachments_VisibleChanged;
		// 
		// pnlInput
		// 
		this.pnlInput.Controls.Add(this.txtRequest);
		this.pnlInput.Controls.Add(this.pnlAttachments);
		this.pnlInput.Dock = DockStyle.Fill;
		this.pnlInput.Name = "pnlInput";
		// 
		// tsTop
		// 
		this.tsTop.GripStyle = ToolStripGripStyle.Hidden;
		this.tsTop.Items.AddRange(new ToolStripItem[] { this.bnNewConversation });
		this.tsTop.Location = new Point(0, 0);
		this.tsTop.Name = "tsTop";
		this.tsTop.Size = new Size(175, 25);
		this.tsTop.TabIndex = 4;
		// 
		// bnNewConversation
		// 
		this.bnNewConversation.DisplayStyle = ToolStripItemDisplayStyle.Text;
		this.bnNewConversation.Name = "bnNewConversation";
		this.bnNewConversation.Size = new Size(35, 22);
		this.bnNewConversation.Text = "New";
		this.bnNewConversation.ToolTipText = "Start a new conversation";
		this.bnNewConversation.Click += this.bnNewConversation_Click;
		// 
		// pnlConfirmation
		// 
		this.pnlConfirmation.ConfirmationHandled += this.PnlConfirmation_ConfirmationHandled;
		// 
		// splitMain
		// 
		this.splitMain.Dock = DockStyle.Fill;
		this.splitMain.FixedPanel = FixedPanel.Panel2;
		this.splitMain.Location = new Point(0, 25);
		this.splitMain.Name = "splitMain";
		this.splitMain.Orientation = Orientation.Horizontal;
		// 
		// splitMain.Panel1
		// 
		this.splitMain.Panel1.Controls.Add(this.rtfResponse);
		this.splitMain.Panel1.Controls.Add(this.pnlConfirmation);
		// 
		// splitMain.Panel2
		// 
		this.splitMain.Panel2.Controls.Add(this.pnlInput);
		this.splitMain.Panel2.Controls.Add(this.tsBottom);
		this.splitMain.Size = new Size(175, 148);
		this.splitMain.SplitterDistance = 70;
		this.splitMain.TabIndex = 3;
		// 
		// PanelChat
		// 
		this.AutoScaleDimensions = new SizeF(7F, 15F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.Controls.Add(this.splitMain);
		this.Controls.Add(this.tsTop);
		this.Margin = new Padding(4, 3, 4, 3);
		this.Name = "PanelChat";
		this.Size = new Size(175, 173);
		this.pnlInput.ResumeLayout(false);
		this.pnlInput.PerformLayout();
		this.tsBottom.ResumeLayout(false);
		this.tsBottom.PerformLayout();
		this.tsTop.ResumeLayout(false);
		this.tsTop.PerformLayout();
		this.splitMain.Panel1.ResumeLayout(false);
		this.splitMain.Panel2.ResumeLayout(false);
		this.splitMain.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitMain).EndInit();
		this.splitMain.ResumeLayout(false);
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private SplitContainer splitMain;
	private ToolStrip tsTop;
	private ToolStripButton bnNewConversation;
	private TextBox txtRequest;
	private RichTextBox rtfResponse;
	private ToolStrip tsBottom;
	private ToolStripButton tsbnSend;
	private ConfirmationPanel pnlConfirmation;
	private Panel pnlInput;
	private AttachmentsPanel pnlAttachments;
	private Image _imgSend;
	private Image _imgCancel;

	private static Image ImageFromBase64(String b64)
		=> new Bitmap(new System.IO.MemoryStream(Convert.FromBase64String(b64)));
}