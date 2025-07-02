using System.Collections.Generic;

public class BWShoppingCartItemBlockPack : BWShoppingCartItem
{
	public int blockItemID;

	private BlockItem blockItem;

	private int packPrice;

	private string packInfo = string.Empty;

	public int infiniteCount;

	public BWShoppingCartItemBlockPack(int blockItemID, int count)
	{
		base.count = count;
		this.blockItemID = blockItemID;
		blockItem = BlockItem.FindByID(blockItemID);
		packPrice = BWBlockItemPricing.CoinsValueOfBlockItem(blockItemID, 1);
		packInfo = BWBlockItemPricing.InfoStringForBlockItemID(blockItemID);
	}

	public override Dictionary<string, object> AttrsForSave()
	{
		return new Dictionary<string, object>
		{
			{ "type", "block-pack" },
			{ "block-item-id", blockItemID },
			{ "count", count }
		};
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		int num = Price();
		return new Dictionary<string, string>
		{
			{ "blockItemName", blockItem.Title },
			{
				"blockItemPackPrice",
				packPrice.ToString()
			},
			{ "blockItemPackInfo", packInfo },
			{ "blockItemIdentifier", blockItem.InternalIdentifier },
			{
				"price",
				num.ToString()
			},
			{
				"count",
				count.ToString()
			}
		};
	}

	public override int Price()
	{
		int num = packPrice * count;
		if (BWUser.currentUser.IsPremiumUser())
		{
			num = BWBlockItemPricing.BlocksworldPremiumDiscount(num);
		}
		return num;
	}
}
