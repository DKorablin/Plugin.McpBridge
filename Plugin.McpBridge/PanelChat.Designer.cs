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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelChat));
		this.rtfResponse = new RichTextBox();
		this.tsBottom = new ToolStrip();
		this.tsbnSend = new ToolStripButton();
		this.txtRequest = new TextBox();
		this.tsTop = new ToolStrip();
		this.bnNewConversation = new ToolStripButton();
		this.splitMain = new SplitContainer();
		this.pnlConfirmation = new ConfirmationPanel();
		this.tsBottom.SuspendLayout();
		this.tsTop.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitMain).BeginInit();
		this.splitMain.Panel1.SuspendLayout();
		this.splitMain.Panel2.SuspendLayout();
		this.splitMain.SuspendLayout();
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
		this.tsbnSend.Image = (Image)resources.GetObject("tsbnSend.Image");
		this.tsbnSend.ImageTransparentColor = Color.Magenta;
		this.tsbnSend.Name = "tsbnSend";
		this.tsbnSend.Size = new Size(23, 22);
		this.tsbnSend.Text = "&Send";
		this.tsbnSend.ToolTipText = "Send message to LLM";
		this.tsbnSend.Click += this.tsbnSend_Click;
		// 
		// txtRequest
		// 
		this.txtRequest.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		this.txtRequest.Location = new Point(0, 3);
		this.txtRequest.Multiline = true;
		this.txtRequest.Name = "txtRequest";
		this.txtRequest.Size = new Size(172, 45);
		this.txtRequest.TabIndex = 0;
		this.txtRequest.KeyDown += this.txtRequest_KeyDown;
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
		this.splitMain.Panel2.Controls.Add(this.tsBottom);
		this.splitMain.Panel2.Controls.Add(this.txtRequest);
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
}