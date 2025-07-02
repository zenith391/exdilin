using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ReleaseGlueOnContactCommand : AbstractDetachCommand
{
	public ReleaseGlueOnContactCommand(Block block)
		: base(block.goT.position, Vector3.zero, 0f)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		for (int i = 0; i < block.chunk.blocks.Count; i++)
		{
			Block block2 = block.chunk.blocks[i];
			if (block2.gluedOnContact)
			{
				block2.gluedOnContact = false;
				hashSet.Add(block2);
			}
		}
		data = new DetachData
		{
			detachBlocks = hashSet,
			detachWithoutBreak = true,
			informExploded = false,
			forceDetachBlock = block
		};
		state = DetachState.COMPUTE_CHUNKS;
	}

	public override void Execute()
	{
		DetachState detachState = state;
		base.Execute();
		done = detachState == DetachState.APPLY_FORCES;
	}
}
