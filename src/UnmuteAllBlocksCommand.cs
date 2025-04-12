using System;

// Token: 0x02000149 RID: 329
public class UnmuteAllBlocksCommand : DelayedCommand
{
	// Token: 0x0600147E RID: 5246 RVA: 0x00090260 File Offset: 0x0008E660
	public UnmuteAllBlocksCommand() : base(2)
	{
	}

	// Token: 0x0600147F RID: 5247 RVA: 0x00090269 File Offset: 0x0008E669
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		Sound.UnmuteAllBlocks();
	}
}
