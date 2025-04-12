using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003DD RID: 989
public class UIDataManager : MonoBehaviour
{
	// Token: 0x06002BCC RID: 11212 RVA: 0x0013BC10 File Offset: 0x0013A010
	public void Init()
	{
		if (this.useExampleData)
		{
			this.InitWithExampleData(30);
			return;
		}
		if (this.initComplete)
		{
			return;
		}
		this.AddToDataSources("CurrentUserProfile", new UIDataSourceCurrentUser(this, BWUser.currentUser));
		this.AddToDataSources("RecentWorlds", UIDataSourceWorldList.NewWorlds(this));
		this.AddToDataSources("PopularWorlds", UIDataSourceWorldList.PopularWorlds(this));
		this.AddToDataSources("WorldSpotlight", UIDataSourceWorldList.WorldSpotlight(this));
		this.AddToDataSources("FeaturedWorlds", UIDataSourceWorldList.HallOfFame(this));
		this.AddToDataSources("CuratedWorlds", UIDataSourceWorldList.Curated(this));
		this.AddToDataSources("CurrentUserWorlds", new UIDataSourceLocalWorldList(this));
		this.AddToDataSources("CurrentUserUnpublishedWorlds", UIDataSourceLocalWorldList.CurrentUserUnpublishedWorlds(this));
		this.AddToDataSources("CurrentUserPublishedWorlds", UIDataSourceLocalWorldList.CurrentUserPublishedWorlds(this));
		this.AddToDataSources("CurrentUserBookmarkedWorlds", UIDataSourceWorldList.LikedWorldsForCurrentUser(this));
		this.AddToDataSources("CurrentUserPublicWorlds", UIDataSourceWorldList.PublicWorldsForCurrentUser(this));
		this.AddToDataSources("CurrentUserActivity", UIDataSourceUserActivity.UserActivityForCurrentUser(this));
		this.AddToDataSources("NewsFeed", new UIDataSourceNewsFeed(this));
		this.AddToDataSources("RecentModels", UIDataSourcePublicModelList.RecentList(this));
		this.AddToDataSources("PopularModels", UIDataSourcePublicModelList.BestSellersList(this));
		this.AddToDataSources("PurchasedModels", new UIDataSourcePurchasedModelList(this));
		this.AddToDataSources("CurrentUserUnpublishedModels", UIDataSourceLocalModelList.CurrentUserUnpublishedModels(this));
		this.AddToDataSources("CurrentUserPublishedModels", UIDataSourceLocalModelList.CurrentUserPublishedModels(this));
		this.AddToDataSources("WorldTemplates", new UIDataSourceWorldTemplates(this));
		this.AddToDataSources("WorldCategories", new UIDataSourceWorldCategories(this));
		this.AddToDataSources("ModelCategories", new UIDataSourceModelCategories(this));
		this.AddToDataSources("ShoppingCart", new UIDataSourceShoppingCart(this, true, true));
		this.AddToDataSources("ShoppingCartBlocks", new UIDataSourceShoppingCart(this, true, false));
		this.AddToDataSources("ShoppingCartModels", new UIDataSourceShoppingCart(this, false, true));
		this.AddToDataSources("PlayerSpotlight", UIDataSourceUserList.UserListForPlayerSpotlight(this));
		this.AddToDataSources("CoinPacks", new UIDataSourceCoinPacks(this));
		this.AddToDataSources("PendingPayouts", new UIDataSourcePendingPayouts(this));
		this.AddToDataSources("CurrentUserFollowers", UIDataSourceSocialUserList.FollowersListForCurrentUser(this));
		this.AddToDataSources("CurrentUserFollowedUsers", UIDataSourceSocialUserList.FollowedUserListForCurrentUser(this));
		foreach (BWCategory bwcategory in BWCategory.visibleWorldCategories)
		{
			string dataSubtype = bwcategory.categoryID.ToString();
			this.AddToDataSourceGroups("RecentWorlds", dataSubtype, new UIDataSourceWorldList(this, bwcategory.categoryID, "recent"));
			this.AddToDataSourceGroups("PopularWorlds", dataSubtype, new UIDataSourceWorldList(this, bwcategory.categoryID, "most_popular"));
			this.AddToDataSourceGroups("FeaturedWorlds", dataSubtype, new UIDataSourceWorldList(this, bwcategory.categoryID, "featured"));
		}
		foreach (BWCategory bwcategory2 in BWCategory.modelCategories)
		{
			int categoryID = bwcategory2.categoryID;
			this.AddToDataSourceGroups("PopularModels", categoryID.ToString(), UIDataSourcePublicModelList.ListForCategory(this, categoryID));
		}
		foreach (string text in BWBlockShopData.GetBlockShopCategoryIdentifiers())
		{
			this.AddToDataSourceGroups("BlockShopSectionTitles", text, new UIDataSourceBlockShopSectionTitles(this, text));
			foreach (string text2 in BWBlockShopData.GetSectionTitlesForCategory(text))
			{
				string dataType = "BlockShop_" + BWBlockShopData.GetTitleForCategory(text);
				this.AddToDataSourceGroups(dataType, text2, new UIDataSourceBlockShopContents(this, text, text2));
			}
		}
		this.AddToDataSourceConstructors("UserProfile", (string userIDStr) => new UIDataSourceUserProfile(this, userIDStr));
		this.AddToDataSourceConstructors("RemoteUserWorlds", (string userIDStr) => UIDataSourceWorldList.PublishedWorldsForUser(this, userIDStr));
		this.AddToDataSourceConstructors("RemoteUserBookmarkedWorlds", (string userIDStr) => UIDataSourceWorldList.LikedWorldsForUser(this, userIDStr));
		this.AddToDataSourceConstructors("WorldSearch", (string searchStr) => UIDataSourceWorldList.WorldSearchResults(this, searchStr));
		this.AddToDataSourceConstructors("RemoteUserModels", (string userIDStr) => UIDataSourcePublicModelList.PublicModelsForUser(this, userIDStr));
		this.AddToDataSourceConstructors("UserFollowers", (string userIDStr) => UIDataSourceSocialUserList.FollowersListForUser(this, userIDStr));
		this.AddToDataSourceConstructors("UserFollowedUsers", (string userIDStr) => UIDataSourceSocialUserList.FollowedUserListForUser(this, userIDStr));
		this.AddToDataSourceConstructors("UserActivity", (string userIDStr) => new UIDataSourceUserActivity(this, userIDStr));
		this.AddToDataSourceConstructors("RemoteWorld", (string worldID) => new UIDataSourceSingleWorld(this, worldID));
		this.AddToDataSourceConstructors("U2UModel", (string modelID) => new UIDataSourceSingleU2UModel(this, modelID));
		this.initComplete = true;
	}

