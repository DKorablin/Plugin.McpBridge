using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Plugin.McpBridge.Data;
using Plugin.McpBridge.UI.PropertyGrid;
using Plugin.McpBridge.UI.PropertyGrid.Converters;

namespace Plugin.McpBridge
{
	/// <summary>Registers WinForms-only PropertyGrid editor/converter attributes for Core DTO/settings types at runtime, since those types live in the cross-platform McpBridge.Core assembly and cannot reference System.Windows.Forms directly. Attributes are merged onto PropertyDescriptors via PropertyAttributeInjector, so both Editor and TypeConverter resolution work exactly as if the attributes were declared inline.</summary>
	internal static class PropertyGridMetadata
	{
		private static Boolean _registered;

		public static void Register()
		{
			if(PropertyGridMetadata._registered)
				return;
			PropertyGridMetadata._registered = true;

			Register(typeof(Settings), new()
			{
				[nameof(Settings.AiProviders)] = new Attribute[] { new EditorAttribute(typeof(CollectionWithDescriptionEditor), typeof(UITypeEditor)) },
				[nameof(Settings.AiAgents)] = new Attribute[] { new EditorAttribute(typeof(CollectionWithDescriptionEditor), typeof(UITypeEditor)) },
				[nameof(Settings.WorkflowsDirectory)] = new Attribute[] { new EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor)) },
				[nameof(Settings.SessionStorageDirectory)] = new Attribute[] { new EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor)) },
				[nameof(Settings.SelectedAgentId)] = new Attribute[] { new TypeConverterAttribute(typeof(AiAgentIdConverter)) },
			});

			Register(typeof(AiAgentDto), new()
			{
				[nameof(AiAgentDto.SelectedProviderId)] = new Attribute[] { new TypeConverterAttribute(typeof(AiProviderIdConverter)) },
				[nameof(AiAgentDto.EmbeddingProviderId)] = new Attribute[] { new TypeConverterAttribute(typeof(EmbeddingProviderIdConverter)) },
				[nameof(AiAgentDto.AssistantSystemPrompt)] = new Attribute[] { new EditorAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor)) },
				[nameof(AiAgentDto.SkillsDirectory)] = new Attribute[] { new EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor)) },
				[nameof(AiAgentDto.RagSupportedExtensions)] = new Attribute[] { new EditorAttribute(typeof(System.ComponentModel.Design.CollectionEditor), typeof(UITypeEditor)) },
				[nameof(AiAgentDto.RagDirectory)] = new Attribute[] { new EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor)) },
				[nameof(AiAgentDto.ToolsPermission)] = new Attribute[] { new EditorAttribute(typeof(ToolsPermissionEditor), typeof(UITypeEditor)) },
				[nameof(AiAgentDto.PluginsPermission)] = new Attribute[] { new EditorAttribute(typeof(PluginsPermissionEditor), typeof(UITypeEditor)), new TypeConverterAttribute(typeof(PluginsPermissionConverter)) },
			});

			Register(typeof(AiProviderDto), new()
			{
				[nameof(AiProviderDto.Capabilities)] = new Attribute[] { new EditorAttribute(typeof(ColumnEditorTyped<ProviderCapabilities>), typeof(UITypeEditor)) },
			});

			Register(typeof(NetworkProviderDto), new()
			{
				[nameof(NetworkProviderDto.EvaluationCacheDirectory)] = new Attribute[] { new EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor)) },
			});

			Register(typeof(EmbeddingSettings), new()
			{
				[nameof(EmbeddingSettings.ModelId)] = new Attribute[] { new EditorAttribute(typeof(EmbeddingModelEditor), typeof(UITypeEditor)) },
			});
		}

		private static void Register(Type type, Dictionary<String, Attribute[]> propertyAttributes)
		{
			TypeDescriptionProvider parent = TypeDescriptor.GetProvider(type);
			TypeDescriptor.AddProvider(new PropertyAttributeInjector(parent, propertyAttributes), type);
		}
	}
}
