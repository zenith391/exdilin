using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000077 RID: 119
	public class BlockAntiGravityColumn : BlockAbstractAntiGravity
	{
		// Token: 0x06000A64 RID: 2660 RVA: 0x0004B120 File Offset: 0x00049520
		public BlockAntiGravityColumn(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x0004B12C File Offset: 0x0004952C
		public new static void Register()
		{
			BlockAntiGravityColumn.predicateAntigravityColumnLevitate = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateAntigravityColumnAlign = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateAntigravityColumnAlignTerrain = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateAntigravityColumnStay = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateIncreaseLocalTorque = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateIncreaseLocalVel = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateAntigravityColumnBankTurn = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockAntiGravityColumn.predicateHover = PredicateRegistry.Add<BlockAntiGravityColumn>("AntiGravityColumn.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("AntiGravityColumn.AlignInGravityFieldChunk", new object[]
			{
				1f
			}), new GAF("AntiGravityColumn.IncreaseModelGravityInfluence", new object[]
			{
				-1f
			}), new string[]
			{
				"Antigravity Column"
			});
		}

		// Token: 0x04000821 RID: 2081
		public static Predicate predicateIncreaseLocalTorque;

		// Token: 0x04000822 RID: 2082
		public static Predicate predicateIncreaseLocalVel;

		// Token: 0x04000823 RID: 2083
		public static Predicate predicateHover;

		// Token: 0x04000824 RID: 2084
		public static Predicate predicateAntigravityColumnAlign;

		// Token: 0x04000825 RID: 2085
		public static Predicate predicateAntigravityColumnAlignTerrain;

		// Token: 0x04000826 RID: 2086
		public static Predicate predicateAntigravityColumnStay;

		// Token: 0x04000827 RID: 2087
		public static Predicate predicateAntigravityColumnLevitate;

		// Token: 0x04000828 RID: 2088
		public static Predicate predicateAntigravityColumnBankTurn;
	}
}
