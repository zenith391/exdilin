using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDataManager : MonoBehaviour
{
	public bool useExampleData;

	private Dictionary<string, UIDataSource> dataSources;

	private Dictionary<string, Dictionary<string, UIDataSource>> dataSourceGroups;

	private Dictionary<string, Func<string, UIDataSource>> dataSourceConstructors;

	private bool initComplete;

	public static List<string> availableDataTypes = new List<string>
	{
		"RecentWorlds", "PopularWorlds", "FeaturedWorlds", "RemoteUserWorlds", "CurrentUserUnpublishedWorlds", "CurrentUserPublishedWorlds", "CurrentUserBookmarkedWorlds", "CurrentUserUnpublishedModels", "WorldTemplates", "WorldCategories",
		"CurrentUserProfile", "UserProfile", "PlayerSpotlight", "CuratedWorlds"
	};

	public void Init()
	{
		if (useExampleData)
		{
			InitWithExampleData(30);
		}
		else
		{
			if (initComplete)
			{
				return;
			}
			AddToDataSources("CurrentUserProfile", new UIDataSourceCurrentUser(this, BWUser.currentUser));
			AddToDataSources("RecentWorlds", UIDataSourceWorldList.NewWorlds(this));
			AddToDataSources("PopularWorlds", UIDataSourceWorldList.PopularWorlds(this));
			AddToDataSources("WorldSpotlight", UIDataSourceWorldList.WorldSpotlight(this));
			AddToDataSources("FeaturedWorlds", UIDataSourceWorldList.HallOfFame(this));
			AddToDataSources("CuratedWorlds", UIDataSourceWorldList.Curated(this));
			AddToDataSources("CurrentUserWorlds", new UIDataSourceLocalWorldList(this));
			AddToDataSources("CurrentUserUnpublishedWorlds", UIDataSourceLocalWorldList.CurrentUserUnpublishedWorlds(this));
			AddToDataSources("CurrentUserPublishedWorlds", UIDataSourceLocalWorldList.CurrentUserPublishedWorlds(this));
			AddToDataSources("CurrentUserBookmarkedWorlds", UIDataSourceWorldList.LikedWorldsForCurrentUser(this));
			AddToDataSources("CurrentUserPublicWorlds", UIDataSourceWorldList.PublicWorldsForCurrentUser(this));
			AddToDataSources("CurrentUserActivity", UIDataSourceUserActivity.UserActivityForCurrentUser(this));
			AddToDataSources("NewsFeed", new UIDataSourceNewsFeed(this));
			AddToDataSources("RecentModels", UIDataSourcePublicModelList.RecentList(this));
			AddToDataSources("PopularModels", UIDataSourcePublicModelList.BestSellersList(this));
			AddToDataSources("PurchasedModels", new UIDataSourcePurchasedModelList(this));
			AddToDataSources("CurrentUserUnpublishedModels", UIDataSourceLocalModelList.CurrentUserUnpublishedModels(this));
			AddToDataSources("CurrentUserPublishedModels", UIDataSourceLocalModelList.CurrentUserPublishedModels(this));
			AddToDataSources("WorldTemplates", new UIDataSourceWorldTemplates(this));
			AddToDataSources("WorldCategories", new UIDataSourceWorldCategories(this));
			AddToDataSources("ModelCategories", new UIDataSourceModelCategories(this));
			AddToDataSources("ShoppingCart", new UIDataSourceShoppingCart(this, blocks: true, models: true));
			AddToDataSources("ShoppingCartBlocks", new UIDataSourceShoppingCart(this, blocks: true, models: false));
			AddToDataSources("ShoppingCartModels", new UIDataSourceShoppingCart(this, blocks: false, models: true));
			AddToDataSources("PlayerSpotlight", UIDataSourceUserList.UserListForPlayerSpotlight(this));
			AddToDataSources("CoinPacks", new UIDataSourceCoinPacks(this));
			AddToDataSources("PendingPayouts", new UIDataSourcePendingPayouts(this));
			AddToDataSources("CurrentUserFollowers", UIDataSourceSocialUserList.FollowersListForCurrentUser(this));
			AddToDataSources("CurrentUserFollowedUsers", UIDataSourceSocialUserList.FollowedUserListForCurrentUser(this));
			foreach (BWCategory visibleWorldCategory in BWCategory.visibleWorldCategories)
			{
				string dataSubtype = visibleWorldCategory.categoryID.ToString();
				AddToDataSourceGroups("RecentWorlds", dataSubtype, new UIDataSourceWorldList(this, visibleWorldCategory.categoryID, "recent"));
				AddToDataSourceGroups("PopularWorlds", dataSubtype, new UIDataSourceWorldList(this, visibleWorldCategory.categoryID, "most_popular"));
				AddToDataSourceGroups("FeaturedWorlds", dataSubtype, new UIDataSourceWorldList(this, visibleWorldCategory.categoryID, "featured"));
			}
			foreach (BWCategory modelCategory in BWCategory.modelCategories)
			{
				int categoryID = modelCategory.categoryID;
				AddToDataSourceGroups("PopularModels", categoryID.ToString(), UIDataSourcePublicModelList.ListForCategory(this, categoryID));
			}
			foreach (string blockShopCategoryIdentifier in BWBlockShopData.GetBlockShopCategoryIdentifiers())
			{
				AddToDataSourceGroups("BlockShopSectionTitles", blockShopCategoryIdentifier, new UIDataSourceBlockShopSectionTitles(this, blockShopCategoryIdentifier));
				foreach (string item in BWBlockShopData.GetSectionTitlesForCategory(blockShopCategoryIdentifier))
				{
					string dataType = "BlockShop_" + BWBlockShopData.GetTitleForCategory(blockShopCategoryIdentifier);
					AddToDataSourceGroups(dataType, item, new UIDataSourceBlockShopContents(this, blockShopCategoryIdentifier, item));
				}
			}
			AddToDataSourceConstructors("UserProfile", (string userIDStr) => new UIDataSourceUserProfile(this, userIDStr));
			AddToDataSourceConstructors("RemoteUserWorlds", (string userIDStr) => UIDataSourceWorldList.PublishedWorldsForUser(this, userIDStr));
			AddToDataSourceConstructors("RemoteUserBookmarkedWorlds", (string userIDStr) => UIDataSourceWorldList.LikedWorldsForUser(this, userIDStr));
			AddToDataSourceConstructors("WorldSearch", (string searchStr) => UIDataSourceWorldList.WorldSearchResults(this, searchStr));
			AddToDataSourceConstructors("RemoteUserModels", (string userIDStr) => UIDataSourcePublicModelList.PublicModelsForUser(this, userIDStr));
			AddToDataSourceConstructors("UserFollowers", (string userIDStr) => UIDataSourceSocialUserList.FollowersListForUser(this, userIDStr));
			AddToDataSourceConstructors("UserFollowedUsers", (string userIDStr) => UIDataSourceSocialUserList.FollowedUserListForUser(this, userIDStr));
			AddToDataSourceConstructors("UserActivity", (string userIDStr) => new UIDataSourceUserActivity(this, userIDStr));
			AddToDataSourceConstructors("RemoteWorld", (string worldID) => new UIDataSourceSingleWorld(this, worldID));
			AddToDataSourceConstructors("U2UModel", (string modelID) => new UIDataSourceSingleU2UModel(this, modelID));
			initComplete = true;
		}
	}

	private void AddToDataSources(string dataType, UIDataSource dataSource)
	{
		if (dataSources == null)
		{
			dataSources = new Dictionary<string, UIDataSource>();
		}
		dataSource.dataType = dataType;
		dataSources.Add(dataType, dataSource);
	}

	private void AddToDataSourceGroups(string dataType, string dataSubtype, UIDataSource dataSource)
	{
		if (dataSourceGroups == null)
		{
			dataSourceGroups = new Dictionary<string, Dictionary<string, UIDataSource>>();
		}
		if (!dataSourceGroups.ContainsKey(dataType))
		{
			dataSourceGroups.Add(dataType, new Dictionary<string, UIDataSource>());
		}
		if (dataSourceGroups[dataType].ContainsKey(dataSubtype))
		{
			BWLog.Error("Duplicate dataSource " + dataType + " subtype " + dataSubtype);
			return;
		}
		dataSource.dataType = dataType;
		dataSource.dataSubtype = dataSubtype;
		dataSourceGroups[dataType].Add(dataSubtype, dataSource);
	}

	private void AddToDataSourceConstructors(string dataType, Func<string, UIDataSource> dataSourceConstructor)
	{
		if (dataSourceConstructors == null)
		{
			dataSourceConstructors = new Dictionary<string, Func<string, UIDataSource>>();
		}
		dataSourceConstructors.Add(dataType, dataSourceConstructor);
	}

	public void InitWithExampleData(int contentSize)
	{
		dataSources = new Dictionary<string, UIDataSource>();
		dataSources.Add("RecentWorlds", new UIDataSourceExample(this, contentSize));
		dataSources.Add("PopularWorlds", new UIDataSourceExample(this, contentSize));
		dataSources.Add("FeaturedWorlds", new UIDataSourceExample(this, contentSize));
		dataSources.Add("CuratedWorlds", new UIDataSourceExample(this, contentSize));
		dataSources.Add("WorldTemplates", new UIDataSourceExampleTemplates(this));
		dataSourceGroups = new Dictionary<string, Dictionary<string, UIDataSource>>();
	}

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
		default:
			return null;
		}
	}

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
		default:
			return null;
		}
	}

	public void ClearListeners()
	{
		if (dataSources == null)
		{
			return;
		}
		foreach (UIDataSource value in dataSources.Values)
		{
			value.ClearListeners();
		}
	}

	public UIDataSource GetDataSource(string dataType, string dataSubtype)
	{
		UIDataSource value = null;
		if (!string.IsNullOrEmpty(dataSubtype))
		{
			if (dataSourceGroups.TryGetValue(dataType, out var value2))
			{
				value2.TryGetValue(dataSubtype, out value);
			}
			if (value == null && dataSourceConstructors.TryGetValue(dataType, out var value3))
			{
				value = value3(dataSubtype);
				value.dataType = dataType;
				value.dataSubtype = dataSubtype;
			}
		}
		else if (!string.IsNullOrEmpty(dataType))
		{
			dataSources.TryGetValue(dataType, out value);
		}
		if (value == null)
		{
			if (useExampleData)
			{
				value = new UIDataSourceExample(this, 10);
			}
			else
			{
				string s = "No data source for data type: " + dataType + ((dataSubtype != null) ? ("subtype: " + dataSubtype) : string.Empty);
				BWLog.Error(s);
			}
		}
		return value;
	}
}
