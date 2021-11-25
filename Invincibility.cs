using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020001AB RID: 427
public class Invincibility
{
	// Token: 0x06001794 RID: 6036 RVA: 0x000A64F8 File Offset: 0x000A48F8
	private static void StepDict(Dictionary<Block, int> dict)
	{
		Invincibility.tempList.Clear();
		foreach (KeyValuePair<Block, int> keyValuePair in dict)
		{
			Invincibility.tempList.Add(keyValuePair.Key);
		}
		for (int i = 0; i < Invincibility.tempList.Count; i++)
		{
			Block key = Invincibility.tempList[i];
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

	// Token: 0x06001795 RID: 6037 RVA: 0x000A65B8 File Offset: 0x000A49B8
	public static void Step()
	{
		Invincibility.StepDict(Invincibility.invincibilityCounters);
		Invincibility.StepDict(Invincibility.modelInvincibilityCounters);
	}

	// Token: 0x06001796 RID: 6038 RVA: 0x000A65CE File Offset: 0x000A49CE
	public static void SetBlockInvincible(Block b)
	{
		Invincibility.invincibilityCounters[b] = 2;
	}

	// Token: 0x06001797 RID: 6039 RVA: 0x000A65DC File Offset: 0x000A49DC
	public static void SetModelInvincible(Block b)
	{
		b.UpdateConnectedCache();
		List<Block> list = Block.connectedCache[b];
		Invincibility.modelInvincibilityCounters[list[0]] = 2;
	}

	// Token: 0x06001798 RID: 6040 RVA: 0x000A6610 File Offset: 0x000A4A10
	public static bool IsInvincible(Block b)
	{
		bool flag = Invincibility.invincibilityCounters.ContainsKey(b);
		if (!flag)
		{
			b.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[b];
			return Invincibility.modelInvincibilityCounters.ContainsKey(list[0]);
		}
		return flag;
	}

	// Token: 0x06001799 RID: 6041 RVA: 0x000A6655 File Offset: 0x000A4A55
	public static void Clear()
	{
		Invincibility.invincibilityCounters.Clear();
		Invincibility.modelInvincibilityCounters.Clear();
	}

	// Token: 0x04001287 RID: 4743
	private static Dictionary<Block, int> invincibilityCounters = new Dictionary<Block, int>();

	// Token: 0x04001288 RID: 4744
	private static Dictionary<Block, int> modelInvincibilityCounters = new Dictionary<Block, int>();

	// Token: 0x04001289 RID: 4745
	private static List<Block> tempList = new List<Block>();
}
