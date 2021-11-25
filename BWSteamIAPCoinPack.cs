using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003BD RID: 957
public class BWSteamIAPCoinPack
{
	// Token: 0x060029B7 RID: 10679 RVA: 0x0013202C File Offset: 0x0013042C
	public BWSteamIAPCoinPack(JObject json)
	{
		this.steamIAP_ID = BWJsonHelpers.PropertyIfExists(this.steamIAP_ID, "steam_iap_id", json);
		this.coins = BWJsonHelpers.PropertyIfExists(this.coins, "currency", json);
		this.internalIdentifier = BWJsonHelpers.PropertyIfExists(this.internalIdentifier, "internal_identifier", json);
		this.price = BWJsonHelpers.PropertyIfExists(this.price, "steam_iap_dollar_price", json);
		this.label = this.coins.ToString() + " Coins";
		this.priceString = "$" + this.price.ToString("F2");
		this.imagePath = "CoinPackImages/" + this.internalIdentifier;
	}

	// Token: 0x060029B8 RID: 10680 RVA: 0x001320F4 File Offset: 0x001304F4
	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{
				"label",
				this.label
			},
			{
				"imagePath",
				this.imagePath
			},
			{
				"price",
				this.priceString
			}
		};
	}

	// Token: 0x040023F3 RID: 9203
	public int steamIAP_ID;

	// Token: 0x040023F4 RID: 9204
	public int coins;

	// Token: 0x040023F5 RID: 9205
	public string internalIdentifier;

	// Token: 0x040023F6 RID: 9206
	public float price;

	// Token: 0x040023F7 RID: 9207
	public string imagePath;

	// Token: 0x040023F8 RID: 9208
	public string label;

	// Token: 0x040023F9 RID: 9209
	public string priceString;
}
