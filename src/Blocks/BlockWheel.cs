using System.Collections.Generic;

namespace Blocks;

public class BlockWheel : BlockAbstractWheel
{
	public BlockWheel(List<List<Tile>> tiles, string axleMeshName = "")
		: base(tiles, axleMeshName, string.Empty)
	{
	}
}
