using System.Collections.Generic;

public class UIDataSourceShoppingCart : UIDataSource
{
	private bool includeBlocks;

	private bool includeModels;

	public UIDataSourceShoppingCart(UIDataManager manager, bool blocks, bool models)
		: base(manager)
	{
		includeBlocks = blocks;
		includeModels = models;
		BWShoppingCartEvents.AddShoppingCartDidLoadListener(OnShoppingCartUpdate);
		BWShoppingCartEvents.AddShoppingCartDidClearListener(OnShoppingCartUpdate);
		BWShoppingCartEvents.AddShoppingCartDidAddItemListener(OnShoppingCartItemUpdate);
		BWShoppingCartEvents.AddShoppingCartDidRemoveItemListener(OnShoppingCartRemoveItem);
		BWShoppingCartEvents.AddShoppingCartDidUpdateItemListener(OnShoppingCartItemUpdate);
	}

	private void OnShoppingCartUpdate(BWShoppingCart shoppingCart)
	{
		Refresh();
	}

	private void OnShoppingCartItemUpdate(BWShoppingCart shoppingCart, int index)
	{
		if (base.Keys == null || index >= shoppingCart.contents.Count)
		{
			Refresh();
			return;
		}
		BWShoppingCartItem bWShoppingCartItem = shoppingCart.contents[index];
		string text = null;
		Dictionary<string, string> dictionary = null;
		base.Info["totalPrice"] = shoppingCart.TotalPrice().ToString();
		if (bWShoppingCartItem is BWShoppingCartItemBlockPack && includeBlocks)
		{
			BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
			text = bWShoppingCartItemBlockPack.blockItemID.ToString();
			dictionary = bWShoppingCartItemBlockPack.AttributesForMenuUI();
		}
		else if (bWShoppingCartItem is BWShoppingCartItemModel && includeModels)
		{
			BWShoppingCartItemModel bWShoppingCartItemModel = bWShoppingCartItem as BWShoppingCartItemModel;
			text = bWShoppingCartItemModel.modelID;
			dictionary = bWShoppingCartItemModel.AttributesForMenuUI();
		}
		if (text != null && dictionary != null)
		{
			if (!base.Keys.Contains(text))
			{
				base.Keys.Add(text);
			}
			base.Data[text] = dictionary;
			NotifyListeners(new List<string> { text });
		}
		else
		{
			Refresh();
		}
	}

	private void OnShoppingCartRemoveItem(BWShoppingCart shoppingCart, int index)
	{
		if (base.Keys == null || index >= shoppingCart.contents.Count)
		{
			Refresh();
			return;
		}
		BWShoppingCartItem bWShoppingCartItem = shoppingCart.contents[index];
		string text = null;
		if (bWShoppingCartItem is BWShoppingCartItemBlockPack && includeBlocks)
		{
			BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
			text = bWShoppingCartItemBlockPack.blockItemID.ToString();
		}
		else if (bWShoppingCartItem is BWShoppingCartItemModel && includeModels)
		{
			BWShoppingCartItemModel bWShoppingCartItemModel = bWShoppingCartItem as BWShoppingCartItemModel;
			text = bWShoppingCartItemModel.modelID;
		}
		if (!string.IsNullOrEmpty(text) && base.Keys.Contains(text))
		{
			base.Info["totalPrice"] = shoppingCart.TotalPrice().ToString();
			base.Keys.Remove(text);
			base.Data.Remove(text);
			NotifyListeners();
		}
		else
		{
			Refresh();
		}
	}

	public override void Refresh()
	{
		ClearData();
		List<BWShoppingCartItem> contents = BWStandalone.ShoppingCart.contents;
		for (int i = 0; i < contents.Count; i++)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[i];
			if (includeBlocks && bWShoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
				string text = bWShoppingCartItemBlockPack.blockItemID.ToString();
				base.Keys.Add(text);
				base.Data.Add(text, bWShoppingCartItemBlockPack.AttributesForMenuUI());
			}
			else if (includeModels && bWShoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bWShoppingCartItemModel = bWShoppingCartItem as BWShoppingCartItemModel;
				string modelID = bWShoppingCartItemModel.modelID;
				base.Keys.Add(modelID);
				base.Data.Add(modelID, bWShoppingCartItemModel.AttributesForMenuUI());
			}
		}
		base.Info.Add("totalPrice", BWStandalone.ShoppingCart.TotalPrice().ToString());
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}
}
