using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockWiserWing : BlockAbstractAntiGravityWing
{
	public BlockWiserWing(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockWiserWing>("WiserWing.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		Block.AddSimpleDefaultTiles(new GAF("WiserWing.AlignInGravityFieldChunk", 1f), new GAF("WiserWing.IncreaseModelGravityInfluence", -1f), "Wiser Wing");
	}
}
