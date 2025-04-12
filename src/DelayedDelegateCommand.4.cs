using System;

// Token: 0x0200012F RID: 303
public class DelayedDelegateCommand<S, T, U, V> : DelayedCommand
{
	// Token: 0x06001420 RID: 5152 RVA: 0x0008CAFD File Offset: 0x0008AEFD
	public DelayedDelegateCommand(S data1, T data2, U data3, V data4, Action<S, T, U, V> action, int delay) : base(delay)
	{
		this.action = action;
		this.data1 = data1;
		this.data2 = data2;
		this.data3 = data3;
		this.data4 = data4;
	}

	// Token: 0x06001421 RID: 5153 RVA: 0x0008CB2C File Offset: 0x0008AF2C
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		this.action(this.data1, this.data2, this.data3, this.data4);
	}

	// Token: 0x04000FBC RID: 4028
	private Action<S, T, U, V> action;

	// Token: 0x04000FBD RID: 4029
	private S data1;

	// Token: 0x04000FBE RID: 4030
	private T data2;

	// Token: 0x04000FBF RID: 4031
	private U data3;

	// Token: 0x04000FC0 RID: 4032
	private V data4;
}
