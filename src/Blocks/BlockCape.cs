using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockCape : BlockAbstractAntiGravityWing
{
	public BlockCape(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockCape>("Cape.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockCape>("Cape.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		List<GAF> list = new List<GAF>();
		list.Add(new GAF("Cape.AlignAlongDPadChunk", "L", 5f));
		list.Add(new GAF("Cape.AlignInGravityFieldChunk", 1f));
		list.Add(new GAF("Cape.IncreaseModelGravityInfluence", -1f));
		Block.AddSimpleDefaultTiles(list, "Cape", "BBG Cape");
	}
}
