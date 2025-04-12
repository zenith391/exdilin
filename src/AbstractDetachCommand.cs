using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000122 RID: 290
public class AbstractDetachCommand : Command
{
	// Token: 0x060013EB RID: 5099 RVA: 0x0008B575 File Offset: 0x00089975
	public AbstractDetachCommand(Vector3 position, Vector3 velocity, float maxRadius)
	{
		this.position = position;
		this.velocity = velocity;
		this.maxRadius = maxRadius;
	}

	// Token: 0x060013EC RID: 5100 RVA: 0x0008B5A8 File Offset: 0x000899A8
	public static bool HitByExplosion(Block b)
	{
		if (AbstractDetachCommand.commands.Count > 0)
		{
			foreach (AbstractDetachCommand abstractDetachCommand in AbstractDetachCommand.commands)
			{
				if (abstractDetachCommand.hitByExplosion.Contains(b))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	// Token: 0x060013ED RID: 5101 RVA: 0x0008B628 File Offset: 0x00089A28
	public virtual bool DetachBlock(Block block)
	{
		return true;
	}

	// Token: 0x060013EE RID: 5102 RVA: 0x0008B62B File Offset: 0x00089A2B
	public virtual Vector3 GetForce(Vector3 position, float time)
	{
		return Vector3.zero;
	}

	// Token: 0x060013EF RID: 5103 RVA: 0x0008B634 File Offset: 0x00089A34
	public override void Execute()
	{
		if (this.visualCounter == 0)
		{
			AbstractDetachCommand.commands.Add(this);
		}
		switch (this.state)
		{
		case AbstractDetachCommand.DetachState.GATHER_COLLIDERS:
			this.data.GatherColliders(this.position, this.maxRadius);
			this.state = AbstractDetachCommand.DetachState.GATHER_BLOCKS;
			break;
		case AbstractDetachCommand.DetachState.GATHER_BLOCKS:
			this.data.GatherBlocks();
			this.state = AbstractDetachCommand.DetachState.COMPUTE_CHUNKS;
			break;
		case AbstractDetachCommand.DetachState.COMPUTE_CHUNKS:
			this.data.ComputeChunks();
			this.state = AbstractDetachCommand.DetachState.APPLY_FORCES;
			break;
		case AbstractDetachCommand.DetachState.APPLY_FORCES:
			this.data.ApplyForces();
			break;
		}
		this.visualCounter++;
		this.position += Blocksworld.fixedDeltaTime * this.velocity;
	}

	// Token: 0x060013F0 RID: 5104 RVA: 0x0008B70A File Offset: 0x00089B0A
	public override void Removed()
	{
		base.Removed();
		AbstractDetachCommand.commands.Remove(this);
		this.hitByExplosion.Clear();
	}

	// Token: 0x04000F92 RID: 3986
	protected float maxRadius = 1f;

	// Token: 0x04000F93 RID: 3987
	protected Vector3 position;

	// Token: 0x04000F94 RID: 3988
	protected Vector3 velocity;

	// Token: 0x04000F95 RID: 3989
	protected DetachData data;

	// Token: 0x04000F96 RID: 3990
	protected int visualCounter;

	// Token: 0x04000F97 RID: 3991
	protected AbstractDetachCommand.DetachState state;

	// Token: 0x04000F98 RID: 3992
	protected static HashSet<AbstractDetachCommand> commands = new HashSet<AbstractDetachCommand>();

	// Token: 0x04000F99 RID: 3993
	public HashSet<Block> hitByExplosion = new HashSet<Block>();

	// Token: 0x02000123 RID: 291
	public enum DetachState
	{
		// Token: 0x04000F9B RID: 3995
		GATHER_COLLIDERS,
		// Token: 0x04000F9C RID: 3996
		GATHER_BLOCKS,
		// Token: 0x04000F9D RID: 3997
		COMPUTE_CHUNKS,
		// Token: 0x04000F9E RID: 3998
		APPLY_FORCES
	}
}
