using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B4 RID: 180
	public class BlockMotorSlab2 : BlockAbstractMotor
	{
		// Token: 0x06000E05 RID: 3589 RVA: 0x0005F42F File Offset: 0x0005D82F
		public BlockMotorSlab2(List<List<Tile>> tiles) : base(tiles, "Motor Slab 2 Axle")
		{
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x0005F440 File Offset: 0x0005D840
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsTurningSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Return", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Return), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.Step", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsStepping), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Step), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.FreeSpin", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsFreeSpinning), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.TargetAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).TargetAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.SetPositiveAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetPositiveAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotorSlab2>("MotorSlab2.SetNegativeAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetNegativeAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("MotorSlab2.Turn", new object[]
			{
				-1f
			}), new string[]
			{
				"Motor Slab 2"
			});
		}
	}
}
