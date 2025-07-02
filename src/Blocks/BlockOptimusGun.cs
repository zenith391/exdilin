using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockOptimusGun : BlockAbstractLaser
{
	public BlockOptimusGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockOptimusGun>("OptimusGun.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockOptimusGun>("OptimusGun.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("OptimusGun.Pulse", 4f), "Optimus Gun");
	}
}
