using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000046 RID: 70
public class TankTreadsBlockGroup : BlockGroup
{
	// Token: 0x0600024E RID: 590 RVA: 0x0000D53E File Offset: 0x0000B93E
	public TankTreadsBlockGroup(List<Block> blocks) : base(blocks, "tank-treads")
	{
	}

	// Token: 0x0600024F RID: 591 RVA: 0x0000D54C File Offset: 0x0000B94C
	public override void Initialize()
	{
		for (int i = 0; i < this.blocks.Length; i++)
		{
			Block block = this.blocks[i];
			BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
			if (blockTankTreadsWheel == null)
			{
				BWLog.Error("All blocks in a tank treads group must be of tank treads wheel type");
			}
			else
			{
				blockTankTreadsWheel.SetBlockGroup(this);
			}
		}
		for (int j = 0; j < this.blocks.Length; j++)
		{
			Block block2 = this.blocks[j];
			BlockTankTreadsWheel blockTankTreadsWheel2 = block2 as BlockTankTreadsWheel;
			if (blockTankTreadsWheel2.IsMainBlockInGroup())
			{
				blockTankTreadsWheel2.CreateTreads(false, true);
				break;
			}
		}
	}
}
