using System;

// Token: 0x02000132 RID: 306
public class DelegateCommand : Command
{
	// Token: 0x0600142D RID: 5165 RVA: 0x0008D08C File Offset: 0x0008B48C
	public DelegateCommand(Action<DelegateCommand> action)
	{
		this.action = action;
	}

	// Token: 0x0600142E RID: 5166 RVA: 0x0008D09B File Offset: 0x0008B49B
	public override void Execute()
	{
		base.Execute();
		this.action(this);
	}

	// Token: 0x04000FD3 RID: 4051
	private Action<DelegateCommand> action;
}
