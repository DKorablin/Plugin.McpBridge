using System.ComponentModel;
using System.Runtime.Serialization;

namespace Plugin.McpBridge.Data;

/// <summary>Read-only provider configuration for the local stub client.</summary>
[DataContract]
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed record StubProviderDto : AiProviderDto, ICustomTypeDescriptor
{
	AttributeCollection ICustomTypeDescriptor.GetAttributes()
		=> TypeDescriptor.GetAttributes(this, true);

	String? ICustomTypeDescriptor.GetClassName()
		=> TypeDescriptor.GetClassName(this, true);

	String? ICustomTypeDescriptor.GetComponentName()
		=> TypeDescriptor.GetComponentName(this, true);

	TypeConverter ICustomTypeDescriptor.GetConverter()
		=> TypeDescriptor.GetConverter(this, true);

	EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent()
		=> TypeDescriptor.GetDefaultEvent(this, true);

	PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty()
		=> TypeDescriptor.GetDefaultProperty(this, true);

	Object? ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		=> TypeDescriptor.GetEditor(this, editorBaseType, true);

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		=> TypeDescriptor.GetEvents(this, true);

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes)
		=> TypeDescriptor.GetEvents(this, attributes, true);

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		=> ((ICustomTypeDescriptor)this).GetProperties(null);

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes)
	{
		PropertyDescriptorCollection original = TypeDescriptor.GetProperties(this, attributes, true);
		PropertyDescriptor[] readOnly = new PropertyDescriptor[original.Count];
		for(Int32 i = 0; i < original.Count; i++)
			readOnly[i] = original[i].Name == nameof(AiProviderDto.ProviderType)
				? original[i]
				: new ReadOnlyPropertyDescriptor(original[i]);
		return new PropertyDescriptorCollection(readOnly);
	}

	Object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd)
		=> this;

	private sealed class ReadOnlyPropertyDescriptor : PropertyDescriptor
	{
		private readonly PropertyDescriptor _inner;

		public ReadOnlyPropertyDescriptor(PropertyDescriptor inner) : base(inner)
			=> this._inner = inner;

		public override Type ComponentType => this._inner.ComponentType;
		public override Boolean IsReadOnly => true;
		public override Type PropertyType => this._inner.PropertyType;
		public override Boolean CanResetValue(Object component) => false;
		public override Object? GetValue(Object? component) => this._inner.GetValue(component);
		public override void ResetValue(Object component) { }
		public override void SetValue(Object component, Object? value) { }
		public override Boolean ShouldSerializeValue(Object component) => false;
	}

}