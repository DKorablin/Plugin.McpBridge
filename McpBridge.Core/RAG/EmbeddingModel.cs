namespace McpBridge.Core.RAG;

internal static class EmbeddingModel
{
	public enum Type
	{
		/// <summary>OpenAI, Azure: Best quality, what this code targets</summary>
		TextEmbedding3Large,
		/// <summary>OpenAI, Azure: Faster/cheaper</summary>
		TextEmbedding3Small,
		/// <summary>OpenAI, Azure: Older model, still supported</summary>
		TextEmbeddingAda002,
		/// <summary>Voyage: Large model</summary>
		Voyage3Large,
		/// <summary>Anthropic-recommended for Claude RAG</summary>
		Voyage3,
		/// <summary>Cohere: Good multilingual support</summary>
		EmbedV40,
		TextEmbedding004,
		/// <summary>Ollama: Most popular, fast, good quality</summary>
		NomicEmbedText,
		/// <summary>Ollama: Large embedding model from Mxbai</summary>
		MxbaiEmbedLarge,
		/// <summary>Ollama: Biggest, most powerful embedding model</summary>
		BgeM3,
	}

	public static String ToModelName(Type embeddingType)
	{
		return embeddingType switch
		{
			Type.TextEmbedding3Large => "text-embedding-3-large",
			Type.TextEmbedding3Small => "text-embedding-3-small",
			Type.TextEmbeddingAda002 => "text-embedding-ada-002",
			Type.Voyage3Large => "voyage-3-large",
			Type.Voyage3 => "voyage-3",
			Type.EmbedV40 => "embed-v4.0",
			Type.TextEmbedding004 => "text-embedding-004",
			Type.NomicEmbedText => "nomic-embed-text",
			Type.MxbaiEmbedLarge => "mxbai-embed-large",
			Type.BgeM3 => "bge-m3",
			_ => throw new ArgumentOutOfRangeException(nameof(embeddingType), $"Unsupported embedding type: {embeddingType}"),
		};
	}

	public static Int32? GetDimention(String modelName)
	{
		Type? embeddingType = FromModelName(modelName);
		return embeddingType.HasValue
			? GetDimension(embeddingType.Value)
			: null;
	}

	public static Int32 GetDimension(Type embeddingType)
	{
		return embeddingType switch
		{
			Type.TextEmbedding3Large => 1024,
			Type.TextEmbedding3Small => 1024,
			Type.TextEmbeddingAda002 => 1536,
			Type.Voyage3Large => 1024,
			Type.Voyage3 => 1024,
			Type.EmbedV40 => 2048,
			Type.TextEmbedding004 => 8192,
			Type.NomicEmbedText => 768,
			Type.MxbaiEmbedLarge => 1024,
			Type.BgeM3 => 1024,
			_ => throw new ArgumentOutOfRangeException(nameof(embeddingType), $"Unsupported embedding type: {embeddingType}"),
		};
	}

	public static Type? FromModelName(String modelName)
	{
		return modelName switch
		{
			"text-embedding-3-large" => Type.TextEmbedding3Large,
			"text-embedding-3-small" => Type.TextEmbedding3Small,
			"text-embedding-ada-002" => Type.TextEmbeddingAda002,
			"voyage-3-large" => Type.Voyage3Large,
			"voyage-3" => Type.Voyage3,
			"embed-v4.0" => Type.EmbedV40,
			"text-embedding-004" => Type.TextEmbedding004,
			_ => null,
		};
	}
}