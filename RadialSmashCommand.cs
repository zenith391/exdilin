using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000143 RID: 323
public class RadialSmashCommand : AbstractDetachCommand
{
	// Token: 0x0600146C RID: 5228 RVA: 0x0008FD70 File Offset: 0x0008E170
	public RadialSmashCommand(Block block, Vector3 velocity, float radius, HashSet<Block> blocksToExclude, bool detachWithForce) : base(block.goT.position, velocity, (radius >= 0.01f) ? radius : block.size.magnitude)
	{
		block.UpdateConnectedCache();
		HashSet<Block> onlyInclude = new HashSet<Block>((radius >= 0.01f) ? Block.connectedCache[block] : new List<Block>
		{
			block
		});
		this.data = new DetachData
		{
			onlyInclude = onlyInclude,
			informExploded = false,
			detachForceGiver = this,
			forceDetachBlock = ((radius >= 0.01f) ? null : block),
			detachWithoutForce = !detachWithForce
		};
		if (blocksToExclude != null)
		{
			this.data.blocksToExclude.UnionWith(blocksToExclude);
		}
	}

	// Token: 0x0600146D RID: 5229 RVA: 0x0008FE40 File Offset: 0x0008E240
	public override void Execute()
	{
		AbstractDetachCommand.DetachState state = this.state;
		base.Execute();
		this.done = (state == AbstractDetachCommand.DetachState.APPLY_FORCES);
	}

	// Token: 0x0600146E RID: 5230 RVA: 0x0008FE64 File Offset: 0x0008E264
	public override Vector3 GetForce(Vector3 position, float time)
	{
		return UnityEngine.Random.insideUnitSphere * 5f;
	}
}
