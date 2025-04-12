using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020003C2 RID: 962
public class BWU2UModelDataManager : MonoBehaviour
{
	// Token: 0x170001E4 RID: 484
	// (get) Token: 0x06002A13 RID: 10771 RVA: 0x00133A48 File Offset: 0x00131E48
	public static BWU2UModelDataManager Instance
	{
		get
		{
			return BWU2UModelDataManager.instance;
		}
	}

	// Token: 0x06002A14 RID: 10772 RVA: 0x00133A4F File Offset: 0x00131E4F
	public void Awake()
	{
		if (BWU2UModelDataManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.Init();
		BWU2UModelDataManager.instance = this;
	}

	// Token: 0x170001E5 RID: 485
	// (get) Token: 0x06002A15 RID: 10773 RVA: 0x00133A79 File Offset: 0x00131E79
	// (set) Token: 0x06002A16 RID: 10774 RVA: 0x00133A81 File Offset: 0x00131E81
	public bool currentUserPurchasedModelsLoaded { get; private set; }

	// Token: 0x06002A17 RID: 10775 RVA: 0x00133A8A File Offset: 0x00131E8A
	private void Init()
	{
		this.modelCache = new Dictionary<string, BWU2UModel>();
		this.cachedModelsForLists = new Dictionary<BWU2UModelListType, List<string>>();
	}

	// Token: 0x06002A18 RID: 10776 RVA: 0x00133AA4 File Offset: 0x00131EA4
	public void LoadRecent(int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=recent";
		this.LoadFromAPI(BWU2UModelListType.Recent, endpoint, page, null, toDataSource);
	}

	// Token: 0x06002A19 RID: 10777 RVA: 0x00133AC4 File Offset: 0x00131EC4
	public void LoadBestSellers(int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=best_sellers";
		this.LoadFromAPI(BWU2UModelListType.Popular, endpoint, page, null, toDataSource);
	}

	// Token: 0x06002A1A RID: 10778 RVA: 0x00133AE4 File Offset: 0x00131EE4
	public void LoadCategory(int categoryID, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=best_sellers";
		Dictionary<string, string> apiParams = new Dictionary<string, string>
		{
			{
				"category_id",
				categoryID.ToString()
			}
		};
		this.LoadFromAPI(BWU2UModelListType.Category, endpoint, page, apiParams, toDataSource);
	}

	// Token: 0x06002A1B RID: 10779 RVA: 0x00133B24 File Offset: 0x00131F24
	public void LoadSearchResults(string searchStr, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = string.Format("/api/v1/u2u_models?search={0}", searchStr);
		this.LoadFromAPI(BWU2UModelListType.SearchResults, endpoint, page, null, toDataSource);
	}

	// Token: 0x06002A1C RID: 10780 RVA: 0x00133B48 File Offset: 0x00131F48
	public void LoadUserPublicModels(string userIDStr, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = string.Format("/api/v1/users/{0}/u2u_models", userIDStr);
		this.LoadFromAPI(BWU2UModelListType.User, endpoint, page, null, toDataSource);
	}

	// Token: 0x06002A1D RID: 10781 RVA: 0x00133B6C File Offset: 0x00131F6C
	public void LoadCurrentUserPurchasedModels()
	{
		this.currentUserPurchasedModelsLoaded = false;
		List<string> modelIDList = new List<string>();
		this.cachedModelsForLists[BWU2UModelListType.CurrentUserPurchased] = modelIDList;
		string path = "/api/v1/current_user/purchased_u2u_models";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			List<JObject> arrayValue = respJson["u2u_models"].ArrayValue;
			foreach (JObject json in arrayValue)
			{
				BWU2UModel bwu2UModel = new BWU2UModel(json);
				modelIDList.Add(bwu2UModel.modelID);
				this.modelCache[bwu2UModel.modelID] = bwu2UModel;
			}
			this.currentUserPurchasedModelsLoaded = true;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			this.currentUserPurchasedModelsLoaded = true;
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x06002A1E RID: 10782 RVA: 0x00133BEC File Offset: 0x00131FEC
	public void AddPurchasedModel(BWU2UModel model)
	{
		List<string> list = null;
		if (!this.cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out list))
		{
			list = new List<string>();
			this.cachedModelsForLists[BWU2UModelListType.CurrentUserPurchased] = list;
		}
		if (!list.Contains(model.modelID))
		{
			list.Add(model.modelID);
		}
		if (!this.modelCache.ContainsKey(model.modelID))
		{
			this.modelCache.Add(model.modelID, model);
		}
	}

	// Token: 0x06002A1F RID: 10783 RVA: 0x00133C68 File Offset: 0x00132068
	public void PurchaseU2UModel(string modelID, UnityAction completion)
	{
		BWU2UModel bwu2UModel;
		if (!this.modelCache.TryGetValue(modelID, out bwu2UModel))
		{
			return;
		}
		int coinsPrice = bwu2UModel.coinsPrice;
		string key = "u2u_model_price";
		string path = "/api/v1/u2u_models/purchases";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("u2u_model_id", modelID);
		bwapirequestBase.AddParam(key, coinsPrice.ToString());
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(responseJson["attrs_for_current_user"]);
			}
			this.LoadCurrentUserPurchasedModels();
			if (completion != null)
			{
				completion();
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
		};
		bwapirequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	// Token: 0x06002A20 RID: 10784 RVA: 0x00133D2C File Offset: 0x0013212C
	public List<BWU2UModel> PurchasedU2UModels()
	{
		List<BWU2UModel> list = new List<BWU2UModel>();
		List<string> list2 = null;
		if (this.cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out list2))
		{
			foreach (string text in list2)
			{
				if (this.modelCache.ContainsKey(text))
				{
					list.Add(this.modelCache[text]);
				}
				else
				{
					BWLog.Error("U2U model with ID " + text + " not found in cache");
				}
			}
		}
		return list;
	}

