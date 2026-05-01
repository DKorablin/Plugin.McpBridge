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

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	private AiProviderDto? CurrentProvider => this.Plugin.Settings.GetSelectedProvider();

	public PanelChat()
		=> this.InitializeComponent();

	protected override void OnCreateControl()
	{
		this.Window.Closed += this.Window_Closed;
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		base.OnCreateControl();
		this.UpdateUiState();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		pnlConfirmation.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void PnlConfirmation_ConfirmationHandled(Object sender, EventArgs e)
		=> this.Invoke(this.UpdateUiState);

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
		this.UpdateUiState();
	}

	private AssistantAgent GetAgent()
	{
		if(this._agent == null)
		{
			if(this.CurrentProvider == null)
				throw new InvalidOperationException("No AI provider configured.");

			this._agent = this.Plugin.InitializeAgent(this.CurrentProvider);
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
			this.UpdateUiState();

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

		this.UpdateUiState();

		CancellationToken token = this._cts.Token;
		AssistantAgent agent = this.GetAgent();

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
						this._cts?.Dispose();
						this._cts = null;

						this.UpdateUiState();
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
				this.UpdateUiState();
			}
		});
	}

	private void Agent_ConfirmationRequired(Object? sender, AgentConfirmationEventArgs e)
	{
		this.BeginInvoke(() =>
		{
			pnlConfirmation.Request(e);
			this.UpdateUiState();
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
			using(MemoryStream ms = new MemoryStream())
			{
				images[i].Save(ms, ImageFormat.Png);
				result[i] = new DataContent(ms.ToArray(), "image/png");
			}
		}
		return result;
	}

	private void UpdateUiState()
	{
		Boolean isProcessing = _cts != null;
		Boolean needsConfirmation = pnlConfirmation.Visible; // Assuming a property exists
		Boolean hasProvider = this.CurrentProvider != null;

		// tsbnSend Logic
		tsbnSend.Enabled = !needsConfirmation && hasProvider;
		tsbnSend.Text = isProcessing ? "&Cancel" : "&Send";
		tsbnSend.Image = isProcessing ? _imgCancel : _imgSend;

		// Window Caption Logic
		String providerInfo = this.CurrentProvider?.ToString() ?? "Undefinded";
		String statusIcon = needsConfirmation ? " (!)" : String.Empty;
		this.Window.Caption = providerInfo + statusIcon;

		// Input Logic
		if(!isProcessing && !needsConfirmation && hasProvider)
			txtRequest.Focus();
	}
}