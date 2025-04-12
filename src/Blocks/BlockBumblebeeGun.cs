using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200007E RID: 126
	public class BlockBumblebeeGun : BlockAbstractLaser
	{
		// Token: 0x06000AD9 RID: 2777 RVA: 0x0004D9C6 File Offset: 0x0004BDC6
		public BlockBumblebeeGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0004D9D0 File Offset: 0x0004BDD0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockBumblebeeGun>("BumblebeeGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockBumblebeeGun>("BumblebeeGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("BumblebeeGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Bumblebee Gun"
			});
		}
	}
}
