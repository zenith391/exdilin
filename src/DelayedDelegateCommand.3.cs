using System;

// Token: 0x0200012E RID: 302
public class DelayedDelegateCommand<S, T> : DelayedCommand
{
	// Token: 0x0600141E RID: 5150 RVA: 0x0008CABF File Offset: 0x0008AEBF
	public DelayedDelegateCommand(S data1, T data2, Action<S, T> action, int delay) : base(delay)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
	}

	// Token: 0x0600141F RID: 5151 RVA: 0x0008CADE File Offset: 0x0008AEDE
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		this.action(this.data1, this.data2);
	}

	// Token: 0x04000FB9 RID: 4025
	private Action<S, T> action;

	// Token: 0x04000FBA RID: 4026
	private S data1;

	// Token: 0x04000FBB RID: 4027
	private T data2;
}
