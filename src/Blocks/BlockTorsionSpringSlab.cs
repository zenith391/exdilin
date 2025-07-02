using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockTorsionSpringSlab : BlockAbstractTorsionSpring
{
	public BlockTorsionSpringSlab(List<List<Tile>> tiles)
		: base(tiles, "Torsion Spring Slab Axle", 90f)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.Charge", null, (Block b) => ((BlockTorsionSpringSlab)b).Charge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.StepCharge", null, (Block b) => ((BlockTorsionSpringSlab)b).StepCharge, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.ChargeGreaterThan", (Block b) => ((BlockTorsionSpringSlab)b).ChargeGreaterThan, null, new Type[2]
		{
			typeof(float),
			typeof(bool)
		});
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.Release", null, (Block b) => ((BlockTorsionSpringSlab)b).Release);
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.FreeSpin", null, (Block b) => ((BlockTorsionSpringSlab)b).FreeSpin);
		PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.SetRigidity", null, (Block b) => ((BlockTorsionSpringSlab)b).SetRigidity, new Type[1] { typeof(float) });
	}
}
