using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000099 RID: 153
	public class BlockJetpack : BlockAbstractJetpack
	{
		// Token: 0x06000C49 RID: 3145 RVA: 0x00057119 File Offset: 0x00055519
		public BlockJetpack(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C4A RID: 3146 RVA: 0x00057124 File Offset: 0x00055524
		public new static void Register()
		{
			BlockJetpack.predicateJetpackLevitate = PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockJetpack.predicateJetpackAlign = PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalAngVelChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalAngularVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			BlockJetpack.predicateJetpackThrust = PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.DPadIncreaseTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseTorqueChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.DPadIncreaseVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).DPadIncreaseVelocityChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.IncreasePositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreasePositionInGravityFieldChunk), new Type[]
			{
				typeof(int),
				typeof(float),
				typeof(float),
				typeof(float)
			}, null, null);
			BlockJetpack.predicateJetpackBankTurn = PredicateRegistry.Add<BlockJetpack>("Jetpack.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockJetpack>("Jetpack.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			BlockJetpack.predicateJetpackSmoke = PredicateRegistry.Add<BlockJetpack>("Jetpack.PlayVFX", null, (Block b) => new PredicateActionDelegate(((BlockAbstractJetpack)b).EmitSmoke), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					new Tile(BlockJetpack.predicateJetpackBankTurn, new object[]
					{
						"L",
						5f
					}),
					new Tile(BlockJetpack.predicateJetpackAlign, new object[]
					{
						5f
					}),
					new Tile(Block.predicateCamFollow, new object[]
					{
						1f,
						0.92f,
						0f
					})
				},
				new List<Tile>
				{
					new Tile(Block.predicateDPadMoved, new object[]
					{
						"L"
					}),
					Block.ThenTile(),
					new Tile(BlockJetpack.predicateJetpackThrust, new object[]
					{
						new Vector3(0f, 0f, 1f),
						10f
					}),
					new Tile(BlockJetpack.predicateJetpackLevitate, new object[]
					{
						-2f
					}),
					new Tile(BlockJetpack.predicateJetpackSmoke, new object[]
					{
						"Jetpack Smoke",
						1f
					})
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["RAR Jet Pack"] = value;
			Block.defaultExtraTiles["SPY Jet Pack"] = value;
			Block.defaultExtraTiles["FUT Space EVA"] = value;
		}

		// Token: 0x040009B3 RID: 2483
		public static Predicate predicateJetpackLevitate;

		// Token: 0x040009B4 RID: 2484
		public static Predicate predicateJetpackAlign;

		// Token: 0x040009B5 RID: 2485
		public static Predicate predicateJetpackThrust;

		// Token: 0x040009B6 RID: 2486
		public static Predicate predicateJetpackSmoke;

		// Token: 0x040009B7 RID: 2487
		public static Predicate predicateJetpackBankTurn;
	}
}
