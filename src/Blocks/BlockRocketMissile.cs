using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000C4 RID: 196
	public class BlockRocketMissile : BlockAbstractRocket
	{
		// Token: 0x06000F0A RID: 3850 RVA: 0x00065864 File Offset: 0x00063C64
		public BlockRocketMissile(List<List<Tile>> tiles) : base(tiles, "Blocks/Rocket Flame", string.Empty)
		{
			this.setSmokeColor = true;
			this.smokeColorMeshIndex = 0;
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x00065888 File Offset: 0x00063C88
		public new static void Register()
		{
			PredicateRegistry.Add<BlockRocketMissile>("MissileRocket.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFiring), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).FireRocket), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockRocketMissile>("MissileRocket.Smoke", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsSmoking), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Smoke), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MissileRocket.Fire", new object[]
			{
				2f
			}), new string[]
			{
				"Missile A"
			});
		}
	}
}
