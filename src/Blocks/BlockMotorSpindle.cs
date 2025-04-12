using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B5 RID: 181
	public class BlockMotorSpindle : BlockAbstractMotor
	{
		// Token: 0x06000E11 RID: 3601 RVA: 0x0005F72F File Offset: 0x0005DB2F
		public BlockMotorSpindle(List<List<Tile>> tiles) : base(tiles, "Motor Spindle Axle")
		{
		}

		// Token: 0x06000E12 RID: 3602 RVA: 0x0005F740 File Offset: 0x0005DB40
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsTurningSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Return", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Return), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.Step", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsStepping), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Step), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.FreeSpin", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsFreeSpinning), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.TargetAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).TargetAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.SetPositiveAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetPositiveAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSpindle>("MotorSpindle.SetNegativeAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetNegativeAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MotorSpindle.Turn", new object[]
			{
				-1f
			}), new string[]
			{
				"Motor Spindle"
			});
		}
	}
}
