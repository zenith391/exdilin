using System;
using System.Collections.Generic;

// Token: 0x020003F0 RID: 1008
public class UIDataSourceShoppingCart : UIDataSource
{
	// Token: 0x06002C5F RID: 11359 RVA: 0x0013E644 File Offset: 0x0013CA44
	public UIDataSourceShoppingCart(UIDataManager manager, bool blocks, bool models) : base(manager)
	{
		this.includeBlocks = blocks;
		this.includeModels = models;
		BWShoppingCartEvents.AddShoppingCartDidLoadListener(new ShoppingCartEventHandler(this.OnShoppingCartUpdate));
		BWShoppingCartEvents.AddShoppingCartDidClearListener(new ShoppingCartEventHandler(this.OnShoppingCartUpdate));
		BWShoppingCartEvents.AddShoppingCartDidAddItemListener(new ShoppingCartItemEventHandler(this.OnShoppingCartItemUpdate));
		BWShoppingCartEvents.AddShoppingCartDidRemoveItemListener(new ShoppingCartItemEventHandler(this.OnShoppingCartRemoveItem));
		BWShoppingCartEvents.AddShoppingCartDidUpdateItemListener(new ShoppingCartItemEventHandler(this.OnShoppingCartItemUpdate));
	}

	// Token: 0x06002C60 RID: 11360 RVA: 0x0013E6BB File Offset: 0x0013CABB
	private void OnShoppingCartUpdate(BWShoppingCart shoppingCart)
	{
		this.Refresh();
	}

	// Token: 0x06002C61 RID: 11361 RVA: 0x0013E6C4 File Offset: 0x0013CAC4
	private void OnShoppingCartItemUpdate(BWShoppingCart shoppingCart, int index)
	{
		if (base.Keys == null || index >= shoppingCart.contents.Count)
		{
			this.Refresh();
			return;
		}
		BWShoppingCartItem bwshoppingCartItem = shoppingCart.contents[index];
		string text = null;
		Dictionary<string, string> dictionary = null;
		base.Info["totalPrice"] = shoppingCart.TotalPrice().ToString();
		if (bwshoppingCartItem is BWShoppingCartItemBlockPack && this.includeBlocks)
		{
			BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
			text = bwshoppingCartItemBlockPack.blockItemID.ToString();
			dictionary = bwshoppingCartItemBlockPack.AttributesForMenuUI();
		}
		else if (bwshoppingCartItem is BWShoppingCartItemModel && this.includeModels)
		{
			BWShoppingCartItemModel bwshoppingCartItemModel = bwshoppingCartItem as BWShoppingCartItemModel;
			text = bwshoppingCartItemModel.modelID;
			dictionary = bwshoppingCartItemModel.AttributesForMenuUI();
		}
		if (text != null && dictionary != null)
		{
			if (!base.Keys.Contains(text))
			{
				base.Keys.Add(text);
			}
			base.Data[text] = dictionary;
			base.NotifyListeners(new List<string>
			{
				text
			});
		}
		else
		{
			this.Refresh();
		}
	}

	// Token: 0x06002C62 RID: 11362 RVA: 0x0013E7F0 File Offset: 0x0013CBF0
	private void OnShoppingCartRemoveItem(BWShoppingCart shoppingCart, int index)
	{
		if (base.Keys == null || index >= shoppingCart.contents.Count)
		{
			this.Refresh();
			return;
		}
		BWShoppingCartItem bwshoppingCartItem = shoppingCart.contents[index];
		string text = null;
		if (bwshoppingCartItem is BWShoppingCartItemBlockPack && this.includeBlocks)
		{
			BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
			text = bwshoppingCartItemBlockPack.blockItemID.ToString();
		}
		else if (bwshoppingCartItem is BWShoppingCartItemModel && this.includeModels)
		{
			BWShoppingCartItemModel bwshoppingCartItemModel = bwshoppingCartItem as BWShoppingCartItemModel;
			text = bwshoppingCartItemModel.modelID;
		}
		if (!string.IsNullOrEmpty(text) && base.Keys.Contains(text))
		{
			base.Info["totalPrice"] = shoppingCart.TotalPrice().ToString();
			base.Keys.Remove(text);
			base.Data.Remove(text);
			base.NotifyListeners();
		}
		else
		{
			this.Refresh();
		}
	}

	// Token: 0x06002C63 RID: 11363 RVA: 0x0013E8F4 File Offset: 0x0013CCF4
	public override void Refresh()
	{
		base.ClearData();
		List<BWShoppingCartItem> contents = BWStandalone.ShoppingCart.contents;
		for (int i = 0; i < contents.Count; i++)
		{
			BWShoppingCartItem bwshoppingCartItem = contents[i];
			if (this.includeBlocks && bwshoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
				string text = bwshoppingCartItemBlockPack.blockItemID.ToString();
				base.Keys.Add(text);
				base.Data.Add(text, bwshoppingCartItemBlockPack.AttributesForMenuUI());
			}
			else if (this.includeModels && bwshoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bwshoppingCartItemModel = bwshoppingCartItem as BWShoppingCartItemModel;
				string modelID = bwshoppingCartItemModel.modelID;
				base.Keys.Add(modelID);
				base.Data.Add(modelID, bwshoppingCartItemModel.AttributesForMenuUI());
			}
		}
		base.Info.Add("totalPrice", BWStandalone.ShoppingCart.TotalPrice().ToString());
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x04002539 RID: 9529
	private bool includeBlocks;

	// Token: 0x0400253A RID: 9530
	private bool includeModels;
}
