using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockBatWing : BlockAbstractAntiGravityWing
{
	public BlockBatWing(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockBatWing>("BatWing.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		Block.AddSimpleDefaultTiles(new GAF("BatWing.AlignInGravityFieldChunk", 1f), new GAF("BatWing.IncreaseModelGravityInfluence", -1f), "Bat Wing");
	}
}
