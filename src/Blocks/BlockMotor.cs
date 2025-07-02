using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMotor : BlockAbstractMotor
{
	public BlockMotor(List<List<Tile>> tiles)
		: base(tiles, "Motor Axle")
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMotor>("Motor.Turn", (Block b) => ((BlockAbstractMotor)b).IsTurningSensor, (Block b) => ((BlockAbstractMotor)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotor>("Motor.Return", null, (Block b) => ((BlockAbstractMotor)b).Return, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotor>("Motor.Step", (Block b) => ((BlockAbstractMotor)b).IsStepping, (Block b) => ((BlockAbstractMotor)b).Step, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotor>("Motor.FreeSpin", (Block b) => ((BlockAbstractMotor)b).IsFreeSpinning, (Block b) => ((BlockAbstractMotor)b).FreeSpin);
		PredicateRegistry.Add<BlockMotor>("Motor.TargetAngle", null, (Block b) => ((BlockAbstractMotor)b).TargetAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotor>("Motor.SetPositiveAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetPositiveAngleLimit, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotor>("Motor.SetNegativeAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetNegativeAngleLimit, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("Motor.Turn", -1f), "Motor");
	}
}
