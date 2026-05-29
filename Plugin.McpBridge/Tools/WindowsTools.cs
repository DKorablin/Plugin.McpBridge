using System.ComponentModel;
using System.Text;
using SAL.Windows;

namespace Plugin.McpBridge.Tools
{
	internal sealed class WindowsTools : ToolsDiscoveryBase
	{
		private readonly IHostWindows _host;

		public WindowsTools(IHostWindows host)
			=> this._host = host ?? throw new ArgumentNullException(nameof(host));

		[Tool]
		[Description("Get a list of all open windows and their captions.")]
		public Task<String> WindowsGet()
		{
			StringBuilder result = new StringBuilder("List of opened windows (PluginId - Caption):");

			foreach(var window in this._host.Windows)
			{
				result.AppendLine($"{window.Plugin.ID} - {window.Caption}");
			}

			return Task.FromResult(result.ToString());
		}

		[Tool]
		[Description("Close an opened window.")]
		public Task WindowClose(
			[Description("Plugin identifier")] String pluginId,
			[Description("Window caption")] String caption)
		{
			var window = this._host.Windows.FirstOrDefault(w => caption.Equals(w.Caption, StringComparison.OrdinalIgnoreCase))
				?? throw new ArgumentException($"No opened window found with pluginId='{pluginId}' and caption='{caption}'.", nameof(caption));

			window.Close();
			return Task.FromResult(0);
		}
	}
}