using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockRocketSquare : BlockAbstractRocket
{
	public static Predicate predicateRocketSquareFire;

	public BlockRocketSquare(List<List<Tile>> tiles)
		: base(tiles, "Blocks/Rocket Flame", string.Empty)
	{
		setSmokeColor = true;
		smokeColorMeshIndex = 0;
	}

	public new static void Register()
	{
		predicateRocketSquareFire = PredicateRegistry.Add<BlockRocketSquare>("SquareRocket.Fire", (Block b) => ((BlockAbstractRocket)b).IsFiring, (Block b) => ((BlockAbstractRocket)b).FireRocket, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockRocketSquare>("SquareRocket.Smoke", (Block b) => ((BlockAbstractRocket)b).IsSmoking, (Block b) => ((BlockAbstractRocket)b).Smoke);
		Block.AddSimpleDefaultTiles(new GAF("SquareRocket.Fire", 2f), "Rocket Square");
	}
}
