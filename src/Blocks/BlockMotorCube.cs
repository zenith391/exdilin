using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMotorCube : BlockAbstractMotor
{
	public BlockMotorCube(List<List<Tile>> tiles)
		: base(tiles, "Motor Cube Axle")
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.Turn", (Block b) => ((BlockAbstractMotor)b).IsTurningSensor, (Block b) => ((BlockAbstractMotor)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.Return", null, (Block b) => ((BlockAbstractMotor)b).Return, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.Step", (Block b) => ((BlockAbstractMotor)b).IsStepping, (Block b) => ((BlockAbstractMotor)b).Step, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.FreeSpin", (Block b) => ((BlockAbstractMotor)b).IsFreeSpinning, (Block b) => ((BlockAbstractMotor)b).FreeSpin);
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.TargetAngle", null, (Block b) => ((BlockAbstractMotor)b).TargetAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.SetPositiveAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetPositiveAngleLimit, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorCube>("MotorCube.SetNegativeAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetNegativeAngleLimit, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("MotorCube.Turn", -1f), "Motor Cube");
	}
}
