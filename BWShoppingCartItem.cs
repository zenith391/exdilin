using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003B5 RID: 949
public abstract class BWShoppingCartItem
{
	// Token: 0x06002943 RID: 10563 RVA: 0x0012E570 File Offset: 0x0012C970
	public static BWShoppingCartItem FromJson(JObject json)
	{
		string stringValue = json["type"].StringValue;
		int intValue = json["count"].IntValue;
		BWShoppingCartItem result = null;
		if (stringValue != null)
		{
			if (!(stringValue == "block-pack"))
			{
				if (stringValue == "u2u-model")
				{
					string stringValue2 = json["model-id"].StringValue;
					bool flag = false;
					flag = BWJsonHelpers.PropertyIfExists(flag, "blueprint-only", json);
					result = new BWShoppingCartItemModel(stringValue2, intValue, flag);
				}
			}
			else
			{
				int intValue2 = json["block-item-id"].IntValue;
				result = new BWShoppingCartItemBlockPack(intValue2, intValue);
			}
		}
		return result;
	}

	// Token: 0x06002944 RID: 10564
	public abstract Dictionary<string, object> AttrsForSave();

	// Token: 0x06002945 RID: 10565
	public abstract int Price();

	// Token: 0x040023B7 RID: 9143
	public int count;

	// Token: 0x040023B8 RID: 9144
	public BWPurchaseStatus purchaseStatus;

	// Token: 0x040023B9 RID: 9145
	public string purchaseFailureMessage;
}
