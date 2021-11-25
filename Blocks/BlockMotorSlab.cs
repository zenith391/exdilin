using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B3 RID: 179
	public class BlockMotorSlab : BlockAbstractMotor
	{
		// Token: 0x06000DF9 RID: 3577 RVA: 0x0005F12F File Offset: 0x0005D52F
		public BlockMotorSlab(List<List<Tile>> tiles) : base(tiles, "Motor Slab Axle")
		{
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x0005F140 File Offset: 0x0005D540
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsTurningSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Return", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Return), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.Step", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsStepping), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Step), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.FreeSpin", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsFreeSpinning), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.TargetAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).TargetAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.SetPositiveAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetPositiveAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab>("MotorSlab.SetNegativeAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetNegativeAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MotorSlab.Turn", new object[]
			{
				-1f
			}), new string[]
			{
				"Motor Slab"
			});
		}
	}
}
