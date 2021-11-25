using System;

// Token: 0x020002E4 RID: 740
public class UIHighlighter : HelpTextHighlighter
{
	// Token: 0x060021E0 RID: 8672 RVA: 0x000FE2E4 File Offset: 0x000FC6E4
	public UIHighlighter(TILE_BUTTON button)
	{
		this._target = new Target();
		this._target.TargetButton(button);
	}

	// Token: 0x060021E1 RID: 8673 RVA: 0x000FE303 File Offset: 0x000FC703
	public UIHighlighter(UIInputControl.ControlType control)
	{
		this._target = new Target();
		this._target.TargetControl(control);
	}

	// Token: 0x060021E2 RID: 8674 RVA: 0x000FE322 File Offset: 0x000FC722
	public override void Show()
	{
		this._target.Show(true);
	}

	// Token: 0x060021E3 RID: 8675 RVA: 0x000FE330 File Offset: 0x000FC730
	public override void Update()
	{
		this._target.Update();
	}

	// Token: 0x060021E4 RID: 8676 RVA: 0x000FE33D File Offset: 0x000FC73D
	public override void Destroy()
	{
		this._target.Destroy();
		this._target = null;
	}

	// Token: 0x04001CEF RID: 7407
	private Target _target;
}
