using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000A6 RID: 166
	public class BlockMagnet : BlockAbstractMagnet
	{
		// Token: 0x06000CE7 RID: 3303 RVA: 0x0005A170 File Offset: 0x00058570
		public BlockMagnet(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x0005A17C File Offset: 0x0005857C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMagnet>("Magnet.InfluenceTag", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).InfluenceTag), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMagnet>("Magnet.InfluencePaint", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).InfluencePaint), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMagnet>("Magnet.InfluenceTexture", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).InfluenceTexture), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMagnet>("Magnet.SetForceMode", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).SetForceMode), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMagnet>("Magnet.SetDistance", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).SetDistance), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMagnet>("Magnet.SetDistanceNear", null, (Block b) => new PredicateActionDelegate(((BlockMagnet)b).SetDistanceNear), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Magnet.SetDistance", new object[]
			{
				50f
			}), new GAF("Magnet.InfluenceTexture", new object[]
			{
				-10f
			}), new string[]
			{
				"Magnet"
			});
		}
	}
}
