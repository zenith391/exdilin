using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockJazzGun : BlockAbstractLaser
{
	public BlockJazzGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockJazzGun>("JazzGun.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockJazzGun>("JazzGun.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("JazzGun.Pulse", 4f), "Jazz Gun");
	}
}
