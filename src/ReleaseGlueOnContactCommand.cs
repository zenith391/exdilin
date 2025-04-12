using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000144 RID: 324
public class ReleaseGlueOnContactCommand : AbstractDetachCommand
{
	// Token: 0x0600146F RID: 5231 RVA: 0x0008FE78 File Offset: 0x0008E278
	public ReleaseGlueOnContactCommand(Block block) : base(block.goT.position, Vector3.zero, 0f)
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
		this.data = new DetachData
		{
			detachBlocks = hashSet,
			detachWithoutBreak = true,
			informExploded = false,
			forceDetachBlock = block
		};
		this.state = AbstractDetachCommand.DetachState.COMPUTE_CHUNKS;
	}

	// Token: 0x06001470 RID: 5232 RVA: 0x0008FF24 File Offset: 0x0008E324
	public override void Execute()
	{
		AbstractDetachCommand.DetachState state = this.state;
		base.Execute();
		this.done = (state == AbstractDetachCommand.DetachState.APPLY_FORCES);
	}
}
