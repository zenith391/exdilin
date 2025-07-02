using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockRocketOctagonal : BlockAbstractRocket
{
	public static Predicate predicateRocketOctagonalFire;

	public BlockRocketOctagonal(List<List<Tile>> tiles)
		: base(tiles, "Blocks/Rocket Flame", string.Empty)
	{
		setSmokeColor = true;
		smokeColorMeshIndex = 0;
	}

	public new static void Register()
	{
		predicateRocketOctagonalFire = PredicateRegistry.Add<BlockRocketOctagonal>("OctagonalRocket.Fire", (Block b) => ((BlockAbstractRocket)b).IsFiring, (Block b) => ((BlockAbstractRocket)b).FireRocket, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockRocketOctagonal>("OctagonalRocket.Smoke", (Block b) => ((BlockAbstractRocket)b).IsSmoking, (Block b) => ((BlockAbstractRocket)b).Smoke);
		Block.AddSimpleDefaultTiles(new GAF("OctagonalRocket.Fire", 2f), "Rocket Octagonal");
	}
}
