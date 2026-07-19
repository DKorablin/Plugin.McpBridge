namespace Plugin.McpBridge.UI;

/// <summary>CheckedListBox that supports checking/unchecking a range of items by clicking and dragging across them, since the base control rejects any SelectionMode other than One.</summary>
internal class MultiCheckListBox : CheckedListBox
{
	private Boolean _dragChecked;
	private Int32 _dragLastIndex = -1;

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);

		Int32 index = this.IndexFromPoint(e.Location);
		if(index == ListBox.NoMatches)
			return;

		this._dragChecked = !this.GetItemChecked(index);
		this._dragLastIndex = index;
		this.SetItemChecked(index, this._dragChecked);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if(e.Button != MouseButtons.Left || this._dragLastIndex == ListBox.NoMatches)
			return;

		Int32 index = this.IndexFromPoint(e.Location);
		if(index == ListBox.NoMatches || index == this._dragLastIndex)
			return;

		Int32 step = index > this._dragLastIndex ? 1 : -1;
		for(Int32 i = this._dragLastIndex + step; ; i += step)
		{
			this.SetItemChecked(i, this._dragChecked);
			if(i == index)
				break;
		}

		this._dragLastIndex = index;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		this._dragLastIndex = ListBox.NoMatches;
	}
}
