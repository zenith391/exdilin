using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockEmitterCampfire : BlockAbstractParticles
{
	public static Predicate predicateCampfireShoot;

	public BlockEmitterCampfire(List<List<Tile>> tiles)
		: base(tiles, "Particles/Campfire FX", shouldHide: false, 0, "815 Fire Jet Loop")
	{
	}

	public new static void Register()
	{
		predicateCampfireShoot = PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleShoot", null, (Block b) => ((BlockAbstractParticles)b).IsFiring, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleSpread", null, (Block b) => ((BlockAbstractParticles)b).ParticleSpread, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterCampfire>("EmitterCampfire.ParticleAngle", null, (Block b) => ((BlockAbstractParticles)b).ParticleAngle, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("EmitterCampfire.ParticleShoot", 1f), "Campfire");
	}
}
