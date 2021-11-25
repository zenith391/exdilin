using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000E4 RID: 228
	public class BlockTorsionSpringCube : BlockAbstractTorsionSpring
	{
		// Token: 0x0600110F RID: 4367 RVA: 0x00075D13 File Offset: 0x00074113
		public BlockTorsionSpringCube(List<List<Tile>> tiles) : base(tiles, "Torsion Spring Cube Axle", 90f)
		{
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x00075D28 File Offset: 0x00074128
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.Charge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringCube)b).Charge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.StepCharge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringCube)b).StepCharge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.ChargeGreaterThan", (Block b) => new PredicateSensorDelegate(((BlockTorsionSpringCube)b).ChargeGreaterThan), null, new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.Release", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringCube)b).Release), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.FreeSpin", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringCube)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringCube>("TorsionSpringCube.SetRigidity", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringCube)b).SetRigidity), new Type[]
			{
				typeof(float)
			}, null, null);
		}
	}
}
