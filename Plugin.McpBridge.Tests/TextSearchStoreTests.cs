using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Plugin.McpBridge.RAG;
using Xunit;

namespace Plugin.McpBridge.Tests;

public class TextSearchStoreTests
{
	[Fact]
	public void GetDocumentFilesFromFolder_Should_UseCustomExtensions()
	{
		String folderPath = Path.Combine(Path.GetTempPath(), $"Plugin.McpBridge.TextSearchStoreTests.{Guid.NewGuid():N}");
		Directory.CreateDirectory(folderPath);

		try
		{
			File.WriteAllText(Path.Combine(folderPath, "note.txt"), "txt");
			File.WriteAllText(Path.Combine(folderPath, "data.json"), "json");
			File.WriteAllText(Path.Combine(folderPath, "image.png"), "png");

			String[] result = TextSearchStore.GetDocumentFilesFromFolder(folderPath, new[] { ".json" }).ToArray();

			result.Should().ContainSingle();
			Path.GetFileName(result[0]).Should().Be("data.json");
		}
		finally
		{
			if(Directory.Exists(folderPath))
				Directory.Delete(folderPath, recursive: true);
		}
	}

	[Fact]
	public void AssertDocumentsInFolder_Should_RespectCustomExtensions()
	{
		String folderPath = Path.Combine(Path.GetTempPath(), $"Plugin.McpBridge.TextSearchStoreTests.{Guid.NewGuid():N}");
		Directory.CreateDirectory(folderPath);

		try
		{
			File.WriteAllText(Path.Combine(folderPath, "data.json"), "json");

			Action act = () => TextSearchStore.AssertDocumentsInFolder(folderPath, new[] { ".json" });

			act.Should().NotThrow();
		}
		finally
		{
			if(Directory.Exists(folderPath))
				Directory.Delete(folderPath, recursive: true);
		}
	}
}