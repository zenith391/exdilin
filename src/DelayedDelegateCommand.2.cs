using System;

// Token: 0x0200012D RID: 301
public class DelayedDelegateCommand<T> : DelayedCommand
{
	// Token: 0x0600141C RID: 5148 RVA: 0x0008CA8F File Offset: 0x0008AE8F
	public DelayedDelegateCommand(T data, Action<T> action, int delay) : base(delay)
	{
		this.action = action;
		this.data = data;
	}

	// Token: 0x0600141D RID: 5149 RVA: 0x0008CAA6 File Offset: 0x0008AEA6
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		this.action(this.data);
	}

	// Token: 0x04000FB7 RID: 4023
	private Action<T> action;

	// Token: 0x04000FB8 RID: 4024
	private T data;
}
