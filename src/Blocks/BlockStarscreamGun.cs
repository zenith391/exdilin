using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000D4 RID: 212
	public class BlockStarscreamGun : BlockAbstractLaser
	{
		// Token: 0x06000FC1 RID: 4033 RVA: 0x000699A6 File Offset: 0x00067DA6
		public BlockStarscreamGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x000699B0 File Offset: 0x00067DB0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockStarscreamGun>("StarscreamGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockStarscreamGun>("StarscreamGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("StarscreamGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Starscream Gun"
			});
		}
	}
}
