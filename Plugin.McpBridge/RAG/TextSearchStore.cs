using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace Plugin.McpBridge.RAG;

public class TextSearchStore
{
	private readonly VectorStoreCollection<String, TextSearchRecord> _collection;
	public TextSearchStore(InMemoryVectorStore vectorStore, String collectionName, Int32 dimensions)
	{
		var definition = new VectorStoreCollectionDefinition
		{
			Properties =
			[
				new VectorStoreKeyProperty(nameof(TextSearchRecord.SourceId), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.SourceName), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.SourceLink), typeof(String)),
				new VectorStoreDataProperty(nameof(TextSearchRecord.Text), typeof(String)),
				new VectorStoreVectorProperty(nameof(TextSearchRecord.Embedding), typeof(String), dimensions),
			]
		};
		_collection = vectorStore.GetCollection<String, TextSearchRecord>(collectionName, definition);
	}
	public async Task UpsertDocumentsAsync(IEnumerable<TextSearchDocument> documents)
	{
		await _collection.EnsureCollectionExistsAsync();
		foreach(var doc in documents)
		{
			var record = new TextSearchRecord
			{
				SourceId = doc.SourceId,
				SourceName = doc.SourceName,
				SourceLink = doc.SourceLink,
				Text = doc.Text,
				Embedding = doc.Text
			};
			await _collection.UpsertAsync(record);
		}
	}
	public async Task<IEnumerable<TextSearchDocument>> SearchAsync(String query, Int32 topK, CancellationToken cancellationToken = default)
	{
		var results = _collection.SearchAsync(query, topK, cancellationToken: cancellationToken);
		var documents = new List<TextSearchDocument>();
		await foreach(var result in results)
		{
			documents.Add(new TextSearchDocument
			{
				SourceId = result.Record.SourceId,
				SourceName = result.Record.SourceName,
				SourceLink = result.Record.SourceLink,
				Text = result.Record.Text
			});
		}
		return documents;
	}

	public static IEnumerable<TextSearchDocument> GetSampleDocuments()
	{
		yield return new TextSearchDocument
		{
			SourceId = "beginner-strength-001",
			SourceName = "Iron Mind AI - Beginner Strength Training Guide",
			SourceLink = "https://ironmindai.com/tips/beginner-strength",
			Text = "For beginners, focus on compound movements like squats, deadlifts, bench press, " +
				"and overhead press. Train 3-4 days per week with at least one rest day between " +
				"sessions. Start with a weight you can control for 8-12 reps with good form. " +
				"Progressive overload is key - aim to gradually increase weight, reps, or sets " +
				"over time. Consistency beats intensity in the early stages."
		};
		yield return new TextSearchDocument
		{
			SourceId = "nutrition-basics-001",
			SourceName = "Iron Mind AI - Nutrition for Muscle Growth",
			SourceLink = "https://ironmindai.com/tips/nutrition-muscle-growth",
			Text = "To support muscle growth, aim for 1.6 to 2.2 grams of protein per kilogram of " +
				"body weight per day. Spread protein intake across 3-5 meals for optimal muscle " +
				"protein synthesis. Prioritize whole food sources like chicken, fish, eggs, Greek " +
				"yogurt, and legumes. Don't neglect carbohydrates - they fuel your workouts and " +
				"aid recovery. A slight caloric surplus of 200-300 calories above maintenance is " +
				"ideal for lean muscle gain."
		};
		yield return new TextSearchDocument
		{
			SourceId = "recovery-sleep-001",
			SourceName = "Iron Mind AI - Recovery and Sleep Guide",
			SourceLink = "https://ironmindai.com/tips/recovery-sleep",
			Text = "Sleep is when your body repairs and builds muscle tissue. Aim for 7-9 hours of " +
				"quality sleep per night. Poor sleep increases cortisol levels, which can impair " +
				"muscle recovery and promote fat storage. Establish a consistent sleep schedule, " +
				"limit screen time before bed, and keep your room cool and dark. Active recovery " +
				"on rest days - such as walking, stretching, or light yoga - also helps reduce " +
				"soreness and improve circulation."
		};
	}
}