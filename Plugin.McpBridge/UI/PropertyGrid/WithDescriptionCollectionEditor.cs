using System.ComponentModel.Design;

namespace Plugin.McpBridge.UI.PropertyGrid;

/// <summary>Collection editor for AI providers with the description panel enabled.</summary>
internal class WithDescriptionCollectionEditor : CollectionEditor
{
	public WithDescriptionCollectionEditor(Type type) : base(type) { }

	protected override CollectionForm CreateCollectionForm()
	{
		CollectionForm form = base.CreateCollectionForm();
		form.Load += (Object? s, EventArgs e) => EnableHelpPanel(form);
		return form;
	}

	private static void EnableHelpPanel(Control parent)
	{
		foreach(Control control in parent.Controls)
		{
			if(control is System.Windows.Forms.PropertyGrid grid)
			{
				grid.HelpVisible = true;
				return;
			}
			EnableHelpPanel(control);
		}
	}
}