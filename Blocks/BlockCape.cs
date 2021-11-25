using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200007F RID: 127
	public class BlockCape : BlockAbstractAntiGravityWing
	{
		// Token: 0x06000AE0 RID: 2784 RVA: 0x0004DB35 File Offset: 0x0004BF35
		public BlockCape(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0004DB40 File Offset: 0x0004BF40
		public new static void Register()
		{
			PredicateRegistry.Add<BlockCape>("Cape.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalAngVelChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.DPadIncreaseTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.DPadIncreaseVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.IncreasePositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockCape>("Cape.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			Block.AddSimpleDefaultTiles(new List<GAF>
			{
				new GAF("Cape.AlignAlongDPadChunk", new object[]
				{
					"L",
					5f
				}),
				new GAF("Cape.AlignInGravityFieldChunk", new object[]
				{
					1f
				}),
				new GAF("Cape.IncreaseModelGravityInfluence", new object[]
				{
					-1f
				})
			}, new string[]
			{
				"Cape",
				"BBG Cape"
			});
		}
	}
}
