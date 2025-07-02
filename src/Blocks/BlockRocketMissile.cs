using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockRocketMissile : BlockAbstractRocket
{
	public BlockRocketMissile(List<List<Tile>> tiles)
		: base(tiles, "Blocks/Rocket Flame", string.Empty)
	{
		setSmokeColor = true;
		smokeColorMeshIndex = 0;
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockRocketMissile>("MissileRocket.Fire", (Block b) => ((BlockAbstractRocket)b).IsFiring, (Block b) => ((BlockAbstractRocket)b).FireRocket, new Type[1] { typeof(float) }, new string[1] { "Force" });
		PredicateRegistry.Add<BlockRocketMissile>("MissileRocket.Smoke", (Block b) => ((BlockAbstractRocket)b).IsSmoking, (Block b) => ((BlockAbstractRocket)b).Smoke);
		Block.AddSimpleDefaultTiles(new GAF("MissileRocket.Fire", 2f), "Missile A");
	}
}
