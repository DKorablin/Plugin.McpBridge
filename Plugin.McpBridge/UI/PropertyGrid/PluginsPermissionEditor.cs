using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms.Design;
using SAL.Flatbed;

namespace Plugin.McpBridge.UI.PropertyGrid;

/// <summary>Drop-down property-grid editor that renders each loaded plugin as a named checkbox.</summary>
internal sealed class PluginsPermissionEditor : UITypeEditor
{
	public override Object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, Object? value)
	{
		var plugin = Plugin.StaticInstance;
		var ctrl = new PluginPermissionControl(plugin.Host.Plugins);
		ctrl.SetValue((String[]?)value);
		((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))!).DropDownControl(ctrl);
		return ctrl.Result;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		=> UITypeEditorEditStyle.DropDown;

	private sealed class PluginPermissionControl : UserControl
	{
		private readonly CheckedListBox _list = new CheckedListBox();
		private readonly List<String> _pluginIds = new List<String>();

		/// <summary>Returns the unchecked plugin IDs (blocked plugins), or an empty array when all items are checked (meaning all plugins are allowed).</summary>
		public String[]? Result
		{
			get
			{
				List<String> allowed = new List<String>();
				Int32 count = this._list.Items.Count;

				for(Int32 i = 0; i < count; i++)
					if(this._list.GetItemChecked(i))
						allowed.Add(this._pluginIds[i]);

				return allowed.Count == count
					? null
					: allowed.ToArray();
			}
		}

		public PluginPermissionControl(IEnumerable<IPluginDescription> plugins)
		{
			this.SuspendLayout();
			this.BackColor = SystemColors.Control;
			this._list.FormattingEnabled = true;
			this._list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			this._list.BorderStyle = BorderStyle.None;

			foreach(var plugin in plugins)
			{
				this._pluginIds.Add(plugin.ID);
				String label = String.IsNullOrWhiteSpace(plugin.Name) || plugin.Name == plugin.ID
					? plugin.ID
					: String.Join(" - ", plugin.ID, plugin.Name);
				this._list.Items.Add(label);
			}

			this.Size = new Size(this._list.Width, this._list.Height);
			this.Controls.Add(this._list);
			this._list.Focus();
			this.ResumeLayout();
		}

		public void SetValue(String[]? enabledPlugins)
		{
			Boolean allowAll = enabledPlugins == null;
			for(Int32 i = 0; i < this._list.Items.Count; i++)
				this._list.SetItemChecked(i, allowAll || Array.Exists(enabledPlugins!, p => p == this._pluginIds[i]));
		}
	}
}

/// <summary>Replaces plugin IDs with their display names when the PluginsPermission array is expanded in the PropertyGrid.</summary>
internal sealed class PluginsPermissionConverter : ArrayConverter
{
	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String))
			return value is null ? "(All)" : value is String[] arr && arr.Length == 0 ? "(None)" : base.ConvertTo(context, culture, value, destinationType);
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, Object value, Attribute[]? attributes)
	{
		PropertyDescriptorCollection baseProps = base.GetProperties(context, value, attributes);
		if(value is not String[])
			return baseProps;

		Dictionary<String, String> idToName = new Dictionary<String, String>(StringComparer.Ordinal);
		foreach(IPluginDescription plugin in Plugin.StaticInstance.Host.Plugins)
			if(!String.IsNullOrWhiteSpace(plugin.Name))
				idToName[plugin.ID] = plugin.Name;

		PropertyDescriptor[] wrapped = new PropertyDescriptor[baseProps.Count];
		for(Int32 i = 0; i < baseProps.Count; i++)
			wrapped[i] = new PluginNameDescriptor(baseProps[i], idToName);
		return new PropertyDescriptorCollection(wrapped);
	}

	private sealed class PluginNameDescriptor : PropertyDescriptor
	{
		private readonly PropertyDescriptor _inner;
		private readonly Dictionary<String, String> _idToName;

		public override Type ComponentType => this._inner.ComponentType;
		public override Boolean IsReadOnly => this._inner.IsReadOnly;
		public override Type PropertyType => this._inner.PropertyType;

		public PluginNameDescriptor(PropertyDescriptor inner, Dictionary<String, String> idToName)
			: base(inner)
		{
			this._inner = inner;
			this._idToName = idToName;
		}

		public override Object? GetValue(Object? component)
		{
			Object? val = this._inner.GetValue(component);
			return val is String id && this._idToName.TryGetValue(id, out String? name) ? name : val;
		}

		public override Boolean CanResetValue(Object component) => this._inner.CanResetValue(component);
		public override void ResetValue(Object component) => this._inner.ResetValue(component);
		public override void SetValue(Object? component, Object? value) => this._inner.SetValue(component, value);
		public override Boolean ShouldSerializeValue(Object component) => this._inner.ShouldSerializeValue(component);
	}
}