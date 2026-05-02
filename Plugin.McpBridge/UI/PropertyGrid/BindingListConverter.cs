using System.ComponentModel;

namespace Plugin.McpBridge.UI.PropertyGrid;

/// <summary>Expands a BindingList&lt;T&gt; as indexed child properties inside a PropertyGrid.</summary>
internal sealed class BindingListConverter<T> : CollectionConverter
{
	public override Boolean GetPropertiesSupported(ITypeDescriptorContext? context) => true;

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, Object value, Attribute[]? attributes)
	{
		if(value is not IList<T> list)
			return base.GetProperties(context, value, attributes);

		PropertyDescriptor[] descriptors = new PropertyDescriptor[list.Count];
		for(Int32 i = 0; i < list.Count; i++)
			descriptors[i] = new IndexedItemDescriptor(i);

		return new PropertyDescriptorCollection(descriptors);
	}

	private sealed class IndexedItemDescriptor : SimplePropertyDescriptor
	{
		private readonly Int32 _index;

		public IndexedItemDescriptor(Int32 index)
			: base(typeof(IList<T>), $"[{index}]", typeof(T))
			=> this._index = index;

		public override Object? GetValue(Object? component)
			=> component is IList<T> list && this._index < list.Count ? list[this._index] : null;

		public override void SetValue(Object? component, Object? value)
		{
			if(component is IList<T> list && value is T typed)
				list[this._index] = typed;
		}
	}
}