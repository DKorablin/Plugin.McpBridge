using System.ComponentModel;
using System.Drawing.Imaging;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Data;
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

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
		=> this.InitializeComponent();

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
		mdResponse.Clear();

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
		pnlAttachments.ClearAttachments();
	}

	private AssistantAgent GetAgent()
	{
		if(this._agent == null)
		{
			AiProviderDto provider = this.Plugin.Settings.GetSelectedProvider()
				?? throw new InvalidOperationException("No AI provider configured.");
			this._agent = this.Plugin.InitializeAgent(provider);
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
		}
		return this._agent;
	}

	private void InvokeMessage(String message, DataContent[] images)
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
		Application.DoEvents();

		Task.Run(async () =>
		{
			try
			{
				await agent.InvokeMessageAsync(message, images, token);
			} catch(Exception ex)
			{
				this.Invoke(() => mdResponse.AppendMessage(ex.Message, MarkdownTextBox.MessageKind.Error));
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

			mdResponse.AppendMarkdown(e.Response);
			mdResponse.ScrollToCaret();

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
		mdResponse.AppendMessage(request, MarkdownTextBox.MessageKind.User);
		Image[] rawImages = pnlAttachments.TakeAttachments();
		DataContent[] images = PanelChat.ImagesToDataContent(rawImages);
		foreach(Image img in rawImages)
			img.Dispose();

		this.InvokeMessage(request, images);
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.V && e.Control && Clipboard.ContainsImage())
		{
			Image? img = Clipboard.GetImage();
			if(img != null)
			{
				pnlAttachments.AddImageAttachment(img);
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

	private void PnlAttachments_VisibleChanged(Object? sender, EventArgs e)
		=> splitMain.SplitterDistance = pnlAttachments.Visible
			? Math.Max(splitMain.Panel1MinSize, splitMain.SplitterDistance - pnlAttachments.Height)
			: Math.Min(splitMain.Height - splitMain.Panel2MinSize - splitMain.SplitterWidth, splitMain.SplitterDistance + pnlAttachments.Height);

	private static DataContent[] ImagesToDataContent(Image[] images)
	{
		DataContent[] result = new DataContent[images.Length];
		for(Int32 i = 0; i < images.Length; i++)
		{
			using MemoryStream ms = new MemoryStream();
			images[i].Save(ms, ImageFormat.Png);
			result[i] = new DataContent(ms.ToArray(), "image/png");
		}
		return result;
	}
}