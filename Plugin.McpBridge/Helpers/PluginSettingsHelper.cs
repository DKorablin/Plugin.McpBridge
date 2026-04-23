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
		private readonly IHost _host;

		public PluginSettingsHelper(IHost host)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
		}

		public static Boolean HasPluginSettings(IPluginDescription pluginDescription)
			=> pluginDescription.Instance is IPluginSettings;

		public String ListPluginSettings(String pluginId)
		{
			var settingsInstance = this.GetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription);

			PropertyInfo[] properties = settingsInstance!.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if(properties.Length == 0)
				throw new ArgumentException($"Plugin '{pluginDescription!.ID}' exposes a settings object, but it does not contain public properties.");

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

				String displayName = GetSettingDisplayName(propertyInfo);
				String propertyDescription = GetSettingDescription(propertyInfo);
				Object? currentValue = propertyInfo.GetValue(settingsInstance, null);

				builder.Append("- ");
				builder.Append(displayName);
				builder.Append(" [");
				builder.Append(propertyInfo.Name);
				builder.Append("] = ");
				builder.Append(this.FormatSettingValue(currentValue));
				builder.Append(" (");
				builder.Append(GetFriendlyTypeName(propertyInfo.PropertyType));
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
			var settingsInstance = this.GetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription);

			PropertyInfo? propertyInfo = this.FindSettingsProperty(settingsInstance!, settingName);
			if(propertyInfo == null || !propertyInfo.CanRead)
				throw new ArgumentException($"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.");

			Object? currentValue = propertyInfo.GetValue(settingsInstance, null);
			String displayName = GetSettingDisplayName(propertyInfo);
			String propertyDescription = GetSettingDescription(propertyInfo);

			StringBuilder builder = new StringBuilder();
			builder.Append($"Plugin '{pluginDescription!.ID}' setting {displayName}");
			builder.Append($" [{propertyInfo.Name}] = {this.FormatSettingValue(currentValue)}");
			builder.Append($" ({GetFriendlyTypeName(propertyInfo.PropertyType)})");

			if(!String.IsNullOrWhiteSpace(propertyDescription))
			{
				builder.Append(" - ");
				builder.Append(propertyDescription);
			}

			return builder.ToString();
		}

		public String UpdatePluginSetting(String pluginId, String settingName, String valueJson)
		{
			var settingsInstance = this.GetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription);

			PropertyInfo? propertyInfo = this.FindSettingsProperty(settingsInstance!, settingName)
				?? throw new ArgumentException($"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.");

			if(!propertyInfo.CanWrite)
				throw new ArgumentException($"Setting '{propertyInfo.Name}' for plugin '{pluginDescription!.ID}' is read-only.");

			var convertedValue = JsonUtils.ConvertValue(valueJson, propertyInfo.PropertyType);

			propertyInfo.SetValue(settingsInstance, convertedValue, null);
			return this.ReadPluginSetting(pluginId, propertyInfo.Name);
		}

		private Object GetPluginSettingsInstance(String pluginId, out IPluginDescription? pluginDescription)
		{
			pluginDescription = this._host.Plugins[pluginId];
			if(pluginDescription == null)
				throw new ArgumentException($"Plugin '{pluginId}' was not found.");

			if(pluginDescription.Instance is IPluginSettings settings)
				return settings.Settings;

			throw new ArgumentException($"Plugin '{pluginDescription.ID}' does not expose settings through {nameof(IPluginSettings)}.");
		}

		private PropertyInfo? FindSettingsProperty(Object settingsInstance, String settingName)
		{
			foreach(PropertyInfo propertyInfo in settingsInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				if(String.Equals(propertyInfo.Name, settingName, StringComparison.OrdinalIgnoreCase) ||
					String.Equals(GetSettingDisplayName(propertyInfo), settingName, StringComparison.OrdinalIgnoreCase))
					return propertyInfo;

			return null;
		}

		private static String GetSettingDisplayName(PropertyInfo propertyInfo)
		{
			DisplayNameAttribute? attribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
			return attribute != null && !String.IsNullOrWhiteSpace(attribute.DisplayName) ? attribute.DisplayName : propertyInfo.Name;
		}

		private static String GetSettingDescription(PropertyInfo propertyInfo)
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

		private static String GetFriendlyTypeName(Type propertyType)
		{
			Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
			return targetType.Name;
		}
	}
}