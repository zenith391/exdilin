using System;

// Token: 0x0200012C RID: 300
public class DelayedDelegateCommand : DelayedCommand
{
	// Token: 0x0600141A RID: 5146 RVA: 0x0008CA6C File Offset: 0x0008AE6C
	public DelayedDelegateCommand(Action action, int delay) : base(delay)
	{
		this.action = action;
	}

	// Token: 0x0600141B RID: 5147 RVA: 0x0008CA7C File Offset: 0x0008AE7C
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		this.action();
	}

	// Token: 0x04000FB6 RID: 4022
	private Action action;
}
