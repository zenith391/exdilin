using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000A9 RID: 169
	public class BlockMiniGun : BlockAbstractLaser
	{
		// Token: 0x06000D62 RID: 3426 RVA: 0x0005C7B1 File Offset: 0x0005ABB1
		public BlockMiniGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x0005C7BC File Offset: 0x0005ABBC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMiniGun>("MiniGun.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsFiringProjectile), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).FireProjectile), null, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MiniGun.Fire", new object[0]), new string[]
			{
				"Minigun"
			});
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x0005C860 File Offset: 0x0005AC60
		public override bool CanFireLaser()
		{
			return false;
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x0005C863 File Offset: 0x0005AC63
		public override bool CanFireProjectiles()
		{
			return true;
		}
	}
}
