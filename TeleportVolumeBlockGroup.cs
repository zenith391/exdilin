using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000048 RID: 72
public class TeleportVolumeBlockGroup : BlockGroup
{
	// Token: 0x06000252 RID: 594 RVA: 0x0000D627 File Offset: 0x0000BA27
	public TeleportVolumeBlockGroup(List<Block> blocks) : base(blocks, "teleport-volume")
	{
	}

	// Token: 0x06000253 RID: 595 RVA: 0x0000D638 File Offset: 0x0000BA38
	public override void Initialize()
	{
		for (int i = 0; i < this.blocks.Length; i++)
		{
			this.blocks[i].SetBlockGroup(this);
		}
	}
}
