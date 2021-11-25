using System;

// Token: 0x02000133 RID: 307
public class DelegateMultistepCommand : Command
{
	// Token: 0x0600142F RID: 5167 RVA: 0x0008D0AF File Offset: 0x0008B4AF
	public DelegateMultistepCommand(Action action, int steps)
	{
		this.action = action;
		this.steps = steps;
	}

	// Token: 0x06001430 RID: 5168 RVA: 0x0008D0CC File Offset: 0x0008B4CC
	public void SetSteps(int steps)
	{
		this.steps = steps;
		this.done = false;
	}

	// Token: 0x06001431 RID: 5169 RVA: 0x0008D0DC File Offset: 0x0008B4DC
	public override void Execute()
	{
		this.action();
		this.steps--;
		this.done = (this.steps <= 0);
	}

	// Token: 0x04000FD4 RID: 4052
	private Action action;

	// Token: 0x04000FD5 RID: 4053
	private int steps = 1;
}
