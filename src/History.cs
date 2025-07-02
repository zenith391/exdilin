using System.Collections.Generic;
using Blocks;

public class History
{
	public static bool activated = true;

	public static int maxHistorySize = 26;

	private static int stateHistoryIndex;

	private static List<HistoryState> stateHistory = new List<HistoryState>();

	private static HashSet<GAF> highlightGafs = new HashSet<GAF>();

	private static Dictionary<GAF, int> highlightStepsLeft = new Dictionary<GAF, int>();

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (highlightGafs.Count > 0)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			foreach (GAF highlightGaf in highlightGafs)
			{
				result.Add(highlightGaf);
			}
		}
		return result;
	}

	public static void RemoveHighlightsIfNecessary()
	{
		if (highlightGafs.Count <= 0)
		{
			return;
		}
		bool flag = false;
		foreach (GAF highlightGaf in highlightGafs)
		{
			int num = highlightStepsLeft[highlightGaf];
			num--;
			if (num > 0)
			{
				flag = true;
			}
			highlightStepsLeft[highlightGaf] = num;
		}
		if (!flag)
		{
			highlightGafs.Clear();
			highlightStepsLeft.Clear();
		}
	}

	public static List<List<GAF>> CopyTilesToGafs(List<List<Tile>> tiles)
	{
		List<List<GAF>> list = new List<List<GAF>>();
		foreach (List<Tile> tile in tiles)
		{
			List<GAF> list2 = new List<GAF>();
			foreach (Tile item in tile)
			{
				list2.Add(item.gaf.Clone());
			}
			list.Add(list2);
		}
		return list;
	}

	public static List<List<Tile>> CopyGafsToTiles(List<List<GAF>> tiles)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<GAF> tile in tiles)
		{
			List<Tile> list2 = new List<Tile>();
			foreach (GAF item in tile)
			{
				list2.Add(new Tile(item.Clone()));
			}
			list.Add(list2);
		}
		return list;
	}

	private static bool IsDifferent(List<List<Tile>> tiles1, List<List<Tile>> tiles2)
	{
		if (tiles1.Count != tiles2.Count)
		{
			return true;
		}
		for (int i = 0; i < tiles1.Count; i++)
		{
			if (tiles1[i].Count != tiles2[i].Count)
			{
				return true;
			}
			for (int j = 0; j < tiles1[i].Count; j++)
			{
				if (!tiles1[i][j].gaf.Equals(tiles2[i][j].gaf))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static HistoryState CreateState()
	{
		return new HistoryState(BWSceneManager.AllBlocks(), Blocksworld.selectedBlock, CharacterEditor.Instance.InEditMode());
	}

	private static bool SetBlock(Block b, List<List<GAF>> gafs)
	{
		bool flag = false;
		if (gafs.Count > 0 && gafs[0].Count > 1)
		{
			List<List<Tile>> list = CopyGafsToTiles(gafs);
			flag = IsDifferent(b.tiles, list);
			if (flag)
			{
				b.tiles = list;
				b.Reset(forceRescale: true);
				b.OnReconstructed();
			}
		}
		else
		{
			BWLog.Info("Used SetBlock() with no tiles");
		}
		return flag;
	}

	private static Block CreateBlock(List<List<Tile>> tiles)
	{
		Block block = Block.NewBlock(tiles);
		if (block != null)
		{
			BWSceneManager.AddBlock(block);
			block.OnReconstructed();
		}
		ConnectednessGraph.Update(block);
		return block;
	}

	public static void SetState(HistoryState state)
	{
		if (CharacterEditor.Instance.InEditMode() && !state.inCharacterEditor)
		{
			CharacterEditor.Instance.Exit();
		}
		List<Block> list = BWSceneManager.AllBlocks();
		Dictionary<int, Block> dictionary = BWSceneManager.BlockInstanceIds();
		HashSet<Block> hashSet = new HashSet<Block>();
		HashSet<Block> hashSet2 = new HashSet<Block>();
		List<Block> list2 = new List<Block>();
		HashSet<int> hashSet3 = new HashSet<int>();
		Block block = null;
		for (int i = 0; i < state.blockGafs.Count; i++)
		{
			List<List<GAF>> list3 = state.blockGafs[i];
			int num = state.blockInstanceIds[i];
			hashSet3.Add(num);
			if (dictionary.ContainsKey(num))
			{
				Block block2 = dictionary[num];
				bool flag = SetBlock(block2, list3);
				if (state.selectedBlockByIndex == i)
				{
					block = block2;
				}
				list2.Add(block2);
				continue;
			}
			Block block3 = CreateBlock(CopyGafsToTiles(list3));
			hashSet2.Add(block3);
			if (block3 != null)
			{
				state.blockInstanceIds[i] = block3.GetInstanceId();
				if (state.selectedBlockByIndex == i)
				{
					block = block3;
				}
			}
			list2.Add(block3);
		}
		foreach (Block item in list)
		{
			if (!hashSet3.Contains(item.GetInstanceId()) && !hashSet2.Contains(item))
			{
				hashSet.Add(item);
				if (CharacterEditor.Instance.InEditMode() && item == Blocksworld.selectedBlock)
				{
					CharacterEditor.Instance.Exit();
				}
			}
		}
		Blocksworld.Deselect();
		foreach (Block item2 in hashSet)
		{
			Blocksworld.DestroyBlock(item2);
		}
		Dictionary<GAF, int> dictionary2 = new Dictionary<GAF, int>();
		Scarcity.UpdateInventory(updateTiles: true, dictionary2);
		foreach (GAF key in dictionary2.Keys)
		{
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(key);
			Scarcity.inventoryScales[normalizedGaf] = 1.5f;
			highlightGafs.Add(normalizedGaf);
			highlightStepsLeft[normalizedGaf] = 20;
			Scarcity.autoRemoveHighlights.Add(normalizedGaf);
		}
		List<Block> list4 = new List<Block>();
		List<Block> list5 = new List<Block>();
		foreach (Block item3 in list2)
		{
			if (item3.ContainsGroupTile())
			{
				if (hashSet2.Contains(item3))
				{
					list4.Add(item3);
				}
				else
				{
					list5.Add(item3);
				}
			}
		}
		if (list4.Count > 0)
		{
			BlockGroups.GatherBlockGroups(list4);
		}
		foreach (Block item4 in list5)
		{
			item4.OnBlockGroupReconstructed();
		}
		if (Blocksworld.modelCollection != null)
		{
			List<ModelData> list6 = Blocksworld.modelCollection.RefreshScarcity();
			for (int j = 0; j < list6.Count; j++)
			{
				GAF gaf = list6[j].tile.gaf;
				Scarcity.inventoryScales[gaf] = 1.5f;
				highlightGafs.Add(gaf);
				highlightStepsLeft[gaf] = 20;
				Scarcity.autoRemoveHighlights.Add(gaf);
			}
		}
		ConnectednessGraph.Update(list2);
		if (!CharacterEditor.Instance.InEditMode())
		{
			if (block != null)
			{
				BlockGrouped blockGrouped = block as BlockGrouped;
				if (blockGrouped == null)
				{
					Blocksworld.SelectBlock(block);
				}
			}
			if (state.inCharacterEditor && Blocksworld.selectedBlock is BlockAnimatedCharacter)
			{
				CharacterEditor.Instance.EditCharacter(Blocksworld.selectedBlock as BlockAnimatedCharacter);
			}
		}
		if (CharacterEditor.Instance.InEditMode())
		{
			CharacterEditor.Instance.RefreshAttachments();
		}
		if (Blocksworld.selectedBlock != null)
		{
			Blocksworld.scriptPanel.SetTilesFromBlock(Blocksworld.selectedBlock);
			Blocksworld.scriptPanel.Layout();
		}
		else
		{
			Blocksworld.scriptPanel.Show(show: false);
		}
		UpdateButtonState();
	}

	private static bool CanUseHistory()
	{
		if (activated && Blocksworld.CurrentState == State.Build)
		{
			if (Tutorial.state != TutorialState.None)
			{
				return Tutorial.state == TutorialState.Puzzle;
			}
			return true;
		}
		return false;
	}

	public static bool AddStateIfNecessary()
	{
		if (!CanUseHistory())
		{
			return false;
		}
		HistoryState historyState = CreateState();
		if (stateHistoryIndex < stateHistory.Count)
		{
			HistoryState historyState2 = stateHistory[stateHistoryIndex];
			if (historyState.Equals(historyState2))
			{
				return false;
			}
			historyState2.RemoveUnusedBlocksAndTextures(historyState);
		}
		if (stateHistoryIndex < stateHistory.Count - 1)
		{
			stateHistory.RemoveRange(stateHistoryIndex + 1, stateHistory.Count - stateHistoryIndex - 1);
		}
		stateHistory.Add(historyState);
		if (stateHistory.Count > maxHistorySize)
		{
			stateHistory.RemoveAt(0);
		}
		stateHistoryIndex = stateHistory.Count - 1;
		UpdateButtonState();
		return true;
	}

	public static void RemoveState()
	{
		int num = 1;
		if (CanUseHistory() && stateHistory.Count > num)
		{
			if (stateHistoryIndex < stateHistory.Count - 1)
			{
				stateHistory.RemoveRange(stateHistoryIndex + 1, stateHistory.Count - stateHistoryIndex - 1);
			}
			stateHistory.RemoveAt(stateHistory.Count - 1);
			stateHistoryIndex = stateHistory.Count - 1;
			UpdateButtonState();
		}
	}

	public static void Initialize()
	{
		Reset();
		stateHistory.Add(CreateState());
		stateHistoryIndex = 0;
		UpdateButtonState();
	}

	public static void Reset()
	{
		stateHistory.Clear();
		stateHistoryIndex = 0;
		UpdateButtonState();
	}

	public static bool CanUndo()
	{
		return stateHistoryIndex > 0;
	}

	public static void Undo()
	{
		if (CanUseHistory() && CanUndo() && stateHistoryIndex > 0)
		{
			HistoryState historyState = stateHistory[stateHistoryIndex];
			stateHistoryIndex--;
			HistoryState historyState2 = stateHistory[stateHistoryIndex];
			SetState(historyState2);
			historyState.RemoveUnusedBlocksAndTextures(historyState2);
		}
	}

	public static bool CanRedo()
	{
		return stateHistoryIndex < stateHistory.Count - 1;
	}

	public static void Redo()
	{
		if (CanUseHistory() && CanRedo())
		{
			HistoryState historyState = stateHistory[stateHistoryIndex];
			stateHistoryIndex++;
			HistoryState historyState2 = stateHistory[stateHistoryIndex];
			SetState(historyState2);
			historyState.RemoveUnusedBlocksAndTextures(historyState2);
		}
	}

	private static void UpdateButtonState()
	{
		if (WorldUILayout.currentLayout != null)
		{
			WorldUILayout.currentLayout.RefreshUndoRedoButtons();
		}
	}
}
