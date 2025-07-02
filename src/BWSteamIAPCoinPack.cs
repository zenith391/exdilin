using System.Collections.Generic;
using SimpleJSON;

public class BWSteamIAPCoinPack
{
	public int steamIAP_ID;

	public int coins;

	public string internalIdentifier;

	public float price;

	public string imagePath;

	public string label;

	public string priceString;

	public BWSteamIAPCoinPack(JObject json)
	{
		steamIAP_ID = BWJsonHelpers.PropertyIfExists(steamIAP_ID, "steam_iap_id", json);
		coins = BWJsonHelpers.PropertyIfExists(coins, "currency", json);
		internalIdentifier = BWJsonHelpers.PropertyIfExists(internalIdentifier, "internal_identifier", json);
		price = BWJsonHelpers.PropertyIfExists(price, "steam_iap_dollar_price", json);
		label = coins + " Coins";
		priceString = "$" + price.ToString("F2");
		imagePath = "CoinPackImages/" + internalIdentifier;
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		return new Dictionary<string, string>
		{
			{ "label", label },
			{ "imagePath", imagePath },
			{ "price", priceString }
		};
	}
}
