using System;

// Token: 0x02000146 RID: 326
public class UnlockDynamicLockPullCommand : Command
{
	// Token: 0x06001474 RID: 5236 RVA: 0x0008FFDC File Offset: 0x0008E3DC
	public override void Execute()
	{
		base.Execute();
		Blocksworld.dynamicLockPull = false;
	}
}
