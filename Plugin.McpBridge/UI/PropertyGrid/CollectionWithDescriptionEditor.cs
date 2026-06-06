using System.ComponentModel.Design;

namespace Plugin.McpBridge.UI.PropertyGrid;

/// <summary>Collection editor for AiAgentDto lists that enables the description panel on the embedded property grid.</summary>
internal sealed class CollectionWithDescriptionEditor : CollectionEditor
{
	public CollectionWithDescriptionEditor(Type type) : base(type) { }

	protected override CollectionForm CreateCollectionForm()
	{
		CollectionForm form = base.CreateCollectionForm();
		form.Load += (sender, e) => SetHelpVisible((Control)sender!);
		return form;
	}

	private static void SetHelpVisible(Control parent)
	{
		foreach(Control ctrl in parent.Controls)
		{
			if(ctrl is System.Windows.Forms.PropertyGrid grid)
				grid.HelpVisible = true;
			else
				SetHelpVisible(ctrl);
		}
	}
}