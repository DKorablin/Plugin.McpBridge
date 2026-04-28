using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAL.Windows;

namespace Plugin.McpBridge.Tools
{
	internal sealed class WindowsTools
	{
		private readonly IHostWindows _host;

		public WindowsTools(IHostWindows host)
			=> this._host = host ?? throw new ArgumentNullException(nameof(host));

		[Tool]
		[Description("Get a list of all open windows and their captions.")]
		public Task<String> WindowsGet()
		{
			StringBuilder result = new StringBuilder("List of opened windows (Caption):");

			foreach(var window in this._host.Windows)
			{
				result.AppendLine($"- {window.Caption}");
			}

			return Task.FromResult(result.ToString());
		}

		[Tool]
		[Description("Close an open window by its caption.")]
		public Task WindowClose(
			[Description("Window caption")] String caption)
		{
			var window = this._host.Windows.FirstOrDefault(w => caption.Equals(w.Caption, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException($"No open window found with caption '{caption}'.", nameof(caption));

			window.Close();
			return Task.FromResult(0);
		}
	}
}