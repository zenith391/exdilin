using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockMagnet : BlockAbstractMagnet
{
	public BlockMagnet(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMagnet>("Magnet.InfluenceTag", null, (Block b) => ((BlockMagnet)b).InfluenceTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		PredicateRegistry.Add<BlockMagnet>("Magnet.InfluencePaint", null, (Block b) => ((BlockMagnet)b).InfluencePaint, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMagnet>("Magnet.InfluenceTexture", null, (Block b) => ((BlockMagnet)b).InfluenceTexture, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMagnet>("Magnet.SetForceMode", null, (Block b) => ((BlockMagnet)b).SetForceMode, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMagnet>("Magnet.SetDistance", null, (Block b) => ((BlockMagnet)b).SetDistance, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMagnet>("Magnet.SetDistanceNear", null, (Block b) => ((BlockMagnet)b).SetDistanceNear, new Type[1] { typeof(float) });
		Block.AddSimpleDefaultTiles(new GAF("Magnet.SetDistance", 50f), new GAF("Magnet.InfluenceTexture", -10f), "Magnet");
	}
}
