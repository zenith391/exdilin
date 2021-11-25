using System;

// Token: 0x02000136 RID: 310
public class DelegateCommand<S, T, U> : Command
{
	// Token: 0x06001436 RID: 5174 RVA: 0x0008D174 File Offset: 0x0008B574
	public DelegateCommand(S data1, T data2, U data3, Action<S, T, U> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
	}

	// Token: 0x06001437 RID: 5175 RVA: 0x0008D199 File Offset: 0x0008B599
	public override void Execute()
	{
		base.Execute();
		this.action(this.data1, this.data2, this.data3);
	}

	// Token: 0x04000FDB RID: 4059
	private Action<S, T, U> action;

	// Token: 0x04000FDC RID: 4060
	private S data1;

	// Token: 0x04000FDD RID: 4061
	private T data2;

	// Token: 0x04000FDE RID: 4062
	private U data3;
}
