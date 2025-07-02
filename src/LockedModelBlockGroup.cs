using System.Collections.Generic;
using Blocks;

public class LockedModelBlockGroup : BlockGroup
{
	public LockedModelBlockGroup(List<Block> blocks)
		: base(blocks, "locked-model")
	{
	}

	public override void Initialize()
	{
		Block[] array = blocks;
		foreach (Block block in array)
		{
			block.SetBlockGroup(this);
		}
	}
}
