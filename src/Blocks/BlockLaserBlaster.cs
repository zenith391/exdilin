using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockLaserBlaster : BlockAbstractLaser
{
	public BlockLaserBlaster(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockLaserBlaster>("LaserBlaster.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockLaserBlaster>("LaserBlaster.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("LaserBlaster.Pulse", 4f), "Laser Blaster");
	}
}
