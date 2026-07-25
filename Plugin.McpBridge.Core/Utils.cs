using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Plugin.McpBridge;

internal static class Utils
{
	private static readonly Char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

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

	public static IEnumerable<String> ParseTokenUsageCount(UsageDetails usage)
	{
		var properties = usage.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach(var prop in properties)
		{
			var value = prop.GetValue(usage);

			if(value is Int64 longValue && longValue > 0)
				yield return $"{prop.Name}: {longValue:N0}";
		}

		if(usage.AdditionalCounts != null)
			foreach(var additionalCount in usage.AdditionalCounts)
				if(additionalCount.Value > 0)
					yield return $"{additionalCount.Key}: {additionalCount.Value:N0}";
	}

	public enum SpecialDirectory
	{
		SessionStore,
		EvaluationCache,
	}

	public static String GetAgentStorageDirectory(SpecialDirectory directoryType, String? agentRole = null)
	{
		String specialSubdir = "." + directoryType.ToString();

		String sanitizedSubdir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Plugin.McpBridge",
			specialSubdir,
			agentRole == null ? String.Empty : Utils.SanitizePath(agentRole));

		return sanitizedSubdir;
	}

	public static String SanitizePath(String input)
		=> new String(Array.ConvertAll(input.ToCharArray(), c => Array.IndexOf(Utils.InvalidFileNameChars, c) >= 0 ? '_' : c));
}