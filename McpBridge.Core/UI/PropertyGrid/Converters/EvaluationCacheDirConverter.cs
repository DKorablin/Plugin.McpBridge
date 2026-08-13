using System.ComponentModel;
using System.Globalization;

namespace McpBridge.Core.UI.PropertyGrid.Converters;

internal sealed class EvaluationCacheDirConverter : StringConverter
{
	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String) && value == null)
			return $"(default) {Utils.GetAgentStorageDirectory(Utils.SpecialDirectory.EvaluationCache)}";

		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		String? s = value as String;
		if(String.IsNullOrWhiteSpace(s) || s.StartsWith("(default)"))
			return null;

		return base.ConvertFrom(context, culture, value);
	}
}