using System.Collections.Generic;
using SimpleJSON;

public static class BWBlockItemPricing
{
	private static Dictionary<int, BlockItemPriceData> blockItemPriceData;

	public static void LoadBlockPrices(JObject json)
	{
		BWBlockItemPricing.blockItemPriceData = new Dictionary<int, BlockItemPriceData>();
		foreach (JObject item in json["block_items_pricing"].ArrayValue)
		{
			BlockItemPriceData blockItemPriceData = new BlockItemPriceData(item);
			int blockItemID = blockItemPriceData.blockItemID;
			if (blockItemID > 0 && !BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
			{
				BWBlockItemPricing.blockItemPriceData.Add(blockItemID, blockItemPriceData);
			}
		}
	}

	public static int GoldCoins(int blockItemID)
	{
		if (blockItemPriceData.ContainsKey(blockItemID))
		{
			return blockItemPriceData[blockItemID].goldPennies;
		}
		return 0;
	}

	public static bool IsCoinsValueFlatRate(int blockItemID)
	{
		if (blockItemPriceData.ContainsKey(blockItemID))
		{
			return blockItemPriceData[blockItemID].isCoinsValueFlatRate;
		}
		return false;
	}

	public static int CoinsValueOfBlocksInventory(BlocksInventory inventory)
	{
		int totalGoldPennies = 0;
		bool hasMaxValue = false;
		inventory.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infinityCount)
		{
			int num = TotalGoldPenniesForInventoryElement(blockItemID, count, infinityCount);
			if (num == int.MaxValue)
			{
				hasMaxValue = true;
			}
			else
			{
				totalGoldPennies += num;
			}
		});
		if (hasMaxValue)
		{
			return int.MaxValue;
		}
		return (totalGoldPennies + 99) / 100;
	}

	public static int CoinsValueOfInventoryElement(int blockItemID, int count, int infinityCount)
	{
		int num = TotalGoldPenniesForInventoryElement(blockItemID, count, infinityCount);
		return (num + 99) / 100;
	}

	public static bool AlaCarteIsAvailable(int blockItemID)
	{
		if (blockItemPriceData.ContainsKey(blockItemID))
		{
			return blockItemPriceData[blockItemID].stackPrice > 0;
		}
		return false;
	}

	public static int PackSizeForBlockItemID(int blockItemID)
	{
		if (blockItemPriceData.ContainsKey(blockItemID))
		{
			return blockItemPriceData[blockItemID].stackSize;
		}
		return 0;
	}

	public static string InfoStringForBlockItemID(int blockItemID)
	{
		int num = PackSizeForBlockItemID(blockItemID);
		if (num > 0)
		{
			if (num == 1)
			{
				return string.Empty;
			}
			return $"x{num}";
		}
		return "Unlimited";
	}

	public static int CoinsValueOfBlockItem(int blockItemID, int quantity)
	{
		if (blockItemPriceData.ContainsKey(blockItemID))
		{
			return blockItemPriceData[blockItemID].stackPrice * quantity;
		}
		return int.MaxValue;
	}

	public static int BlocksworldPremiumDiscount(int coinsValue)
	{
		return coinsValue * 9 / 10;
	}

	private static int TotalGoldPenniesForInventoryElement(int blockItemID, int count, int infinityCount)
	{
		if (!blockItemPriceData.ContainsKey(blockItemID))
		{
			return 0;
		}
		int goldPennies = blockItemPriceData[blockItemID].goldPennies;
		bool isCoinsValueFlatRate = blockItemPriceData[blockItemID].isCoinsValueFlatRate;
		if (goldPennies == 0 || (count == 0 && infinityCount == 0))
		{
			return 0;
		}
		if (goldPennies <= -1)
		{
			return int.MaxValue;
		}
		if (isCoinsValueFlatRate)
		{
			return goldPennies;
		}
		if (infinityCount > 0)
		{
			return int.MaxValue;
		}
		return goldPennies * count;
	}
}
