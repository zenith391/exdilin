using System;
using System.Collections.Generic;
using System.Text;

public class BlocksInventory
{
	private List<int> blockItemIds;

	private Dictionary<int, int> blockItemCounts;

	private Dictionary<int, int> blockItemInfinityCounts;

	public Action OnChange;

	public List<int> BlockItemIds => blockItemIds;

	public BlocksInventory()
	{
		blockItemIds = new List<int>();
		blockItemCounts = new Dictionary<int, int>();
		blockItemInfinityCounts = new Dictionary<int, int>();
	}

	public static BlocksInventory CreateUnlimited()
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		foreach (BlockItem allBlockItem in BlockItem.AllBlockItems)
		{
			blocksInventory.Add(allBlockItem.Id, 0, 1);
		}
		return blocksInventory;
	}

	public static BlocksInventory FromString(string blocksInventoryStr, bool verbose = true)
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		blocksInventory.blockItemIds = new List<int>();
		blocksInventory.blockItemCounts = new Dictionary<int, int>();
		blocksInventory.blockItemInfinityCounts = new Dictionary<int, int>();
		if (blocksInventoryStr.Length > 0)
		{
			string[] array = blocksInventoryStr.Split('|');
			int num = 0;
			int num2 = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(':');
				if (!int.TryParse(array3[0], out var result))
				{
					if (verbose)
					{
						BWLog.Error("Failed to parse block id " + text + " in block inventory string (expected to be int)");
					}
					continue;
				}
				num = 0;
				num2 = 0;
				string text2 = array3[1];
				string[] array4 = text2.Split(';');
				int.TryParse(array4[0], out num);
				if (array4.Length == 2)
				{
					int.TryParse(array4[1], out num2);
				}
				blocksInventory.blockItemIds.Add(result);
				blocksInventory.blockItemCounts[result] = num;
				blocksInventory.blockItemInfinityCounts[result] = num2;
			}
		}
		return blocksInventory;
	}

	public new string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (int blockItemId in blockItemIds)
		{
			stringBuilder.Append($"{blockItemId}:{blockItemCounts[blockItemId]};{blockItemInfinityCounts[blockItemId]}");
			num++;
			if (num < blockItemIds.Count)
			{
				stringBuilder.Append('|');
			}
		}
		return stringBuilder.ToString();
	}

	public bool ContainsBlockItemId(int blockItemId)
	{
		return blockItemCounts.ContainsKey(blockItemId);
	}

	public bool ContainsBlockItemIdentifier(string blockItemIdentifier)
	{
		BlockItem blockItem = BlockItem.FindByInternalIdentifier(blockItemIdentifier);
		if (blockItem != null)
		{
			return ContainsBlockItemId(blockItem.Id);
		}
		return false;
	}

	public void Add(int blockItemId, int count, int infinityCount = 0)
	{
		if (!ContainsBlockItemId(blockItemId))
		{
			blockItemIds.Add(blockItemId);
			blockItemCounts[blockItemId] = count;
			blockItemInfinityCounts[blockItemId] = infinityCount;
		}
		else
		{
			blockItemCounts[blockItemId] += count;
			blockItemInfinityCounts[blockItemId] += infinityCount;
		}
		if (OnChange != null)
		{
			OnChange();
		}
	}

	public int CountOf(int blockItemId)
	{
		if (blockItemCounts.ContainsKey(blockItemId))
		{
			return blockItemCounts[blockItemId];
		}
		return 0;
	}

	public void SetCountFor(int blockItemId, int count)
	{
		if (!ContainsBlockItemId(blockItemId))
		{
			blockItemIds.Add(blockItemId);
		}
		blockItemCounts[blockItemId] = count;
		if (OnChange != null)
		{
			OnChange();
		}
	}

	public int InfinityCountOf(int blockItemId)
	{
		if (blockItemInfinityCounts.ContainsKey(blockItemId))
		{
			return blockItemInfinityCounts[blockItemId];
		}
		return 0;
	}

	public void SetInfinityCountFor(int blockItemId, int infinityCount)
	{
		if (!ContainsBlockItemId(blockItemId))
		{
			blockItemIds.Add(blockItemId);
		}
		blockItemInfinityCounts[blockItemId] = infinityCount;
		if (OnChange != null)
		{
			OnChange();
		}
	}

	public int CountOrMinusOneIfInfinityOf(int blockItemId)
	{
		if (InfinityCountOf(blockItemId) > 0)
		{
			return -1;
		}
		return CountOf(blockItemId);
	}

	public void ReorderBlockItemIndex(int fromIdx, int toIdx)
	{
		if (fromIdx >= 0 && toIdx >= 0 && fromIdx < blockItemIds.Count && toIdx < blockItemIds.Count && fromIdx != toIdx)
		{
			int value = blockItemIds[fromIdx];
			int num = ((fromIdx < toIdx) ? 1 : (-1));
			for (int i = fromIdx; i != toIdx; i += num)
			{
				blockItemIds[i] = blockItemIds[i + num];
			}
			blockItemIds[toIdx] = value;
			if (OnChange != null)
			{
				OnChange();
			}
		}
	}

	public int TotalCount()
	{
		int num = 0;
		foreach (int value in blockItemCounts.Values)
		{
			num += value;
		}
		return num;
	}

	public new bool Equals(object obj)
	{
		if (!(obj is BlocksInventory blocksInventory))
		{
			return false;
		}
		if (!BlockItemIds.Equals(blocksInventory.BlockItemIds))
		{
			return false;
		}
		for (int i = 0; i < blockItemIds.Count; i++)
		{
			int blockItemId = blockItemIds[i];
			if (CountOf(blockItemId) != blocksInventory.CountOf(blockItemId))
			{
				return false;
			}
			if (InfinityCountOf(blockItemId) != blocksInventory.InfinityCountOf(blockItemId))
			{
				return false;
			}
		}
		return true;
	}

	public void SortByBuildPanelOrder()
	{
		blockItemIds.Sort(delegate(int a, int b)
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
			if (!PanelSlots.GetBuildPanelInfo(blockItem, out var tabIndex, out var positionInTab) || !PanelSlots.GetBuildPanelInfo(blockItem2, out var tabIndex2, out var positionInTab2))
			{
				return 0;
			}
			int num = tabIndex.CompareTo(tabIndex2);
			return (num != 0) ? num : positionInTab.CompareTo(positionInTab2);
		});
	}

	public void EnumerateInventoryWithAction(Action<int, int, int> action)
	{
		foreach (int blockItemId in blockItemIds)
		{
			action(blockItemId, blockItemCounts[blockItemId], blockItemInfinityCounts[blockItemId]);
		}
	}

	public void AddAutomaticallyIncludedItems()
	{
		HashSet<string> hashSet = new HashSet<string> { "Phantom Block", "Unphantom Block", "Phantom Model", "Unphantom Model", "Torsion Spring Rigidity", "Torsion Spring Cube Rigidity", "Torsion Spring Slab Rigidity" };
		foreach (string item in hashSet)
		{
			BlockItem blockItem = BlockItem.FindByInternalIdentifier(item);
			if (!BlockItemIds.Contains(blockItem.Id))
			{
				Add(blockItem.Id, 0, 1);
			}
		}
		HashSet<string> hashSet2 = new HashSet<string> { "Block Anim Character Male", "Block Anim Character Female", "Block Anim Character Mini", "Block Anim Character Mini Female", "Block Anim Character Random", "Block Anim Character Skeleton" };
		HashSet<string> hashSet3 = new HashSet<string> { "Anim Character Block Leg", "Anim Character Block Arm", "Anim Character Default Leg", "Anim Character Default Arm", "Anim Character Pants", "Anim Character Long Sleeve" };
		HashSet<string> hashSet4 = new HashSet<string> { "Anim Character Skeleton Leg", "Anim Character Skeleton Arm" };
		bool flag = false;
		bool flag2 = false;
		List<int> list = new List<int>();
		foreach (int blockItemId in blockItemIds)
		{
			if (!BlockItem.Exists(blockItemId))
			{
				continue;
			}
			BlockItem blockItem2 = BlockItem.FindByID(blockItemId);
			if (!flag && hashSet2.Contains(blockItem2.InternalIdentifier))
			{
				foreach (string item2 in hashSet3)
				{
					BlockItem blockItem3 = BlockItem.FindByInternalIdentifier(item2);
					if (!BlockItemIds.Contains(blockItem3.Id))
					{
						list.Add(blockItem3.Id);
					}
				}
				flag = true;
			}
			if (flag2 || !("Block Anim Character Skeleton" == blockItem2.InternalIdentifier))
			{
				continue;
			}
			foreach (string item3 in hashSet4)
			{
				BlockItem blockItem4 = BlockItem.FindByInternalIdentifier(item3);
				if (!BlockItemIds.Contains(blockItem4.Id))
				{
					list.Add(blockItem4.Id);
				}
			}
			flag2 = true;
		}
		foreach (int item4 in list)
		{
			Add(item4, 0, 1);
		}
	}

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
}
