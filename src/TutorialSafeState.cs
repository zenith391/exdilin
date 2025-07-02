using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TutorialSafeState
{
	public List<List<List<Tile>>> placedBlockTiles = new List<List<List<Tile>>>();

	public List<List<List<Tile>>> notPlacedBlockTiles = new List<List<List<Tile>>>();

	public void ExtractState()
	{
		placedBlockTiles.Clear();
		notPlacedBlockTiles.Clear();
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			placedBlockTiles.Add(Blocksworld.CloneBlockTiles(item));
		}
		for (int i = Mathf.Max(0, Tutorial.step); i < Tutorial.blocks.Count; i++)
		{
			notPlacedBlockTiles.Add(Blocksworld.CloneBlockTiles(Tutorial.blocks[i]));
		}
	}
}
