using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000145 RID: 325
public class SplitChunkCommand : AbstractDetachCommand
{
	// Token: 0x06001471 RID: 5233 RVA: 0x0008FF48 File Offset: 0x0008E348
	public SplitChunkCommand(Block block) : base(block.goT.position, Vector3.zero, 0f)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		hashSet.Add(block);
		this.data = new DetachData
		{
			detachBlocks = hashSet,
			detachWithoutBreak = true,
			informExploded = false,
			forceDetachBlock = block
		};
		this.state = AbstractDetachCommand.DetachState.COMPUTE_CHUNKS;
	}

	// Token: 0x06001472 RID: 5234 RVA: 0x0008FFB0 File Offset: 0x0008E3B0
	public override void Execute()
	{
		AbstractDetachCommand.DetachState state = this.state;
		base.Execute();
		this.done = (state == AbstractDetachCommand.DetachState.APPLY_FORCES);
	}
}
