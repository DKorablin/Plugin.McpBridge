using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools
{
	/// <summary>Reflection-based helpers for inspecting and mutating SAL plugin settings.</summary>
	internal sealed class PluginSettingsTools
	{
		private readonly IHost _host;

		public PluginSettingsTools(IHost host)
		{
			this._host = host ?? throw new ArgumentNullException(nameof(host));
		}

		public static Boolean HasPluginSettings(IPluginDescription pluginDescription)
			=> pluginDescription.Instance is IPluginSettings;

		[Tool]
		[Description("List all available settings for a plugin")]
		public Task<String> SettingsList([Description("Plugin identifier")] String pluginId)
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
				builder.Append(FormatSettingValue(currentValue));
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

			return Task.FromResult(builder.ToString().Trim());

			String GetSettingDescription(PropertyInfo propertyInfo)
			{
				DescriptionAttribute? attribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
				return attribute?.Description ?? String.Empty;
			}

			String FormatSettingValue(Object? value)
				=> value == null
					? "<null>" : Convert.ToString(value, CultureInfo.InvariantCulture) ?? String.Empty;

			String GetFriendlyTypeName(Type propertyType)
			{
				Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
				return targetType.Name;
			}
		}

		[Tool]
		[Description("Get the current value of a specific plugin setting")]
		public Task<Object?> SettingsGet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName)
		{
			var settingsInstance = this.GetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription);

			PropertyInfo? propertyInfo = FindSettingsProperty(settingsInstance!, settingName);
			if(propertyInfo == null || !propertyInfo.CanRead)
				throw new ArgumentException($"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.");

			return Task.FromResult(propertyInfo.GetValue(settingsInstance, null));
		}

		[Tool(confirmationRequired: true)]
		[Description("Update a plugin setting value; requires user confirmation")]
		public Task<Object?> SettingsSet(
			[Description("Plugin identifier")] String pluginId,
			[Description("Setting name")] String settingName,
			[Description("New value as JSON")] String valueJson)
		{
			var settingsInstance = this.GetPluginSettingsInstance(pluginId, out IPluginDescription? pluginDescription);

			PropertyInfo? propertyInfo = FindSettingsProperty(settingsInstance!, settingName)
				?? throw new ArgumentException($"Setting '{settingName}' was not found for plugin '{pluginDescription!.ID}'.");

			if(!propertyInfo.CanWrite)
				throw new ArgumentException($"Setting '{propertyInfo.Name}' for plugin '{pluginDescription!.ID}' is read-only.");

			var convertedValue = Utils.ConvertValue(valueJson, propertyInfo.PropertyType);

			propertyInfo.SetValue(settingsInstance, convertedValue, null);
			return this.SettingsGet(pluginId, propertyInfo.Name);
		}

		private Object GetPluginSettingsInstance(String pluginId, out IPluginDescription? pluginDescription)
		{
			pluginDescription = this._host.Plugins[pluginId]
				?? throw new ArgumentException($"Plugin '{pluginId}' was not found.");

			if(pluginDescription.Instance is IPluginSettings settings)
				return settings.Settings;

			throw new ArgumentException($"Plugin '{pluginDescription.ID}' does not expose settings through {nameof(IPluginSettings)}.");
		}

		private static PropertyInfo? FindSettingsProperty(Object settingsInstance, String settingName)
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
	}
}