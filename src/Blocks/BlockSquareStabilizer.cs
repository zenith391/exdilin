using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockSquareStabilizer : BlockAbstractStabilizer
{
	public static Predicate predicateSquareStabilizerStabilize;

	public static Predicate predicateSquareStabilizerHold;

	public static Predicate predicateSquareStabilizerBurst;

	public BlockSquareStabilizer(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateSquareStabilizerStabilize = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.Stabilize", (Block b) => ((BlockAbstractStabilizer)b).IsStabilizing, (Block b) => ((BlockAbstractStabilizer)b).Stabilize, new Type[1] { typeof(float) });
		predicateSquareStabilizerHold = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.ControlPosition", (Block b) => ((BlockAbstractStabilizer)b).IsCloseToSomething, (Block b) => ((BlockAbstractStabilizer)b).ControlPosition, new Type[1] { typeof(float) });
		predicateSquareStabilizerBurst = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.Burst", (Block b) => ((BlockAbstractStabilizer)b).IsBursting, (Block b) => ((BlockAbstractStabilizer)b).Burst, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("SquareStabilizer.ControlPosition", 1f), "Stabilizer Square");
	}
}
