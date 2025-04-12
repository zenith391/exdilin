using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002E2 RID: 738
public class TutorialSafeState
{
	// Token: 0x060021DF RID: 8671 RVA: 0x000FE224 File Offset: 0x000FC624
	public void ExtractState()
	{
		this.placedBlockTiles.Clear();
		this.notPlacedBlockTiles.Clear();
		foreach (Block block in BWSceneManager.AllBlocks())
		{
			this.placedBlockTiles.Add(Blocksworld.CloneBlockTiles(block, false, false));
		}
		for (int i = Mathf.Max(0, Tutorial.step); i < Tutorial.blocks.Count; i++)
		{
			this.notPlacedBlockTiles.Add(Blocksworld.CloneBlockTiles(Tutorial.blocks[i], false, false));
		}
	}

	// Token: 0x04001CD2 RID: 7378
	public List<List<List<Tile>>> placedBlockTiles = new List<List<List<Tile>>>();

	// Token: 0x04001CD3 RID: 7379
	public List<List<List<Tile>>> notPlacedBlockTiles = new List<List<List<Tile>>>();
}
