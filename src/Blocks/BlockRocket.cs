using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockRocket : BlockAbstractRocket
{
	public static Predicate predicateRocketFire;

	public BlockRocket(List<List<Tile>> tiles)
		: base(tiles, "Blocks/Rocket Flame", string.Empty)
	{
	}

	public new static void Register()
	{
		predicateRocketFire = PredicateRegistry.Add<BlockRocket>("Rocket.Fire", (Block b) => ((BlockAbstractRocket)b).IsFiring, (Block b) => ((BlockAbstractRocket)b).FireRocket, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockRocket>("Rocket.Smoke", (Block b) => ((BlockAbstractRocket)b).IsSmoking, (Block b) => ((BlockAbstractRocket)b).Smoke);
		Block.AddSimpleDefaultTiles(new GAF("Rocket.Fire", 2f), "Rocket");
	}
}
