using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000E5 RID: 229
	public class BlockTorsionSpringSlab : BlockAbstractTorsionSpring
	{
		// Token: 0x06001117 RID: 4375 RVA: 0x00075F1E File Offset: 0x0007431E
		public BlockTorsionSpringSlab(List<List<Tile>> tiles) : base(tiles, "Torsion Spring Slab Axle", 90f)
		{
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x00075F34 File Offset: 0x00074334
		public new static void Register()
		{
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.Charge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSlab)b).Charge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.StepCharge", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSlab)b).StepCharge), new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.ChargeGreaterThan", (Block b) => new PredicateSensorDelegate(((BlockTorsionSpringSlab)b).ChargeGreaterThan), null, new Type[]
			{
				typeof(float),
				typeof(bool)
			}, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.Release", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSlab)b).Release), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.FreeSpin", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSlab)b).FreeSpin), null, null, null);
			PredicateRegistry.Add<BlockTorsionSpringSlab>("TorsionSpringSlab.SetRigidity", null, (Block b) => new PredicateActionDelegate(((BlockTorsionSpringSlab)b).SetRigidity), new Type[]
			{
				typeof(float)
			}, null, null);
		}
	}
}
