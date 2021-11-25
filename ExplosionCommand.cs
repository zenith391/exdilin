using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200013C RID: 316
public class ExplosionCommand : AbstractDetachCommand
{
	// Token: 0x06001449 RID: 5193 RVA: 0x0008E824 File Offset: 0x0008CC24
	public ExplosionCommand(Vector3 position, Vector3 velocity, float maxRadius, float duration, HashSet<Block> blocksToExclude, string blockTag = "") : base(position, velocity, maxRadius)
	{
		this.visualEffectDuration = duration;
		this.data = new DetachData
		{
			informExploded = true,
			detachForceGiver = this,
			forceDuration = Mathf.Max(0.1f, duration * 0.5f),
			hitByExplosion = this.hitByExplosion,
			onlyBlocksWithTag = blockTag
		};
		if (blocksToExclude != null)
		{
			this.data.blocksToExclude.UnionWith(blocksToExclude);
		}
	}

	// Token: 0x0600144A RID: 5194 RVA: 0x0008E8AD File Offset: 0x0008CCAD
	public override void Execute()
	{
		base.Execute();
		this.done = (this.data.detachBlocks != null && this.VisualEffectDone());
	}

	// Token: 0x0600144B RID: 5195 RVA: 0x0008E8D4 File Offset: 0x0008CCD4
	protected virtual bool VisualEffectDone()
	{
		return (float)this.visualCounter * Blocksworld.fixedDeltaTime >= this.visualEffectDuration;
	}

	// Token: 0x04001003 RID: 4099
	protected float visualEffectDuration = 1f;
}
