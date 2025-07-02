using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWShoppingCart
{
	public readonly List<BWShoppingCartItem> contents;

	private bool processingContents;

	public int itemCount
	{
		get
		{
			int num = 0;
			foreach (BWShoppingCartItem content in contents)
			{
				if (content.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
				{
					num++;
				}
			}
			return num;
		}
	}

	public BWShoppingCart()
	{
		contents = new List<BWShoppingCartItem>();
		LoadContents();
	}

	private void LoadContents()
	{
		contents.Clear();
		JObject jObject = BWUserDataManager.Instance.LoadShoppingCartJSON();
		if (jObject != null)
		{
			foreach (JObject item in jObject.ArrayValue)
			{
				BWShoppingCartItem bWShoppingCartItem = BWShoppingCartItem.FromJson(item);
				if (bWShoppingCartItem != null)
				{
					contents.Add(bWShoppingCartItem);
				}
			}
		}
		BWShoppingCartEvents.OnShoppingCartLoad(this);
		for (int i = 0; i < contents.Count; i++)
		{
			if (!(contents[i] is BWShoppingCartItemModel))
			{
				continue;
			}
			BWShoppingCartItemModel modelItem = contents[i] as BWShoppingCartItemModel;
			if (modelItem.model == null)
			{
				BWU2UModelDataManager.Instance.LoadModelFromRemote(modelItem.modelID, delegate(BWU2UModel model)
				{
					modelItem.model = model;
					BWShoppingCartEvents.OnShoppingCartUpdateItem(this, i);
				});
			}
		}
	}

	private void SaveContents()
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (BWShoppingCartItem content in contents)
		{
			list.Add(content.AttrsForSave());
		}
		BWUserDataManager.Instance.SaveShoppingCartJSON(JSONEncoder.Encode(list));
	}

	private BlocksInventory ConvertToBlocksInventory(List<BWShoppingCartItemBlockPack> items)
	{
		BlocksInventory blocksInventory = new BlocksInventory();
		foreach (BWShoppingCartItemBlockPack item in items)
		{
			int num = BWBlockItemPricing.PackSizeForBlockItemID(item.blockItemID);
			if (num > 0)
			{
				blocksInventory.Add(item.blockItemID, item.count);
				continue;
			}
			blocksInventory.SetCountFor(item.blockItemID, 0);
			blocksInventory.SetInfinityCountFor(item.blockItemID, 1);
		}
		return blocksInventory;
	}

	private List<BWShoppingCartItemBlockPack> ConvertFromBlocksInventory(BlocksInventory blocksInventory)
	{
		List<BWShoppingCartItemBlockPack> list = new List<BWShoppingCartItemBlockPack>();
		blocksInventory.EnumerateInventoryWithAction(delegate(int blockItemID, int count, int infiniteCount)
		{
			if (BWBlockItemPricing.AlaCarteIsAvailable(blockItemID) && BlockItem.FindByID(blockItemID) != null)
			{
				BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = new BWShoppingCartItemBlockPack(blockItemID, count);
				if (BWBlockItemPricing.PackSizeForBlockItemID(blockItemID) == 0)
				{
					bWShoppingCartItemBlockPack.count = 0;
					bWShoppingCartItemBlockPack.infiniteCount = 1;
				}
				list.Add(bWShoppingCartItemBlockPack);
			}
		});
		return list;
	}

	public void AddBlockItemPack(int blockItemID, int packCount)
	{
		bool flag = BWBlockItemPricing.PackSizeForBlockItemID(blockItemID) == 0;
		for (int i = 0; i < contents.Count; i++)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[i];
			if (!(bWShoppingCartItem is BWShoppingCartItemBlockPack))
			{
				continue;
			}
			BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
			if (bWShoppingCartItemBlockPack.blockItemID == blockItemID)
			{
				if (!flag && bWShoppingCartItem.count < 999)
				{
					bWShoppingCartItem.count += packCount;
					bWShoppingCartItem.count = Mathf.Min(999, bWShoppingCartItem.count);
					SaveContents();
				}
				BWShoppingCartEvents.OnShoppingCartUpdateItem(this, i);
				return;
			}
		}
		BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack2 = new BWShoppingCartItemBlockPack(blockItemID, packCount);
		if (flag)
		{
			bWShoppingCartItemBlockPack2.count = 0;
			bWShoppingCartItemBlockPack2.infiniteCount = 1;
		}
		contents.Add(bWShoppingCartItemBlockPack2);
		SaveContents();
		BWShoppingCartEvents.OnShoppingCartAddItem(this, contents.Count - 1);
	}

	public void RemoveBlockItemPack(int blockItemID, int packCount)
	{
		for (int num = contents.Count - 1; num >= 0; num--)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[num];
			if (bWShoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
				if (bWShoppingCartItemBlockPack.blockItemID == blockItemID)
				{
					if (bWShoppingCartItem.count > 1)
					{
						bWShoppingCartItem.count -= packCount;
						bWShoppingCartItem.count = Mathf.Max(1, bWShoppingCartItem.count);
						SaveContents();
						BWShoppingCartEvents.OnShoppingCartUpdateItem(this, num);
					}
					break;
				}
			}
		}
	}

	public void ClearBlockItemPacks(int blockItemID)
	{
		for (int num = contents.Count - 1; num >= 0; num--)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[num];
			if (bWShoppingCartItem is BWShoppingCartItemBlockPack)
			{
				BWShoppingCartItemBlockPack bWShoppingCartItemBlockPack = bWShoppingCartItem as BWShoppingCartItemBlockPack;
				if (bWShoppingCartItemBlockPack.blockItemID == blockItemID)
				{
					bWShoppingCartItem.purchaseStatus = BWPurchaseStatus.DO_NOT_PURCHASE;
					BWShoppingCartEvents.OnShoppingCartRemoveItem(this, num);
					contents.RemoveAt(num);
					SaveContents();
					break;
				}
			}
		}
	}

	public bool ContainsU2UModel(string modelID)
	{
		for (int i = 0; i < contents.Count; i++)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[i];
			if (bWShoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bWShoppingCartItemModel = bWShoppingCartItem as BWShoppingCartItemModel;
				if (bWShoppingCartItemModel.modelID == modelID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddU2UModel(BWU2UModel model, bool blueprintOnly)
	{
		contents.Add(new BWShoppingCartItemModel(model, 1, blueprintOnly));
		SaveContents();
		BWShoppingCartEvents.OnShoppingCartAddItem(this, contents.Count - 1);
	}

	public void ClearU2UModel(string modelID)
	{
		for (int num = contents.Count - 1; num >= 0; num--)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[num];
			if (bWShoppingCartItem is BWShoppingCartItemModel)
			{
				BWShoppingCartItemModel bWShoppingCartItemModel = bWShoppingCartItem as BWShoppingCartItemModel;
				if (bWShoppingCartItemModel.modelID == modelID)
				{
					bWShoppingCartItem.purchaseStatus = BWPurchaseStatus.DO_NOT_PURCHASE;
					BWShoppingCartEvents.OnShoppingCartRemoveItem(this, num);
					contents.RemoveAt(num);
					SaveContents();
				}
			}
		}
	}

	public void AddBlocksInventory(BlocksInventory blocksInventory)
	{
		blocksInventory.EnumerateInventoryWithAction(delegate(int blockItemId, int count, int infinityCount)
		{
			if (BWBlockItemPricing.AlaCarteIsAvailable(blockItemId))
			{
				int num = BWBlockItemPricing.PackSizeForBlockItemID(blockItemId);
				if (num == 0)
				{
					AddBlockItemPack(blockItemId, 1);
				}
				else
				{
					int packCount = Mathf.CeilToInt((float)count / (float)num);
					AddBlockItemPack(blockItemId, packCount);
				}
			}
		});
	}

	public bool ClearContents()
	{
		if (contents == null || contents.Count == 0)
		{
			return false;
		}
		BWStandalone.Overlays.ShowConfirmationDialog("Clear shopping cart contents?", delegate
		{
			contents.Clear();
			SaveContents();
			BWShoppingCartEvents.OnShoppingCartClear(this);
			UISoundPlayer.Instance.PlayClip("close_default", 0.8f);
		});
		return true;
	}

	public bool BuyContents()
	{
		if (processingContents)
		{
			return false;
		}
		if (contents == null || contents.Count == 0)
		{
			return false;
		}
		if (BWUser.currentUser.coins < TotalPrice())
		{
			BWStandalone.Overlays.ShowPopupInsufficentCoins();
			return false;
		}
		BWStandalone.Instance.StartCoroutine(ProcessContents());
		return true;
	}

	public int TotalPrice()
	{
		int num = 0;
		foreach (BWShoppingCartItem content in contents)
		{
			if (content.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
			{
				num += content.Price();
			}
		}
		return num;
	}

	private void BuyBlockPacks(List<BWShoppingCartItemBlockPack> items)
	{
		BlocksInventory blocksInventory = ConvertToBlocksInventory(items);
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
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", "/api/v1/block_items/a_la_carte_purchases");
		bWAPIRequestBase.AddParam("a_la_carte_blocks_inventory_str", valueStr);
		bWAPIRequestBase.AddParam("a_la_carte_blocks_inventory_price", blocksCost.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
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
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
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
		bWAPIRequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	private void BuyModel(BWShoppingCartItemModel item)
	{
		item.purchaseStatus = BWPurchaseStatus.PURCHASE_IN_PROGRESS;
		int num = item.Price();
		string key = ((!item.blueprintOnly) ? "u2u_model_price" : "u2u_model_blueprint_price");
		string path = ((!item.blueprintOnly) ? "/api/v1/u2u_models/purchases" : "/api/v1/u2u_models/blueprints/purchases");
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("u2u_model_id", item.modelID);
		bWAPIRequestBase.AddParam(key, num.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(responseJson["attrs_for_current_user"]);
			}
			item.purchaseStatus = BWPurchaseStatus.PURCHASE_SUCCESS;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			item.purchaseFailureMessage = error.message;
			item.purchaseStatus = BWPurchaseStatus.PURCHASE_FAILED;
		};
		bWAPIRequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	private IEnumerator ProcessContents()
	{
		processingContents = true;
		if (BWUser.currentUser.coins < TotalPrice())
		{
			BWStandalone.Overlays.ShowPopupInsufficentCoins();
			processingContents = false;
			yield break;
		}
		List<BWShoppingCartItemBlockPack> list = new List<BWShoppingCartItemBlockPack>();
		bool buyingModels = false;
		foreach (BWShoppingCartItem content in contents)
		{
			if (content.purchaseStatus != BWPurchaseStatus.DO_NOT_PURCHASE)
			{
				if (content is BWShoppingCartItemBlockPack)
				{
					list.Add(content as BWShoppingCartItemBlockPack);
				}
				else if (content is BWShoppingCartItemModel)
				{
					BuyModel(content as BWShoppingCartItemModel);
					buyingModels = true;
				}
			}
		}
		if (list.Count > 0)
		{
			BuyBlockPacks(list);
		}
		bool complete = contents.TrueForAll((BWShoppingCartItem item) => item.purchaseStatus != BWPurchaseStatus.PURCHASE_IN_PROGRESS);
		while (!complete)
		{
			complete = contents.TrueForAll((BWShoppingCartItem item) => item.purchaseStatus != BWPurchaseStatus.PURCHASE_IN_PROGRESS);
			yield return null;
		}
		bool flag = false;
		string text = string.Empty;
		for (int num = contents.Count - 1; num >= 0; num--)
		{
			BWShoppingCartItem bWShoppingCartItem = contents[num];
			if (bWShoppingCartItem.purchaseStatus == BWPurchaseStatus.PURCHASE_SUCCESS)
			{
				contents.RemoveAt(num);
				BWShoppingCartEvents.OnShoppingCartRemoveItem(this, num);
			}
			else if (bWShoppingCartItem.purchaseStatus == BWPurchaseStatus.PURCHASE_FAILED)
			{
				if (!flag || string.IsNullOrEmpty(text))
				{
					flag = true;
					text = bWShoppingCartItem.purchaseFailureMessage;
				}
				bWShoppingCartItem.purchaseStatus = BWPurchaseStatus.NOT_PURCHASED;
				bWShoppingCartItem.purchaseFailureMessage = string.Empty;
			}
		}
		if (flag)
		{
			UISoundPlayer.Instance.PlayClip("forbidden_01", 0.6f);
		}
		else
		{
			UISoundPlayer.Instance.PlayClip("shop_purchase");
		}
		if (flag)
		{
			string text2 = "Purchase Failed";
			if (text == "not_enough_coins")
			{
				text2 += ": \n Not Enough Coins";
			}
			BWStandalone.Overlays.ShowMessage(text2);
		}
		else if (buyingModels)
		{
			BWU2UModelDataManager.Instance.LoadCurrentUserPurchasedModels();
			while (!BWU2UModelDataManager.Instance.currentUserPurchasedModelsLoaded)
			{
				yield return null;
			}
		}
		SaveContents();
		processingContents = false;
	}
}
