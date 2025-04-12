using System;

// Token: 0x02000137 RID: 311
public class DelegateCommand<S, T, U, V> : Command
{
	// Token: 0x06001438 RID: 5176 RVA: 0x0008D1BE File Offset: 0x0008B5BE
	public DelegateCommand(S data1, T data2, U data3, V data4, Action<S, T, U, V> action)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
		this.data4 = data4;
	}

	// Token: 0x06001439 RID: 5177 RVA: 0x0008D1EB File Offset: 0x0008B5EB
	public override void Execute()
	{
		base.Execute();
		this.action(this.data1, this.data2, this.data3, this.data4);
	}

	// Token: 0x04000FDF RID: 4063
	private Action<S, T, U, V> action;

	// Token: 0x04000FE0 RID: 4064
	private S data1;

	// Token: 0x04000FE1 RID: 4065
	private T data2;

	// Token: 0x04000FE2 RID: 4066
	private U data3;

	// Token: 0x04000FE3 RID: 4067
	private V data4;
}
