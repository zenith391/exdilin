using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class UpdateBlockIsTriggerCommand : Command
{
	private Dictionary<Block, int> blockCounters = new Dictionary<Block, int>();

	private List<Block> toRemove = new List<Block>();

	private List<Block> tempList = new List<Block>();

	public override void Execute()
	{
		toRemove.Clear();
		if (blockCounters.Count <= 0)
		{
			return;
		}
		tempList.Clear();
		foreach (KeyValuePair<Block, int> blockCounter in blockCounters)
		{
			tempList.Add(blockCounter.Key);
		}
		for (int i = 0; i < tempList.Count; i++)
		{
			Block block = tempList[i];
			int num = blockCounters[block];
			num--;
			if (num < 0)
			{
				toRemove.Add(block);
			}
			blockCounters[block] = num;
		}
		for (int j = 0; j < toRemove.Count; j++)
		{
			Block block2 = toRemove[j];
			blockCounters.Remove(block2);
			SetTrigger(block2, t: false);
		}
	}

	private void SetTrigger(Block b, bool t)
	{
		Collider component = b.go.GetComponent<Collider>();
		if (component != null)
		{
			MeshCollider component2 = b.go.GetComponent<MeshCollider>();
			if (component2 != null && !component2.convex && t)
			{
				component2.convex = true;
			}
			component.isTrigger = t;
		}
	}

	public void BlockIsTrigger(Block b)
	{
		if (!b.vanished && !blockCounters.ContainsKey(b))
		{
			SetTrigger(b, t: true);
		}
		blockCounters[b] = 2;
	}

	public override void Removed()
	{
		base.Removed();
		blockCounters.Clear();
	}
}
