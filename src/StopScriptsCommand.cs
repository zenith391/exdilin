using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class StopScriptsCommand : Command
{
	private HashSet<Block> toStop = new HashSet<Block>();

	private Dictionary<float, List<Block>> toStart = new Dictionary<float, List<Block>>();

	public override void Execute()
	{
		foreach (Block item in new List<Block>(toStop))
		{
			if (!Block.vanishingOrAppearingBlocks.Contains(item))
			{
				BWSceneManager.RemoveScriptBlock(item);
				toStop.Remove(item);
			}
		}
		if (toStart.Count > 0)
		{
			float time = Time.time;
			foreach (float item2 in new List<float>(toStart.Keys))
			{
				if (!(item2 < time))
				{
					continue;
				}
				List<Block> list = toStart[item2];
				toStart.Remove(item2);
				foreach (Block item3 in list)
				{
					BWSceneManager.AddScriptBlock(item3);
					if (item3 is BlockAbstractUI)
					{
						((BlockAbstractUI)item3).EnableUI(enable: true);
					}
				}
			}
		}
		done = toStop.Count == 0 && toStart.Count == 0;
	}

	public void StartBlockScripts(List<Block> toAdd, float timeOffset)
	{
		float key = Time.time + timeOffset;
		if (!toStart.TryGetValue(key, out var value))
		{
			value = new List<Block>();
			toStart[key] = value;
		}
		value.AddRange(toAdd);
	}

	public void StopBlockScripts(Block b)
	{
		if (b is BlockAbstractUI)
		{
			((BlockAbstractUI)b).EnableUI(enable: false);
		}
		toStop.Add(b);
	}

	public override void Removed()
	{
		toStop.Clear();
		toStart.Clear();
	}
}
