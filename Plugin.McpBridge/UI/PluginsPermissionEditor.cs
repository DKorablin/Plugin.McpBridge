using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using SAL.Flatbed;

namespace Plugin.McpBridge.UI;

/// <summary>Drop-down property-grid editor that renders each loaded plugin as a named checkbox.</summary>
internal sealed class PluginsPermissionEditor : UITypeEditor
{
	public override Object EditValue(ITypeDescriptorContext? context, IServiceProvider provider, Object? value)
	{
		if(context?.Instance is Settings settings)
		{
			IEnumerable<(String Id, String Name)> plugins = GetPlugins(settings.Host.Plugins);
			PluginPermissionControl control = new PluginPermissionControl(plugins);
			control.SetValue(value as String[] ?? Array.Empty<String>());
			((IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))!).DropDownControl(control);
			return control.Result;
		} else
			return value ?? Array.Empty<String>();
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		=> UITypeEditorEditStyle.DropDown;

	private static IEnumerable<(String Id, String Name)> GetPlugins(IPluginStorage plugins)
	{
		foreach(IPluginDescription plugin in plugins)
			yield return (plugin.ID, plugin.Name);
	}

	private sealed class PluginPermissionControl : UserControl
	{
		private readonly CheckedListBox _list = new CheckedListBox();
		private readonly List<String> _pluginIds = new List<String>();

		/// <summary>Returns the unchecked plugin IDs (blocked plugins), or an empty array when all items are checked (meaning all plugins are allowed).</summary>
		public String[] Result
		{
			get
			{
				List<String> blocked = new List<String>();
				for(Int32 i = 0; i < this._list.Items.Count; i++)
					if(!this._list.GetItemChecked(i))
						blocked.Add(this._pluginIds[i]);
				return blocked.ToArray();
			}
		}

		public PluginPermissionControl(IEnumerable<(String Id, String Name)> plugins)
		{
			this.SuspendLayout();
			this.BackColor = SystemColors.Control;
			this._list.FormattingEnabled = true;
			this._list.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			this._list.BorderStyle = BorderStyle.None;

			foreach((String id, String name) in plugins)
			{
				this._pluginIds.Add(id);
				String label = String.IsNullOrWhiteSpace(name) || name == id
					? id
					: $"{id} — {name}";
				this._list.Items.Add(label);
			}

			this.Size = new Size(this._list.Width, this._list.Height);
			this.Controls.Add(this._list);
			this._list.Focus();
			this.ResumeLayout();
		}

		public void SetValue(String[] blockedPlugins)
		{
			for(Int32 i = 0; i < this._list.Items.Count; i++)
				this._list.SetItemChecked(i, !Array.Exists(blockedPlugins, p => p == this._pluginIds[i]));
		}
	}
}

/// <summary>Replaces plugin IDs with their display names when the PluginsPermission array is expanded in the PropertyGrid.</summary>
internal sealed class PluginsPermissionConverter : ArrayConverter
{
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, Object value, Attribute[]? attributes)
	{
		PropertyDescriptorCollection baseProps = base.GetProperties(context, value, attributes);
		if(context?.Instance is not Settings settings || value is not String[])
			return baseProps;

		Dictionary<String, String> idToName = new Dictionary<String, String>(StringComparer.Ordinal);
		foreach(IPluginDescription plugin in settings.Host.Plugins)
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