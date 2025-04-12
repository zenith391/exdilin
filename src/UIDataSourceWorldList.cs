using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Events;

// Token: 0x020003F8 RID: 1016
public class UIDataSourceWorldList : UIDataSource
{
	// Token: 0x06002C8A RID: 11402 RVA: 0x0013F990 File Offset: 0x0013DD90
	public UIDataSourceWorldList(UIDataManager dataManager) : base(dataManager)
	{
		this.apiPath = "/api/v1/worlds";
		this.timeOut = 10f;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(this.CurrentUserDataChanged));
	}

	// Token: 0x06002C8B RID: 11403 RVA: 0x0013F9DE File Offset: 0x0013DDDE
	public UIDataSourceWorldList(UIDataManager dataManager, int categoryId, string kind) : this(dataManager)
	{
		this.categoryId = categoryId;
		this.kind = kind;
		this.minimumStarRating = 3;
	}

	// Token: 0x06002C8C RID: 11404 RVA: 0x0013F9FC File Offset: 0x0013DDFC
	public static UIDataSourceWorldList NewWorlds(UIDataManager dataManager)
	{
		UIDataSourceWorldList uidataSourceWorldList = new UIDataSourceWorldList(dataManager);
		UIDataSourceWorldList uidataSourceWorldList2 = new UIDataSourceWorldList(dataManager);
		uidataSourceWorldList.kind = "unmoderated";
		uidataSourceWorldList2.kind = "recent";
		uidataSourceWorldList.minimumStarRating = 3;
		uidataSourceWorldList2.minimumStarRating = 3;
		return new UIDataSourceWorldListConcatenated(dataManager, uidataSourceWorldList, uidataSourceWorldList2);
	}

	// Token: 0x06002C8D RID: 11405 RVA: 0x0013FA48 File Offset: 0x0013DE48
	public static UIDataSourceWorldList PopularWorlds(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "most_popular"
		};
	}

	// Token: 0x06002C8E RID: 11406 RVA: 0x0013FA68 File Offset: 0x0013DE68
	public static UIDataSourceWorldList WorldSpotlight(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "featured"
		};
	}

	// Token: 0x06002C8F RID: 11407 RVA: 0x0013FA88 File Offset: 0x0013DE88
	public static UIDataSourceWorldList HallOfFame(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "arcade"
		};
	}

	// Token: 0x06002C90 RID: 11408 RVA: 0x0013FAA8 File Offset: 0x0013DEA8
	public static UIDataSourceWorldList Curated(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "curated"
		};
	}

	// Token: 0x06002C91 RID: 11409 RVA: 0x0013FAC8 File Offset: 0x0013DEC8
	public static UIDataSourceWorldList PublicWorldsForCurrentUser(UIDataManager dataManager)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			apiPath = "/api/v1/current_user/worlds?is_published=yes"
		};
	}

	// Token: 0x06002C92 RID: 11410 RVA: 0x0013FAE8 File Offset: 0x0013DEE8
	public static UIDataSourceWorldList PublishedWorldsForUser(UIDataManager dataManager, string userIDStr)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			kind = "recent",
			apiPath = string.Format("/api/v1/users/{0}/worlds", userIDStr)
		};
	}

	// Token: 0x06002C93 RID: 11411 RVA: 0x0013FB1C File Offset: 0x0013DF1C
	public static UIDataSourceWorldList WorldSearchResults(UIDataManager dataManager, string searchStr)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			searchStr = searchStr
		};
	}

	// Token: 0x06002C94 RID: 11412 RVA: 0x0013FB38 File Offset: 0x0013DF38
	public static UIDataSourceWorldList LikedWorldsForCurrentUser(UIDataManager dataManager)
	{
		UIDataSourceWorldList uidataSourceWorldList = new UIDataSourceWorldList(dataManager);
		uidataSourceWorldList.apiPath = string.Format("/api/v1/users/{0}/liked_worlds", BWUser.currentUser.userID);
		BWUserDataManager.Instance.RemoveCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(uidataSourceWorldList.CurrentUserDataChanged));
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(uidataSourceWorldList.Refresh));
		return uidataSourceWorldList;
	}

	// Token: 0x06002C95 RID: 11413 RVA: 0x0013FB9C File Offset: 0x0013DF9C
	public static UIDataSourceWorldList LikedWorldsForUser(UIDataManager dataManager, string userId)
	{
		return new UIDataSourceWorldList(dataManager)
		{
			apiPath = string.Format("/api/v1/users/{0}/liked_worlds", userId)
		};
	}

	// Token: 0x06002C96 RID: 11414 RVA: 0x0013FBC2 File Offset: 0x0013DFC2
	public bool AllPagesLoaded()
	{
		return this.allPagesLoaded;
	}

	// Token: 0x06002C97 RID: 11415 RVA: 0x0013FBCA File Offset: 0x0013DFCA
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		this.nextPage = 1;
		this.DoRequest(null);
	}

	// Token: 0x06002C98 RID: 11416 RVA: 0x0013FBE8 File Offset: 0x0013DFE8
	public void DoRequest(UnityAction successCallback = null)
	{
		base.loadState = UIDataSource.LoadState.Loading;
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", this.apiPath);
		if (!string.IsNullOrEmpty(this.kind))
		{
			bwapirequestBase.AddParam("kind", this.kind);
		}
		if (!string.IsNullOrEmpty(this.searchStr))
		{
			bwapirequestBase.AddParam("search", this.searchStr);
		}
		if (!this.allPagesLoaded)
		{
			bwapirequestBase.AddParam("page", this.nextPage.ToString());
		}
		if (this.categoryId > 0)
		{
			bwapirequestBase.AddParam("category_id", this.categoryId.ToString());
		}
		if (this.minimumStarRating > 0)
		{
			bwapirequestBase.AddParam("minimum_star_rating", this.minimumStarRating.ToString());
		}
		BWUserDataManager.Instance.LoadUserData();
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject jobject = responseJson["worlds"];
			foreach (JObject json in jobject.ArrayValue)
			{
				BWWorld bwworld = new BWWorld(json);
				if (!this.Keys.Contains(bwworld.worldID))
				{
					if (!BWUserDataManager.Instance.HasReportedWorld(bwworld.worldID))
					{
						Dictionary<string, string> dictionary = bwworld.AttributesForMenuUI();
						dictionary.Add("bookmarked", BWUserDataManager.Instance.HasBookmarkedWorld(bwworld.worldID).ToString());
						this.Keys.Add(bwworld.worldID);
						this.Data.Add(bwworld.worldID, dictionary);
					}
				}
			}
			if (responseJson.ContainsKey("pagination_next_page"))
			{
				JObject jobject2 = responseJson["pagination_next_page"];
				if (jobject2.Kind == JObjectKind.Null)
				{
					this.allPagesLoaded = true;
				}
				else if (jobject2.Kind == JObjectKind.Number)
				{
					this.nextPage = jobject2.IntValue;
					this.allPagesLoaded = false;
				}
			}
			this.SetDataTimestamp();
			this.loadState = UIDataSource.LoadState.Loaded;
			this.NotifyListeners();
			if (successCallback != null)
			{
				successCallback();
			}
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			this.loadState = UIDataSource.LoadState.Failed;
			BWLog.Error(error.message);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x06002C99 RID: 11417 RVA: 0x0013FD13 File Offset: 0x0013E113
	public override bool CanExpand()
	{
		return base.loadState == UIDataSource.LoadState.Loaded && !this.allPagesLoaded;
	}

	// Token: 0x06002C9A RID: 11418 RVA: 0x0013FD2D File Offset: 0x0013E12D
	public override void Expand()
	{
		if (!this.allPagesLoaded)
		{
			this.DoRequest(null);
		}
	}

	// Token: 0x06002C9B RID: 11419 RVA: 0x0013FD41 File Offset: 0x0013E141
	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}

	// Token: 0x06002C9C RID: 11420 RVA: 0x0013FD48 File Offset: 0x0013E148
	private void CurrentUserDataChanged()
	{
		if (base.Data == null)
		{
			return;
		}
		foreach (string text in BWUserDataManager.Instance.ReportedWorldIDs())
		{
			if (base.Keys.Contains(text))
			{
				base.Keys.Remove(text);
				base.Data.Remove(text);
			}
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in base.Data)
		{
			Dictionary<string, string> value = keyValuePair.Value;
			string key = keyValuePair.Key;
			value["bookmarked"] = BWUserDataManager.Instance.HasBookmarkedWorld(key).ToString();
		}
		base.NotifyListeners();
	}

	// Token: 0x06002C9D RID: 11421 RVA: 0x0013FE5C File Offset: 0x0013E25C
	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWWorld.expectedImageUrlsForUI;
	}

	// Token: 0x06002C9E RID: 11422 RVA: 0x0013FE63 File Offset: 0x0013E263
	public static List<string> ExpectedDataKeysForUI()
	{
		return BWWorld.expectedDataKeysForUI;
	}

	// Token: 0x04002547 RID: 9543
	private string kind;

	// Token: 0x04002548 RID: 9544
	private string searchStr;

	// Token: 0x04002549 RID: 9545
	private int categoryId = -1;

	// Token: 0x0400254A RID: 9546
	private string apiPath;

	// Token: 0x0400254B RID: 9547
	private new int nextPage = 1;

	// Token: 0x0400254C RID: 9548
	private bool allPagesLoaded;

	// Token: 0x0400254D RID: 9549
	private int minimumStarRating;
}
