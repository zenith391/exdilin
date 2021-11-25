using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000148 RID: 328
public class StopScriptsCommand : Command
{
	// Token: 0x0600147A RID: 5242 RVA: 0x00090050 File Offset: 0x0008E450
	public override void Execute()
	{
		foreach (Block block in new List<Block>(this.toStop))
		{
			if (!Block.vanishingOrAppearingBlocks.Contains(block))
			{
				BWSceneManager.RemoveScriptBlock(block);
				this.toStop.Remove(block);
			}
		}
		if (this.toStart.Count > 0)
		{
			float time = Time.time;
			foreach (float num in new List<float>(this.toStart.Keys))
			{
				if (num < time)
				{
					List<Block> list = this.toStart[num];
					this.toStart.Remove(num);
					foreach (Block block2 in list)
					{
						BWSceneManager.AddScriptBlock(block2);
						if (block2 is BlockAbstractUI)
						{
							((BlockAbstractUI)block2).EnableUI(true);
						}
					}
				}
			}
		}
		this.done = (this.toStop.Count == 0 && this.toStart.Count == 0);
	}

	// Token: 0x0600147B RID: 5243 RVA: 0x000901E0 File Offset: 0x0008E5E0
	public void StartBlockScripts(List<Block> toAdd, float timeOffset)
	{
		float key = Time.time + timeOffset;
		List<Block> list;
		if (!this.toStart.TryGetValue(key, out list))
		{
			list = new List<Block>();
			this.toStart[key] = list;
		}
		list.AddRange(toAdd);
	}

	// Token: 0x0600147C RID: 5244 RVA: 0x00090222 File Offset: 0x0008E622
	public void StopBlockScripts(Block b)
	{
		if (b is BlockAbstractUI)
		{
			((BlockAbstractUI)b).EnableUI(false);
		}
		this.toStop.Add(b);
	}

	// Token: 0x0600147D RID: 5245 RVA: 0x00090248 File Offset: 0x0008E648
	public override void Removed()
	{
		this.toStop.Clear();
		this.toStart.Clear();
	}

	// Token: 0x04001036 RID: 4150
	private HashSet<Block> toStop = new HashSet<Block>();

	// Token: 0x04001037 RID: 4151
	private Dictionary<float, List<Block>> toStart = new Dictionary<float, List<Block>>();
}
