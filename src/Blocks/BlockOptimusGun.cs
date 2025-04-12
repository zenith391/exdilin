using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000BA RID: 186
	public class BlockOptimusGun : BlockAbstractLaser
	{
		// Token: 0x06000E65 RID: 3685 RVA: 0x00061AD4 File Offset: 0x0005FED4
		public BlockOptimusGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x00061AE0 File Offset: 0x0005FEE0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockOptimusGun>("OptimusGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockOptimusGun>("OptimusGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("OptimusGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Optimus Gun"
			});
		}
	}
}
