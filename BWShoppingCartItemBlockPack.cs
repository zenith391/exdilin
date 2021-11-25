using System;
using System.Collections.Generic;

// Token: 0x020003B6 RID: 950
public class BWShoppingCartItemBlockPack : BWShoppingCartItem
{
	// Token: 0x06002946 RID: 10566 RVA: 0x0012E620 File Offset: 0x0012CA20
	public BWShoppingCartItemBlockPack(int blockItemID, int count)
	{
		this.count = count;
		this.blockItemID = blockItemID;
		this.blockItem = BlockItem.FindByID(blockItemID);
		this.packPrice = BWBlockItemPricing.CoinsValueOfBlockItem(blockItemID, 1);
		this.packInfo = BWBlockItemPricing.InfoStringForBlockItemID(blockItemID);
	}

	// Token: 0x06002947 RID: 10567 RVA: 0x0012E674 File Offset: 0x0012CA74
	public override Dictionary<string, object> AttrsForSave()
	{
		return new Dictionary<string, object>
		{
			{
				"type",
				"block-pack"
			},
			{
				"block-item-id",
				this.blockItemID
			},
			{
				"count",
				this.count
			}
		};
	}

	// Token: 0x06002948 RID: 10568 RVA: 0x0012E6C4 File Offset: 0x0012CAC4
	public Dictionary<string, string> AttributesForMenuUI()
	{
		int num = this.Price();
		return new Dictionary<string, string>
		{
			{
				"blockItemName",
				this.blockItem.Title
			},
			{
				"blockItemPackPrice",
				this.packPrice.ToString()
			},
			{
				"blockItemPackInfo",
				this.packInfo
			},
			{
				"blockItemIdentifier",
				this.blockItem.InternalIdentifier
			},
			{
				"price",
				num.ToString()
			},
			{
				"count",
				this.count.ToString()
			}
		};
	}

	// Token: 0x06002949 RID: 10569 RVA: 0x0012E770 File Offset: 0x0012CB70
	public override int Price()
	{
		int num = this.packPrice * this.count;
		if (BWUser.currentUser.IsPremiumUser())
		{
			num = BWBlockItemPricing.BlocksworldPremiumDiscount(num);
		}
		return num;
	}

	// Token: 0x040023BA RID: 9146
	public int blockItemID;

	// Token: 0x040023BB RID: 9147
	private BlockItem blockItem;

	// Token: 0x040023BC RID: 9148
	private int packPrice;

	// Token: 0x040023BD RID: 9149
	private string packInfo = string.Empty;

	// Token: 0x040023BE RID: 9150
	public int infiniteCount;
}
