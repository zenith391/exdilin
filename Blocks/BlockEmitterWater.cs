using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200008D RID: 141
	public class BlockEmitterWater : BlockAbstractParticles
	{
		// Token: 0x06000BAF RID: 2991 RVA: 0x0005406C File Offset: 0x0005246C
		public BlockEmitterWater(List<List<Tile>> tiles, bool shouldHide = false) : base(tiles, "Particles/Water Stream", shouldHide, 0, "815 Water Jet")
		{
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x00054084 File Offset: 0x00052484
		public new static void Register()
		{
			BlockEmitterWater.predicateWaterShoot = PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleShoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).IsFiring), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleSpread", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleSpread), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("EmitterWater.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Water Emitter Block"
			});
			Block.AddSimpleDefaultTiles(new GAF("Block.Fixed", new object[0]), new GAF("EmitterWater.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Water Volume Block"
			});
		}

		// Token: 0x04000945 RID: 2373
		public static Predicate predicateWaterShoot;
	}
}
