using System.Collections.Generic;

public class UIDataSourceBlockShopContents : UIDataSource
{
	private string blockShopCategoryID;

	private string blockShopSection;

	public UIDataSourceBlockShopContents(UIDataManager manager, string categoryID, string sectionTitle)
		: base(manager)
	{
		blockShopCategoryID = categoryID;
		blockShopSection = sectionTitle;
	}

	public override void Refresh()
	{
		ClearData();
		List<BlockItem> blockItemsForShop = BWBlockShopData.GetBlockItemsForShop(blockShopCategoryID, blockShopSection);
		if (blockItemsForShop != null)
		{
			foreach (BlockItem item in blockItemsForShop)
			{
				int id = item.Id;
				string internalIdentifier = item.InternalIdentifier;
				if (IsBlockItemAvailableInShop(id))
				{
					string title = item.Title;
					int num = BWBlockItemPricing.CoinsValueOfBlockItem(id, 1);
					string value = BWBlockItemPricing.InfoStringForBlockItemID(id);
					Dictionary<string, string> value2 = new Dictionary<string, string>
					{
						{ "sectionTitle", blockShopSection },
						{ "blockItemName", title },
						{
							"blockItemPackPrice",
							num.ToString()
						},
						{ "blockItemPackInfo", value },
						{ "blockItemIdentifier", internalIdentifier }
					};
					base.Keys.Add(id.ToString());
					base.Data.Add(id.ToString(), value2);
				}
			}
		}
		base.loadState = LoadState.Loaded;
	}

	private bool IsBlockItemAvailableInShop(int blockItemID)
	{
		if (!BWBlockItemPricing.AlaCarteIsAvailable(blockItemID))
		{
			return false;
		}
		BlocksInventory blocksInventory = BWUser.currentUser.blocksInventory;
		int num = blocksInventory.InfinityCountOf(blockItemID);
		if (num > 0)
		{
			return false;
		}
		int num2 = blocksInventory.CountOf(blockItemID);
		int num3 = BWBlockItemPricing.PackSizeForBlockItemID(blockItemID);
		if (num2 > 0)
		{
			return num3 != 0;
		}
		return true;
	}
}
