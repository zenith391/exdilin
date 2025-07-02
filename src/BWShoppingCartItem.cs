using System.Collections.Generic;
using SimpleJSON;

public abstract class BWShoppingCartItem
{
	public int count;

	public BWPurchaseStatus purchaseStatus;

	public string purchaseFailureMessage;

	public static BWShoppingCartItem FromJson(JObject json)
	{
		string stringValue = json["type"].StringValue;
		int intValue = json["count"].IntValue;
		BWShoppingCartItem result = null;
		switch (stringValue)
		{
		case "u2u-model":
		{
			string stringValue2 = json["model-id"].StringValue;
			bool property = false;
			property = BWJsonHelpers.PropertyIfExists(property, "blueprint-only", json);
			result = new BWShoppingCartItemModel(stringValue2, intValue, property);
			break;
		}
		case "block-pack":
		{
			int intValue2 = json["block-item-id"].IntValue;
			result = new BWShoppingCartItemBlockPack(intValue2, intValue);
			break;
		}
		}
		return result;
	}

	public abstract Dictionary<string, object> AttrsForSave();

	public abstract int Price();
}
