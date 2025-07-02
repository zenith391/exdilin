using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class SplitChunkCommand : AbstractDetachCommand
{
	public SplitChunkCommand(Block block)
		: base(block.goT.position, Vector3.zero, 0f)
	{
		HashSet<Block> detachBlocks = new HashSet<Block> { block };
		data = new DetachData
		{
			detachBlocks = detachBlocks,
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
