using System.ComponentModel;
using System.Globalization;

namespace Plugin.McpBridge.UI;

public class AiProviderIdConverter : GuidConverter
{
	private const String NoneDisplay = "(First)";

	public override Boolean GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

	public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

	public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
	{
		if(context?.Instance is Settings settings)
		{
			var values = new List<Guid?>() { null }; // Start with null for default "(None)" option

			if(settings.AiProviders != null)
				values.AddRange(settings.AiProviders.Select(p => (Guid?)p.Id));
			return new StandardValuesCollection(values);
		}
		return base.GetStandardValues(context);
	}

	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String))
		{
			if(value == null)
				return NoneDisplay;

			if(context?.Instance is Settings settings && settings.AiProviders != null)
			{
				var provider = settings.AiProviders.FirstOrDefault(p => p.Id == (Guid)value);
				if(provider != null)
					return $"{provider.ProviderType} ({provider.ModelId})";
			}
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
			if(context?.Instance is Settings settings && settings.AiProviders != null)
			{
				var match = settings.AiProviders.FirstOrDefault(p => $"{p.ProviderType} ({p.ModelId})" == s);

				if(match != null)
					return match.Id;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}
}