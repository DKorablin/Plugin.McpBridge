using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Plugin.McpBridge.UI;

internal class MarkdownTextBox : RichTextBox
{
	private static readonly Regex _inlineMarkdown = new Regex(@"(\*\*[^*\n]+\*\*|\*[^*\n]+\*|`[^`\n]+`|\[[^\]\n]+\]\([^\)\n]+\))", RegexOptions.Compiled);
	private static readonly Regex _inlineImage = new Regex(@"data:image/[a-zA-Z]+;base64,([A-Za-z0-9+/=]+)", RegexOptions.Compiled);
	private static readonly Regex _markdownLink = new Regex(@"^\[([^\]\n]+)\]\(([^\)\n]+)\)$", RegexOptions.Compiled);

	private readonly List<(Int32 Start, Int32 Length, String Url)> _links = new List<(Int32, Int32, String)>();
	private readonly ToolTip _toolTip = new ToolTip();
	private String? _lastTooltipUrl = null;

	public enum MessageKind { User, Error }

	public void AppendMessage(String text, MessageKind kind)
	{
		Color color = kind == MessageKind.User ? Color.FromArgb(0, 102, 204) : Color.FromArgb(185, 43, 39);
		FontStyle style = kind == MessageKind.User ? FontStyle.Bold : FontStyle.Italic;
		String prefix = kind == MessageKind.User ? "You: " : "Error: ";

		this.SelectionStart = this.TextLength;
		this.SelectionLength = 0;
		this.SelectionColor = color;
		this.SelectionFont = new Font(this.Font, style);
		this.AppendText(prefix);

		this.SelectionStart = this.TextLength;
		this.SelectionLength = 0;
		this.SelectionColor = Color.FromArgb(33, 37, 41);
		this.SelectionFont = this.Font;
		this.AppendText(text + Environment.NewLine);
		this.ScrollToCaret();
	}

	public void AppendMarkdown(String markdown)
	{
		Boolean inCodeBlock = false;
		Font baseFont = this.Font;
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

			Match imageMatch = _inlineImage.Match(line);
			if(imageMatch.Success)
			{
				this.AppendImage(imageMatch.Groups[1].Value);
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
			{
				Match linkMatch = _markdownLink.Match(part);
				if(linkMatch.Success)
					this.AppendLink(linkMatch.Groups[1].Value, linkMatch.Groups[2].Value, baseFont);
				else
					this.AppendRun(part, baseFont, defaultColor);
			}
		}
	}

	public void AppendImage(String base64)
	{
		Byte[] bytes = Convert.FromBase64String(base64);
		using MemoryStream ms = new MemoryStream(bytes);
		Image img = Image.FromStream(ms);

		this.SelectionStart = this.TextLength;
		this.SelectionLength = 0;
		Clipboard.SetImage(img);
		this.ReadOnly = false;
		this.Paste();
		this.ReadOnly = true;
		this.AppendText(Environment.NewLine);
	}

	private void AppendLink(String label, String url, Font baseFont)
	{
		Int32 start = this.TextLength;
		using Font linkFont = new Font(baseFont, FontStyle.Underline);
		this.SelectionStart = start;
		this.SelectionLength = 0;
		this.SelectionFont = linkFont;
		this.SelectionColor = Color.FromArgb(0, 102, 204);
		this.AppendText(label);
		_links.Add((start, label.Length, url));
	}

	private void AppendRun(String text, Font font, Color color)
	{
		this.SelectionStart = this.TextLength;
		this.SelectionLength = 0;
		this.SelectionFont = font;
		this.SelectionColor = color;
		this.AppendText(text);
	}

	protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if(e.Button != MouseButtons.Left || this.SelectionLength > 0)
			return;
		Int32 charIndex = this.GetCharIndexFromPosition(e.Location);
		foreach((Int32 Start, Int32 Length, String Url) link in _links)
		{
			if(charIndex >= link.Start && charIndex < link.Start + link.Length)
			{
				Process.Start(new ProcessStartInfo(link.Url) { UseShellExecute = true });
				break;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		Int32 charIndex = this.GetCharIndexFromPosition(e.Location);
		String? hoveredUrl = null;
		foreach((Int32 Start, Int32 Length, String Url) link in _links)
		{
			if(charIndex >= link.Start && charIndex < link.Start + link.Length)
			{
				hoveredUrl = link.Url;
				break;
			}
		}
		this.Cursor = hoveredUrl != null ? Cursors.Hand : Cursors.IBeam;
		if(hoveredUrl != this._lastTooltipUrl)
		{
			this._lastTooltipUrl = hoveredUrl;
			this._toolTip.Hide(this);
			if(hoveredUrl != null)
				this._toolTip.Show(hoveredUrl, this, e.Location.X + 16, e.Location.Y + 16);
		}
	}
}