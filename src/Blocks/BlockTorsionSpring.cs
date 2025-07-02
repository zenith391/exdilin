using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockTorsionSpring : BlockAbstractTorsionSpring
{
	public BlockTorsionSpring(List<List<Tile>> tiles, string axleName = "Torsion Spring Axle")
		: base(tiles, axleName, 0f)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.Charge", null, (Block b) => ((BlockTorsionSpring)b).Charge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.StepCharge", null, (Block b) => ((BlockTorsionSpring)b).StepCharge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.ChargeGreaterThan", (Block b) => ((BlockTorsionSpring)b).ChargeGreaterThan, null, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.Release", null, (Block b) => ((BlockTorsionSpring)b).Release);
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.FreeSpin", null, (Block b) => ((BlockTorsionSpring)b).FreeSpin);
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.SetSpringStiffness", null, (Block b) => ((BlockTorsionSpring)b).SetSpringStiffness, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.SetRigidity", null, (Block b) => ((BlockTorsionSpring)b).SetRigidity, new Type[1] { typeof(float) });
	}
}
