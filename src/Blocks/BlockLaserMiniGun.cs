using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockLaserMiniGun : BlockAbstractLaser
{
	public BlockLaserMiniGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockLaserMiniGun>("LaserMiniGun.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockLaserMiniGun>("LaserMiniGun.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("LaserMiniGun.Pulse", 4f), "Laser Minigun");
	}
}
