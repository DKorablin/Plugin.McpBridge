using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Plugin.McpBridge.UI.PropertyGrid;

internal class EmbeddingModelEditor : UITypeEditor
{
	private IWindowsFormsEditorService? _editorService;

	private IEnumerable<KeyValuePair<String, String>> GetValues()
		=> new Dictionary<String, String>
			{
				{ "text-embedding-3-large", "text-embedding-3-large" },
				{ "text-embedding-3-small", "text-embedding-3-small" },
				{ "text-embedding-ada-002", "text-embedding-ada-002" },
				{ "voyage-3-large", "voyage-3-large" },
				{ "voyage-3", "voyage-3" },
				{ "embed-v4.0", "embed-v4.0" },
				{ "text-embedding-004", "text-embedding-004" },
			};

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		=> UITypeEditorEditStyle.DropDown;

	public override Object EditValue(ITypeDescriptorContext? context, IServiceProvider provider, Object? value)
	{
		this._editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

		// use a list box
		ListBox lb = new ListBox
		{
			SelectionMode = SelectionMode.One,
			DisplayMember = "Key",
			ValueMember = "Value",
		};
		lb.SelectedValueChanged += (sender, e) => this._editorService.CloseDropDown();// close the drop down as soon as something is clicked

		//context.Instance
		foreach(KeyValuePair<String, String> item in this.GetValues())
		{
			Int32 index = lb.Items.Add(item);
			if(item.Value.Equals(value))
				lb.SelectedIndex = index;
		}

		// show this model stuff
		this._editorService.DropDownControl(lb);
		return lb.SelectedItem == null
			? value // no selection, return the passed-in value as is
			: ((KeyValuePair<String, String>)lb.SelectedItem).Value;
	}
}