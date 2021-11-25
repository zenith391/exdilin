using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000A0 RID: 160
	public class BlockLaserPistol2 : BlockAbstractLaser
	{
		// Token: 0x06000C91 RID: 3217 RVA: 0x00058611 File Offset: 0x00056A11
		public BlockLaserPistol2(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0005861C File Offset: 0x00056A1C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserPistol2>("LaserPistol2.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserPistol2>("LaserPistol2.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserPistol2.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Laser Pistol2",
				"BBG Ray Gun",
				"FUT Space Gun"
			});
		}
	}
}
