using System.ComponentModel;

namespace Plugin.McpBridge
{
	/// <summary>Merges extra attributes (Editor, TypeConverter, etc.) onto named properties of a type at runtime, so the type itself never has to reference those attribute types. Unlike TypeDescriptor.AddAttributes(Type, ...), which only affects class-level attributes, this rebuilds each targeted PropertyDescriptor via TypeDescriptor.CreateProperty so both Converter and GetEditor resolution pick up the merged attributes.</summary>
	internal sealed class PropertyAttributeInjector : TypeDescriptionProvider
	{
		private readonly Dictionary<String, Attribute[]> _propertyAttributes;

		public PropertyAttributeInjector(TypeDescriptionProvider parent, Dictionary<String, Attribute[]> propertyAttributes)
			: base(parent)
			=> this._propertyAttributes = propertyAttributes;

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, Object? instance)
			=> new Descriptor(base.GetTypeDescriptor(objectType, instance), this._propertyAttributes);

		private sealed class Descriptor : CustomTypeDescriptor
		{
			private readonly Dictionary<String, Attribute[]> _propertyAttributes;

			public Descriptor(ICustomTypeDescriptor parent, Dictionary<String, Attribute[]> propertyAttributes)
				: base(parent)
				=> this._propertyAttributes = propertyAttributes;

			public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
				=> this.Merge(base.GetProperties(attributes));

			public override PropertyDescriptorCollection GetProperties()
				=> this.Merge(base.GetProperties());

			private PropertyDescriptorCollection Merge(PropertyDescriptorCollection original)
			{
				PropertyDescriptor[] result = new PropertyDescriptor[original.Count];
				for(Int32 i = 0; i < original.Count; i++)
				{
					PropertyDescriptor prop = original[i];
					result[i] = this._propertyAttributes.TryGetValue(prop.Name, out Attribute[]? extra)
						? TypeDescriptor.CreateProperty(prop.ComponentType, prop, extra)
						: prop;
				}
				return new PropertyDescriptorCollection(result);
			}
		}
	}
}