	// Token: 0x06002BCD RID: 11213 RVA: 0x0013C10C File Offset: 0x0013A50C
	private void AddToDataSources(string dataType, UIDataSource dataSource)
	{
		if (this.dataSources == null)
		{
			this.dataSources = new Dictionary<string, UIDataSource>();
		}
		dataSource.dataType = dataType;
		this.dataSources.Add(dataType, dataSource);
	}

	// Token: 0x06002BCE RID: 11214 RVA: 0x0013C138 File Offset: 0x0013A538
	private void AddToDataSourceGroups(string dataType, string dataSubtype, UIDataSource dataSource)
	{
		if (this.dataSourceGroups == null)
		{
			this.dataSourceGroups = new Dictionary<string, Dictionary<string, UIDataSource>>();
		}
		if (!this.dataSourceGroups.ContainsKey(dataType))
		{
			this.dataSourceGroups.Add(dataType, new Dictionary<string, UIDataSource>());
		}
		if (this.dataSourceGroups[dataType].ContainsKey(dataSubtype))
		{
			BWLog.Error("Duplicate dataSource " + dataType + " subtype " + dataSubtype);
			return;
		}
		dataSource.dataType = dataType;
		dataSource.dataSubtype = dataSubtype;
		this.dataSourceGroups[dataType].Add(dataSubtype, dataSource);
	}

	// Token: 0x06002BCF RID: 11215 RVA: 0x0013C1CC File Offset: 0x0013A5CC
	private void AddToDataSourceConstructors(string dataType, Func<string, UIDataSource> dataSourceConstructor)
	{
		if (this.dataSourceConstructors == null)
		{
			this.dataSourceConstructors = new Dictionary<string, Func<string, UIDataSource>>();
		}
		this.dataSourceConstructors.Add(dataType, dataSourceConstructor);
	}

	// Token: 0x06002BD0 RID: 11216 RVA: 0x0013C1F4 File Offset: 0x0013A5F4
	public void InitWithExampleData(int contentSize)
	{
		this.dataSources = new Dictionary<string, UIDataSource>();
		this.dataSources.Add("RecentWorlds", new UIDataSourceExample(this, contentSize));
		this.dataSources.Add("PopularWorlds", new UIDataSourceExample(this, contentSize));
		this.dataSources.Add("FeaturedWorlds", new UIDataSourceExample(this, contentSize));
		this.dataSources.Add("CuratedWorlds", new UIDataSourceExample(this, contentSize));
		this.dataSources.Add("WorldTemplates", new UIDataSourceExampleTemplates(this));
		this.dataSourceGroups = new Dictionary<string, Dictionary<string, UIDataSource>>();
	}

