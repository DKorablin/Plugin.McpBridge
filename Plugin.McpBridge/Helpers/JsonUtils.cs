using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SAL.Flatbed;

namespace Plugin.McpBridge.Helpers;

internal static class JsonUtils
{
	public static Object?[] ConvertArgumentsValue(IPluginMethodInfo method, String argumentsJson)
	{
		using(JsonDocument doc = JsonDocument.Parse(argumentsJson))
		{
			JsonElement root = doc.RootElement;

			var arguments = method.GetParameters().ToArray();
			var result = new Object?[arguments.Length];

			for(var loop = 0; loop < arguments.Length; loop++)
			{
				var argument = arguments[loop];

				if(root.TryGetProperty(argument.Name, out JsonElement element))
				{
					Type targetType = Type.GetType(argument.AssemblyQualifiedName, true)
						?? throw new InvalidOperationException($"Could not resolve type '{argument.TypeName}' for argument '{argument.Name}'.");

					result[loop] = JsonUtils.ConvertValue(element.GetRawText(), targetType);
				} else
					result[loop] = null; // Or handle missing arguments as needed
			}

			return result;
		}
	}

	public static Object? ConvertValue(String valueJson, Type targetType)
	{
		_ = targetType ?? throw new ArgumentNullException(nameof(targetType));

		// 1. Handle Nullable types and null/empty strings
		Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
		Boolean isNullable = !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

		if(String.IsNullOrWhiteSpace(valueJson))
			return isNullable ? null : Activator.CreateInstance(underlyingType);

		// 2. Special case for Enums
		if(underlyingType.IsEnum)
			return Enum.Parse(underlyingType, valueJson, true);

		// 3. Try JSON deserialization
		try
		{
			var options = new JsonSerializerOptions
			{
				Converters = { new JsonStringEnumConverter() }
			};
			return JsonSerializer.Deserialize(valueJson, targetType, options);
		} catch(JsonException) { }

		// 4. Use TypeConverter
		TypeConverter converter = TypeDescriptor.GetConverter(underlyingType);
		if(converter != null && converter.CanConvertFrom(typeof(String)))
			return converter.ConvertFromString(null, CultureInfo.InvariantCulture, valueJson);

		// 5. Fallback to Convert.ChangeType for primitives
		return Convert.ChangeType(valueJson, underlyingType, CultureInfo.InvariantCulture);
	}
}