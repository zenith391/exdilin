using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockEmitterWater : BlockAbstractParticles
{
	public static Predicate predicateWaterShoot;

	public BlockEmitterWater(List<List<Tile>> tiles, bool shouldHide = false)
		: base(tiles, "Particles/Water Stream", shouldHide)
	{
	}

	public new static void Register()
	{
		predicateWaterShoot = PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleShoot", null, (Block b) => ((BlockAbstractParticles)b).IsFiring, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleSpread", null, (Block b) => ((BlockAbstractParticles)b).ParticleSpread, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterWater>("EmitterWater.ParticleAngle", null, (Block b) => ((BlockAbstractParticles)b).ParticleAngle, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("EmitterWater.ParticleShoot", 1f), "Water Emitter Block");
		Block.AddSimpleDefaultTiles(new GAF("Block.Fixed"), new GAF("EmitterWater.ParticleShoot", 1f), "Water Volume Block");
	}
}
