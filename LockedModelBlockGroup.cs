using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000047 RID: 71
public class LockedModelBlockGroup : BlockGroup
{
	// Token: 0x06000250 RID: 592 RVA: 0x0000D5E4 File Offset: 0x0000B9E4
	public LockedModelBlockGroup(List<Block> blocks) : base(blocks, "locked-model")
	{
	}

	// Token: 0x06000251 RID: 593 RVA: 0x0000D5F4 File Offset: 0x0000B9F4
	public override void Initialize()
	{
		foreach (Block block in this.blocks)
		{
			block.SetBlockGroup(this);
		}
	}
}
