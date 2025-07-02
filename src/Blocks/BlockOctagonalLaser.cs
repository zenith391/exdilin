using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockOctagonalLaser : BlockAbstractLaser
{
	public BlockOctagonalLaser(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockOctagonalLaser>("OctagonalLaser.Beam", (Block b) => ((BlockAbstractLaser)b).IsBeaming, (Block b) => ((BlockAbstractLaser)b).Beam);
		PredicateRegistry.Add<BlockOctagonalLaser>("OctagonalLaser.Pulse", (Block b) => ((BlockAbstractLaser)b).IsPulsing, (Block b) => ((BlockAbstractLaser)b).Pulse, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockAbstractLaser>("BlockAbstractLaser.Fired", (Block b) => b.IsFiredAsWeapon);
		Block.AddSimpleDefaultTiles(new GAF("OctagonalLaser.Beam"), "Laser Octagonal");
	}
}
