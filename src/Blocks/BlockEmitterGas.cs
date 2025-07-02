using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockEmitterGas : BlockAbstractParticles
{
	public static Predicate predicateGasShoot;

	public BlockEmitterGas(List<List<Tile>> tiles, bool shouldHide = false)
		: base(tiles, "Particles/Gas Stream", shouldHide, 0, "815 Jet Loop")
	{
	}

	public new static void Register()
	{
		predicateGasShoot = PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleShoot", null, (Block b) => ((BlockAbstractParticles)b).IsFiring, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleSpread", null, (Block b) => ((BlockAbstractParticles)b).ParticleSpread, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockEmitterGas>("EmitterGas.ParticleAngle", null, (Block b) => ((BlockAbstractParticles)b).ParticleAngle, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("EmitterGas.ParticleShoot", 1f), "Gas Emitter Block");
		Block.AddSimpleDefaultTiles(new GAF("Block.Fixed"), new GAF("EmitterGas.ParticleShoot", 1f), "Gas Volume Block");
	}
}
