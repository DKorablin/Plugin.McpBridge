using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using SAL.Flatbed;

namespace Plugin.McpBridge.Helpers
{
	/// <summary>Reflection-based helpers for inspecting and mutating SAL plugin settings.</summary>
	internal sealed class PluginSettingsHelper
	{
		private readonly SAL.Flatbed.IHost _host;

		public PluginSettingsHelper(SAL.Flatbed.IHost host)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
		}

		public static Boolean HasPluginSettings(IPluginDescription pluginDescription)
			=> pluginDescription.Instance is IPluginSettings;

		public String ListPluginSettings(String pluginId)
		{
			if(!this.TryGetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription, out Object? settingsInstance, out String? errorMessage))
				return errorMessage ?? String.Empty;

			PropertyInfo[] properties = settingsInstance!.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if(properties.Length == 0)
				return $"Plugin '{pluginDescription!.ID}' exposes a settings object, but it does not contain public properties.";

			StringBuilder builder = new StringBuilder();
			builder.Append("Settings for plugin '");
			builder.Append(pluginDescription!.ID);
			builder.Append("' (");
			builder.Append(pluginDescription.Name);
			builder.AppendLine("):");

			foreach(PropertyInfo propertyInfo in properties)
			{
				if(!propertyInfo.CanRead)
					continue;

				String displayName = this.GetSettingDisplayName(propertyInfo);
				String propertyDescription = this.GetSettingDescription(propertyInfo);
				Object? currentValue = propertyInfo.GetValue(settingsInstance, null);

				builder.Append("- ");
				builder.Append(displayName);
				builder.Append(" [");
				builder.Append(propertyInfo.Name);
				builder.Append("] = ");
				builder.Append(this.FormatSettingValue(currentValue));
				builder.Append(" (");
				builder.Append(this.GetFriendlyTypeName(propertyInfo.PropertyType));
				builder.Append(')');

				if(!String.IsNullOrWhiteSpace(propertyDescription))
				{
					builder.Append(" - ");
					builder.Append(propertyDescription);
				}

				builder.AppendLine();
			}

			return builder.ToString().Trim();
		}

		public String ReadPluginSetting(String pluginId, String settingName)
		{
			if(!this.TryGetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription, out Object? settingsInstance, out String errorMessage))
				return errorMessage;

			PropertyInfo? propertyInfo = this.FindSettingsProperty(settingsInstance!, settingName);
			if(propertyInfo == null || !propertyInfo.CanRead)
				return $"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.";

			Object? currentValue = propertyInfo.GetValue(settingsInstance, null);
			String displayName = this.GetSettingDisplayName(propertyInfo);
			String propertyDescription = this.GetSettingDescription(propertyInfo);

			StringBuilder builder = new StringBuilder();
			builder.Append("Plugin '");
			builder.Append(pluginDescription!.ID);
			builder.Append("' setting ");
			builder.Append(displayName);
			builder.Append(" [");
			builder.Append(propertyInfo.Name);
			builder.Append("] = ");
			builder.Append(this.FormatSettingValue(currentValue));
			builder.Append(" (");
			builder.Append(this.GetFriendlyTypeName(propertyInfo.PropertyType));
			builder.Append(')');

			if(!String.IsNullOrWhiteSpace(propertyDescription))
			{
				builder.Append(" - ");
				builder.Append(propertyDescription);
			}

			return builder.ToString();
		}

		public String UpdatePluginSetting(String pluginId, String settingName, String settingValue)
		{
			if(!this.TryGetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription, out Object? settingsInstance, out String errorMessage))
				return errorMessage;

			PropertyInfo? propertyInfo = this.FindSettingsProperty(settingsInstance!, settingName);
			if(propertyInfo == null)
				return $"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.";

			if(!propertyInfo.CanWrite)
				return $"Setting '{propertyInfo.Name}' for plugin '{pluginDescription!.ID}' is read-only.";

			if(!this.TryConvertSettingValue(settingValue, propertyInfo.PropertyType, out Object? convertedValue, out errorMessage))
				return errorMessage;

			propertyInfo.SetValue(settingsInstance, convertedValue, null);
			return this.ReadPluginSetting(pluginId, propertyInfo.Name);
		}

		private Boolean TryGetPluginSettingsInstance(String pluginId, out IPluginDescription? pluginDescription, out Object? settingsInstance, out String errorMessage)
		{
			pluginDescription = this._host.Plugins[pluginId];
			if(pluginDescription == null)
			{
				settingsInstance = null;
				errorMessage = $"Plugin '{pluginId}' was not found.";
				return false;
			}

			if(pluginDescription.Instance is IPluginSettings settings)
			{
				settingsInstance = settings.Settings;
				errorMessage = String.Empty;
				return true;
			} else
			{
				settingsInstance = null;
				errorMessage = $"Plugin '{pluginDescription.ID}' does not expose settings through {nameof(IPluginSettings)}.";
				return false;
			}
		}

		private PropertyInfo? FindSettingsProperty(Object settingsInstance, String settingName)
		{
			foreach(PropertyInfo propertyInfo in settingsInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				if(String.Equals(propertyInfo.Name, settingName, StringComparison.OrdinalIgnoreCase) ||
					String.Equals(this.GetSettingDisplayName(propertyInfo), settingName, StringComparison.OrdinalIgnoreCase))
					return propertyInfo;

			return null;
		}

		private String GetSettingDisplayName(PropertyInfo propertyInfo)
		{
			DisplayNameAttribute? attribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
			return attribute != null && !String.IsNullOrWhiteSpace(attribute.DisplayName) ? attribute.DisplayName : propertyInfo.Name;
		}

		private String GetSettingDescription(PropertyInfo propertyInfo)
		{
			DescriptionAttribute? attribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
			return attribute != null ? attribute.Description : String.Empty;
		}

		private String FormatSettingValue(Object? value)
		{
			if(value == null)
				return "<null>";

			return Convert.ToString(value, CultureInfo.InvariantCulture) ?? String.Empty;
		}

		private String GetFriendlyTypeName(Type propertyType)
		{
			Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
			return targetType.Name;
		}

		private Boolean TryConvertSettingValue(String rawValue, Type targetType, out Object? convertedValue, out String errorMessage)
		{
			Type nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;
			Boolean isNullable = !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

			if(String.IsNullOrWhiteSpace(rawValue) && isNullable)
			{
				convertedValue = null;
				errorMessage = String.Empty;
				return true;
			}

			try
			{
				if(nonNullableType == typeof(String))
					convertedValue = rawValue;
				else if(nonNullableType.IsEnum)
					convertedValue = Enum.Parse(nonNullableType, rawValue, true);
				else if(nonNullableType == typeof(Boolean))
					convertedValue = Boolean.Parse(rawValue);
				else if(nonNullableType == typeof(Guid))
					convertedValue = Guid.Parse(rawValue);
				else if(nonNullableType == typeof(TimeSpan))
					convertedValue = TimeSpan.Parse(rawValue, CultureInfo.InvariantCulture);
				else
					convertedValue = Convert.ChangeType(rawValue, nonNullableType, CultureInfo.InvariantCulture);

				errorMessage = String.Empty;
				return true;
			}
			catch(Exception exception)
			{
				convertedValue = null;
				errorMessage = $"Unable to convert value '{rawValue}' to {this.GetFriendlyTypeName(targetType)}: {exception.Message}";
				return false;
			}
		}
	}
}
