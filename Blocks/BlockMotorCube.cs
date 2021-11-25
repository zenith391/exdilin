using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B2 RID: 178
	public class BlockMotorCube : BlockAbstractMotor
	{
		// Token: 0x06000DED RID: 3565 RVA: 0x0005EE2F File Offset: 0x0005D22F
		public BlockMotorCube(List<List<Tile>> tiles) : base(tiles, "Motor Cube Axle")
		{
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x0005EE40 File Offset: 0x0005D240
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsTurningSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.Return", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Return), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.Step", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsStepping), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Step), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.FreeSpin", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsFreeSpinning), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.TargetAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).TargetAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.SetPositiveAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetPositiveAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorCube>("MotorCube.SetNegativeAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetNegativeAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MotorCube.Turn", new object[]
			{
				-1f
			}), new string[]
			{
				"Motor Cube"
			});
		}
	}
}
