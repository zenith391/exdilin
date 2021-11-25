using System;
using System.Collections.Generic;

// Token: 0x020003B7 RID: 951
public class BWShoppingCartItemModel : BWShoppingCartItem
{
	// Token: 0x0600294A RID: 10570 RVA: 0x0012E7A2 File Offset: 0x0012CBA2
	public BWShoppingCartItemModel(BWU2UModel model, int count, bool blueprintOnly)
	{
		this.model = model;
		this.modelID = model.modelID;
		this.count = count;
		this.blueprintOnly = blueprintOnly;
	}

	// Token: 0x0600294B RID: 10571 RVA: 0x0012E7CB File Offset: 0x0012CBCB
	public BWShoppingCartItemModel(string modelID, int count, bool blueprintOnly)
	{
		this.modelID = modelID;
		this.count = count;
		this.blueprintOnly = blueprintOnly;
		this.model = BWU2UModelDataManager.Instance.GetCachedModel(modelID);
	}

	// Token: 0x0600294C RID: 10572 RVA: 0x0012E7FC File Offset: 0x0012CBFC
	public override Dictionary<string, object> AttrsForSave()
	{
		return new Dictionary<string, object>
		{
			{
				"type",
				"u2u-model"
			},
			{
				"model-id",
				this.modelID
			},
			{
				"blueprint-only",
				this.blueprintOnly.ToString()
			},
			{
				"count",
				this.count
			}
		};
	}

	// Token: 0x0600294D RID: 10573 RVA: 0x0012E864 File Offset: 0x0012CC64
	public Dictionary<string, string> AttributesForMenuUI()
	{
		if (this.model == null)
		{
			return new Dictionary<string, string>();
		}
		Dictionary<string, string> dictionary = this.model.AttributesForMenuUI();
		dictionary.Add("count", this.count.ToString());
		dictionary.Add("price", this.Price().ToString());
		return dictionary;
	}

	// Token: 0x0600294E RID: 10574 RVA: 0x0012E8CC File Offset: 0x0012CCCC
	public override int Price()
	{
		if (this.model == null)
		{
			return 0;
		}
		if (BWUser.currentUser.IsPremiumUser())
		{
			if (this.blueprintOnly)
			{
				return this.model.blocksworldPremiumCoinsPriceBlueprint * this.count;
			}
			return this.model.blocksworldPremiumCoinsPrice * this.count;
		}
		else
		{
			if (this.blueprintOnly)
			{
				return this.model.coinsPriceBlueprint * this.count;
			}
			return this.model.coinsPrice * this.count;
		}
	}

	// Token: 0x040023BF RID: 9151
	public string modelID;

	// Token: 0x040023C0 RID: 9152
	public bool blueprintOnly;

	// Token: 0x040023C1 RID: 9153
	public BWU2UModel model;
}
