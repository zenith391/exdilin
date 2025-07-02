using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class AbstractDetachCommand : Command
{
	public enum DetachState
	{
		GATHER_COLLIDERS,
		GATHER_BLOCKS,
		COMPUTE_CHUNKS,
		APPLY_FORCES
	}

	protected float maxRadius = 1f;

	protected Vector3 position;

	protected Vector3 velocity;

	protected DetachData data;

	protected int visualCounter;

	protected DetachState state;

	protected static HashSet<AbstractDetachCommand> commands = new HashSet<AbstractDetachCommand>();

	public HashSet<Block> hitByExplosion = new HashSet<Block>();

	public AbstractDetachCommand(Vector3 position, Vector3 velocity, float maxRadius)
	{
		this.position = position;
		this.velocity = velocity;
		this.maxRadius = maxRadius;
	}

	public static bool HitByExplosion(Block b)
	{
		if (commands.Count > 0)
		{
			foreach (AbstractDetachCommand command in commands)
			{
				if (command.hitByExplosion.Contains(b))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public virtual bool DetachBlock(Block block)
	{
		return true;
	}

	public virtual Vector3 GetForce(Vector3 position, float time)
	{
		return Vector3.zero;
	}

	public override void Execute()
	{
		if (visualCounter == 0)
		{
			commands.Add(this);
		}
		switch (state)
		{
		case DetachState.GATHER_COLLIDERS:
			data.GatherColliders(position, maxRadius);
			state = DetachState.GATHER_BLOCKS;
			break;
		case DetachState.GATHER_BLOCKS:
			data.GatherBlocks();
			state = DetachState.COMPUTE_CHUNKS;
			break;
		case DetachState.COMPUTE_CHUNKS:
			data.ComputeChunks();
			state = DetachState.APPLY_FORCES;
			break;
		case DetachState.APPLY_FORCES:
			data.ApplyForces();
			break;
		}
		visualCounter++;
		position += Blocksworld.fixedDeltaTime * velocity;
	}

	public override void Removed()
	{
		base.Removed();
		commands.Remove(this);
		hitByExplosion.Clear();
	}
}
