using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000D2 RID: 210
	public class BlockSquareStabilizer : BlockAbstractStabilizer
	{
		// Token: 0x06000FAA RID: 4010 RVA: 0x00069444 File Offset: 0x00067844
		public BlockSquareStabilizer(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000FAB RID: 4011 RVA: 0x00069450 File Offset: 0x00067850
		public new static void Register()
		{
			BlockSquareStabilizer.predicateSquareStabilizerStabilize = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.Stabilize", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsStabilizing), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).Stabilize), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockSquareStabilizer.predicateSquareStabilizerHold = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.ControlPosition", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsCloseToSomething), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).ControlPosition), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockSquareStabilizer.predicateSquareStabilizerBurst = PredicateRegistry.Add<BlockSquareStabilizer>("SquareStabilizer.Burst", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsBursting), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).Burst), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("SquareStabilizer.ControlPosition", new object[]
			{
				1f
			}), new string[]
			{
				"Stabilizer Square"
			});
		}

		// Token: 0x04000C34 RID: 3124
		public static Predicate predicateSquareStabilizerStabilize;

		// Token: 0x04000C35 RID: 3125
		public static Predicate predicateSquareStabilizerHold;

		// Token: 0x04000C36 RID: 3126
		public static Predicate predicateSquareStabilizerBurst;
	}
}
