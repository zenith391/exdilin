using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockFairyWings : BlockAbstractAntiGravityWing
{
	public BlockFairyWings(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockFairyWings>("FairyWings.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		Block.AddSimpleDefaultTiles(new GAF("FairyWings.AlignInGravityFieldChunk", 1f), new GAF("FairyWings.IncreaseModelGravityInfluence", -1f), "Fairy Wings");
	}
}
