using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020002DD RID: 733
public class TutorialAction
{
	// Token: 0x060021C9 RID: 8649 RVA: 0x000F3EBB File Offset: 0x000F22BB
	public virtual bool Step()
	{
		return false;
	}

	// Token: 0x060021CA RID: 8650 RVA: 0x000F3EBE File Offset: 0x000F22BE
	public virtual void EnterContext()
	{
		this.executing = true;
	}

	// Token: 0x060021CB RID: 8651 RVA: 0x000F3EC7 File Offset: 0x000F22C7
	public virtual void LeaveContext()
	{
		this.executing = false;
		this.done = true;
	}

	// Token: 0x060021CC RID: 8652 RVA: 0x000F3ED7 File Offset: 0x000F22D7
	public virtual void Execute()
	{
	}

	// Token: 0x060021CD RID: 8653 RVA: 0x000F3ED9 File Offset: 0x000F22D9
	public override string ToString()
	{
		return string.Format("Tutorial action ctx: " + this.context, new object[0]);
	}

	// Token: 0x060021CE RID: 8654 RVA: 0x000F3EFB File Offset: 0x000F22FB
	public virtual bool CancelsAction(TutorialAction otherAction)
	{
		return false;
	}

	// Token: 0x04001CAE RID: 7342
	public TutorialActionContext context;

	// Token: 0x04001CAF RID: 7343
	public bool stopProgressUntilDone;

	// Token: 0x04001CB0 RID: 7344
	public bool done;

	// Token: 0x04001CB1 RID: 7345
	public bool executing;

	// Token: 0x04001CB2 RID: 7346
	public Block block;

	// Token: 0x04001CB3 RID: 7347
	public List<Tile> tileRow;

	// Token: 0x04001CB4 RID: 7348
	public Tile tileBefore;

	// Token: 0x04001CB5 RID: 7349
	public Tile tileAfter;
}
