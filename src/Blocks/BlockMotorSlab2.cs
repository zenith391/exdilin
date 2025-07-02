using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMotorSlab2 : BlockAbstractMotor
{
	public BlockMotorSlab2(List<List<Tile>> tiles)
		: base(tiles, "Motor Slab 2 Axle")
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Turn", (Block b) => ((BlockAbstractMotor)b).IsTurningSensor, (Block b) => ((BlockAbstractMotor)b).Turn, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Return", null, (Block b) => ((BlockAbstractMotor)b).Return, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Step", (Block b) => ((BlockAbstractMotor)b).IsStepping, (Block b) => ((BlockAbstractMotor)b).Step, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.FreeSpin", (Block b) => ((BlockAbstractMotor)b).IsFreeSpinning, (Block b) => ((BlockAbstractMotor)b).FreeSpin);
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.TargetAngle", null, (Block b) => ((BlockAbstractMotor)b).TargetAngle, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.SetPositiveAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetPositiveAngleLimit, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.SetNegativeAngleLimit", null, (Block b) => ((BlockAbstractMotor)b).SetNegativeAngleLimit, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("MotorSlab2.Turn", -1f), "Motor Slab 2");
	}
}
