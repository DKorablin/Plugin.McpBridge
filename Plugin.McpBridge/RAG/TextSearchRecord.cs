namespace Plugin.McpBridge.RAG;

public class TextSearchRecord
{
	public String SourceId { get; set; } = String.Empty;
	public String SourceName { get; set; } = String.Empty;
	public String SourceLink { get; set; } = String.Empty;
	public String Text { get; set; } = String.Empty;
	public String Embedding { get; set; } = String.Empty;
}