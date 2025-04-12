using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000193 RID: 403
public class History
{
	// Token: 0x060016AA RID: 5802 RVA: 0x000A1D8C File Offset: 0x000A018C
	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (History.highlightGafs.Count > 0)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			foreach (GAF item in History.highlightGafs)
			{
				result.Add(item);
			}
		}
		return result;
	}

	// Token: 0x060016AB RID: 5803 RVA: 0x000A1E08 File Offset: 0x000A0208
	public static void RemoveHighlightsIfNecessary()
	{
		if (History.highlightGafs.Count > 0)
		{
			bool flag = false;
			foreach (GAF key in History.highlightGafs)
			{
				int num = History.highlightStepsLeft[key];
				num--;
				if (num > 0)
				{
					flag = true;
				}
				History.highlightStepsLeft[key] = num;
			}
			if (!flag)
			{
				History.highlightGafs.Clear();
				History.highlightStepsLeft.Clear();
			}
		}
	}

	// Token: 0x060016AC RID: 5804 RVA: 0x000A1EB0 File Offset: 0x000A02B0
	public static List<List<GAF>> CopyTilesToGafs(List<List<Tile>> tiles)
	{
		List<List<GAF>> list = new List<List<GAF>>();
		foreach (List<Tile> list2 in tiles)
		{
			List<GAF> list3 = new List<GAF>();
			foreach (Tile tile in list2)
			{
				list3.Add(tile.gaf.Clone());
			}
			list.Add(list3);
		}
		return list;
	}

	// Token: 0x060016AD RID: 5805 RVA: 0x000A1F6C File Offset: 0x000A036C
	public static List<List<Tile>> CopyGafsToTiles(List<List<GAF>> tiles)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<GAF> list2 in tiles)
		{
			List<Tile> list3 = new List<Tile>();
			foreach (GAF gaf in list2)
			{
				list3.Add(new Tile(gaf.Clone()));
			}
			list.Add(list3);
		}
		return list;
	}

	// Token: 0x060016AE RID: 5806 RVA: 0x000A2028 File Offset: 0x000A0428
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

	// Token: 0x060016AF RID: 5807 RVA: 0x000A20CC File Offset: 0x000A04CC
	public static HistoryState CreateState()
	{
		return new HistoryState(BWSceneManager.AllBlocks(), Blocksworld.selectedBlock, CharacterEditor.Instance.InEditMode());
	}

	// Token: 0x060016B0 RID: 5808 RVA: 0x000A20F4 File Offset: 0x000A04F4
	private static bool SetBlock(Block b, List<List<GAF>> gafs)
	{
		bool flag = false;
		if (gafs.Count > 0 && gafs[0].Count > 1)
		{
			List<List<Tile>> list = History.CopyGafsToTiles(gafs);
			flag = History.IsDifferent(b.tiles, list);
			if (flag)
			{
				b.tiles = list;
				b.Reset(true);
				b.OnReconstructed();
			}
		}
		else
		{
			BWLog.Info("Used SetBlock() with no tiles");
		}
		return flag;
	}

	// Token: 0x060016B1 RID: 5809 RVA: 0x000A2160 File Offset: 0x000A0560
	private static Block CreateBlock(List<List<Tile>> tiles)
	{
		Block block = Block.NewBlock(tiles, false, false);
		if (block != null)
		{
			BWSceneManager.AddBlock(block);
			block.OnReconstructed();
		}
		ConnectednessGraph.Update(block);
		return block;
	}

	// Token: 0x060016B2 RID: 5810 RVA: 0x000A2190 File Offset: 0x000A0590
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
				bool flag = History.SetBlock(block2, list3);
				if (state.selectedBlockByIndex == i)
				{
					block = block2;
				}
				list2.Add(block2);
			}
			else
			{
				Block block3 = History.CreateBlock(History.CopyGafsToTiles(list3));
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
		}
		foreach (Block block4 in list)
		{
			if (!hashSet3.Contains(block4.GetInstanceId()) && !hashSet2.Contains(block4))
			{
				hashSet.Add(block4);
				if (CharacterEditor.Instance.InEditMode() && block4 == Blocksworld.selectedBlock)
				{
					CharacterEditor.Instance.Exit();
				}
			}
		}
		Blocksworld.Deselect(false, true);
		foreach (Block block5 in hashSet)
		{
			Blocksworld.DestroyBlock(block5);
		}
		Dictionary<GAF, int> dictionary2 = new Dictionary<GAF, int>();
		Scarcity.UpdateInventory(true, dictionary2);
		foreach (GAF gaf in dictionary2.Keys)
		{
			GAF normalizedGaf = Scarcity.GetNormalizedGaf(gaf, false);
			Scarcity.inventoryScales[normalizedGaf] = 1.5f;
			History.highlightGafs.Add(normalizedGaf);
			History.highlightStepsLeft[normalizedGaf] = 20;
			Scarcity.autoRemoveHighlights.Add(normalizedGaf);
		}
		List<Block> list4 = new List<Block>();
		List<Block> list5 = new List<Block>();
		foreach (Block block6 in list2)
		{
			if (block6.ContainsGroupTile())
			{
				if (hashSet2.Contains(block6))
				{
					list4.Add(block6);
				}
				else
				{
					list5.Add(block6);
				}
			}
		}
		if (list4.Count > 0)
		{
			BlockGroups.GatherBlockGroups(list4);
		}
		foreach (Block block7 in list5)
		{
			block7.OnBlockGroupReconstructed();
		}
		if (Blocksworld.modelCollection != null)
		{
			List<ModelData> list6 = Blocksworld.modelCollection.RefreshScarcity();
			for (int j = 0; j < list6.Count; j++)
			{
				GAF gaf2 = list6[j].tile.gaf;
				Scarcity.inventoryScales[gaf2] = 1.5f;
				History.highlightGafs.Add(gaf2);
				History.highlightStepsLeft[gaf2] = 20;
				Scarcity.autoRemoveHighlights.Add(gaf2);
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
					Blocksworld.SelectBlock(block, false, true);
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
			Blocksworld.scriptPanel.Show(false);
		}
		History.UpdateButtonState();
	}

	// Token: 0x060016B3 RID: 5811 RVA: 0x000A2654 File Offset: 0x000A0A54
	private static bool CanUseHistory()
	{
		return History.activated && Blocksworld.CurrentState == State.Build && (Tutorial.state == TutorialState.None || Tutorial.state == TutorialState.Puzzle);
	}

	// Token: 0x060016B4 RID: 5812 RVA: 0x000A2684 File Offset: 0x000A0A84
	public static bool AddStateIfNecessary()
	{
		if (!History.CanUseHistory())
		{
			return false;
		}
		HistoryState historyState = History.CreateState();
		if (History.stateHistoryIndex < History.stateHistory.Count)
		{
			HistoryState historyState2 = History.stateHistory[History.stateHistoryIndex];
			if (historyState.Equals(historyState2))
			{
				return false;
			}
			historyState2.RemoveUnusedBlocksAndTextures(historyState);
		}
		if (History.stateHistoryIndex < History.stateHistory.Count - 1)
		{
			History.stateHistory.RemoveRange(History.stateHistoryIndex + 1, History.stateHistory.Count - History.stateHistoryIndex - 1);
		}
		History.stateHistory.Add(historyState);
		if (History.stateHistory.Count > History.maxHistorySize)
		{
			History.stateHistory.RemoveAt(0);
		}
		History.stateHistoryIndex = History.stateHistory.Count - 1;
		History.UpdateButtonState();
		return true;
	}

	// Token: 0x060016B5 RID: 5813 RVA: 0x000A2758 File Offset: 0x000A0B58
	public static void RemoveState()
	{
		int num = 1;
		if (History.CanUseHistory() && History.stateHistory.Count > num)
		{
			if (History.stateHistoryIndex < History.stateHistory.Count - 1)
			{
				History.stateHistory.RemoveRange(History.stateHistoryIndex + 1, History.stateHistory.Count - History.stateHistoryIndex - 1);
			}
			History.stateHistory.RemoveAt(History.stateHistory.Count - 1);
			History.stateHistoryIndex = History.stateHistory.Count - 1;
			History.UpdateButtonState();
		}
	}

	// Token: 0x060016B6 RID: 5814 RVA: 0x000A27E6 File Offset: 0x000A0BE6
	public static void Initialize()
	{
		History.Reset();
		History.stateHistory.Add(History.CreateState());
		History.stateHistoryIndex = 0;
		History.UpdateButtonState();
	}

	// Token: 0x060016B7 RID: 5815 RVA: 0x000A2807 File Offset: 0x000A0C07
	public static void Reset()
	{
		History.stateHistory.Clear();
		History.stateHistoryIndex = 0;
		History.UpdateButtonState();
	}

	// Token: 0x060016B8 RID: 5816 RVA: 0x000A281E File Offset: 0x000A0C1E
	public static bool CanUndo()
	{
		return History.stateHistoryIndex > 0;
	}

	// Token: 0x060016B9 RID: 5817 RVA: 0x000A2828 File Offset: 0x000A0C28
	public static void Undo()
	{
		if (!History.CanUseHistory() || !History.CanUndo())
		{
			return;
		}
		if (History.stateHistoryIndex > 0)
		{
			HistoryState historyState = History.stateHistory[History.stateHistoryIndex];
			History.stateHistoryIndex--;
			HistoryState historyState2 = History.stateHistory[History.stateHistoryIndex];
			History.SetState(historyState2);
			historyState.RemoveUnusedBlocksAndTextures(historyState2);
		}
	}

	// Token: 0x060016BA RID: 5818 RVA: 0x000A288E File Offset: 0x000A0C8E
	public static bool CanRedo()
	{
		return History.stateHistoryIndex < History.stateHistory.Count - 1;
	}

	// Token: 0x060016BB RID: 5819 RVA: 0x000A28A4 File Offset: 0x000A0CA4
	public static void Redo()
	{
		if (!History.CanUseHistory() || !History.CanRedo())
		{
			return;
		}
		HistoryState historyState = History.stateHistory[History.stateHistoryIndex];
		History.stateHistoryIndex++;
		HistoryState historyState2 = History.stateHistory[History.stateHistoryIndex];
		History.SetState(historyState2);
		historyState.RemoveUnusedBlocksAndTextures(historyState2);
	}

	// Token: 0x060016BC RID: 5820 RVA: 0x000A28FF File Offset: 0x000A0CFF
	private static void UpdateButtonState()
	{
		if (WorldUILayout.currentLayout != null)
		{
			WorldUILayout.currentLayout.RefreshUndoRedoButtons();
		}
	}

	// Token: 0x040011B3 RID: 4531
	public static bool activated = true;

	// Token: 0x040011B4 RID: 4532
	public static int maxHistorySize = 26;

	// Token: 0x040011B5 RID: 4533
	private static int stateHistoryIndex;

	// Token: 0x040011B6 RID: 4534
	private static List<HistoryState> stateHistory = new List<HistoryState>();

	// Token: 0x040011B7 RID: 4535
	private static HashSet<GAF> highlightGafs = new HashSet<GAF>();

	// Token: 0x040011B8 RID: 4536
	private static Dictionary<GAF, int> highlightStepsLeft = new Dictionary<GAF, int>();
}
