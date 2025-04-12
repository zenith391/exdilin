using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009F RID: 159
	public class BlockLaserPistol : BlockAbstractLaser
	{
		// Token: 0x06000C8A RID: 3210 RVA: 0x000584A1 File Offset: 0x000568A1
		public BlockLaserPistol(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x000584AC File Offset: 0x000568AC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserPistol>("LaserPistol.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserPistol>("LaserPistol.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserPistol.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Laser Pistol"
			});
		}
	}
}
