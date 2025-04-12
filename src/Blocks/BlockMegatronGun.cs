using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000A8 RID: 168
	public class BlockMegatronGun : BlockAbstractLaser
	{
		// Token: 0x06000D5B RID: 3419 RVA: 0x0005C643 File Offset: 0x0005AA43
		public BlockMegatronGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x0005C64C File Offset: 0x0005AA4C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMegatronGun>("MegatronGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockMegatronGun>("MegatronGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MegatronGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Megatron Gun"
			});
		}
	}
}
