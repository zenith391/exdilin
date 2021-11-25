using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000078 RID: 120
	public class BlockBatWing : BlockAbstractAntiGravityWing
	{
		// Token: 0x06000A71 RID: 2673 RVA: 0x0004B584 File Offset: 0x00049984
		public BlockBatWing(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x0004B590 File Offset: 0x00049990
		public new static void Register()
		{
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalAngVelChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.DPadIncreaseTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.DPadIncreaseVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.IncreasePositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockBatWing>("BatWing.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("BatWing.AlignInGravityFieldChunk", new object[]
			{
				1f
			}), new GAF("BatWing.IncreaseModelGravityInfluence", new object[]
			{
				-1f
			}), new string[]
			{
				"Bat Wing"
			});
		}
	}
}
