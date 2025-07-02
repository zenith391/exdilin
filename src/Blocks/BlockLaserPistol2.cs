using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockLaserPistol2 : BlockAbstractLaser
{
	public BlockLaserPistol2(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockLaserPistol2>("LaserPistol2.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockLaserPistol2>("LaserPistol2.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("LaserPistol2.Pulse", 4f), "Laser Pistol2", "BBG Ray Gun", "FUT Space Gun");
	}
}
