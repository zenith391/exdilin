using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009E RID: 158
	public class BlockLaserMiniGun : BlockAbstractLaser
	{
		// Token: 0x06000C83 RID: 3203 RVA: 0x00058330 File Offset: 0x00056730
		public BlockLaserMiniGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x0005833C File Offset: 0x0005673C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserMiniGun>("LaserMiniGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserMiniGun>("LaserMiniGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserMiniGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Laser Minigun"
			});
		}
	}
}
