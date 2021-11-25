using System;

// Token: 0x02000134 RID: 308
public class DelegateCommand<T> : Command
{
	// Token: 0x06001432 RID: 5170 RVA: 0x0008D109 File Offset: 0x0008B509
	public DelegateCommand(T data, Action<T> action)
	{
		this.action = action;
		this.data = data;
	}

	// Token: 0x06001433 RID: 5171 RVA: 0x0008D11F File Offset: 0x0008B51F
	public override void Execute()
	{
		base.Execute();
		this.action(this.data);
	}

	// Token: 0x04000FD6 RID: 4054
	private Action<T> action;

	// Token: 0x04000FD7 RID: 4055
	private T data;
}
