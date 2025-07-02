using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockSoundwaveGun : BlockAbstractLaser
{
	public BlockSoundwaveGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockSoundwaveGun>("SoundwaveGun.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockSoundwaveGun>("SoundwaveGun.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("SoundwaveGun.Pulse", 4f), "Soundwave Gun");
	}
}
