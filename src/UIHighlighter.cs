public class UIHighlighter : HelpTextHighlighter
{
	private Target _target;

	public UIHighlighter(TILE_BUTTON button)
	{
		_target = new Target();
		_target.TargetButton(button);
	}

	public UIHighlighter(UIInputControl.ControlType control)
	{
		_target = new Target();
		_target.TargetControl(control);
	}

	public override void Show()
	{
		_target.Show(show: true);
	}

	public override void Update()
	{
		_target.Update();
	}

	public override void Destroy()
	{
		_target.Destroy();
		_target = null;
	}
}
