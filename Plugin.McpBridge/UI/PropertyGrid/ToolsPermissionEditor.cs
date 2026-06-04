using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms.Design;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.Tools;

namespace Plugin.McpBridge.UI.PropertyGrid;

internal sealed class ToolsPermissionConverter : ArrayConverter
{
	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String))
			return value is null ? "(All)" : value is String[] arr && arr.Length == 0 ? "(None)" : base.ConvertTo(context, culture, value, destinationType);
		return base.ConvertTo(context, culture, value, destinationType);
	}
}

/// <summary>Drop-down property-grid editor that renders each discovered tool method as a named, described checkbox.</summary>
internal sealed class ToolsPermissionEditor : UITypeEditor
{
	public override Object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, Object? value)
	{
		var plugin = Plugin.StaticInstance!;
		var ctrl = new ToolPermissionControl(new ToolsFactory(plugin.Host, plugin.Settings, plugin.Settings.SelectedAgent).GetTools());
		ctrl.SetValue((String[]?)value);
		((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))!).DropDownControl(ctrl);
		return ctrl.Result;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		=> UITypeEditorEditStyle.DropDown;

	private sealed class ToolPermissionControl : UserControl
	{
		private readonly CheckedListBox _list = new CheckedListBox();
		private readonly List<String> _methodNames = new List<String>();

		/// <summary>Returns the unchecked method names (blocked tools), or an empty array when all items are checked (meaning all tools are allowed).</summary>
		public String[]? Result
		{
			get
			{
				List<String> allowed = new List<String>();
				Int32 count = this._list.Items.Count;

				for(Int32 i = 0; i < count; i++)
					if(this._list.GetItemChecked(i))
						allowed.Add(this._methodNames[i]);

				return allowed.Count == count
					? null
					: allowed.ToArray();
			}
		}

		public ToolPermissionControl(IEnumerable<ToolMethodDto> tools)
		{
			this.SuspendLayout();
			this.BackColor = SystemColors.Control;
			this._list.FormattingEnabled = true;
			this._list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			this._list.BorderStyle = BorderStyle.None;

			foreach(var tool in tools)
			{
				this._methodNames.Add(tool.Name);
				String label = String.IsNullOrWhiteSpace(tool.Description)
					? tool.Name
					: String.Join(" - ", tool.Name, tool.Description);
				this._list.Items.Add(label);
			}

			this.Size = new Size(this._list.Width, this._list.Height);
			this.Controls.Add(this._list);
			this._list.Focus();
			this.ResumeLayout();
		}

		public void SetValue(String[]? availableTools)
		{
			Boolean allowAll = availableTools == null;
			for(Int32 i = 0; i < this._list.Items.Count; i++)
				this._list.SetItemChecked(i, allowAll || Array.Exists(availableTools!, p => p == this._methodNames[i]));
		}
	}
}