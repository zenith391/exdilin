using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x02000093 RID: 147
	public class BlockHandCannon : BlockAbstractLaser
	{
		// Token: 0x06000C1C RID: 3100 RVA: 0x00056567 File Offset: 0x00054967
		public BlockHandCannon(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x00056570 File Offset: 0x00054970
		public new static void Register()
		{
			PredicateRegistry.Add<BlockHandCannon>("HandCannon.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockHandCannon>("HandCannon.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("HandCannon.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Hand Cannon"
			});
		}
	}
}
