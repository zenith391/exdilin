using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Events;

public class UIDataSourceWorldList : UIDataSource
{
	private string kind;

	private string searchStr;

	private int categoryId = -1;

	private string apiPath;

	private new int nextPage = 1;

	private bool allPagesLoaded;

	private int minimumStarRating;

	public UIDataSourceWorldList(UIDataManager dataManager)
		: base(dataManager)
	{
		apiPath = "/api/v1/worlds";
		timeOut = 10f;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(CurrentUserDataChanged);
	}

	public UIDataSourceWorldList(UIDataManager dataManager, int categoryId, string kind)
		: this(dataManager)
	{
		this.categoryId = categoryId;
		this.kind = kind;
		minimumStarRating = 3;
	}

	public static UIDataSourceWorldList NewWorlds(UIDataManager dataManager)
	{
		UIDataSourceWorldList uIDataSourceWorldList = new UIDataSourceWorldList(dataManager);
		UIDataSourceWorldList uIDataSourceWorldList2 = new UIDataSourceWorldList(dataManager);
		uIDataSourceWorldList.kind = "unmoderated";
		uIDataSourceWorldList2.kind = "recent";
		uIDataSourceWorldList.minimumStarRating = 3;
		uIDataSourceWorldList2.minimumStarRating = 3;
		return new UIDataSourceWorldListConcatenated(dataManager, uIDataSourceWorldList, uIDataSourceWorldList2);
	}

	public static UIDataSourceWorldList PopularWorlds(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "most_popular"
		};
	}

	public static UIDataSourceWorldList WorldSpotlight(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "featured"
		};
	}

	public static UIDataSourceWorldList HallOfFame(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "arcade"
		};
	}

	public static UIDataSourceWorldList Curated(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "curated"
		};
	}

	public static UIDataSourceWorldList PublicWorldsForCurrentUser(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			apiPath = "/api/v1/current_user/worlds?is_published=yes"
		};
	}

	public static UIDataSourceWorldList PublishedWorldsForUser(UIDataManager dataManager, string userIDStr)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "recent",
			apiPath = $"/api/v1/users/{userIDStr}/worlds"
		};
	}

	public static UIDataSourceWorldList WorldSearchResults(UIDataManager dataManager, string searchStr)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			searchStr = searchStr
		};
	}

	public static UIDataSourceWorldList LikedWorldsForCurrentUser(UIDataManager dataManager)
	{
		UIDataSourceWorldList uIDataSourceWorldList = new UIDataSourceWorldList(dataManager);
		uIDataSourceWorldList.apiPath = $"/api/v1/users/{BWUser.currentUser.userID}/liked_worlds";
		BWUserDataManager.Instance.RemoveCurrentUserDataChangedListener(uIDataSourceWorldList.CurrentUserDataChanged);
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(uIDataSourceWorldList.Refresh);
		return uIDataSourceWorldList;
	}

	public static UIDataSourceWorldList LikedWorldsForUser(UIDataManager dataManager, string userId)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			apiPath = $"/api/v1/users/{userId}/liked_worlds"
		};
	}

	public bool AllPagesLoaded()
	{
		return allPagesLoaded;
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		nextPage = 1;
		DoRequest();
	}

	public void DoRequest(UnityAction successCallback = null)
	{
		base.loadState = LoadState.Loading;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", apiPath);
		if (!string.IsNullOrEmpty(kind))
		{
			bWAPIRequestBase.AddParam("kind", kind);
		}
		if (!string.IsNullOrEmpty(searchStr))
		{
			bWAPIRequestBase.AddParam("search", searchStr);
		}
		if (!allPagesLoaded)
		{
			bWAPIRequestBase.AddParam("page", nextPage.ToString());
		}
		if (categoryId > 0)
		{
			bWAPIRequestBase.AddParam("category_id", categoryId.ToString());
		}
		if (minimumStarRating > 0)
		{
			bWAPIRequestBase.AddParam("minimum_star_rating", minimumStarRating.ToString());
		}
		BWUserDataManager.Instance.LoadUserData();
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jObject = responseJson["worlds"];
			foreach (JObject item in jObject.ArrayValue)
			{
				BWWorld bWWorld = new BWWorld(item);
				if (!base.Keys.Contains(bWWorld.worldID) && !BWUserDataManager.Instance.HasReportedWorld(bWWorld.worldID))
				{
					Dictionary<string, string> dictionary = bWWorld.AttributesForMenuUI();
					dictionary.Add("bookmarked", BWUserDataManager.Instance.HasBookmarkedWorld(bWWorld.worldID).ToString());
					base.Keys.Add(bWWorld.worldID);
					base.Data.Add(bWWorld.worldID, dictionary);
				}
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
			SetDataTimestamp();
			base.loadState = LoadState.Loaded;
			NotifyListeners();
			if (successCallback != null)
			{
				successCallback();
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			base.loadState = LoadState.Failed;
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.Send();
	}

	public override bool CanExpand()
	{
		if (base.loadState == LoadState.Loaded)
		{
			return !allPagesLoaded;
		}
		return false;
	}

	public override void Expand()
	{
		if (!allPagesLoaded)
		{
			DoRequest();
		}
	}

	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}

	private void CurrentUserDataChanged()
	{
		if (base.Data == null)
		{
			return;
		}
		foreach (string item in BWUserDataManager.Instance.ReportedWorldIDs())
		{
			if (base.Keys.Contains(item))
			{
				base.Keys.Remove(item);
				base.Data.Remove(item);
			}
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> datum in base.Data)
		{
			Dictionary<string, string> value = datum.Value;
			string key = datum.Key;
			value["bookmarked"] = BWUserDataManager.Instance.HasBookmarkedWorld(key).ToString();
		}
		NotifyListeners();
	}

	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWWorld.expectedImageUrlsForUI;
	}

	public static List<string> ExpectedDataKeysForUI()
	{
		return BWWorld.expectedDataKeysForUI;
	}
}
