using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Plugin.McpBridge.Helpers;
using Plugin.McpBridge.UI;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private Boolean _streamingActive;
	private StringBuilder? _streamingBuffer;

	private static readonly Regex _inlineMarkdown = new Regex(
		@"(\*\*[^*\n]+\*\*|\*[^*\n]+\*|`[^`\n]+`)",
		RegexOptions.Compiled);

	private ConfirmationPanel _confirmationPanel = null!;

	private Plugin Plugin => (Plugin)this.Window.Plugin;

	private IWindow Window => (IWindow)base.Parent;

	public PanelChat()
	{
		this.InitializeComponent();
	}

	protected override void OnCreateControl()
	{
		this.Window.Caption = "OpenAI Chat";
		this.Plugin.Settings.PropertyChanged += this.Settings_PropertyChanged;
		this.Window.Closed += this.Window_Closed;
		this._confirmationPanel = new ConfirmationPanel();
		this._confirmationPanel.ConfirmationHandled += (Object? s, EventArgs e) => this.Invoke(() => bnSend.Enabled = true);
		this.splitMain.Panel1.Controls.Add(this._confirmationPanel);
		base.OnCreateControl();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		this._confirmationPanel.Dismiss();
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
	{
		this._confirmationPanel.Dismiss();
		bnSend.Enabled = true;

		if(this._agent != null)
		{
			this._agent.AiResponseReceived -= this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired -= this.Agent_ConfirmationRequired;
		}
		this._agent = null;
		this._streamingActive = false;
		this._streamingBuffer = null;
	}

	private void EnsureConnected()
	{
		if(this._agent == null)
		{
			this._agent = this.Plugin.InitializeAgent();
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
		}
	}

	private void InvokeMessage(String message)
	{
		if(String.IsNullOrWhiteSpace(message))
			return;

		this.EnsureConnected();
		this._confirmationPanel.Dismiss();
		this._streamingActive = false;
		this._streamingBuffer = null;
		bnSend.Enabled = false;

		AssistantAgent agent = this._agent!;
		Task.Run(async () =>
		{
			try
			{
				await agent.InvokeMessageAsync(message, CancellationToken.None);
			} catch(Exception ex)
			{
				this.Invoke(() => this.AppendMessage(ex.Message, MessageKind.Error));
			} finally
			{
				this.Invoke(() => bnSend.Enabled = true);
			}
		});
	}

	private void Agent_AiResponseReceived(Object? sender, AgentResponseEventArgs e)
	{
		this.Invoke(() =>
		{
			if(!this._streamingActive)
			{
				this._streamingBuffer = new StringBuilder();
				this._streamingActive = true;
			}
			this._streamingBuffer!.Append(e.Response);
			if(e.IsFinal)
			{
				this.AppendMarkdown(this._streamingBuffer.ToString());
				rtfResponse.ScrollToCaret();
				this._streamingActive = false;
				this._streamingBuffer = null;
			}
		});
	}

	private void Agent_ConfirmationRequired(Object? sender, AgentConfirmationEventArgs e)
	{
		this.BeginInvoke(() =>
		{
			this._confirmationPanel.Request(e);
			bnSend.Enabled = false;
		});
	}

	private void bnSend_Click(Object sender, EventArgs e)
	{
		String request = txtRequest.Text.Trim();

		txtRequest.Clear();
		this.AppendMessage(request, MessageKind.User);

		this.InvokeMessage(request);
	}

	private void txtRequest_KeyDown(Object sender, KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Enter && !e.Shift)
			this.bnSend_Click(sender, e);
	}

	private enum MessageKind { User, Error }

	private void AppendMessage(String text, MessageKind kind)
	{
		Color color = kind == MessageKind.User ? Color.FromArgb(0, 102, 204) : Color.FromArgb(185, 43, 39);
		FontStyle style = kind == MessageKind.User ? FontStyle.Bold : FontStyle.Italic;
		String prefix = kind == MessageKind.User ? "You: " : "Error: ";

		rtfResponse.SelectionStart = rtfResponse.TextLength;
		rtfResponse.SelectionLength = 0;
		rtfResponse.SelectionColor = color;
		rtfResponse.SelectionFont = new Font(rtfResponse.Font, style);
		rtfResponse.AppendText(prefix);

		rtfResponse.SelectionStart = rtfResponse.TextLength;
		rtfResponse.SelectionLength = 0;
		rtfResponse.SelectionColor = Color.FromArgb(33, 37, 41);
		rtfResponse.SelectionFont = rtfResponse.Font;
		rtfResponse.AppendText(text + Environment.NewLine);
		rtfResponse.ScrollToCaret();
	}

	private void AppendMarkdown(String markdown)
	{
		Boolean inCodeBlock = false;
		Font baseFont = rtfResponse.Font;
		Color defaultColor = Color.FromArgb(33, 37, 41);
		Color codeColor = Color.FromArgb(180, 60, 120);
		using Font boldFont = new Font(baseFont, FontStyle.Bold);
		using Font italicFont = new Font(baseFont, FontStyle.Italic);
		using Font codeFont = new Font("Consolas", baseFont.Size);
		using Font h1Font = new Font(baseFont.FontFamily, baseFont.Size + 4, FontStyle.Bold);
		using Font h2Font = new Font(baseFont.FontFamily, baseFont.Size + 2, FontStyle.Bold);
		using Font h3Font = new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold);

		foreach(String rawLine in markdown.Replace("\r\n", "\n").Split('\n'))
		{
			String line = rawLine.TrimEnd('\r');

			if(line.StartsWith("```"))
			{
				inCodeBlock = !inCodeBlock;
				continue;
			}

			if(inCodeBlock)
			{
				this.AppendRun(line + Environment.NewLine, codeFont, codeColor);
				continue;
			}

			if(line.StartsWith("### "))
				this.AppendRun(line.Substring(4) + Environment.NewLine, h3Font, defaultColor);
			else if(line.StartsWith("## "))
				this.AppendRun(line.Substring(3) + Environment.NewLine, h2Font, defaultColor);
			else if(line.StartsWith("# "))
				this.AppendRun(line.Substring(2) + Environment.NewLine, h1Font, defaultColor);
			else
			{
				Boolean isList = line.Length >= 2 && (line[0] == '-' || line[0] == '+') && line[1] == ' ';
				if(isList)
					this.AppendRun("• ", boldFont, defaultColor);
				this.AppendInline((isList ? line.Substring(2) : line) + Environment.NewLine, baseFont, boldFont, italicFont, codeFont, defaultColor, codeColor);
			}
		}
	}

	private void AppendInline(String text, Font baseFont, Font boldFont, Font italicFont, Font codeFont, Color defaultColor, Color codeColor)
	{
		foreach(String part in _inlineMarkdown.Split(text))
		{
			if(part.Length >= 4 && part.StartsWith("**") && part.EndsWith("**"))
				this.AppendRun(part.Substring(2, part.Length - 4), boldFont, defaultColor);
			else if(part.Length >= 2 && part.StartsWith("*") && part.EndsWith("*"))
				this.AppendRun(part.Substring(1, part.Length - 2), italicFont, defaultColor);
			else if(part.Length >= 2 && part.StartsWith("`") && part.EndsWith("`"))
				this.AppendRun(part.Substring(1, part.Length - 2), codeFont, codeColor);
			else
				this.AppendRun(part, baseFont, defaultColor);
		}
	}

	private void AppendRun(String text, Font font, Color color)
	{
		rtfResponse.SelectionStart = rtfResponse.TextLength;
		rtfResponse.SelectionLength = 0;
		rtfResponse.SelectionFont = font;
		rtfResponse.SelectionColor = color;
		rtfResponse.AppendText(text);
	}
}