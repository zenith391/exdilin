using System;

// Token: 0x020002EB RID: 747
public class Dialog_GenericTwoButton : UIDialogPanel
{
	// Token: 0x06002209 RID: 8713 RVA: 0x000FED74 File Offset: 0x000FD174
	public override void Init()
	{
		this.mainText.Init();
		this.buttonAText.Init();
		this.buttonBText.Init();
	}

	// Token: 0x0600220A RID: 8714 RVA: 0x000FED97 File Offset: 0x000FD197
	public void Setup(string mainTextStr, string buttonATextStr, string buttonBTextStr, Action buttonAAction, Action buttonBAction)
	{
		this.mainText.Set(mainTextStr);
		this.buttonAText.Set(buttonATextStr);
		this.buttonBText.Set(buttonBTextStr);
		this.onButtonA = buttonAAction;
		this.onButtonB = buttonBAction;
	}

	// Token: 0x0600220B RID: 8715 RVA: 0x000FEDCD File Offset: 0x000FD1CD
	public void DidTapButtonA()
	{
		this.doCloseDialog();
		if (this.onButtonA != null)
		{
			this.onButtonA();
		}
	}

	// Token: 0x0600220C RID: 8716 RVA: 0x000FEDF0 File Offset: 0x000FD1F0
	public void DidTapButtonB()
	{
		this.doCloseDialog();
		if (this.onButtonB != null)
		{
			this.onButtonB();
		}
	}

	// Token: 0x04001D13 RID: 7443
	public Action onButtonA;

	// Token: 0x04001D14 RID: 7444
	public Action onButtonB;

	// Token: 0x04001D15 RID: 7445
	public UIEditableText mainText;

	// Token: 0x04001D16 RID: 7446
	public UIEditableText buttonAText;

	// Token: 0x04001D17 RID: 7447
	public UIEditableText buttonBText;
}
