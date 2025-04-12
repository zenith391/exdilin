using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000194 RID: 404
public class HistoryState
{
	// Token: 0x060016BE RID: 5822 RVA: 0x000A2944 File Offset: 0x000A0D44
	public HistoryState(List<Block> blocks, Block selectedBlock, bool characterEditorActive)
	{
		this.selectedBlockByIndex = -1;
		foreach (Block block in blocks)
		{
			this.blockGafs.Add(History.CopyTilesToGafs(block.tiles));
			this.blockInstanceIds.Add(block.GetInstanceId());
			if (selectedBlock == block)
			{
				this.selectedBlockByIndex = this.blockInstanceIds.Count - 1;
			}
		}
		this.inCharacterEditor = (characterEditorActive && selectedBlock != null && selectedBlock is BlockAnimatedCharacter);
	}

	// Token: 0x060016BF RID: 5823 RVA: 0x000A2A20 File Offset: 0x000A0E20
	public void CalculateBlockAndTextureUsage()
	{
		this.textureUsage = new HashSet<string>
		{
			"Plain"
		};
		this.blockUsage = new HashSet<string>();
		foreach (List<List<GAF>> list in this.blockGafs)
		{
			foreach (List<GAF> list2 in list)
			{
				foreach (GAF gaf in list2)
				{
					Predicate predicate = gaf.Predicate;
					if (predicate == Block.predicateCreate)
					{
						this.blockUsage.Add((string)gaf.Args[0]);
					}
					else if (predicate == Block.predicateTextureTo)
					{
						this.textureUsage.Add((string)gaf.Args[0]);
					}
				}
			}
		}
	}

	// Token: 0x060016C0 RID: 5824 RVA: 0x000A2B74 File Offset: 0x000A0F74
	public void RemoveUnusedBlocksAndTextures(HistoryState newState)
	{
		if (newState.textureUsage == null)
		{
			newState.CalculateBlockAndTextureUsage();
		}
		if (this.textureUsage == null)
		{
			this.CalculateBlockAndTextureUsage();
		}
		foreach (string item in this.textureUsage)
		{
			if (!newState.textureUsage.Contains(item))
			{
				ResourceLoader.UnloadUnusedTextures(newState.textureUsage);
				break;
			}
		}
		foreach (string item2 in this.blockUsage)
		{
			if (!newState.blockUsage.Contains(item2))
			{
				ResourceLoader.UnloadUnusedBlockPrefabs(newState.blockUsage);
				break;
			}
		}
	}

	// Token: 0x060016C1 RID: 5825 RVA: 0x000A2C74 File Offset: 0x000A1074
	public bool Equals(HistoryState s)
	{
		if (this.blockGafs.Count != s.blockGafs.Count)
		{
			return false;
		}
		for (int i = 0; i < this.blockGafs.Count; i++)
		{
			if (this.blockInstanceIds[i] != s.blockInstanceIds[i])
			{
				return false;
			}
			List<List<GAF>> list = this.blockGafs[i];
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
					GAF gaf = list3[k];
					GAF obj = list4[k];
					if (!gaf.Equals(obj))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	// Token: 0x040011B9 RID: 4537
	public List<List<List<GAF>>> blockGafs = new List<List<List<GAF>>>();

	// Token: 0x040011BA RID: 4538
	public List<int> blockInstanceIds = new List<int>();

	// Token: 0x040011BB RID: 4539
	public int selectedBlockByIndex = -1;

	// Token: 0x040011BC RID: 4540
	public HashSet<string> textureUsage;

	// Token: 0x040011BD RID: 4541
	public HashSet<string> blockUsage;

	// Token: 0x040011BE RID: 4542
	public bool inCharacterEditor;
}
