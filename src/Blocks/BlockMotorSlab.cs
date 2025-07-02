using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMotorSlab : BlockAbstractMotor
{
	public BlockMotorSlab(List<List<Tile>> tiles)
		: base(tiles, "Motor Slab Axle")
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Turn", (Block b) => ((BlockAbstractMotor)b).IsTurningSensor, (Block b) => ((BlockAbstractMotor)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Return", null, (Block b) => ((BlockAbstractMotor)b).Return, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Step", (Block b) => ((BlockAbstractMotor)b).IsStepping, (Block b) => ((BlockAbstractMotor)b).Step, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.FreeSpin", (Block b) => ((BlockAbstractMotor)b).IsFreeSpinning, (Block b) => ((BlockAbstractMotor)b).FreeSpin);
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.TargetAngle", null, (Block b) => ((BlockAbstractMotor)b).TargetAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.SetPositiveAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetPositiveAngleLimit, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.SetNegativeAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetNegativeAngleLimit, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("MotorSlab.Turn", -1f), "Motor Slab");
	}
}
