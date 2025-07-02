using System;
using System.Collections.Generic;

namespace Blocks;

public class BlockHighscoreList : BlockAbstractUI
{
	public BlockHighscoreList(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockMaster>("HighscoreList.ShowUI", null, (Block b) => ((BlockAbstractUI)b).ShowUI, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMaster>("HighscoreList.ShowPhysical", null, (Block b) => ((BlockAbstractUI)b).ShowPhysical, new Type[1] { typeof(int) });
	}
}
