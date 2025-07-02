using System.Collections.Generic;

public class UIDataSourcePublicModelList : UIDataSource
{
	private bool allPagesLoaded;

	private int categoryID;

	private string userIDStr;

	private string searchStr;

	private bool hideUsersOwnWorlds;

	private bool hideAlreadyPurchasedByUser;

	private BWU2UModelListType listType;

	public UIDataSourcePublicModelList(UIDataManager dataManager)
		: base(dataManager)
	{
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(CurrentUserDataChanged);
	}

	public static UIDataSourcePublicModelList PublicModelsForUser(UIDataManager dataManager, string userIDStr)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.User,
			userIDStr = userIDStr
		};
	}

	public static UIDataSourcePublicModelList BestSellersList(UIDataManager dataManager)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Popular
		};
	}

	public static UIDataSourcePublicModelList RecentList(UIDataManager dataManager)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Recent
		};
	}

	public static UIDataSourcePublicModelList ModelSearchResults(UIDataManager dataManager, string searchStr)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.SearchResults,
			searchStr = searchStr
		};
	}

	public static UIDataSourcePublicModelList ListForCategory(UIDataManager dataManager, int categoryID)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Category,
			categoryID = categoryID
		};
	}

	public override string GetPlayButtonMessage()
	{
		return "CommunityModelPreview";
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
		LoadFromDataManager();
	}

	public override void RequestFullData(string itemID)
	{
		base.loadState = LoadState.Loading;
		BWU2UModelDataManager.Instance.LoadFullModelFromRemote(itemID, this);
	}

	private void CurrentUserDataChanged()
	{
		ClearData();
		LoadFromDataManager();
	}

	private void LoadFromDataManager()
	{
		BWUserDataManager.Instance.LoadUserData();
		base.loadState = LoadState.Loading;
		switch (listType)
		{
		case BWU2UModelListType.Recent:
			BWU2UModelDataManager.Instance.LoadRecent(nextPage, this);
			break;
		case BWU2UModelListType.Popular:
			BWU2UModelDataManager.Instance.LoadBestSellers(nextPage, this);
			break;
		case BWU2UModelListType.SearchResults:
			BWU2UModelDataManager.Instance.LoadSearchResults(searchStr, nextPage, this);
			break;
		case BWU2UModelListType.Category:
			BWU2UModelDataManager.Instance.LoadCategory(categoryID, nextPage, this);
			break;
		case BWU2UModelListType.User:
			BWU2UModelDataManager.Instance.LoadUserPublicModels(userIDStr, nextPage, this);
			break;
		default:
			base.loadState = LoadState.Failed;
			break;
		}
	}

	public void DataManagerLoadedModelList(List<BWU2UModel> modelList, int nextPage, bool allPagesLoaded)
	{
		foreach (BWU2UModel model in modelList)
		{
			if (!base.Keys.Contains(model.modelID) && !BWUserDataManager.Instance.HasReportedModel(model.modelID))
			{
				Dictionary<string, string> value = model.AttributesForMenuUI();
				base.Keys.Add(model.modelID);
				base.Data.Add(model.modelID, value);
			}
		}
		base.nextPage = nextPage;
		this.allPagesLoaded = allPagesLoaded;
		SetDataTimestamp();
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	public void DataManagerFailedToLoadList()
	{
		base.loadState = LoadState.Failed;
		NotifyListeners();
	}

	public void DataManagerLoadedModel(BWU2UModel model)
	{
		if (!base.Keys.Contains(model.modelID))
		{
			base.Keys.Add(model.modelID);
		}
		base.Data[model.modelID] = model.AttributesForMenuUI();
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	public void DataManagerFailedToLoadModel()
	{
		base.loadState = LoadState.Failed;
		NotifyListeners();
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
			LoadFromDataManager();
		}
	}
}
