using System.Collections.Generic;
using Blocks;

public class HistoryState
{
	public List<List<List<GAF>>> blockGafs = new List<List<List<GAF>>>();

	public List<int> blockInstanceIds = new List<int>();

	public int selectedBlockByIndex = -1;

	public HashSet<string> textureUsage;

	public HashSet<string> blockUsage;

	public bool inCharacterEditor;

	public HistoryState(List<Block> blocks, Block selectedBlock, bool characterEditorActive)
	{
		selectedBlockByIndex = -1;
		foreach (Block block in blocks)
		{
			blockGafs.Add(History.CopyTilesToGafs(block.tiles));
			blockInstanceIds.Add(block.GetInstanceId());
			if (selectedBlock == block)
			{
				selectedBlockByIndex = blockInstanceIds.Count - 1;
			}
		}
		inCharacterEditor = characterEditorActive && selectedBlock != null && selectedBlock is BlockAnimatedCharacter;
	}

	public void CalculateBlockAndTextureUsage()
	{
		textureUsage = new HashSet<string> { "Plain" };
		blockUsage = new HashSet<string>();
		foreach (List<List<GAF>> blockGaf in blockGafs)
		{
			foreach (List<GAF> item in blockGaf)
			{
				foreach (GAF item2 in item)
				{
					Predicate predicate = item2.Predicate;
					if (predicate == Block.predicateCreate)
					{
						blockUsage.Add((string)item2.Args[0]);
					}
					else if (predicate == Block.predicateTextureTo)
					{
						textureUsage.Add((string)item2.Args[0]);
					}
				}
			}
		}
	}

	public void RemoveUnusedBlocksAndTextures(HistoryState newState)
	{
		if (newState.textureUsage == null)
		{
			newState.CalculateBlockAndTextureUsage();
		}
		if (textureUsage == null)
		{
			CalculateBlockAndTextureUsage();
		}
		foreach (string item in textureUsage)
		{
			if (!newState.textureUsage.Contains(item))
			{
				ResourceLoader.UnloadUnusedTextures(newState.textureUsage);
				break;
			}
		}
		foreach (string item2 in blockUsage)
		{
			if (!newState.blockUsage.Contains(item2))
			{
				ResourceLoader.UnloadUnusedBlockPrefabs(newState.blockUsage);
				break;
			}
		}
	}

	public bool Equals(HistoryState s)
	{
		if (blockGafs.Count != s.blockGafs.Count)
		{
			return false;
		}
		for (int i = 0; i < blockGafs.Count; i++)
		{
			if (blockInstanceIds[i] != s.blockInstanceIds[i])
			{
				return false;
			}
			List<List<GAF>> list = blockGafs[i];
			List<List<GAF>> list2 = s.blockGafs[i];
			if (list.Count != list2.Count)
			{
				return false;
			}
			for (int j = 0; j < list.Count; j++)
			{
				List<GAF> list3 = list[j];
				List<GAF> list4 = list2[j];
				if (list3.Count != list4.Count)
				{
					return false;
				}
				for (int k = 0; k < list3.Count; k++)
				{
					GAF gAF = list3[k];
					GAF obj = list4[k];
					if (!gAF.Equals(obj))
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
