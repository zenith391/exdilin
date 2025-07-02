using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class RadialSmashCommand : AbstractDetachCommand
{
	public RadialSmashCommand(Block block, Vector3 velocity, float radius, HashSet<Block> blocksToExclude, bool detachWithForce)
		: base(block.goT.position, velocity, (radius >= 0.01f) ? radius : block.size.magnitude)
	{
		block.UpdateConnectedCache();
		HashSet<Block> onlyInclude = new HashSet<Block>((radius >= 0.01f) ? Block.connectedCache[block] : new List<Block> { block });
		data = new DetachData
		{
			onlyInclude = onlyInclude,
			informExploded = false,
			detachForceGiver = this,
			forceDetachBlock = ((radius >= 0.01f) ? null : block),
			detachWithoutForce = !detachWithForce
		};
		if (blocksToExclude != null)
		{
			data.blocksToExclude.UnionWith(blocksToExclude);
		}
	}

	public override void Execute()
	{
		DetachState detachState = state;
		base.Execute();
		done = detachState == DetachState.APPLY_FORCES;
	}

	public override Vector3 GetForce(Vector3 position, float time)
	{
		return Random.insideUnitSphere * 5f;
	}
}
