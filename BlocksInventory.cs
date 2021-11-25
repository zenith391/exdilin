using System;
using System.Collections.Generic;
using System.Text;

// Token: 0x020000F6 RID: 246
public class BlocksInventory
{
	// Token: 0x060011FF RID: 4607 RVA: 0x0007B389 File Offset: 0x00079789
	public BlocksInventory()
	{
		this.blockItemIds = new List<int>();
		this.blockItemCounts = new Dictionary<int, int>();
		this.blockItemInfinityCounts = new Dictionary<int, int>();
	}

	// Token: 0x06001200 RID: 4608 RVA: 0x0007B3B4 File Offset: 0x000797B4
	public static BlocksInventory CreateUnlimited()
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		foreach (BlockItem blockItem in BlockItem.AllBlockItems)
		{
			blocksInventory.Add(blockItem.Id, 0, 1);
		}
		return blocksInventory;
	}

	// Token: 0x06001201 RID: 4609 RVA: 0x0007B420 File Offset: 0x00079820
	public static BlocksInventory FromString(string blocksInventoryStr, bool verbose = true)
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		blocksInventory.blockItemIds = new List<int>();
		blocksInventory.blockItemCounts = new Dictionary<int, int>();
		blocksInventory.blockItemInfinityCounts = new Dictionary<int, int>();
		if (blocksInventoryStr.Length > 0)
		{
			string[] array = blocksInventoryStr.Split(new char[]
			{
				'|'
			});
			int value = 0;
			int value2 = 0;
			foreach (string text in array)
			{
				string[] array3 = text.Split(new char[]
				{
					':'
				});
				int num;
				if (!int.TryParse(array3[0], out num))
				{
					if (verbose)
					{
						BWLog.Error("Failed to parse block id " + text + " in block inventory string (expected to be int)");
					}
				}
				else
				{
					value = 0;
					value2 = 0;
					string text2 = array3[1];
					string[] array4 = text2.Split(new char[]
					{
						';'
					});
					int.TryParse(array4[0], out value);
					if (array4.Length == 2)
					{
						int.TryParse(array4[1], out value2);
					}
					blocksInventory.blockItemIds.Add(num);
					blocksInventory.blockItemCounts[num] = value;
					blocksInventory.blockItemInfinityCounts[num] = value2;
				}
			}
		}
		return blocksInventory;
	}

	// Token: 0x06001202 RID: 4610 RVA: 0x0007B548 File Offset: 0x00079948
	public new string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (int num2 in this.blockItemIds)
		{
			stringBuilder.Append(string.Format("{0}:{1};{2}", num2, this.blockItemCounts[num2], this.blockItemInfinityCounts[num2]));
			num++;
			if (num < this.blockItemIds.Count)
			{
				stringBuilder.Append('|');
			}
		}
		return stringBuilder.ToString();
	}

	// Token: 0x1700004D RID: 77
	// (get) Token: 0x06001203 RID: 4611 RVA: 0x0007B604 File Offset: 0x00079A04
	public List<int> BlockItemIds
	{
		get
		{
			return this.blockItemIds;
		}
	}

	// Token: 0x06001204 RID: 4612 RVA: 0x0007B60C File Offset: 0x00079A0C
	public bool ContainsBlockItemId(int blockItemId)
	{
		return this.blockItemCounts.ContainsKey(blockItemId);
	}

	// Token: 0x06001205 RID: 4613 RVA: 0x0007B61C File Offset: 0x00079A1C
	public bool ContainsBlockItemIdentifier(string blockItemIdentifier)
	{
		BlockItem blockItem = BlockItem.FindByInternalIdentifier(blockItemIdentifier);
		return blockItem != null && this.ContainsBlockItemId(blockItem.Id);
	}

	// Token: 0x06001206 RID: 4614 RVA: 0x0007B644 File Offset: 0x00079A44
	public void Add(int blockItemId, int count, int infinityCount = 0)
	{
		if (!this.ContainsBlockItemId(blockItemId))
		{
			this.blockItemIds.Add(blockItemId);
			this.blockItemCounts[blockItemId] = count;
			this.blockItemInfinityCounts[blockItemId] = infinityCount;
		}
		else
		{
			Dictionary<int, int> dictionary;
			(dictionary = this.blockItemCounts)[blockItemId] = dictionary[blockItemId] + count;
			(dictionary = this.blockItemInfinityCounts)[blockItemId] = dictionary[blockItemId] + infinityCount;
		}
		if (this.OnChange != null)
		{
			this.OnChange();
		}
	}

	// Token: 0x06001207 RID: 4615 RVA: 0x0007B6D0 File Offset: 0x00079AD0
	public int CountOf(int blockItemId)
	{
		return (!this.blockItemCounts.ContainsKey(blockItemId)) ? 0 : this.blockItemCounts[blockItemId];
	}

	// Token: 0x06001208 RID: 4616 RVA: 0x0007B6F5 File Offset: 0x00079AF5
	public void SetCountFor(int blockItemId, int count)
	{
		if (!this.ContainsBlockItemId(blockItemId))
		{
			this.blockItemIds.Add(blockItemId);
		}
		this.blockItemCounts[blockItemId] = count;
		if (this.OnChange != null)
		{
			this.OnChange();
		}
	}

	// Token: 0x06001209 RID: 4617 RVA: 0x0007B732 File Offset: 0x00079B32
	public int InfinityCountOf(int blockItemId)
	{
		return (!this.blockItemInfinityCounts.ContainsKey(blockItemId)) ? 0 : this.blockItemInfinityCounts[blockItemId];
	}

	// Token: 0x0600120A RID: 4618 RVA: 0x0007B757 File Offset: 0x00079B57
	public void SetInfinityCountFor(int blockItemId, int infinityCount)
	{
		if (!this.ContainsBlockItemId(blockItemId))
		{
			this.blockItemIds.Add(blockItemId);
		}
		this.blockItemInfinityCounts[blockItemId] = infinityCount;
		if (this.OnChange != null)
		{
			this.OnChange();
		}
	}

	// Token: 0x0600120B RID: 4619 RVA: 0x0007B794 File Offset: 0x00079B94
	public int CountOrMinusOneIfInfinityOf(int blockItemId)
	{
		return (this.InfinityCountOf(blockItemId) <= 0) ? this.CountOf(blockItemId) : -1;
	}

	// Token: 0x0600120C RID: 4620 RVA: 0x0007B7B0 File Offset: 0x00079BB0
	public void ReorderBlockItemIndex(int fromIdx, int toIdx)
	{
		if (fromIdx < 0 || toIdx < 0 || fromIdx >= this.blockItemIds.Count || toIdx >= this.blockItemIds.Count || fromIdx == toIdx)
		{
			return;
		}
		int value = this.blockItemIds[fromIdx];
		int num = (fromIdx >= toIdx) ? -1 : 1;
		for (int num2 = fromIdx; num2 != toIdx; num2 += num)
		{
			this.blockItemIds[num2] = this.blockItemIds[num2 + num];
		}
		this.blockItemIds[toIdx] = value;
		if (this.OnChange != null)
		{
			this.OnChange();
		}
	}

	// Token: 0x0600120D RID: 4621 RVA: 0x0007B860 File Offset: 0x00079C60
	public int TotalCount()
	{
		int num = 0;
		foreach (int num2 in this.blockItemCounts.Values)
		{
			num += num2;
		}
		return num;
	}

	// Token: 0x0600120E RID: 4622 RVA: 0x0007B8C4 File Offset: 0x00079CC4
	public new bool Equals(object obj)
	{
		BlocksInventory blocksInventory = obj as BlocksInventory;
		if (blocksInventory == null)
		{
			return false;
		}
		if (!this.BlockItemIds.Equals(blocksInventory.BlockItemIds))
		{
			return false;
		}
		for (int i = 0; i < this.blockItemIds.Count; i++)
		{
			int blockItemId = this.blockItemIds[i];
			if (this.CountOf(blockItemId) != blocksInventory.CountOf(blockItemId))
			{
				return false;
			}
			if (this.InfinityCountOf(blockItemId) != blocksInventory.InfinityCountOf(blockItemId))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600120F RID: 4623 RVA: 0x0007B94C File Offset: 0x00079D4C
	public void SortByBuildPanelOrder()
	{
		this.blockItemIds.Sort(delegate(int a, int b)
		{
			BlockItem blockItem = BlockItem.FindByID(a);
			BlockItem blockItem2 = BlockItem.FindByID(b);
			if (blockItem == null && blockItem2 == null)
			{
				return 0;
			}
			if (blockItem == null)
			{
				return 1;
			}
			if (blockItem2 == null)
			{
				return -1;
			}
			int num;
			int num2;
			int value;
			int value2;
			if (!PanelSlots.GetBuildPanelInfo(blockItem, out num, out num2) || !PanelSlots.GetBuildPanelInfo(blockItem2, out value, out value2))
			{
				return 0;
			}
			int num3 = num.CompareTo(value);
			if (num3 != 0)
			{
				return num3;
			}
			return num2.CompareTo(value2);
		});
	}

	// Token: 0x06001210 RID: 4624 RVA: 0x0007B978 File Offset: 0x00079D78
	public void EnumerateInventoryWithAction(Action<int, int, int> action)
	{
		foreach (int num in this.blockItemIds)
		{
			action(num, this.blockItemCounts[num], this.blockItemInfinityCounts[num]);
		}
	}

	// Token: 0x06001211 RID: 4625 RVA: 0x0007B9EC File Offset: 0x00079DEC
	public void AddAutomaticallyIncludedItems()
	{
		HashSet<string> hashSet = new HashSet<string>
		{
			"Phantom Block",
			"Unphantom Block",
			"Phantom Model",
			"Unphantom Model",
			"Torsion Spring Rigidity",
			"Torsion Spring Cube Rigidity",
			"Torsion Spring Slab Rigidity"
		};
		foreach (string internalIdentifier in hashSet)
		{
			BlockItem blockItem = BlockItem.FindByInternalIdentifier(internalIdentifier);
			if (!this.BlockItemIds.Contains(blockItem.Id))
			{
				this.Add(blockItem.Id, 0, 1);
			}
		}
		HashSet<string> hashSet2 = new HashSet<string>
		{
			"Block Anim Character Male",
			"Block Anim Character Female",
			"Block Anim Character Mini",
			"Block Anim Character Mini Female",
			"Block Anim Character Random",
			"Block Anim Character Skeleton"
		};
		HashSet<string> hashSet3 = new HashSet<string>
		{
			"Anim Character Block Leg",
			"Anim Character Block Arm",
			"Anim Character Default Leg",
			"Anim Character Default Arm",
			"Anim Character Pants",
			"Anim Character Long Sleeve"
		};
		HashSet<string> hashSet4 = new HashSet<string>
		{
			"Anim Character Skeleton Leg",
			"Anim Character Skeleton Arm"
		};
		bool flag = false;
		bool flag2 = false;
		List<int> list = new List<int>();
		foreach (int id in this.blockItemIds)
		{
			if (BlockItem.Exists(id))
			{
				BlockItem blockItem2 = BlockItem.FindByID(id);
				if (!flag && hashSet2.Contains(blockItem2.InternalIdentifier))
				{
					foreach (string internalIdentifier2 in hashSet3)
					{
						BlockItem blockItem3 = BlockItem.FindByInternalIdentifier(internalIdentifier2);
						if (!this.BlockItemIds.Contains(blockItem3.Id))
						{
							list.Add(blockItem3.Id);
						}
					}
					flag = true;
				}
				if (!flag2 && "Block Anim Character Skeleton" == blockItem2.InternalIdentifier)
				{
					foreach (string internalIdentifier3 in hashSet4)
					{
						BlockItem blockItem4 = BlockItem.FindByInternalIdentifier(internalIdentifier3);
						if (!this.BlockItemIds.Contains(blockItem4.Id))
						{
							list.Add(blockItem4.Id);
						}
					}
					flag2 = true;
				}
			}
		}
		foreach (int blockItemId in list)
		{
			this.Add(blockItemId, 0, 1);
		}
	}

	// Token: 0x06001212 RID: 4626 RVA: 0x0007BD9C File Offset: 0x0007A19C
	public static BlocksInventory operator +(BlocksInventory A, BlocksInventory B)
	{
		BlocksInventory result = new BlocksInventory();
		A.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infiniteCount)
		{
			result.Add(blockItemID, count, infiniteCount);
		});
		B.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infiniteCount)
		{
			result.Add(blockItemID, count, infiniteCount);
		});
		return result;
	}

	// Token: 0x04000E4A RID: 3658
	private List<int> blockItemIds;

	// Token: 0x04000E4B RID: 3659
	private Dictionary<int, int> blockItemCounts;

	// Token: 0x04000E4C RID: 3660
	private Dictionary<int, int> blockItemInfinityCounts;

	// Token: 0x04000E4D RID: 3661
	public Action OnChange;
}
