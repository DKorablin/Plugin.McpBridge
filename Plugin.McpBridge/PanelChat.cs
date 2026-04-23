using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Plugin.McpBridge.Helpers;
using SAL.Windows;

namespace Plugin.McpBridge;

public partial class PanelChat : UserControl
{
	private AssistantAgent? _agent;
	private AgentConfirmationEventArgs? _pendingConfirmation;
	private Boolean _streamingActive;
	private StringBuilder? _streamingBuffer;

	private static readonly Regex _inlineMarkdown = new Regex(
		@"(\*\*[^*\n]+\*\*|\*[^*\n]+\*|`[^`\n]+`)",
		RegexOptions.Compiled);

	private Panel _pnlConfirmation = null!;
	private Label _lblConfirmationText = null!;
	private Button _bnConfirmAllow = null!;
	private Button _bnConfirmDeny = null!;

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
		this.InitializeConfirmationPanel();
		base.OnCreateControl();
	}

	private void Window_Closed(Object? sender, EventArgs e)
	{
		this.HandleConfirmation(false);
		this.Plugin.Settings.PropertyChanged -= this.Settings_PropertyChanged;
	}

	private void InitializeConfirmationPanel()
	{
		this._lblConfirmationText = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
		};
		this._bnConfirmAllow = new Button { Text = "Allow", Dock = DockStyle.Right, Width = 75 };
		this._bnConfirmDeny = new Button { Text = "Deny", Dock = DockStyle.Right, Width = 75 };
		this._bnConfirmAllow.Click += (Object? s, EventArgs e) => this.HandleConfirmation(true);
		this._bnConfirmDeny.Click += (Object? s, EventArgs e) => this.HandleConfirmation(false);

		this._pnlConfirmation = new Panel { Dock = DockStyle.Bottom, Height = 30, Visible = false, Padding = new Padding(2) };
		this._pnlConfirmation.Controls.Add(this._lblConfirmationText);
		this._pnlConfirmation.Controls.Add(this._bnConfirmAllow);
		this._pnlConfirmation.Controls.Add(this._bnConfirmDeny);
		this.Controls.Add(this._pnlConfirmation);
	}

	private void Settings_PropertyChanged(Object? sender, PropertyChangedEventArgs e)
	{
		AgentConfirmationEventArgs? pending = this._pendingConfirmation;
		this._pendingConfirmation = null;
		pending?.Cancel();
		this._pnlConfirmation.Visible = false;
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
			this.Plugin.InitializeAgent(out this._agent);
			this._agent.AiResponseReceived += this.Agent_AiResponseReceived;
			this._agent.ConfirmationRequired += this.Agent_ConfirmationRequired;
		}
	}

	private void InvokeMessage(String message)
	{
		if(String.IsNullOrWhiteSpace(message))
			return;

		this.EnsureConnected();
		this._pendingConfirmation = null;
		this._streamingActive = false;
		this._streamingBuffer = null;
		this._pnlConfirmation.Visible = false;
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
			this._pendingConfirmation = e;
			this._lblConfirmationText.Text = e.ActionDescription;
			this._pnlConfirmation.Visible = true;
			bnSend.Enabled = false;
		});
	}

	private void HandleConfirmation(Boolean allowed)
	{
		AgentConfirmationEventArgs? pending = this._pendingConfirmation;
		if(pending == null)
			return;

		this._pendingConfirmation = null;
		this._pnlConfirmation.Visible = false;
		bnSend.Enabled = true;
		pending.Confirm(allowed);
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