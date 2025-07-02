using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockBumblebeeGun : BlockAbstractLaser
{
	public BlockBumblebeeGun(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockBumblebeeGun>("BumblebeeGun.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockBumblebeeGun>("BumblebeeGun.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("BumblebeeGun.Pulse", 4f), "Bumblebee Gun");
	}
}
