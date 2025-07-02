using System.Collections.Generic;

public class BWShoppingCartItemModel : BWShoppingCartItem
{
	public string modelID;

	public bool blueprintOnly;

	public BWU2UModel model;

	public BWShoppingCartItemModel(BWU2UModel model, int count, bool blueprintOnly)
	{
		this.model = model;
		modelID = model.modelID;
		base.count = count;
		this.blueprintOnly = blueprintOnly;
	}

	public BWShoppingCartItemModel(string modelID, int count, bool blueprintOnly)
	{
		this.modelID = modelID;
		base.count = count;
		this.blueprintOnly = blueprintOnly;
		model = BWU2UModelDataManager.Instance.GetCachedModel(modelID);
	}

	public override Dictionary<string, object> AttrsForSave()
	{
		return new Dictionary<string, object>
		{
			{ "type", "u2u-model" },
			{ "model-id", modelID },
			{
				"blueprint-only",
				blueprintOnly.ToString()
			},
			{ "count", count }
		};
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		if (model == null)
		{
			return new Dictionary<string, string>();
		}
		Dictionary<string, string> dictionary = model.AttributesForMenuUI();
		dictionary.Add("count", count.ToString());
		dictionary.Add("price", Price().ToString());
		return dictionary;
	}

	public override int Price()
	{
		if (model == null)
		{
			return 0;
		}
		if (BWUser.currentUser.IsPremiumUser())
		{
			if (blueprintOnly)
			{
				return model.blocksworldPremiumCoinsPriceBlueprint * count;
			}
			return model.blocksworldPremiumCoinsPrice * count;
		}
		if (blueprintOnly)
		{
			return model.coinsPriceBlueprint * count;
		}
		return model.coinsPrice * count;
	}
}
