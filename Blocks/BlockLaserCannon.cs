using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009D RID: 157
	public class BlockLaserCannon : BlockAbstractLaser
	{
		// Token: 0x06000C7C RID: 3196 RVA: 0x000581CD File Offset: 0x000565CD
		public BlockLaserCannon(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x000581D8 File Offset: 0x000565D8
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserCannon>("LaserCannon.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserCannon>("LaserCannon.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserCannon.Beam", new object[0]), new string[]
			{
				"Laser Cannon"
			});
		}
	}
}
