using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x0200008B RID: 139
	public class BlockEmitterFire : BlockAbstractParticles
	{
		// Token: 0x06000BA5 RID: 2981 RVA: 0x00053D54 File Offset: 0x00052154
		public BlockEmitterFire(List<List<Tile>> tiles, bool shouldHide = false) : base(tiles, "Particles/Fire Stream", shouldHide, 0, "815 Fire Jet Loop")
		{
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x00053D6C File Offset: 0x0005216C
		public new static void Register()
		{
			BlockEmitterFire.predicateFireShoot = PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleShoot", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).IsFiring), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleSpread", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleSpread), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleAngle", null, (Block b) => new PredicateActionDelegate(((BlockAbstractParticles)b).ParticleAngle), new Type[]
			{
				typeof(float)
			}, null, null);
			Block.AddSimpleDefaultTiles(new GAF("EmitterFire.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Fire Emitter Block"
			});
			Block.AddSimpleDefaultTiles(new GAF("Block.Fixed", new object[0]), new GAF("EmitterFire.ParticleShoot", new object[]
			{
				1f
			}), new string[]
			{
				"Fire Volume Block"
			});
		}

		// Token: 0x0400093D RID: 2365
		public static Predicate predicateFireShoot;
	}
}
