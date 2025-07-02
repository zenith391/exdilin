using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class TagManager
{
	public static Dictionary<string, List<Block>> registeredBlocks = new Dictionary<string, List<Block>>();

	public static Dictionary<Block, List<string>> blockTags = new Dictionary<Block, List<string>>();

	public static Dictionary<int, int> stayCounters = new Dictionary<int, int>();

	public static int stayCounter = 0;

	private const int TAG_KEY_MULTIPLIER = 1000000;

	private static List<KeyValuePair<string, Block>> toRemove = new List<KeyValuePair<string, Block>>();

	private static int GetKey(string tag, Block b)
	{
		int instanceId = b.GetInstanceId();
		return tag.Length switch
		{
			1 => instanceId + tag[0] * 1000000, 
			2 => instanceId + tag[0] * 1000000 + tag[1] * 1000000 * 100, 
			_ => tag.GetHashCode() + instanceId * 1000, 
		};
	}

	public static void ClearRegisteredBlocks(bool hard = false)
	{
		if (hard)
		{
			stayCounters.Clear();
			registeredBlocks.Clear();
			blockTags.Clear();
			return;
		}
		bool flag = false;
		toRemove.Clear();
		foreach (KeyValuePair<string, List<Block>> registeredBlock in registeredBlocks)
		{
			string key = registeredBlock.Key;
			List<Block> list = registeredBlocks[key];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (block == null)
				{
					flag = true;
					continue;
				}
				int key2 = GetKey(key, block);
				if (stayCounters.ContainsKey(key2))
				{
					int num = stayCounters[key2];
					num--;
					stayCounters[key2] = num;
					if (num <= 0)
					{
						toRemove.Add(new KeyValuePair<string, Block>(key, block));
					}
				}
			}
		}
		if (flag)
		{
			stayCounters.Clear();
			registeredBlocks.Clear();
			blockTags.Clear();
			BWLog.Info("Found a removed block, clearing all");
		}
		else
		{
			if (toRemove.Count <= 0)
			{
				return;
			}
			for (int j = 0; j < toRemove.Count; j++)
			{
				KeyValuePair<string, Block> keyValuePair = toRemove[j];
				List<Block> orAddList = GetOrAddList(keyValuePair.Key, registeredBlocks);
				if (orAddList != null && keyValuePair.Value != null)
				{
					orAddList.Remove(keyValuePair.Value);
				}
				else
				{
					BWLog.Info("Set s was null");
				}
				List<string> orAddList2 = GetOrAddList(keyValuePair.Value, blockTags);
				if (orAddList2 != null && keyValuePair.Value != null)
				{
					orAddList2.Remove(keyValuePair.Key);
				}
				else
				{
					BWLog.Info("Set s2 was null");
				}
			}
			toRemove.Clear();
		}
	}

	private static List<Block> GetOrAddList(string key, Dictionary<string, List<Block>> register)
	{
		if (!register.TryGetValue(key, out var value))
		{
			value = (register[key] = new List<Block>());
		}
		return value;
	}

	private static List<string> GetOrAddList(Block key, Dictionary<Block, List<string>> register)
	{
		if (!register.TryGetValue(key, out var value))
		{
			value = (register[key] = new List<string>());
		}
		return value;
	}

	public static List<Block> GetBlocksWithTag(string posName)
	{
		return GetOrAddList(posName, registeredBlocks);
	}

	public static void RegisterBlockTag(Block block, string tagName)
	{
		List<Block> orAddList = GetOrAddList(tagName, registeredBlocks);
		if (!orAddList.Contains(block))
		{
			orAddList.Add(block);
		}
		List<string> orAddList2 = GetOrAddList(block, blockTags);
		if (!orAddList2.Contains(tagName))
		{
			orAddList2.Add(tagName);
		}
		int key = GetKey(tagName, block);
		stayCounters[key] = 2;
	}

	public static bool TryGetClosestBlockWithTag(string tagName, Vector3 pos, out Block block, HashSet<Block> toExclude = null)
	{
		block = null;
		List<Block> blocksWithTag = GetBlocksWithTag(tagName);
		if (blocksWithTag.Count > 0)
		{
			float num = float.PositiveInfinity;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block2 = blocksWithTag[i];
				if ((toExclude == null || !toExclude.Contains(block2)) && (!block2.isTreasure || (!TreasureHandler.IsHiddenTreasureModel(block2) && TreasureHandler.GetTreasureModelScale(block2) >= 0.15f)))
				{
					float sqrMagnitude = (block2.go.transform.position - pos).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						block = block2;
					}
				}
			}
			return block != null;
		}
		return false;
	}

	public static bool BlockHasTag(Block b, string tagName)
	{
		if (blockTags.TryGetValue(b, out var value))
		{
			return value.Contains(tagName);
		}
		return false;
	}

	public static List<string> GetBlockTags(Block b)
	{
		return GetOrAddList(b, blockTags);
	}
}
