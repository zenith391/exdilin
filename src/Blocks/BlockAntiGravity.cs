using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000076 RID: 118
	public class BlockAntiGravity : BlockAbstractAntiGravity
	{
		// Token: 0x06000A51 RID: 2641 RVA: 0x0004AA80 File Offset: 0x00048E80
		public BlockAntiGravity(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x0004AA8C File Offset: 0x00048E8C
		public new static void Register()
		{
			BlockAntiGravity.predicateAntigravityLevitate = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityAlign = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityAlignTerrain = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityStay = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalAngVelChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityIncreaseTorqueChunk = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.DPadIncreaseTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityIncreaseVelocityChunk = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.DPadIncreaseVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.IncreasePositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityAlignAlongMover = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockAntiGravity.predicateAntigravityBankTurn = PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAntiGravity>("AntiGravity.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("AntiGravity.AlignInGravityFieldChunk", new object[]
			{
				1f
			}), new GAF("AntiGravity.IncreaseModelGravityInfluence", new object[]
			{
				-1f
			}), new string[]
			{
				"Antigravity Pump",
				"Antigravity Cube"
			});
		}

		// Token: 0x04000808 RID: 2056
		public static Predicate predicateAntigravityIncreaseTorqueChunk;

		// Token: 0x04000809 RID: 2057
		public static Predicate predicateAntigravityIncreaseVelocityChunk;

		// Token: 0x0400080A RID: 2058
		public static Predicate predicateAntigravityAlignAlongMover;

		// Token: 0x0400080B RID: 2059
		public static Predicate predicateAntigravityAlign;

		// Token: 0x0400080C RID: 2060
		public static Predicate predicateAntigravityAlignTerrain;

		// Token: 0x0400080D RID: 2061
		public static Predicate predicateAntigravityStay;

		// Token: 0x0400080E RID: 2062
		public static Predicate predicateAntigravityLevitate;

		// Token: 0x0400080F RID: 2063
		public static Predicate predicateAntigravityBankTurn;
	}
}
