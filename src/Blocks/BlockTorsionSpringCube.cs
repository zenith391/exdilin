using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockTorsionSpringCube : BlockAbstractTorsionSpring
{
	public BlockTorsionSpringCube(List<List<Tile>> tiles)
		: base(tiles, "Torsion Spring Cube Axle", 90f)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.Charge", null, (Block b) => ((BlockTorsionSpringCube)b).Charge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.StepCharge", null, (Block b) => ((BlockTorsionSpringCube)b).StepCharge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.ChargeGreaterThan", (Block b) => ((BlockTorsionSpringCube)b).ChargeGreaterThan, null, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.Release", null, (Block b) => ((BlockTorsionSpringCube)b).Release);
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.FreeSpin", null, (Block b) => ((BlockTorsionSpringCube)b).FreeSpin);
		PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.SetRigidity", null, (Block b) => ((BlockTorsionSpringCube)b).SetRigidity, new Type[1] { typeof(float) });
	}
}
