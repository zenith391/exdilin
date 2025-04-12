using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002BD RID: 701
public class TagManager
{
	// Token: 0x06002038 RID: 8248 RVA: 0x000ED064 File Offset: 0x000EB464
	private static int GetKey(string tag, Block b)
	{
		int instanceId = b.GetInstanceId();
		int length = tag.Length;
		if (length == 1)
		{
			return instanceId + (int)tag[0] * 1000000;
		}
		if (length != 2)
		{
			return tag.GetHashCode() + instanceId * 1000;
		}
		return instanceId + (int)tag[0] * 1000000 + (int)tag[1] * 1000000 * 100;
	}

	// Token: 0x06002039 RID: 8249 RVA: 0x000ED0D4 File Offset: 0x000EB4D4
	public static void ClearRegisteredBlocks(bool hard = false)
	{
		if (hard)
		{
			TagManager.stayCounters.Clear();
			TagManager.registeredBlocks.Clear();
			TagManager.blockTags.Clear();
		}
		else
		{
			bool flag = false;
			TagManager.toRemove.Clear();
			foreach (KeyValuePair<string, List<Block>> keyValuePair in TagManager.registeredBlocks)
			{
				string key = keyValuePair.Key;
				List<Block> list = TagManager.registeredBlocks[key];
				for (int i = 0; i < list.Count; i++)
				{
					Block block = list[i];
					if (block == null)
					{
						flag = true;
					}
					else
					{
						int key2 = TagManager.GetKey(key, block);
						if (TagManager.stayCounters.ContainsKey(key2))
						{
							int num = TagManager.stayCounters[key2];
							num--;
							TagManager.stayCounters[key2] = num;
							if (num <= 0)
							{
								TagManager.toRemove.Add(new KeyValuePair<string, Block>(key, block));
							}
						}
					}
				}
			}
			if (flag)
			{
				TagManager.stayCounters.Clear();
				TagManager.registeredBlocks.Clear();
				TagManager.blockTags.Clear();
				BWLog.Info("Found a removed block, clearing all");
			}
			else if (TagManager.toRemove.Count > 0)
			{
				for (int j = 0; j < TagManager.toRemove.Count; j++)
				{
					KeyValuePair<string, Block> keyValuePair2 = TagManager.toRemove[j];
					List<Block> orAddList = TagManager.GetOrAddList(keyValuePair2.Key, TagManager.registeredBlocks);
					if (orAddList != null && keyValuePair2.Value != null)
					{
						orAddList.Remove(keyValuePair2.Value);
					}
					else
					{
						BWLog.Info("Set s was null");
					}
					List<string> orAddList2 = TagManager.GetOrAddList(keyValuePair2.Value, TagManager.blockTags);
					if (orAddList2 != null && keyValuePair2.Value != null)
					{
						orAddList2.Remove(keyValuePair2.Key);
					}
					else
					{
						BWLog.Info("Set s2 was null");
					}
				}
				TagManager.toRemove.Clear();
			}
		}
	}

	// Token: 0x0600203A RID: 8250 RVA: 0x000ED300 File Offset: 0x000EB700
	private static List<Block> GetOrAddList(string key, Dictionary<string, List<Block>> register)
	{
		List<Block> list;
		if (!register.TryGetValue(key, out list))
		{
			list = new List<Block>();
			register[key] = list;
		}
		return list;
	}

	// Token: 0x0600203B RID: 8251 RVA: 0x000ED32C File Offset: 0x000EB72C
	private static List<string> GetOrAddList(Block key, Dictionary<Block, List<string>> register)
	{
		List<string> list;
		if (!register.TryGetValue(key, out list))
		{
			list = new List<string>();
			register[key] = list;
		}
		return list;
	}

	// Token: 0x0600203C RID: 8252 RVA: 0x000ED356 File Offset: 0x000EB756
	public static List<Block> GetBlocksWithTag(string posName)
	{
		return TagManager.GetOrAddList(posName, TagManager.registeredBlocks);
	}

	// Token: 0x0600203D RID: 8253 RVA: 0x000ED364 File Offset: 0x000EB764
	public static void RegisterBlockTag(Block block, string tagName)
	{
		List<Block> orAddList = TagManager.GetOrAddList(tagName, TagManager.registeredBlocks);
		if (!orAddList.Contains(block))
		{
			orAddList.Add(block);
		}
		List<string> orAddList2 = TagManager.GetOrAddList(block, TagManager.blockTags);
		if (!orAddList2.Contains(tagName))
		{
			orAddList2.Add(tagName);
		}
		int key = TagManager.GetKey(tagName, block);
		TagManager.stayCounters[key] = 2;
	}

	// Token: 0x0600203E RID: 8254 RVA: 0x000ED3C4 File Offset: 0x000EB7C4
	public static bool TryGetClosestBlockWithTag(string tagName, Vector3 pos, out Block block, HashSet<Block> toExclude = null)
	{
		block = null;
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		if (blocksWithTag.Count > 0)
		{
			float num = float.PositiveInfinity;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block2 = blocksWithTag[i];
				if (toExclude == null || !toExclude.Contains(block2))
				{
					if (!block2.isTreasure || (!TreasureHandler.IsHiddenTreasureModel(block2) && TreasureHandler.GetTreasureModelScale(block2) >= 0.15f))
					{
						float sqrMagnitude = (block2.go.transform.position - pos).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							block = block2;
						}
					}
				}
			}
			return block != null;
		}
		return false;
	}

	// Token: 0x0600203F RID: 8255 RVA: 0x000ED488 File Offset: 0x000EB888
	public static bool BlockHasTag(Block b, string tagName)
	{
		List<string> list;
		return TagManager.blockTags.TryGetValue(b, out list) && list.Contains(tagName);
	}

	// Token: 0x06002040 RID: 8256 RVA: 0x000ED4B0 File Offset: 0x000EB8B0
	public static List<string> GetBlockTags(Block b)
	{
		return TagManager.GetOrAddList(b, TagManager.blockTags);
	}

	// Token: 0x04001B79 RID: 7033
	public static Dictionary<string, List<Block>> registeredBlocks = new Dictionary<string, List<Block>>();

	// Token: 0x04001B7A RID: 7034
	public static Dictionary<Block, List<string>> blockTags = new Dictionary<Block, List<string>>();

	// Token: 0x04001B7B RID: 7035
	public static Dictionary<int, int> stayCounters = new Dictionary<int, int>();

	// Token: 0x04001B7C RID: 7036
	public static int stayCounter = 0;

	// Token: 0x04001B7D RID: 7037
	private const int TAG_KEY_MULTIPLIER = 1000000;

	// Token: 0x04001B7E RID: 7038
	private static List<KeyValuePair<string, Block>> toRemove = new List<KeyValuePair<string, Block>>();
}
