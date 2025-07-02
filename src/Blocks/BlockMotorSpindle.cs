using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMotorSpindle : BlockAbstractMotor
{
	public BlockMotorSpindle(List<List<Tile>> tiles)
		: base(tiles, "Motor Spindle Axle")
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Turn", (Block b) => ((BlockAbstractMotor)b).IsTurningSensor, (Block b) => ((BlockAbstractMotor)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Return", null, (Block b) => ((BlockAbstractMotor)b).Return, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Step", (Block b) => ((BlockAbstractMotor)b).IsStepping, (Block b) => ((BlockAbstractMotor)b).Step, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.FreeSpin", (Block b) => ((BlockAbstractMotor)b).IsFreeSpinning, (Block b) => ((BlockAbstractMotor)b).FreeSpin);
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.TargetAngle", null, (Block b) => ((BlockAbstractMotor)b).TargetAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.SetPositiveAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetPositiveAngleLimit, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.SetNegativeAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetNegativeAngleLimit, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("MotorSpindle.Turn", -1f), "Motor Spindle");
	}
}
