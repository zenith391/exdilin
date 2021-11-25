using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000E3 RID: 227
	public class BlockTorsionSpring : BlockAbstractTorsionSpring
	{
		// Token: 0x06001106 RID: 4358 RVA: 0x00075ABD File Offset: 0x00073EBD
		public BlockTorsionSpring(List<List<Tile>> tiles, string axleName = "Torsion Spring Axle") : base(tiles, axleName, 0f)
		{
		}

		// Token: 0x06001107 RID: 4359 RVA: 0x00075ACC File Offset: 0x00073ECC
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.Charge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).Charge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.StepCharge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).StepCharge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.ChargeGreaterThan", (Block b) => new PredicateSensorDelegate(((BlockTorsionSpring)b).ChargeGreaterThan), null, new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.Release", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).Release), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.FreeSpin", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.SetSpringStiffness", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).SetSpringStiffness), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpring>("TorsionSpring.SetRigidity", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpring)b).SetRigidity), new Type[]
			{
				typeof(float)
			}, null, null);
		}
	}
}
