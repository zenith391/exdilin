using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020000D3 RID: 211
	public class BlockStabilizer : BlockAbstractStabilizer
	{
		// Token: 0x06000FB2 RID: 4018 RVA: 0x00069619 File Offset: 0x00067A19
		public BlockStabilizer(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000FB3 RID: 4019 RVA: 0x00069624 File Offset: 0x00067A24
		public new static void Register()
		{
			BlockStabilizer.predicateStabilizerStabilize = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Stabilize", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsStabilizing), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).Stabilize), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockStabilizer.predicateStabilizerHold = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.ControlPosition", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsCloseToSomething), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).ControlPosition), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockStabilizer.predicateStabilizerBurst = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Burst", (Block b) => new PredicateSensorDelegate(((BlockAbstractStabilizer)b).IsBursting), (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).Burst), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.StabilizePlane", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).StabilizePlane), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.ControlZeroAngVel", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).ControlZeroAngVel), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Boost", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).BoostStabilizer), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.IncreaseAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).IncreaseAngle), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.DecreaseAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).DecreaseAngle), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.IncreasePosition", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).IncreasePosition), null, null, null);
			PredicateRegistry.Add<BlockStabilizer>("Stabilizer.DecreasePosition", null, (Block b) => new PredicateActionDelegate(((BlockAbstractStabilizer)b).DecreasePosition), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("Stabilizer.ControlPosition", new object[]
			{
				1f
			}), new string[]
			{
				"Stabilizer"
			});
		}

		// Token: 0x04000C3D RID: 3133
		public static Predicate predicateStabilizerStabilize;

		// Token: 0x04000C3E RID: 3134
		public static Predicate predicateStabilizerHold;

		// Token: 0x04000C3F RID: 3135
		public static Predicate predicateStabilizerBurst;
	}
}
