using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200008A RID: 138
	public class BlockEmitterCampfire : BlockAbstractParticles
	{
		// Token: 0x06000BA0 RID: 2976 RVA: 0x00053C0B File Offset: 0x0005200B
		public BlockEmitterCampfire(List<List<Tile>> tiles) : base(tiles, "Particles/Campfire FX", false, 0, "815 Fire Jet Loop")
		{
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x00053C20 File Offset: 0x00052020
		public new static void Register()
		{
			BlockEmitterCampfire.predicateCampfireShoot = PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleShoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).IsFiring), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleSpread", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleSpread), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("EmitterCampfire.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Campfire"
			});
		}

		// Token: 0x04000939 RID: 2361
		public static Predicate predicateCampfireShoot;
	}
}
