using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x0200039C RID: 924
public static class BWBlockItemPricing
{
	// Token: 0x0600284B RID: 10315 RVA: 0x00129A94 File Offset: 0x00127E94
	public static void LoadBlockPrices(JObject json)
	{
		BWBlockItemPricing.blockItemPriceData = new Dictionary<int, BlockItemPriceData>();
		foreach (JObject json2 in json["block_items_pricing"].ArrayValue)
		{
			BlockItemPriceData blockItemPriceData = new BlockItemPriceData(json2);
			int blockItemID = blockItemPriceData.blockItemID;
			if (blockItemID > 0 && !BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
			{
				BWBlockItemPricing.blockItemPriceData.Add(blockItemID, blockItemPriceData);
			}
		}
	}

	// Token: 0x0600284C RID: 10316 RVA: 0x00129B30 File Offset: 0x00127F30
	public static int GoldCoins(int blockItemID)
	{
		if (BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
		{
			return BWBlockItemPricing.blockItemPriceData[blockItemID].goldPennies;
		}
		return 0;
	}

	// Token: 0x0600284D RID: 10317 RVA: 0x00129B54 File Offset: 0x00127F54
	public static bool IsCoinsValueFlatRate(int blockItemID)
	{
		return BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID) && BWBlockItemPricing.blockItemPriceData[blockItemID].isCoinsValueFlatRate;
	}

	// Token: 0x0600284E RID: 10318 RVA: 0x00129B78 File Offset: 0x00127F78
	public static int CoinsValueOfBlocksInventory(BlocksInventory inventory)
	{
		int totalGoldPennies = 0;
		bool hasMaxValue = false;
		inventory.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infinityCount)
		{
			int num = BWBlockItemPricing.TotalGoldPenniesForInventoryElement(blockItemID, count, infinityCount);
			if (num == 2147483647)
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

	// Token: 0x0600284F RID: 10319 RVA: 0x00129BC8 File Offset: 0x00127FC8
	public static int CoinsValueOfInventoryElement(int blockItemID, int count, int infinityCount)
	{
		int num = BWBlockItemPricing.TotalGoldPenniesForInventoryElement(blockItemID, count, infinityCount);
		return (num + 99) / 100;
	}

	// Token: 0x06002850 RID: 10320 RVA: 0x00129BE5 File Offset: 0x00127FE5
	public static bool AlaCarteIsAvailable(int blockItemID)
	{
		return BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID) && BWBlockItemPricing.blockItemPriceData[blockItemID].stackPrice > 0;
	}

	// Token: 0x06002851 RID: 10321 RVA: 0x00129C0C File Offset: 0x0012800C
	public static int PackSizeForBlockItemID(int blockItemID)
	{
		if (BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
		{
			return BWBlockItemPricing.blockItemPriceData[blockItemID].stackSize;
		}
		return 0;
	}

	// Token: 0x06002852 RID: 10322 RVA: 0x00129C30 File Offset: 0x00128030
	public static string InfoStringForBlockItemID(int blockItemID)
	{
		int num = BWBlockItemPricing.PackSizeForBlockItemID(blockItemID);
		if (num > 0)
		{
			return (num != 1) ? string.Format("x{0}", num) : string.Empty;
		}
		return "Unlimited";
	}

	// Token: 0x06002853 RID: 10323 RVA: 0x00129C72 File Offset: 0x00128072
	public static int CoinsValueOfBlockItem(int blockItemID, int quantity)
	{
		if (BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
		{
			return BWBlockItemPricing.blockItemPriceData[blockItemID].stackPrice * quantity;
		}
		return int.MaxValue;
	}

	// Token: 0x06002854 RID: 10324 RVA: 0x00129C9C File Offset: 0x0012809C
	public static int BlocksworldPremiumDiscount(int coinsValue)
	{
		return coinsValue * 9 / 10;
	}

	// Token: 0x06002855 RID: 10325 RVA: 0x00129CA8 File Offset: 0x001280A8
	private static int TotalGoldPenniesForInventoryElement(int blockItemID, int count, int infinityCount)
	{
		if (!BWBlockItemPricing.blockItemPriceData.ContainsKey(blockItemID))
		{
			return 0;
		}
		int goldPennies = BWBlockItemPricing.blockItemPriceData[blockItemID].goldPennies;
		bool isCoinsValueFlatRate = BWBlockItemPricing.blockItemPriceData[blockItemID].isCoinsValueFlatRate;
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

	// Token: 0x0400234B RID: 9035
	private static Dictionary<int, BlockItemPriceData> blockItemPriceData;
}
