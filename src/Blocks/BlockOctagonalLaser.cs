using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B9 RID: 185
	public class BlockOctagonalLaser : BlockAbstractLaser
	{
		// Token: 0x06000E5E RID: 3678 RVA: 0x00061973 File Offset: 0x0005FD73
		public BlockOctagonalLaser(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000E5F RID: 3679 RVA: 0x0006197C File Offset: 0x0005FD7C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockOctagonalLaser>("OctagonalLaser.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockOctagonalLaser>("OctagonalLaser.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("OctagonalLaser.Beam", new object[0]), new string[]
			{
				"Laser Octagonal"
			});
		}
	}
}
