using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.McpBridge;

partial class PanelChat
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;
	private SplitContainer splitMain = null!;

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
		this.bnSend = new Button();
		this.txtRequest = new TextBox();
		this.toolStrip1 = new ToolStrip();
		this.splitMain = new SplitContainer();
		((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
		splitMain.Panel1.SuspendLayout();
		splitMain.Panel2.SuspendLayout();
		splitMain.SuspendLayout();
		this.SuspendLayout();
		// 
		// splitMain
		// 
		splitMain.Dock = DockStyle.Fill;
		splitMain.FixedPanel = FixedPanel.Panel2;
		splitMain.Location = new Point(0, 25);
		splitMain.Name = "splitMain";
		splitMain.Orientation = Orientation.Horizontal;
		// 
		// splitMain.Panel1
		// 
		splitMain.Panel1.Controls.Add(this.rtfResponse);
		// 
		// splitMain.Panel2
		// 
		splitMain.Panel2.Controls.Add(this.bnSend);
		splitMain.Panel2.Controls.Add(this.txtRequest);
		splitMain.Size = new Size(175, 148);
		splitMain.SplitterDistance = 70;
		splitMain.TabIndex = 3;
		// 
		// rtfResponse
		// 
		this.rtfResponse.BackColor = SystemColors.Window;
		this.rtfResponse.BorderStyle = BorderStyle.None;
		this.rtfResponse.DetectUrls = true;
		this.rtfResponse.Dock = DockStyle.Fill;
		this.rtfResponse.Location = new Point(0, 0);
		this.rtfResponse.Name = "rtfResponse";
		this.rtfResponse.ReadOnly = true;
		this.rtfResponse.ScrollBars = RichTextBoxScrollBars.Vertical;
		this.rtfResponse.Size = new Size(175, 70);
		this.rtfResponse.TabIndex = 2;
		// 
		// bnSend
		// 
		this.bnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		this.bnSend.Location = new Point(97, 48);
		this.bnSend.Name = "bnSend";
		this.bnSend.Size = new Size(75, 23);
		this.bnSend.TabIndex = 1;
		this.bnSend.Text = "&Send";
		this.bnSend.UseVisualStyleBackColor = true;
		this.bnSend.Click += this.bnSend_Click;
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
		// toolStrip1
		// 
		this.toolStrip1.Location = new Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new Size(175, 25);
		this.toolStrip1.TabIndex = 4;
		this.toolStrip1.Text = "toolStrip1";
		// 
		// PanelChat
		// 
		this.AutoScaleDimensions = new SizeF(7F, 15F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.Controls.Add(splitMain);
		this.Controls.Add(this.toolStrip1);
		this.Margin = new Padding(4, 3, 4, 3);
		this.Name = "PanelChat";
		this.Size = new Size(175, 173);
		splitMain.Panel1.ResumeLayout(false);
		splitMain.Panel1.PerformLayout();
		splitMain.Panel2.ResumeLayout(false);
		splitMain.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
		splitMain.ResumeLayout(false);
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private ToolStrip toolStrip1;
	private Button bnSend;
	private TextBox txtRequest;
	private RichTextBox rtfResponse;
}