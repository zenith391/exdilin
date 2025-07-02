using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ExplosionCommand : AbstractDetachCommand
{
	protected float visualEffectDuration = 1f;

	public ExplosionCommand(Vector3 position, Vector3 velocity, float maxRadius, float duration, HashSet<Block> blocksToExclude, string blockTag = "")
		: base(position, velocity, maxRadius)
	{
		visualEffectDuration = duration;
		data = new DetachData
		{
			informExploded = true,
			detachForceGiver = this,
			forceDuration = Mathf.Max(0.1f, duration * 0.5f),
			hitByExplosion = hitByExplosion,
			onlyBlocksWithTag = blockTag
		};
		if (blocksToExclude != null)
		{
			data.blocksToExclude.UnionWith(blocksToExclude);
		}
	}

	public override void Execute()
	{
		base.Execute();
		done = data.detachBlocks != null && VisualEffectDone();
	}

	protected virtual bool VisualEffectDone()
	{
		return (float)visualCounter * Blocksworld.fixedDeltaTime >= visualEffectDuration;
	}
}
