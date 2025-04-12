using System;

// Token: 0x0200012B RID: 299
public class DelayedCommand : Command
{
	// Token: 0x06001416 RID: 5142 RVA: 0x0008C490 File Offset: 0x0008A890
	public DelayedCommand(int delay)
	{
		this.delay = delay;
	}

	// Token: 0x06001417 RID: 5143 RVA: 0x0008C49F File Offset: 0x0008A89F
	public override void Reset()
	{
		base.Reset();
		this.counter = 0;
	}

	// Token: 0x06001418 RID: 5144 RVA: 0x0008C4AE File Offset: 0x0008A8AE
	public override void Execute()
	{
		this.counter++;
		if (this.counter > this.delay)
		{
			this.DelayedExecute();
		}
	}

	// Token: 0x06001419 RID: 5145 RVA: 0x0008C4D5 File Offset: 0x0008A8D5
	protected virtual void DelayedExecute()
	{
		this.done = true;
	}

	// Token: 0x04000FB4 RID: 4020
	protected int counter;

	// Token: 0x04000FB5 RID: 4021
	protected int delay;
}
