using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x02000097 RID: 151
	public class BlockJazzGun : BlockAbstractLaser
	{
		// Token: 0x06000C33 RID: 3123 RVA: 0x00056BF7 File Offset: 0x00054FF7
		public BlockJazzGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x00056C00 File Offset: 0x00055000
		public new static void Register()
		{
			PredicateRegistry.Add<BlockJazzGun>("JazzGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockJazzGun>("JazzGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("JazzGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Jazz Gun"
			});
		}
	}
}
