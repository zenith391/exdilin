using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockEmitterFire : BlockAbstractParticles
{
	public static Predicate predicateFireShoot;

	public BlockEmitterFire(List<List<Tile>> tiles, bool shouldHide = false)
		: base(tiles, "Particles/Fire Stream", shouldHide, 0, "815 Fire Jet Loop")
	{
	}

	public new static void Register()
	{
		predicateFireShoot = PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleShoot", null, (Block b) => ((BlockAbstractParticles)b).IsFiring, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleSpread", null, (Block b) => ((BlockAbstractParticles)b).ParticleSpread, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterFire>("EmitterFire.ParticleAngle", null, (Block b) => ((BlockAbstractParticles)b).ParticleAngle, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("EmitterFire.ParticleShoot", 1f), "Fire Emitter Block");
		Block.AddSimpleDefaultTiles(new GAF("Block.Fixed"), new GAF("EmitterFire.ParticleShoot", 1f), "Fire Volume Block");
	}
}
