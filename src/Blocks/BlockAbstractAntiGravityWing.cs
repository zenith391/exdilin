using System.Collections.Generic;

namespace Blocks;

public class BlockAbstractAntiGravityWing : BlockAbstractAntiGravity
{
	public BlockAbstractAntiGravityWing(List<List<Tile>> tiles)
		: base(tiles)
	{
		playLoop = false;
		informAboutVaryingGravity = false;
	}
}
