using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003B0 RID: 944
public class BWShoppingCart
{
	// Token: 0x06002909 RID: 10505 RVA: 0x0012D220 File Offset: 0x0012B620
	public BWShoppingCart()
	{
		this.contents = new List<BWShoppingCartItem>();
		this.LoadContents();
	}

	// Token: 0x170001BD RID: 445
	// (get) Token: 0x0600290A RID: 10506 RVA: 0x0012D23C File Offset: 0x0012B63C
	public int itemCount
	{
		get
		{
			int num = 0;
			foreach (BWShoppingCartItem bwshoppingCartItem in this.contents)
			{
				if (bwshoppingCartItem.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
				{
					num++;
				}
			}
			return num;
		}
	}

	// Token: 0x0600290B RID: 10507 RVA: 0x0012D2A4 File Offset: 0x0012B6A4
	private void LoadContents()
	{
		this.contents.Clear();
		JObject jobject = BWUserDataManager.Instance.LoadShoppingCartJSON();
		if (jobject != null)
		{
			foreach (JObject json in jobject.ArrayValue)
			{
				BWShoppingCartItem bwshoppingCartItem = BWShoppingCartItem.FromJson(json);
				if (bwshoppingCartItem != null)
				{
					this.contents.Add(bwshoppingCartItem);
				}
			}
		}
		BWShoppingCartEvents.OnShoppingCartLoad(this);
		int i;
		for (i = 0; i < this.contents.Count; i++)
		{
			if (this.contents[i] is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel modelItem = this.contents[i] as BWShoppingCartItemModel;
				if (modelItem.model == null)
				{
					BWU2UModelDataManager.Instance.LoadModelFromRemote(modelItem.modelID, delegate(BWU2UModel model)
					{
						modelItem.model = model;
						BWShoppingCartEvents.OnShoppingCartUpdateItem(this, i);
					}, null);
				}
			}
		}
	}

	// Token: 0x0600290C RID: 10508 RVA: 0x0012D3F8 File Offset: 0x0012B7F8
	private void SaveContents()
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (BWShoppingCartItem bwshoppingCartItem in this.contents)
		{
			list.Add(bwshoppingCartItem.AttrsForSave());
		}
		BWUserDataManager.Instance.SaveShoppingCartJSON(JSONEncoder.Encode(list));
	}

