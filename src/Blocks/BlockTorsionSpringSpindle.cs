using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockTorsionSpringSpindle : BlockAbstractTorsionSpring
{
	public BlockTorsionSpringSpindle(List<List<Tile>> tiles)
		: base(tiles, "Torsion Spring Spindle Axle", 0f)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.Charge", null, (Block b) => ((BlockTorsionSpringSpindle)b).Charge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.StepCharge", null, (Block b) => ((BlockTorsionSpringSpindle)b).StepCharge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.ChargeGreaterThan", (Block b) => ((BlockTorsionSpringSpindle)b).ChargeGreaterThan, null, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.Release", null, (Block b) => ((BlockTorsionSpringSpindle)b).Release);
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.FreeSpin", null, (Block b) => ((BlockTorsionSpringSpindle)b).FreeSpin);
		PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.SetRigidity", null, (Block b) => ((BlockTorsionSpringSpindle)b).SetRigidity, new Type[1] { typeof(float) });
	}
}
