using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SAL.Flatbed;

namespace Plugin.McpBridge;

internal static class Utils
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

					result[loop] = Utils.ConvertValue(element.GetRawText(), targetType);
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

	public static Boolean IsBitSet(UInt32 flags, Int32 bit)
		=> (flags & (1U << bit)) != 0;

	public static UInt32[] BitToInt(params Boolean[] bits)
	{
		UInt32[] result = new UInt32[] { };
		Int32 counter = 0;
		for(Int32 loop = 0; loop < bits.Length; loop++)
		{
			if(result.Length <= loop)//Increase the array by one if the value does not fit
				Array.Resize<UInt32>(ref result, result.Length + 1);

			for(Int32 innerLoop = 0; innerLoop < 32; innerLoop++)
			{
				result[loop] |= Convert.ToUInt32(bits[counter++]) << innerLoop;
				if(counter >= bits.Length)
					break;
			}
			if(counter >= bits.Length)
				break;
		}
		return result;
	}
}