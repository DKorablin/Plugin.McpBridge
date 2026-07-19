namespace Plugin.McpBridge.UI;

internal class TargetWindowCtrl : ToolStripSplitButton
{
	private Cursor _hoverCursor;
	private readonly ImageList imageList;
	private Boolean _searching;

	private Cursor HoverCursor
	{
		get
		{
			if(this._hoverCursor == null)
			{
				MemoryStream stream = new MemoryStream(Properties.Resources.winfinder);
				this._hoverCursor = new Cursor(stream);
			}
			return this._hoverCursor;
		}
	}

	public event EventHandler<MouseEventArgs> Searching;
	public event EventHandler<MouseEventArgs> SearchFinished;
	public event EventHandler<MouseEventArgs> SearchCancelled;

	public TargetWindowCtrl()
	{
		this.imageList = new ImageList
		{
			ImageStream = global::Plugin.McpBridge.Properties.Resources.imageList_ImageStream,
			TransparentColor = System.Drawing.Color.Transparent
		};
		this.imageList.Images.SetKeyName(0, String.Empty);
		this.imageList.Images.SetKeyName(1, String.Empty);

		this.DisplayStyle = ToolStripItemDisplayStyle.Image;
		this.Image = global::Plugin.McpBridge.Properties.Resources.Icon1;
		this.ToolTipText = "Drag to pick a window, or use the dropdown to attach an open window";
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		switch(e.Button)
		{
		case MouseButtons.Left:
			if(!this._searching && this.ButtonBounds.Contains(e.Location))
				this.BeginSearch();
			break;
		case MouseButtons.Right:
			if(this._searching)
			{
				this.EndSearch();
				this.SearchCancelled?.Invoke(this, e);
			}
			break;
		}

		base.OnMouseDown(e);
	}

	private void BeginSearch()
	{
		this._searching = true;
		this.Owner.Cursor = this.HoverCursor;
		this.Image = this.imageList.Images[0];
		this.Owner.Capture = true;
		this.Owner.MouseMove += this.Owner_MouseMove;
		this.Owner.MouseUp += this.Owner_MouseUp;
	}

	private void EndSearch()
	{
		this._searching = false;
		this.Owner.MouseMove -= this.Owner_MouseMove;
		this.Owner.MouseUp -= this.Owner_MouseUp;
		this.Owner.Capture = false;
		this.Owner.Cursor = Cursors.Default;
		this.Image = this.imageList.Images[1];
	}

	private void Owner_MouseMove(Object sender, MouseEventArgs e)
	{
		if(this._searching)
			this.Searching?.Invoke(this, e);
	}

	private void Owner_MouseUp(Object sender, MouseEventArgs e)
	{
		if(!this._searching)
			return;

		this.EndSearch();
		this.SearchFinished?.Invoke(this, e);
	}
}