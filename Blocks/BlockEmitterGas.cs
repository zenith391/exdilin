using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200008C RID: 140
	public class BlockEmitterGas : BlockAbstractParticles
	{
		// Token: 0x06000BAA RID: 2986 RVA: 0x00053EE0 File Offset: 0x000522E0
		public BlockEmitterGas(List<List<Tile>> tiles, bool shouldHide = false) : base(tiles, "Particles/Gas Stream", shouldHide, 0, "815 Jet Loop")
		{
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x00053EF8 File Offset: 0x000522F8
		public new static void Register()
		{
			BlockEmitterGas.predicateGasShoot = PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleShoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).IsFiring), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleSpread", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleSpread), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("EmitterGas.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Gas Emitter Block"
			});
			Block.AddSimpleDefaultTiles(new GAF("Block.Fixed", new object[0]), new GAF("EmitterGas.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Gas Volume Block"
			});
		}

		// Token: 0x04000941 RID: 2369
		public static Predicate predicateGasShoot;
	}
}
