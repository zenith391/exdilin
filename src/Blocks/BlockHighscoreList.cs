using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x02000094 RID: 148
	public class BlockHighscoreList : BlockAbstractUI
	{
		// Token: 0x06000C23 RID: 3107 RVA: 0x000566D5 File Offset: 0x00054AD5
		public BlockHighscoreList(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
		}

		// Token: 0x06000C24 RID: 3108 RVA: 0x000566E0 File Offset: 0x00054AE0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockMaster>("HighscoreList.ShowUI", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowUI), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockMaster>("HighscoreList.ShowPhysical", null, (Block b) => new PredicateActionDelegate(((BlockAbstractUI)b).ShowPhysical), new Type[]
			{
				typeof(int)
			}, null, null);
		}
	}
}
