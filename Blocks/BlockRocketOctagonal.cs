using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000C5 RID: 197
	public class BlockRocketOctagonal : BlockAbstractRocket
	{
		// Token: 0x06000F10 RID: 3856 RVA: 0x000659C0 File Offset: 0x00063DC0
		public BlockRocketOctagonal(List<List<Tile>> tiles) : base(tiles, "Blocks/Rocket Flame", string.Empty)
		{
			this.setSmokeColor = true;
			this.smokeColorMeshIndex = 0;
		}

		// Token: 0x06000F11 RID: 3857 RVA: 0x000659E4 File Offset: 0x00063DE4
		public new static void Register()
		{
			BlockRocketOctagonal.predicateRocketOctagonalFire = PredicateRegistry.Add<BlockRocketOctagonal>("OctagonalRocket.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFiring), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).FireRocket), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockRocketOctagonal>("OctagonalRocket.Smoke", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsSmoking), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Smoke), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("OctagonalRocket.Fire", new object[]
			{
				2f
			}), new string[]
			{
				"Rocket Octagonal"
			});
		}

		// Token: 0x04000BB2 RID: 2994
		public static Predicate predicateRocketOctagonalFire;
	}
}
