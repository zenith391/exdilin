using System;
using System.Collections.Generic;

// Token: 0x020003E1 RID: 993
public class UIDataSourceBlockShopContents : UIDataSource
{
	// Token: 0x06002C09 RID: 11273 RVA: 0x0013CB12 File Offset: 0x0013AF12
	public UIDataSourceBlockShopContents(UIDataManager manager, string categoryID, string sectionTitle) : base(manager)
	{
		this.blockShopCategoryID = categoryID;
		this.blockShopSection = sectionTitle;
	}

	// Token: 0x06002C0A RID: 11274 RVA: 0x0013CB2C File Offset: 0x0013AF2C
	public override void Refresh()
	{
		base.ClearData();
		List<BlockItem> blockItemsForShop = BWBlockShopData.GetBlockItemsForShop(this.blockShopCategoryID, this.blockShopSection);
		if (blockItemsForShop != null)
		{
			foreach (BlockItem blockItem in blockItemsForShop)
			{
				int id = blockItem.Id;
				string internalIdentifier = blockItem.InternalIdentifier;
				if (this.IsBlockItemAvailableInShop(id))
				{
					string title = blockItem.Title;
					int num = BWBlockItemPricing.CoinsValueOfBlockItem(id, 1);
					string value = BWBlockItemPricing.InfoStringForBlockItemID(id);
					Dictionary<string, string> value2 = new Dictionary<string, string>
					{
						{
							"sectionTitle",
							this.blockShopSection
						},
						{
							"blockItemName",
							title
						},
						{
							"blockItemPackPrice",
							num.ToString()
						},
						{
							"blockItemPackInfo",
							value
						},
						{
							"blockItemIdentifier",
							internalIdentifier
						}
					};
					base.Keys.Add(id.ToString());
					base.Data.Add(id.ToString(), value2);
				}
			}
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}

	// Token: 0x06002C0B RID: 11275 RVA: 0x0013CC6C File Offset: 0x0013B06C
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
		return num2 <= 0 || num3 != 0;
	}

	// Token: 0x04002525 RID: 9509
	private string blockShopCategoryID;

	// Token: 0x04002526 RID: 9510
	private string blockShopSection;
}
