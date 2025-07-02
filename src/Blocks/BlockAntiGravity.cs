using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAntiGravity : BlockAbstractAntiGravity
{
	public static Predicate predicateAntigravityIncreaseTorqueChunk;

	public static Predicate predicateAntigravityIncreaseVelocityChunk;

	public static Predicate predicateAntigravityAlignAlongMover;

	public static Predicate predicateAntigravityAlign;

	public static Predicate predicateAntigravityAlignTerrain;

	public static Predicate predicateAntigravityStay;

	public static Predicate predicateAntigravityLevitate;

	public static Predicate predicateAntigravityBankTurn;

	public BlockAntiGravity(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateAntigravityLevitate = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		predicateAntigravityAlign = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		predicateAntigravityAlignTerrain = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		predicateAntigravityStay = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		predicateAntigravityIncreaseTorqueChunk = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateAntigravityIncreaseVelocityChunk = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		predicateAntigravityAlignAlongMover = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateAntigravityBankTurn = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		Block.AddSimpleDefaultTiles(new GAF("AntiGravity.AlignInGravityFieldChunk", 1f), new GAF("AntiGravity.IncreaseModelGravityInfluence", -1f), "Antigravity Pump", "Antigravity Cube");
	}
}
