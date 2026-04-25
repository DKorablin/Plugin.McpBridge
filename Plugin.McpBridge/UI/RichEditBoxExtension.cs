using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin.McpBridge.UI;

internal static class RichEditBoxExtension
{
	private static readonly Regex _inlineMarkdown = new Regex(
		@"(\*\*[^*\n]+\*\*|\*[^*\n]+\*|`[^`\n]+`)",
		RegexOptions.Compiled);

	public enum MessageKind { User, Error }

	internal static void AppendMessage(this RichTextBox rtf, String text, MessageKind kind)
	{
		Color color = kind == MessageKind.User ? Color.FromArgb(0, 102, 204) : Color.FromArgb(185, 43, 39);
		FontStyle style = kind == MessageKind.User ? FontStyle.Bold : FontStyle.Italic;
		String prefix = kind == MessageKind.User ? "You: " : "Error: ";

		rtf.SelectionStart = rtf.TextLength;
		rtf.SelectionLength = 0;
		rtf.SelectionColor = color;
		rtf.SelectionFont = new Font(rtf.Font, style);
		rtf.AppendText(prefix);

		rtf.SelectionStart = rtf.TextLength;
		rtf.SelectionLength = 0;
		rtf.SelectionColor = Color.FromArgb(33, 37, 41);
		rtf.SelectionFont = rtf.Font;
		rtf.AppendText(text + Environment.NewLine);
		rtf.ScrollToCaret();
	}

	internal static void AppendMarkdown(this RichTextBox rtf, String markdown)
	{
		Boolean inCodeBlock = false;
		Font baseFont = rtf.Font;
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
				rtf.AppendRun(line + Environment.NewLine, codeFont, codeColor);
				continue;
			}

			if(line.StartsWith("### "))
				rtf.AppendRun(line.Substring(4) + Environment.NewLine, h3Font, defaultColor);
			else if(line.StartsWith("## "))
				rtf.AppendRun(line.Substring(3) + Environment.NewLine, h2Font, defaultColor);
			else if(line.StartsWith("# "))
				rtf.AppendRun(line.Substring(2) + Environment.NewLine, h1Font, defaultColor);
			else
			{
				Boolean isList = line.Length >= 2 && (line[0] == '-' || line[0] == '+') && line[1] == ' ';
				if(isList)
					rtf.AppendRun("• ", boldFont, defaultColor);
				rtf.AppendInline((isList ? line.Substring(2) : line) + Environment.NewLine, baseFont, boldFont, italicFont, codeFont, defaultColor, codeColor);
			}
		}
	}

	private static void AppendInline(this RichTextBox rtf, String text, Font baseFont, Font boldFont, Font italicFont, Font codeFont, Color defaultColor, Color codeColor)
	{
		foreach(String part in _inlineMarkdown.Split(text))
		{
			if(part.Length >= 4 && part.StartsWith("**") && part.EndsWith("**"))
				rtf.AppendRun(part.Substring(2, part.Length - 4), boldFont, defaultColor);
			else if(part.Length >= 2 && part.StartsWith("*") && part.EndsWith("*"))
				rtf.AppendRun(part.Substring(1, part.Length - 2), italicFont, defaultColor);
			else if(part.Length >= 2 && part.StartsWith("`") && part.EndsWith("`"))
				rtf.AppendRun(part.Substring(1, part.Length - 2), codeFont, codeColor);
			else
				rtf.AppendRun(part, baseFont, defaultColor);
		}
	}

	private static void AppendRun(this RichTextBox rtf, String text, Font font, Color color)
	{
		rtf.SelectionStart = rtf.TextLength;
		rtf.SelectionLength = 0;
		rtf.SelectionFont = font;
		rtf.SelectionColor = color;
		rtf.AppendText(text);
	}
}