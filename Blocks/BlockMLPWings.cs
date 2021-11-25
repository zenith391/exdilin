using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000A5 RID: 165
	public class BlockMLPWings : BlockAbstractAntiGravityWing
	{
		// Token: 0x06000CD4 RID: 3284 RVA: 0x00059AFA File Offset: 0x00057EFA
		public BlockMLPWings(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x00059B04 File Offset: 0x00057F04
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreaseLocalAngVelChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.DPadIncreaseTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.DPadIncreaseVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.IncreasePositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMLPWings>("MLPWings.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("MLPWings.AlignInGravityFieldChunk", new object[]
			{
				1f
			}), new GAF("MLPWings.IncreaseModelGravityInfluence", new object[]
			{
				-1f
			}), new string[]
			{
				"MLP Wings"
			});
		}
	}
}