	// Token: 0x06002A21 RID: 10785 RVA: 0x00133DD8 File Offset: 0x001321D8
	public bool HasPurchasedModel(string modelID)
	{
		List<string> list = null;
		return this.cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out list) && list.Contains(modelID);
	}

	// Token: 0x06002A22 RID: 10786 RVA: 0x00133E03 File Offset: 0x00132203
	public BWU2UModel GetCachedModel(string modelID)
	{
		if (this.modelCache.ContainsKey(modelID))
		{
			return this.modelCache[modelID];
		}
		return null;
	}

	// Token: 0x06002A23 RID: 10787 RVA: 0x00133E24 File Offset: 0x00132224
	public void LoadFullModelFromRemote(string modelID, UIDataSourcePublicModelList toDataSource)
	{
		if (this.modelCache.ContainsKey(modelID))
		{
			BWU2UModel bwu2UModel = this.modelCache[modelID];
			if (!string.IsNullOrEmpty(bwu2UModel.sourceJsonStr))
			{
				toDataSource.DataManagerLoadedModel(bwu2UModel);
				return;
			}
		}
		this.LoadModelFromRemote(modelID, delegate(BWU2UModel model)
		{
			toDataSource.DataManagerLoadedModel(model);
		}, delegate
		{
			toDataSource.DataManagerFailedToLoadModel();
		});
	}

	// Token: 0x06002A24 RID: 10788 RVA: 0x00133E98 File Offset: 0x00132298
	public void LoadModelFromRemote(string modelID, UnityAction<BWU2UModel> completion, UnityAction failure = null)
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", string.Format("/api/v1/u2u_models/{0}", modelID));
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("u2u_model"))
			{
				BWU2UModel bwu2UModel = new BWU2UModel(responseJson["u2u_model"]);
				this.modelCache[modelID] = bwu2UModel;
				if (completion != null)
				{
					completion(bwu2UModel);
				}
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
			if (failure != null)
			{
				failure();
			}
		};
		bwapirequestBase.Send();
	}

	// Token: 0x06002A25 RID: 10789 RVA: 0x00133F14 File Offset: 0x00132314
	private void LoadFromAPI(BWU2UModelListType listType, string endpoint, int page, Dictionary<string, string> apiParams, UIDataSourcePublicModelList toDataSource)
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", endpoint);
		bwapirequestBase.AddParam("page", page.ToString());
		if (apiParams != null)
		{
			bwapirequestBase.AddParams(apiParams);
		}
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			List<BWU2UModel> list = new List<BWU2UModel>();
			bool allPagesLoaded = false;
			int nextPage = page;
			JObject jobject = responseJson["u2u_models"];
			foreach (JObject json in jobject.ArrayValue)
			{
				BWU2UModel item = new BWU2UModel(json);
				list.Add(item);
			}
			if (responseJson.ContainsKey("pagination_next_page"))
			{
				JObject jobject2 = responseJson["pagination_next_page"];
				if (jobject2.Kind == JObjectKind.Null)
				{
					allPagesLoaded = true;
				}
				else if (jobject2.Kind == JObjectKind.Number)
				{
					nextPage = jobject2.IntValue;
					allPagesLoaded = false;
				}
			}
			foreach (BWU2UModel bwu2UModel in list)
			{
				this.modelCache[bwu2UModel.modelID] = bwu2UModel;
			}
			toDataSource.DataManagerLoadedModelList(list, nextPage, allPagesLoaded);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			toDataSource.DataManagerFailedToLoadList();
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x04002428 RID: 9256
	private static BWU2UModelDataManager instance;

	// Token: 0x0400242A RID: 9258
	private Dictionary<string, BWU2UModel> modelCache;

	// Token: 0x0400242B RID: 9259
	private Dictionary<BWU2UModelListType, List<string>> cachedModelsForLists;
}
