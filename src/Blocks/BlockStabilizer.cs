using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockStabilizer : BlockAbstractStabilizer
{
	public static Predicate predicateStabilizerStabilize;

	public static Predicate predicateStabilizerHold;

	public static Predicate predicateStabilizerBurst;

	public BlockStabilizer(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		predicateStabilizerStabilize = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Stabilize", (Block b) => ((BlockAbstractStabilizer)b).IsStabilizing, (Block b) => ((BlockAbstractStabilizer)b).Stabilize, new Type[1] { typeof(float) });
		predicateStabilizerHold = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.ControlPosition", (Block b) => ((BlockAbstractStabilizer)b).IsCloseToSomething, (Block b) => ((BlockAbstractStabilizer)b).ControlPosition, new Type[1] { typeof(float) });
		predicateStabilizerBurst = PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Burst", (Block b) => ((BlockAbstractStabilizer)b).IsBursting, (Block b) => ((BlockAbstractStabilizer)b).Burst, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.StabilizePlane", null, (Block b) => ((BlockAbstractStabilizer)b).StabilizePlane);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.ControlZeroAngVel", null, (Block b) => ((BlockAbstractStabilizer)b).ControlZeroAngVel);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.Boost", null, (Block b) => ((BlockAbstractStabilizer)b).BoostStabilizer);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.IncreaseAngle", null, (Block b) => ((BlockAbstractStabilizer)b).IncreaseAngle);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.DecreaseAngle", null, (Block b) => ((BlockAbstractStabilizer)b).DecreaseAngle);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.IncreasePosition", null, (Block b) => ((BlockAbstractStabilizer)b).IncreasePosition);
		PredicateRegistry.Add<BlockStabilizer>("Stabilizer.DecreasePosition", null, (Block b) => ((BlockAbstractStabilizer)b).DecreasePosition);
		Block.AddSimpleDefaultTiles(new GAF("Stabilizer.ControlPosition", 1f), "Stabilizer");
	}
}
