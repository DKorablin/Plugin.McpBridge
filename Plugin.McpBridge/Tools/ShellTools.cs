using System.ComponentModel;
using System.Globalization;

namespace Plugin.McpBridge.Tools
{
	internal sealed class ShellTools
	{
		private readonly TimeProvider _timeProvider;

		public ShellTools(TimeProvider? timeProvider = null)
		{
			this._timeProvider = timeProvider ?? TimeProvider.System;
		}

		[Tool]
		[Description("Get the current host environment system information including OS version, DateTime format and UTC")]
		public async Task<String> SystemInformation()
		{
			DateTimeFormatInfo formatPreferences = CultureInfo.CurrentCulture.DateTimeFormat;
			String result = @$"
Short date pattern: {formatPreferences.ShortDatePattern}
Long date pattern; {formatPreferences.LongTimePattern}
Current time: {this._timeProvider.GetLocalNow()}
OS Version: {Environment.OSVersion}";

			return result;
		}
	}
}