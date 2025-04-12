using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000B1 RID: 177
	public class BlockMotor : BlockAbstractMotor
	{
		// Token: 0x06000DE1 RID: 3553 RVA: 0x0005EB30 File Offset: 0x0005CF30
		public BlockMotor(List<List<Tile>> tiles) : base(tiles, "Motor Axle")
		{
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0005EB40 File Offset: 0x0005CF40
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMotor>("Motor.Turn", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsTurningSensor), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Turn), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.Return", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Return), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.Step", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsStepping), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).Step), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.FreeSpin", (Block b) => new PredicateSensorDelegate(((BlockAbstractMotor)b).IsFreeSpinning), (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.TargetAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).TargetAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.SetPositiveAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetPositiveAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockMotor>("Motor.SetNegativeAngleLimit", null, (Block b) => new PredicateActionDelegate(((BlockAbstractMotor)b).SetNegativeAngleLimit), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Motor.Turn", new object[]
			{
				-1f
			}), new string[]
			{
				"Motor"
			});
		}
	}
}
