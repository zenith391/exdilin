using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAntiGravityColumn : BlockAbstractAntiGravity
{
	public static Predicate predicateIncreaseLocalTorque;

	public static Predicate predicateIncreaseLocalVel;

	public static Predicate predicateHover;

	public static Predicate predicateAntigravityColumnAlign;

	public static Predicate predicateAntigravityColumnAlignTerrain;

	public static Predicate predicateAntigravityColumnStay;

	public static Predicate predicateAntigravityColumnLevitate;

	public static Predicate predicateAntigravityColumnBankTurn;

	public BlockAntiGravityColumn(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateAntigravityColumnLevitate = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		predicateAntigravityColumnAlign = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		predicateAntigravityColumnAlignTerrain = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		predicateAntigravityColumnStay = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		predicateIncreaseLocalTorque = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		predicateIncreaseLocalVel = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateAntigravityColumnBankTurn = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateHover = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		Block.AddSimpleDefaultTiles(new GAF("AntiGravityColumn.AlignInGravityFieldChunk", 1f), new GAF("AntiGravityColumn.IncreaseModelGravityInfluence", -1f), "Antigravity Column");
	}
}
