using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200014A RID: 330
public class UpdateBlockIsTriggerCommand : Command
{
	// Token: 0x06001481 RID: 5249 RVA: 0x000902A0 File Offset: 0x0008E6A0
	public override void Execute()
	{
		this.toRemove.Clear();
		if (this.blockCounters.Count > 0)
		{
			this.tempList.Clear();
			foreach (KeyValuePair<Block, int> keyValuePair in this.blockCounters)
			{
				this.tempList.Add(keyValuePair.Key);
			}
			for (int i = 0; i < this.tempList.Count; i++)
			{
				Block block = this.tempList[i];
				int num = this.blockCounters[block];
				num--;
				if (num < 0)
				{
					this.toRemove.Add(block);
				}
				this.blockCounters[block] = num;
			}
			for (int j = 0; j < this.toRemove.Count; j++)
			{
				Block block2 = this.toRemove[j];
				this.blockCounters.Remove(block2);
				this.SetTrigger(block2, false);
			}
		}
	}

	// Token: 0x06001482 RID: 5250 RVA: 0x000903D4 File Offset: 0x0008E7D4
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

	// Token: 0x06001483 RID: 5251 RVA: 0x00090430 File Offset: 0x0008E830
	public void BlockIsTrigger(Block b)
	{
		if (!b.vanished && !this.blockCounters.ContainsKey(b))
		{
			this.SetTrigger(b, true);
		}
		this.blockCounters[b] = 2;
	}

	// Token: 0x06001484 RID: 5252 RVA: 0x00090463 File Offset: 0x0008E863
	public override void Removed()
	{
		base.Removed();
		this.blockCounters.Clear();
	}

	// Token: 0x04001038 RID: 4152
	private Dictionary<Block, int> blockCounters = new Dictionary<Block, int>();

	// Token: 0x04001039 RID: 4153
	private List<Block> toRemove = new List<Block>();

	// Token: 0x0400103A RID: 4154
	private List<Block> tempList = new List<Block>();
}
