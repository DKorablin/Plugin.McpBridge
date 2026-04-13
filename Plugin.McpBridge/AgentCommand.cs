using System.Text.RegularExpressions;

namespace Plugin.McpBridge
{
	/// <summary>Represents a structured COMMAND: payload parsed from an AI response.</summary>
	/// <param name="IsCommand">Indicates whether the response starts with the COMMAND: prefix.</param>
	/// <param name="CommandGroup">The top-level command category, e.g. SETTINGS or METHODS.</param>
	/// <param name="Command">The operation within the group, e.g. LIST, GET, SET, INVOKE.</param>
	/// <param name="PluginId">Identifies the target plugin for the command.</param>
	/// <param name="MemberName">For SETTINGS commands, the setting key; for METHODS INVOKE, the method name; otherwise empty.</param>
	/// <param name="Arguments">Contains the raw JSON string of the command arguments, or null if no arguments are present.</param>
	internal sealed class AgentCommand
	{
		/// <summary>Represents a non-command AI response.</summary>
		public static readonly AgentCommand None = new AgentCommand();

		/// <summary>Indicates whether the response starts with the COMMAND: prefix.</summary>
		public Boolean IsCommand { get; private set; } = false;

		/// <summary>The top-level command category, e.g. SETTINGS or METHODS.</summary>
		public String CommandGroup { get; private set; } = String.Empty;

		/// <summary>The operation within the group, e.g. LIST, GET, SET, INVOKE.</summary>
		public String Command { get; private set; } = String.Empty;

		/// <summary>Identifies the target plugin for the command.</summary>
		public String PluginId { get; private set; } = String.Empty;

		/// <summary>For SETTINGS commands, the setting key; for METHODS INVOKE, the method name; otherwise empty.</summary>
		public String MemberName { get; private set; } = String.Empty;

		/// <summary>Contains the raw JSON string of the command arguments, or null if no arguments are present.</summary>
		public String? Arguments { get; private set; } = null;

		private static readonly (String Group, String SubCmd, Regex Pattern)[] CommandPatterns =
		{
			// SETTINGS LIST <plugin>
			("SETTINGS", "LIST",   new Regex(@"^SETTINGS\s+LIST\s+(?<id>\S+)", RegexOptions.IgnoreCase)),
			// SETTINGS GET <plugin> <key>
			("SETTINGS", "GET",    new Regex(@"^SETTINGS\s+GET\s+(?<id>\S+)\s+(?<member>\S+)", RegexOptions.IgnoreCase)),
			// SETTINGS SET <plugin> <key>=<val>
			("SETTINGS", "SET",    new Regex(@"^SETTINGS\s+SET\s+(?<id>\S+)\s+(?<member>[^=]+)=(?<args>.*)", RegexOptions.IgnoreCase)),
			// METHODS LIST <plugin>
			("METHODS",  "LIST",   new Regex(@"^METHODS\s+LIST\s+(?<id>\S+)", RegexOptions.IgnoreCase)),
			// METHODS INVOKE <plugin> <method> <json>
			("METHODS",  "INVOKE", new Regex(@"^METHODS\s+INVOKE\s+(?<id>\S+)\s+(?<member>\S+)\s*(?<args>.*)", RegexOptions.IgnoreCase))
		};

		public static AgentCommand Parse(String aiResponse)
		{
			if(!aiResponse.StartsWith("COMMAND:", StringComparison.OrdinalIgnoreCase))
				return None;

			var payload = aiResponse["COMMAND:".Length..].Trim();

			foreach(var (group, subCmd, regex) in CommandPatterns)
			{
				var match = regex.Match(payload);
				if(match.Success)
				{
					return new AgentCommand
					{
						IsCommand = true,
						CommandGroup = group,
						Command = subCmd,
						PluginId = match.Groups["id"].Value,
						MemberName = match.Groups["member"].Value,
						Arguments = match.Groups["args"].Success ? match.Groups["args"].Value : null
					};
				}
			}

			return None;
		}
	}
}