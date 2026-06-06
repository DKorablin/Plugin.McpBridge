using System.ComponentModel;
using System.Globalization;

namespace Plugin.McpBridge.UI.PropertyGrid;

internal class AiAgentIdConverter : GuidConverter
{
	private const String NoneDisplay = "(First)";

	public override Boolean GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

	public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

	public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
	{
		var values = new List<Guid?>() { null }; // Start with null for default "(None)" option

		if(Plugin.StaticInstance.Settings.AiAgents != null)
			values.AddRange(Plugin.StaticInstance.Settings.AiAgents.Select(p => (Guid?)p.Id));
		return new StandardValuesCollection(values);
	}

	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String))
		{
			if(value == null)
				return NoneDisplay;

			var provider = Plugin.StaticInstance.Settings.AiAgents?.FirstOrDefault(p => p.Id == (Guid)value);
			if(provider != null)
				return provider.ToString();
		}

		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if(value is String s)
		{
			if(String.IsNullOrWhiteSpace(s) || s == NoneDisplay)
				return null;

			// If the user selected from the dropdown, find the matching ID by name
			var match = Plugin.StaticInstance.Settings.AiAgents?.FirstOrDefault(p => p.ToString() == s);
			if(match != null)
				return match.Id;
		}

		return base.ConvertFrom(context, culture, value);
	}
}