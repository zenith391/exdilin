using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000C3 RID: 195
	public class BlockRocket : BlockAbstractRocket
	{
		// Token: 0x06000F04 RID: 3844 RVA: 0x00065714 File Offset: 0x00063B14
		public BlockRocket(List<List<Tile>> tiles) : base(tiles, "Blocks/Rocket Flame", string.Empty)
		{
		}

		// Token: 0x06000F05 RID: 3845 RVA: 0x00065728 File Offset: 0x00063B28
		public new static void Register()
		{
			BlockRocket.predicateRocketFire = PredicateRegistry.Add<BlockRocket>("Rocket.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFiring), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).FireRocket), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockRocket>("Rocket.Smoke", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsSmoking), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Smoke), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Rocket.Fire", new object[]
			{
				2f
			}), new string[]
			{
				"Rocket"
			});
		}

		// Token: 0x04000BA9 RID: 2985
		public static Predicate predicateRocketFire;
	}
}
