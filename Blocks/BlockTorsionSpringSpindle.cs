using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000E6 RID: 230
	public class BlockTorsionSpringSpindle : BlockAbstractTorsionSpring
	{
		// Token: 0x0600111F RID: 4383 RVA: 0x0007612A File Offset: 0x0007452A
		public BlockTorsionSpringSpindle(List<List<Tile>> tiles) : base(tiles, "Torsion Spring Spindle Axle", 0f)
		{
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x00076140 File Offset: 0x00074540
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.Charge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSpindle)b).Charge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.StepCharge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSpindle)b).StepCharge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.ChargeGreaterThan", (Block b) => new PredicateSensorDelegate(((BlockTorsionSpringSpindle)b).ChargeGreaterThan), null, new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.Release", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSpindle)b).Release), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.FreeSpin", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSpindle)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSpindle>("TorsionSpringSpindle.SetRigidity", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSpindle)b).SetRigidity), new Type[]
			{
				typeof(float)
			}, null, null);
		}
	}
}
