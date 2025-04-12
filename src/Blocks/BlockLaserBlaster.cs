using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200009C RID: 156
	public class BlockLaserBlaster : BlockAbstractLaser
	{
		// Token: 0x06000C75 RID: 3189 RVA: 0x0005805C File Offset: 0x0005645C
		public BlockLaserBlaster(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x00058068 File Offset: 0x00056468
		public new static void Register()
		{
			PredicateRegistry.Add<BlockLaserBlaster>("LaserBlaster.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockLaserBlaster>("LaserBlaster.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("LaserBlaster.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Laser Blaster"
			});
		}
	}
}
