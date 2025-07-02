using System.Collections.Generic;
using Blocks;

public class TankTreadsBlockGroup : BlockGroup
{
	public TankTreadsBlockGroup(List<Block> blocks)
		: base(blocks, "tank-treads")
	{
	}

	public override void Initialize()
	{
		for (int i = 0; i < blocks.Length; i++)
		{
			Block block = blocks[i];
			if (!(block is BlockTankTreadsWheel blockTankTreadsWheel))
			{
				BWLog.Error("All blocks in a tank treads group must be of tank treads wheel type");
			}
			else
			{
				blockTankTreadsWheel.SetBlockGroup(this);
			}
		}
		for (int j = 0; j < blocks.Length; j++)
		{
			Block block2 = blocks[j];
			BlockTankTreadsWheel blockTankTreadsWheel2 = block2 as BlockTankTreadsWheel;
			if (blockTankTreadsWheel2.IsMainBlockInGroup())
			{
				blockTankTreadsWheel2.CreateTreads(shapeOnly: false, parentIsBlock: true);
				break;
			}
		}
	}
}
