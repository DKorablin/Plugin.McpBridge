using System.ComponentModel;
using System.Globalization;
using McpBridge.Core.Data;

namespace Plugin.McpBridge.UI.PropertyGrid.Converters;

public class EmbeddingProviderIdConverter : GuidConverter
{
	private const String NoneDisplay = "(Selected Provider)";

	public override Boolean GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

	public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

	public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
	{
		List<Guid?> values = new List<Guid?>() { null };
		BindingList<AiProviderDto>? providers = Plugin.StaticInstance?.Settings.AiProviders;

		if(providers != null)
			values.AddRange(providers
				.Where(x => x.SupportsCapability(ProviderCapabilities.Embeddings))
				.Select(x => (Guid?)x.Id));

		return new StandardValuesCollection(values);
	}

	public override Object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
	{
		if(destinationType == typeof(String))
		{
			if(value == null)
				return NoneDisplay;

			AiProviderDto? provider = Plugin.StaticInstance?.Settings.AiProviders?
				.Where(x => x.SupportsCapability(ProviderCapabilities.Embeddings))
				.FirstOrDefault(x => x.Id == (Guid)value);
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

			AiProviderDto? provider = Plugin.StaticInstance?.Settings.AiProviders?
				.Where(x => x.SupportsCapability(ProviderCapabilities.Embeddings))
				.FirstOrDefault(x => x.ToString() == s);
			if(provider != null)
				return provider.Id;
		}

		return base.ConvertFrom(context, culture, value);
	}
}