	// Token: 0x06002BD1 RID: 11217 RVA: 0x0013C28C File Offset: 0x0013A68C
	public static List<string> AvailableImageUrlsForType(string dataType)
	{
		switch (dataType)
		{
		case "RecentWorlds":
		case "PopularWorlds":
		case "FeaturedWorlds":
		case "RemoteUserWorlds":
		case "CuratedWorlds":
			return UIDataSourceWorldList.ExpectedImageUrlsForUI();
		case "CurrentUserUnpublishedWorlds":
		case "CurrentUserPublishedWorlds":
			return UIDataSourceLocalWorldList.ExpectedImageUrlsForUI();
		case "WorldTemplates":
			return UIDataSourceWorldTemplates.ExpectedImageUrlsForUI();
		case "WorldCategories":
			return UIDataSourceWorldCategories.ExpectedImageUrlsForUI();
		case "CurrentUserProfile":
			return UIDataSourceCurrentUser.ExpectedImageUrlsForUI();
		case "UserProfile":
			return UIDataSourceUserProfile.ExpectedImageUrlsForUI();
		case "PlayerSpotlight":
			return UIDataSourceUserList.ExpectedImageUrlsForUI();
		}
		return null;
	}

	// Token: 0x06002BD2 RID: 11218 RVA: 0x0013C3AC File Offset: 0x0013A7AC
	public static List<string> AvailableDataKeysForType(string dataType)
	{
		switch (dataType)
		{
		case "RecentWorlds":
		case "PopularWorlds":
		case "FeaturedWorlds":
		case "RemoteUserWorlds":
		case "CuratedWorlds":
			return UIDataSourceWorldList.ExpectedDataKeysForUI();
		case "CurrentUserUnpublishedWorlds":
		case "CurrentUserPublishedWorlds":
			return UIDataSourceLocalWorldList.ExpectedDataKeysForUI();
		case "WorldTemplates":
			return UIDataSourceWorldTemplates.ExpectedDataKeysForUI();
		case "WorldCategories":
			return UIDataSourceWorldCategories.ExpectedDataKeysForUI();
		case "CurrentUserProfile":
			return UIDataSourceCurrentUser.ExpectedDataKeysForUI();
		case "UserProfile":
			return UIDataSourceUserProfile.ExpectedDataKeysForUI();
		case "PlayerSpotlight":
			return UIDataSourceUserList.ExpectedDataKeysForUI();
		}
		return null;
	}

	// Token: 0x06002BD3 RID: 11219 RVA: 0x0013C4CC File Offset: 0x0013A8CC
	public void ClearListeners()
	{
		if (this.dataSources == null)
		{
			return;
		}
		foreach (UIDataSource uidataSource in this.dataSources.Values)
		{
			uidataSource.ClearListeners();
		}
	}

	// Token: 0x06002BD4 RID: 11220 RVA: 0x0013C538 File Offset: 0x0013A938
	public UIDataSource GetDataSource(string dataType, string dataSubtype)
	{
		UIDataSource uidataSource = null;
		if (!string.IsNullOrEmpty(dataSubtype))
		{
			Dictionary<string, UIDataSource> dictionary;
			if (this.dataSourceGroups.TryGetValue(dataType, out dictionary))
			{
				dictionary.TryGetValue(dataSubtype, out uidataSource);
			}
			Func<string, UIDataSource> func;
			if (uidataSource == null && this.dataSourceConstructors.TryGetValue(dataType, out func))
			{
				uidataSource = func(dataSubtype);
				uidataSource.dataType = dataType;
				uidataSource.dataSubtype = dataSubtype;
			}
		}
		else if (!string.IsNullOrEmpty(dataType))
		{
			this.dataSources.TryGetValue(dataType, out uidataSource);
		}
		if (uidataSource == null)
		{
			if (this.useExampleData)
			{
				uidataSource = new UIDataSourceExample(this, 10);
			}
			else
			{
				string s = "No data source for data type: " + dataType + ((dataSubtype != null) ? ("subtype: " + dataSubtype) : string.Empty);
				BWLog.Error(s);
			}
		}
		return uidataSource;
	}

	// Token: 0x0400250A RID: 9482
	public bool useExampleData;

	// Token: 0x0400250B RID: 9483
	private Dictionary<string, UIDataSource> dataSources;

	// Token: 0x0400250C RID: 9484
	private Dictionary<string, Dictionary<string, UIDataSource>> dataSourceGroups;

	// Token: 0x0400250D RID: 9485
	private Dictionary<string, Func<string, UIDataSource>> dataSourceConstructors;

	// Token: 0x0400250E RID: 9486
	private bool initComplete;

	// Token: 0x0400250F RID: 9487
	public static List<string> availableDataTypes = new List<string>
	{
		"RecentWorlds",
		"PopularWorlds",
		"FeaturedWorlds",
		"RemoteUserWorlds",
		"CurrentUserUnpublishedWorlds",
		"CurrentUserPublishedWorlds",
		"CurrentUserBookmarkedWorlds",
		"CurrentUserUnpublishedModels",
		"WorldTemplates",
		"WorldCategories",
		"CurrentUserProfile",
		"UserProfile",
		"PlayerSpotlight",
		"CuratedWorlds"
	};
}
