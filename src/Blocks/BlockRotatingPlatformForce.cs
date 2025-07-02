using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockRotatingPlatformForce : BlockAbstractRotatingPlatform
{
	public BlockRotatingPlatformForce(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockRotatingPlatformForce>("RotatingPlatformForce.IncreaseAngle", null, (Block b) => ((BlockAbstractRotatingPlatform)b).IncreaseAngleDurational, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
		PredicateRegistry.Add<BlockRotatingPlatformForce>("RotatingPlatformForce.ReturnAngle", null, (Block b) => ((BlockAbstractRotatingPlatform)b).ReturnAngleDurational, new Type[2]
		{
			typeof(float),
			typeof(float)
		});
	}
}
