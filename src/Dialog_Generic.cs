using System;

// Token: 0x020002E9 RID: 745
public class Dialog_Generic : UIDialogPanel
{
	// Token: 0x06002201 RID: 8705 RVA: 0x000FEB93 File Offset: 0x000FCF93
	public void DidTapNo()
	{
		this.doCloseDialog();
		if (this.noAction != null)
		{
			this.noAction();
		}
	}

	// Token: 0x06002202 RID: 8706 RVA: 0x000FEBB6 File Offset: 0x000FCFB6
	public void DidTapYes()
	{
		this.doCloseDialog();
		if (this.yesAction != null)
		{
			this.yesAction();
		}
	}

	// Token: 0x04001D0B RID: 7435
	public Action yesAction;

	// Token: 0x04001D0C RID: 7436
	public Action noAction;
}
