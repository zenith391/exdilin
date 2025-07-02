using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockJetpack : BlockAbstractJetpack
{
	public static Predicate predicateJetpackLevitate;

	public static Predicate predicateJetpackAlign;

	public static Predicate predicateJetpackThrust;

	public static Predicate predicateJetpackSmoke;

	public static Predicate predicateJetpackBankTurn;

	public BlockJetpack(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateJetpackLevitate = PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		predicateJetpackAlign = PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalAngVelChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		predicateJetpackThrust = PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.DPadIncreaseTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.DPadIncreaseVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreasePositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk, new Type[4]
		{
			typeof(int),
			typeof(float),
			typeof(float),
			typeof(float)
		});
		predicateJetpackBankTurn = PredicateRegistry.Add<BlockJetpack>("Jetpack.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockJetpack>("Jetpack.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		predicateJetpackSmoke = PredicateRegistry.Add<BlockJetpack>("Jetpack.PlayVFX", null, (Block b) => ((BlockAbstractJetpack)b).EmitSmoke, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(predicateJetpackBankTurn, "L", 5f),
			new Tile(predicateJetpackAlign, 5f),
			new Tile(Block.predicateCamFollow, 1f, 0.92f, 0f)
		});
		list.Add(new List<Tile>
		{
			new Tile(Block.predicateDPadMoved, "L"),
			Block.ThenTile(),
			new Tile(predicateJetpackThrust, new Vector3(0f, 0f, 1f), 10f),
			new Tile(predicateJetpackLevitate, -2f),
			new Tile(predicateJetpackSmoke, "Jetpack Smoke", 1f)
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["RAR Jet Pack"] = value;
		Block.defaultExtraTiles["SPY Jet Pack"] = value;
		Block.defaultExtraTiles["FUT Space EVA"] = value;
	}
}
