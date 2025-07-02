using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class BWU2UModelDataManager : MonoBehaviour
{
	private static BWU2UModelDataManager instance;

	private Dictionary<string, BWU2UModel> modelCache;

	private Dictionary<BWU2UModelListType, List<string>> cachedModelsForLists;

	public static BWU2UModelDataManager Instance => instance;

	public bool currentUserPurchasedModelsLoaded { get; private set; }

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Init();
		instance = this;
	}

	private void Init()
	{
		modelCache = new Dictionary<string, BWU2UModel>();
		cachedModelsForLists = new Dictionary<BWU2UModelListType, List<string>>();
	}

	public void LoadRecent(int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=recent";
		LoadFromAPI(BWU2UModelListType.Recent, endpoint, page, null, toDataSource);
	}

	public void LoadBestSellers(int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=best_sellers";
		LoadFromAPI(BWU2UModelListType.Popular, endpoint, page, null, toDataSource);
	}

	public void LoadCategory(int categoryID, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = "/api/v1/u2u_models?kind=best_sellers";
		Dictionary<string, string> apiParams = new Dictionary<string, string> { 
		{
			"category_id",
			categoryID.ToString()
		} };
		LoadFromAPI(BWU2UModelListType.Category, endpoint, page, apiParams, toDataSource);
	}

	public void LoadSearchResults(string searchStr, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = $"/api/v1/u2u_models?search={searchStr}";
		LoadFromAPI(BWU2UModelListType.SearchResults, endpoint, page, null, toDataSource);
	}

	public void LoadUserPublicModels(string userIDStr, int page, UIDataSourcePublicModelList toDataSource)
	{
		string endpoint = $"/api/v1/users/{userIDStr}/u2u_models";
		LoadFromAPI(BWU2UModelListType.User, endpoint, page, null, toDataSource);
	}

	public void LoadCurrentUserPurchasedModels()
	{
		currentUserPurchasedModelsLoaded = false;
		List<string> modelIDList = new List<string>();
		cachedModelsForLists[BWU2UModelListType.CurrentUserPurchased] = modelIDList;
		string path = "/api/v1/current_user/purchased_u2u_models";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			List<JObject> arrayValue = respJson["u2u_models"].ArrayValue;
			foreach (JObject item in arrayValue)
			{
				BWU2UModel bWU2UModel = new BWU2UModel(item);
				modelIDList.Add(bWU2UModel.modelID);
				modelCache[bWU2UModel.modelID] = bWU2UModel;
			}
			currentUserPurchasedModelsLoaded = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			currentUserPurchasedModelsLoaded = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}

	public void AddPurchasedModel(BWU2UModel model)
	{
		List<string> value = null;
		if (!cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out value))
		{
			value = new List<string>();
			cachedModelsForLists[BWU2UModelListType.CurrentUserPurchased] = value;
		}
		if (!value.Contains(model.modelID))
		{
			value.Add(model.modelID);
		}
		if (!modelCache.ContainsKey(model.modelID))
		{
			modelCache.Add(model.modelID, model);
		}
	}

	public void PurchaseU2UModel(string modelID, UnityAction completion)
	{
		if (!modelCache.TryGetValue(modelID, out var value))
		{
			return;
		}
		int coinsPrice = value.coinsPrice;
		string key = "u2u_model_price";
		string path = "/api/v1/u2u_models/purchases";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("u2u_model_id", modelID);
		bWAPIRequestBase.AddParam(key, coinsPrice.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("attrs_for_current_user"))
			{
				BWUser.UpdateCurrentUserAndNotifyListeners(responseJson["attrs_for_current_user"]);
			}
			LoadCurrentUserPurchasedModels();
			if (completion != null)
			{
				completion();
			}
		};
		bWAPIRequestBase.onFailure = delegate
		{
		};
		bWAPIRequestBase.SendOwnerCoroutine(BWStandalone.Instance);
	}

	public List<BWU2UModel> PurchasedU2UModels()
	{
		List<BWU2UModel> list = new List<BWU2UModel>();
		List<string> value = null;
		if (cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out value))
		{
			foreach (string item in value)
			{
				if (modelCache.ContainsKey(item))
				{
					list.Add(modelCache[item]);
				}
				else
				{
					BWLog.Error("U2U model with ID " + item + " not found in cache");
				}
			}
		}
		return list;
	}

	public bool HasPurchasedModel(string modelID)
	{
		List<string> value = null;
		if (cachedModelsForLists.TryGetValue(BWU2UModelListType.CurrentUserPurchased, out value))
		{
			return value.Contains(modelID);
		}
		return false;
	}

	public BWU2UModel GetCachedModel(string modelID)
	{
		if (modelCache.ContainsKey(modelID))
		{
			return modelCache[modelID];
		}
		return null;
	}

	public void LoadFullModelFromRemote(string modelID, UIDataSourcePublicModelList toDataSource)
	{
		if (modelCache.ContainsKey(modelID))
		{
			BWU2UModel bWU2UModel = modelCache[modelID];
			if (!string.IsNullOrEmpty(bWU2UModel.sourceJsonStr))
			{
				toDataSource.DataManagerLoadedModel(bWU2UModel);
				return;
			}
		}
		LoadModelFromRemote(modelID, delegate(BWU2UModel model)
		{
			toDataSource.DataManagerLoadedModel(model);
		}, delegate
		{
			toDataSource.DataManagerFailedToLoadModel();
		});
	}

	public void LoadModelFromRemote(string modelID, UnityAction<BWU2UModel> completion, UnityAction failure = null)
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/u2u_models/{modelID}");
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson.ContainsKey("u2u_model"))
			{
				BWU2UModel bWU2UModel = new BWU2UModel(responseJson["u2u_model"]);
				modelCache[modelID] = bWU2UModel;
				if (completion != null)
				{
					completion(bWU2UModel);
				}
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
			if (failure != null)
			{
				failure();
			}
		};
		bWAPIRequestBase.Send();
	}

	private void LoadFromAPI(BWU2UModelListType listType, string endpoint, int page, Dictionary<string, string> apiParams, UIDataSourcePublicModelList toDataSource)
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", endpoint);
		bWAPIRequestBase.AddParam("page", page.ToString());
		if (apiParams != null)
		{
			bWAPIRequestBase.AddParams(apiParams);
		}
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			List<BWU2UModel> list = new List<BWU2UModel>();
			bool allPagesLoaded = false;
			int nextPage = page;
			JObject jObject = responseJson["u2u_models"];
			foreach (JObject item2 in jObject.ArrayValue)
			{
				BWU2UModel item = new BWU2UModel(item2);
				list.Add(item);
			}
			if (responseJson.ContainsKey("pagination_next_page"))
			{
				JObject jObject2 = responseJson["pagination_next_page"];
				if (jObject2.Kind == JObjectKind.Null)
				{
					allPagesLoaded = true;
				}
				else if (jObject2.Kind == JObjectKind.Number)
				{
					nextPage = jObject2.IntValue;
					allPagesLoaded = false;
				}
			}
			foreach (BWU2UModel item3 in list)
			{
				modelCache[item3.modelID] = item3;
			}
			toDataSource.DataManagerLoadedModelList(list, nextPage, allPagesLoaded);
		};
		bWAPIRequestBase.onFailure = delegate
		{
			toDataSource.DataManagerFailedToLoadList();
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
