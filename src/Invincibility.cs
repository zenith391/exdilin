using System.Collections.Generic;
using Blocks;

public class Invincibility
{
	private static Dictionary<Block, int> invincibilityCounters = new Dictionary<Block, int>();

	private static Dictionary<Block, int> modelInvincibilityCounters = new Dictionary<Block, int>();

	private static List<Block> tempList = new List<Block>();

	private static void StepDict(Dictionary<Block, int> dict)
	{
		tempList.Clear();
		foreach (KeyValuePair<Block, int> item in dict)
		{
			tempList.Add(item.Key);
		}
		for (int i = 0; i < tempList.Count; i++)
		{
			Block key = tempList[i];
			int num = dict[key];
			num--;
			if (num <= 0)
			{
				dict.Remove(key);
			}
			else
			{
				dict[key] = num;
			}
		}
	}

	public static void Step()
	{
		StepDict(invincibilityCounters);
		StepDict(modelInvincibilityCounters);
	}

	public static void SetBlockInvincible(Block b)
	{
		invincibilityCounters[b] = 2;
	}

	public static void SetModelInvincible(Block b)
	{
		b.UpdateConnectedCache();
		List<Block> list = Block.connectedCache[b];
		modelInvincibilityCounters[list[0]] = 2;
	}

	public static bool IsInvincible(Block b)
	{
		bool flag = invincibilityCounters.ContainsKey(b);
		if (!flag)
		{
			b.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[b];
			return modelInvincibilityCounters.ContainsKey(list[0]);
		}
		return flag;
	}

	public static void Clear()
	{
		invincibilityCounters.Clear();
		modelInvincibilityCounters.Clear();
	}
}
