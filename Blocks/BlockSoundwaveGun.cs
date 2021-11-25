using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000CF RID: 207
	public class BlockSoundwaveGun : BlockAbstractLaser
	{
		// Token: 0x06000F7E RID: 3966 RVA: 0x000682AA File Offset: 0x000666AA
		public BlockSoundwaveGun(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000F7F RID: 3967 RVA: 0x000682B4 File Offset: 0x000666B4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockSoundwaveGun>("SoundwaveGun.Beam", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsBeaming), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Beam), null, null, null);
			PredicateRegistry.Add<BlockSoundwaveGun>("SoundwaveGun.Pulse", (Block b) => new PredicateSensorDelegate(((BlockAbstractLaser)b).IsPulsing), (Block b) => new PredicateActionDelegate(((BlockAbstractLaser)b).Pulse), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => new PredicateSensorDelegate(b.IsFiredAsWeapon), null, null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("SoundwaveGun.Pulse", new object[]
			{
				4f
			}), new string[]
			{
				"Soundwave Gun"
			});
		}
	}
}
