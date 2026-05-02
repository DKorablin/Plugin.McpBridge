using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms.Design;
using Plugin.McpBridge.Tools;

namespace Plugin.McpBridge.UI.PropertyGrid;

/// <summary>Drop-down property-grid editor that renders each discovered tool method as a named, described checkbox.</summary>
internal sealed class ToolsPermissionEditor : UITypeEditor
{
	private ToolPermissionControl? _control;

	public override Object EditValue(ITypeDescriptorContext? context, IServiceProvider provider, Object? value)
	{
		if(this._control == null)
			this._control = new ToolPermissionControl(ToolsFactory.DiscoverTools(Assembly.GetExecutingAssembly()));

		this._control.SetValue(value as String[] ?? Array.Empty<String>());
		((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))!).DropDownControl(this._control);
		return this._control.Result;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		=> UITypeEditorEditStyle.DropDown;

	private sealed class ToolPermissionControl : UserControl
	{
		private readonly CheckedListBox _list = new CheckedListBox();
		private readonly List<String> _methodNames = new List<String>();

		/// <summary>Returns the unchecked method names (blocked tools), or an empty array when all items are checked (meaning all tools are allowed).</summary>
		public String[] Result
		{
			get
			{
				List<String> blocked = new List<String>();
				for(Int32 i = 0; i < this._list.Items.Count; i++)
					if(!this._list.GetItemChecked(i))
						blocked.Add(this._methodNames[i]);
				return blocked.ToArray();
			}
		}

		public ToolPermissionControl(IEnumerable<(String MethodName, String Description)> tools)
		{
			this.SuspendLayout();
			this.BackColor = SystemColors.Control;
			this._list.FormattingEnabled = true;
			this._list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			this._list.BorderStyle = BorderStyle.None;

			foreach((String methodName, String description) in tools)
			{
				this._methodNames.Add(methodName);
				String label = String.IsNullOrWhiteSpace(description)
					? methodName
					: $"{methodName} — {description}";
				this._list.Items.Add(label);
			}

			this.Size = new Size(this._list.Width, this._list.Height);
			this.Controls.Add(this._list);
			this._list.Focus();
			this.ResumeLayout();
		}

		public void SetValue(String[] blockedTools)
		{
			for(Int32 i = 0; i < this._list.Items.Count; i++)
				this._list.SetItemChecked(i, !Array.Exists(blockedTools, p => p == this._methodNames[i]));
		}
	}
}
