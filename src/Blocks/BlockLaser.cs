using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009B RID: 155
	public class BlockLaser : BlockAbstractLaser
	{
		// Token: 0x06000C6E RID: 3182 RVA: 0x00057EF8 File Offset: 0x000562F8
		public BlockLaser(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C6F RID: 3183 RVA: 0x00057F04 File Offset: 0x00056304
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaser>("Laser.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaser>("Laser.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Laser.Beam", new object[0]), new string[]
			{
				"Laser"
			});
		}
	}
}
