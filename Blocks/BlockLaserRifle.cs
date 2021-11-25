using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000A1 RID: 161
	public class BlockLaserRifle : BlockAbstractLaser
	{
		// Token: 0x06000C98 RID: 3224 RVA: 0x00058791 File Offset: 0x00056B91
		public BlockLaserRifle(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x0005879C File Offset: 0x00056B9C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserRifle>("LaserRifle.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserRifle>("LaserRifle.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserRifle.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Laser Rifle"
			});
		}
	}
}