	// Token: 0x0600290D RID: 10509 RVA: 0x0012D470 File Offset: 0x0012B870
	private BlocksInventory ConvertToBlocksInventory(List<BWShoppingCartItemBlockPack> items)
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		foreach (BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack in items)
		{
			int num = BWBlockItemPricing.PackSizeForBlockItemID(bwshoppingCartItemBlockPack.blockItemID);
			if (num > 0)
			{
				blocksInventory.Add(bwshoppingCartItemBlockPack.blockItemID, bwshoppingCartItemBlockPack.count, 0);
			}
			else
			{
				blocksInventory.SetCountFor(bwshoppingCartItemBlockPack.blockItemID, 0);
				blocksInventory.SetInfinityCountFor(bwshoppingCartItemBlockPack.blockItemID, 1);
			}
		}
		return blocksInventory;
	}

	// Token: 0x0600290E RID: 10510 RVA: 0x0012D50C File Offset: 0x0012B90C
	private List<BWShoppingCartItemBlockPack> ConvertFromBlocksInventory(BlocksInventory blocksInventory)
	{
		List<BWShoppingCartItemBlockPack> list = new List<BWShoppingCartItemBlockPack>();
		blocksInventory.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infiniteCount)
		{
			if (!BWBlockItemPricing.AlaCarteIsAvailable(blockItemID))
			{
				return;
			}
			if (BlockItem.FindByID(blockItemID) == null)
			{
				return;
			}
			BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = new BWShoppingCartItemBlockPack(blockItemID, count);
			if (BWBlockItemPricing.PackSizeForBlockItemID(blockItemID) == 0)
			{
				bwshoppingCartItemBlockPack.count = 0;
				bwshoppingCartItemBlockPack.infiniteCount = 1;
			}
			list.Add(bwshoppingCartItemBlockPack);
		});
		return list;
	}

	// Token: 0x0600290F RID: 10511 RVA: 0x0012D544 File Offset: 0x0012B944
	public void AddBlockItemPack(int blockItemID, int packCount)
	{
		bool flag = BWBlockItemPricing.PackSizeForBlockItemID(blockItemID) == 0;
		for (int i = 0; i < this.contents.Count; i++)
		{
			BWShoppingCartItem bwshoppingCartItem = this.contents[i];
			if (bwshoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
				if (bwshoppingCartItemBlockPack.blockItemID == blockItemID)
				{
					if (!flag && bwshoppingCartItem.count < 999)
					{
						bwshoppingCartItem.count += packCount;
						bwshoppingCartItem.count = Mathf.Min(999, bwshoppingCartItem.count);
						this.SaveContents();
					}
					BWShoppingCartEvents.OnShoppingCartUpdateItem(this, i);
					return;
				}
			}
		}
		BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack2 = new BWShoppingCartItemBlockPack(blockItemID, packCount);
		if (flag)
		{
			bwshoppingCartItemBlockPack2.count = 0;
			bwshoppingCartItemBlockPack2.infiniteCount = 1;
		}
		this.contents.Add(bwshoppingCartItemBlockPack2);
		this.SaveContents();
		BWShoppingCartEvents.OnShoppingCartAddItem(this, this.contents.Count - 1);
	}

	// Token: 0x06002910 RID: 10512 RVA: 0x0012D630 File Offset: 0x0012BA30
	public void RemoveBlockItemPack(int blockItemID, int packCount)
	{
		for (int i = this.contents.Count - 1; i >= 0; i--)
		{
			BWShoppingCartItem bwshoppingCartItem = this.contents[i];
			if (bwshoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
				if (bwshoppingCartItemBlockPack.blockItemID == blockItemID)
				{
					if (bwshoppingCartItem.count > 1)
					{
						bwshoppingCartItem.count -= packCount;
						bwshoppingCartItem.count = Mathf.Max(1, bwshoppingCartItem.count);
						this.SaveContents();
						BWShoppingCartEvents.OnShoppingCartUpdateItem(this, i);
					}
					return;
				}
			}
		}
	}

	// Token: 0x06002911 RID: 10513 RVA: 0x0012D6C0 File Offset: 0x0012BAC0
	public void ClearBlockItemPacks(int blockItemID)
	{
		for (int i = this.contents.Count - 1; i >= 0; i--)
		{
			BWShoppingCartItem bwshoppingCartItem = this.contents[i];
			if (bwshoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bwshoppingCartItemBlockPack = bwshoppingCartItem as BWShoppingCartItemBlockPack;
				if (bwshoppingCartItemBlockPack.blockItemID == blockItemID)
				{
					bwshoppingCartItem.purchaseStatus = BWPurchaseStatus.DO_NOT_PURCHASE;
					BWShoppingCartEvents.OnShoppingCartRemoveItem(this, i);
					this.contents.RemoveAt(i);
					this.SaveContents();
					return;
				}
			}
		}
	}

	// Token: 0x06002912 RID: 10514 RVA: 0x0012D738 File Offset: 0x0012BB38
	public bool ContainsU2UModel(string modelID)
	{
		for (int i = 0; i < this.contents.Count; i++)
		{
			BWShoppingCartItem bwshoppingCartItem = this.contents[i];
			if (bwshoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bwshoppingCartItemModel = bwshoppingCartItem as BWShoppingCartItemModel;
				if (bwshoppingCartItemModel.modelID == modelID)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06002913 RID: 10515 RVA: 0x0012D794 File Offset: 0x0012BB94
	public void AddU2UModel(BWU2UModel model, bool blueprintOnly)
	{
		this.contents.Add(new BWShoppingCartItemModel(model, 1, blueprintOnly));
		this.SaveContents();
		BWShoppingCartEvents.OnShoppingCartAddItem(this, this.contents.Count - 1);
	}

	// Token: 0x06002914 RID: 10516 RVA: 0x0012D7C4 File Offset: 0x0012BBC4
	public void ClearU2UModel(string modelID)
	{
		for (int i = this.contents.Count - 1; i >= 0; i--)
		{
			BWShoppingCartItem bwshoppingCartItem = this.contents[i];
			if (bwshoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bwshoppingCartItemModel = bwshoppingCartItem as BWShoppingCartItemModel;
				if (bwshoppingCartItemModel.modelID == modelID)
				{
					bwshoppingCartItem.purchaseStatus = BWPurchaseStatus.DO_NOT_PURCHASE;
					BWShoppingCartEvents.OnShoppingCartRemoveItem(this, i);
					this.contents.RemoveAt(i);
					this.SaveContents();
				}
			}
		}
	}

	// Token: 0x06002915 RID: 10517 RVA: 0x0012D83F File Offset: 0x0012BC3F
	public void AddBlocksInventory(BlocksInventory blocksInventory)
	{
		blocksInventory.EnumerateInventoryWithAction(delegate(int blockItemId, int count, int infinityCount)
		{
			if (!BWBlockItemPricing.AlaCarteIsAvailable(blockItemId))
			{
				return;
			}
			int num = BWBlockItemPricing.PackSizeForBlockItemID(blockItemId);
			if (num == 0)
			{
				this.AddBlockItemPack(blockItemId, 1);
			}
			else
			{
				int packCount = Mathf.CeilToInt((float)count / (float)num);
				this.AddBlockItemPack(blockItemId, packCount);
			}
		});
	}

	// Token: 0x06002916 RID: 10518 RVA: 0x0012D853 File Offset: 0x0012BC53
	public bool ClearContents()
	{
		if (this.contents == null || this.contents.Count == 0)
		{
			return false;
		}
		BWStandalone.Overlays.ShowConfirmationDialog("Clear shopping cart contents?", delegate()
		{
			this.contents.Clear();
			this.SaveContents();
			BWShoppingCartEvents.OnShoppingCartClear(this);
			UISoundPlayer.Instance.PlayClip("close_default", 0.8f);
		});
		return true;
	}

	// Token: 0x06002917 RID: 10519 RVA: 0x0012D890 File Offset: 0x0012BC90
	public bool BuyContents()
	{
		if (this.processingContents)
		{
			return false;
		}
		if (this.contents == null || this.contents.Count == 0)
		{
			return false;
		}
		if (BWUser.currentUser.coins < this.TotalPrice())
		{
			BWStandalone.Overlays.ShowPopupInsufficentCoins();
			return false;
		}
		BWStandalone.Instance.StartCoroutine(this.ProcessContents());
		return true;
	}

	// Token: 0x06002918 RID: 10520 RVA: 0x0012D8FC File Offset: 0x0012BCFC
	public int TotalPrice()
	{
		int num = 0;
		foreach (BWShoppingCartItem bwshoppingCartItem in this.contents)
		{
			if (bwshoppingCartItem.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
			{
				num += bwshoppingCartItem.Price();
			}
		}
		return num;
	}

	// Token: 0x06002919 RID: 10521 RVA: 0x0012D96C File Offset: 0x0012BD6C
	private void BuyBlockPacks(List<BWShoppingCartItemBlockPack> items)
	{
		BlocksInventory blocksInventory = this.ConvertToBlocksInventory(items);
		string valueStr = blocksInventory.ToString();
		int blocksCost = 0;
		blocksInventory.EnumerateInventoryWithAction(delegate(int blockItemId, int count, int infinityCount)
		{
			if (BWBlockItemPricing.PackSizeForBlockItemID(blockItemId) > 0)
			{
				blocksCost += BWBlockItemPricing.CoinsValueOfBlockItem(blockItemId, count);
			}
			else
			{
				blocksCost += BWBlockItemPricing.CoinsValueOfBlockItem(blockItemId, infinityCount);
			}
		});
		if (BWUser.currentUser.IsPremiumUser())
		{
			blocksCost = BWBlockItemPricing.BlocksworldPremiumDiscount(blocksCost);
		}
		items.ForEach(delegate(BWShoppingCartItemBlockPack item)
		{
			item.purchaseStatus = BWPurchaseStatus.PURCHASE_IN_PROGRESS;
		});
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", "/api/v1/block_items/a_la_carte_purchases");
		bwapirequestBase.AddParam("a_la_carte_blocks_inventory_str", valueStr);
		bwapirequestBase.AddParam("a_la_carte_blocks_inventory_price", blocksCost.ToString());
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(responseJson["attrs_for_current_user"]);
			}
			items.ForEach(delegate(BWShoppingCartItemBlockPack item)
			{
				item.purchaseStatus = BWPurchaseStatus.PURCHASE_SUCCESS;
			});
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			items.ForEach(delegate(BWShoppingCartItemBlockPack item)
			{
				item.purchaseStatus = BWPurchaseStatus.PURCHASE_FAILED;
				item.purchaseFailureMessage = error.message;
			});
			if (error.httpStatusCode == 400 && error.message == "not_enough_coins" && error.responseBodyJson != null && error.responseBodyJson.ContainsKey("user_coins"))
			{
				int intValue = error.responseBodyJson["user_coins"].IntValue;
				BWUser.SetCurrentUserCoinCount(intValue);
				BWLog.Info("Setting actual user coins: " + intValue);
			}
		};
		bwapirequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	// Token: 0x0600291A RID: 10522 RVA: 0x0012DA68 File Offset: 0x0012BE68
	private void BuyModel(BWShoppingCartItemModel item)
	{
		item.purchaseStatus = BWPurchaseStatus.PURCHASE_IN_PROGRESS;
		int num = item.Price();
		string key = (!item.blueprintOnly) ? "u2u_model_price" : "u2u_model_blueprint_price";
		string path = (!item.blueprintOnly) ? "/api/v1/u2u_models/purchases" : "/api/v1/u2u_models/blueprints/purchases";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("u2u_model_id", item.modelID);
		bwapirequestBase.AddParam(key, num.ToString());
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(responseJson["attrs_for_current_user"]);
			}
			item.purchaseStatus = BWPurchaseStatus.PURCHASE_SUCCESS;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			item.purchaseFailureMessage = error.message;
			item.purchaseStatus = BWPurchaseStatus.PURCHASE_FAILED;
		};
		bwapirequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	// Token: 0x0600291B RID: 10523 RVA: 0x0012DB4C File Offset: 0x0012BF4C
	private IEnumerator ProcessContents()
	{
		this.processingContents = true;
		if (BWUser.currentUser.coins < this.TotalPrice())
		{
			BWStandalone.Overlays.ShowPopupInsufficentCoins();
			this.processingContents = false;
			yield break;
		}
		List<BWShoppingCartItemBlockPack> blockPackItemsToBuy = new List<BWShoppingCartItemBlockPack>();
		bool buyingModels = false;
		foreach (BWShoppingCartItem bwshoppingCartItem in this.contents)
		{
			if (bwshoppingCartItem.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
			{
				if (bwshoppingCartItem is BWShoppingCartItemBlockPack)
				{
					blockPackItemsToBuy.Add(bwshoppingCartItem as BWShoppingCartItemBlockPack);
				}
				else if (bwshoppingCartItem is BWShoppingCartItemModel)
				{
					this.BuyModel(bwshoppingCartItem as BWShoppingCartItemModel);
					buyingModels = true;
				}
			}
		}
		if (blockPackItemsToBuy.Count > 0)
		{
			this.BuyBlockPacks(blockPackItemsToBuy);
		}
		bool complete = this.contents.TrueForAll((BWShoppingCartItem item) => item.purchaseStatus != BWPurchaseStatus.PURCHASE_IN_PROGRESS);
		while (!complete)
		{
			complete = this.contents.TrueForAll((BWShoppingCartItem item) => item.purchaseStatus != BWPurchaseStatus.PURCHASE_IN_PROGRESS);
			yield return null;
		}
		bool purchaseError = false;
		string purchaseErrorMessage = string.Empty;
		for (int i = this.contents.Count - 1; i >= 0; i--)
		{
			BWShoppingCartItem bwshoppingCartItem2 = this.contents[i];
			if (bwshoppingCartItem2.purchaseStatus == BWPurchaseStatus.PURCHASE_SUCCESS)
			{
				this.contents.RemoveAt(i);
				BWShoppingCartEvents.OnShoppingCartRemoveItem(this, i);
			}
			else if (bwshoppingCartItem2.purchaseStatus == BWPurchaseStatus.PURCHASE_FAILED)
			{
				if (!purchaseError || string.IsNullOrEmpty(purchaseErrorMessage))
				{
					purchaseError = true;
					purchaseErrorMessage = bwshoppingCartItem2.purchaseFailureMessage;
				}
				bwshoppingCartItem2.purchaseStatus = BWPurchaseStatus.NOT_PURCHASED;
				bwshoppingCartItem2.purchaseFailureMessage = string.Empty;
			}
		}
		if (purchaseError)
		{
			UISoundPlayer.Instance.PlayClip("forbidden_01", 0.6f);
		}
		else
		{
			UISoundPlayer.Instance.PlayClip("shop_purchase", 1f);
		}
		if (purchaseError)
		{
			string text = "Purchase Failed";
			if (purchaseErrorMessage == "not_enough_coins")
			{
				text += ": \n Not Enough Coins";
			}
			BWStandalone.Overlays.ShowMessage(text);
		}
		else if (buyingModels)
		{
			BWU2UModelDataManager.Instance.LoadCurrentUserPurchasedModels();
			while (!BWU2UModelDataManager.Instance.currentUserPurchasedModelsLoaded)
			{
				yield return null;
			}
		}
		this.SaveContents();
		this.processingContents = false;
		yield break;
	}

	// Token: 0x040023A9 RID: 9129
	public readonly List<BWShoppingCartItem> contents;

	// Token: 0x040023AA RID: 9130
	private bool processingContents;
}
