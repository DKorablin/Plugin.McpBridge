using System.ComponentModel;
using System.Globalization;

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
