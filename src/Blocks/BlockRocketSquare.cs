using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000C6 RID: 198
	public class BlockRocketSquare : BlockAbstractRocket
	{
		// Token: 0x06000F16 RID: 3862 RVA: 0x00065B20 File Offset: 0x00063F20
		public BlockRocketSquare(List<List<Tile>> tiles) : base(tiles, "Blocks/Rocket Flame", string.Empty)
		{
			this.setSmokeColor = true;
			this.smokeColorMeshIndex = 0;
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x00065B44 File Offset: 0x00063F44
		public new static void Register()
		{
			BlockRocketSquare.predicateRocketSquareFire = PredicateRegistry.Add<BlockRocketSquare>("SquareRocket.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFiring), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).FireRocket), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockRocketSquare>("SquareRocket.Smoke", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsSmoking), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Smoke), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("SquareRocket.Fire", new object[]
			{
				2f
			}), new string[]
			{
				"Rocket Square"
			});
		}

		// Token: 0x04000BB7 RID: 2999
		public static Predicate predicateRocketSquareFire;
	}
}
