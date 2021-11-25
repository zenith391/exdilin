using System;

// Token: 0x02000135 RID: 309
public class DelegateCommand<S, T> : Command
{
	// Token: 0x06001434 RID: 5172 RVA: 0x0008D138 File Offset: 0x0008B538
	public DelegateCommand(S data1, T data2, Action<S, T> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
	}

	// Token: 0x06001435 RID: 5173 RVA: 0x0008D155 File Offset: 0x0008B555
	public override void Execute()
	{
		base.Execute();
		this.action(this.data1, this.data2);
	}

	// Token: 0x04000FD8 RID: 4056
	private Action<S, T> action;

	// Token: 0x04000FD9 RID: 4057
	private S data1;

	// Token: 0x04000FDA RID: 4058
	private T data2;
}
