using System.Collections.Generic;
using Blocks;

public class TeleportVolumeBlockGroup : BlockGroup
{
	public TeleportVolumeBlockGroup(List<Block> blocks)
		: base(blocks, "teleport-volume")
	{
	}

	public override void Initialize()
	{
		for (int i = 0; i < blocks.Length; i++)
		{
			blocks[i].SetBlockGroup(this);
		}
	}
}
