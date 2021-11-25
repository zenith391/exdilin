using System;
using System.Collections.Generic;

// Token: 0x020003EE RID: 1006
public class UIDataSourcePublicModelList : UIDataSource
{
	// Token: 0x06002C49 RID: 11337 RVA: 0x0013E238 File Offset: 0x0013C638
	public UIDataSourcePublicModelList(UIDataManager dataManager) : base(dataManager)
	{
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(this.CurrentUserDataChanged));
	}

	// Token: 0x06002C4A RID: 11338 RVA: 0x0013E258 File Offset: 0x0013C658
	public static UIDataSourcePublicModelList PublicModelsForUser(UIDataManager dataManager, string userIDStr)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.User,
			userIDStr = userIDStr
		};
	}

	// Token: 0x06002C4B RID: 11339 RVA: 0x0013E27C File Offset: 0x0013C67C
	public static UIDataSourcePublicModelList BestSellersList(UIDataManager dataManager)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Popular
		};
	}

	// Token: 0x06002C4C RID: 11340 RVA: 0x0013E298 File Offset: 0x0013C698
	public static UIDataSourcePublicModelList RecentList(UIDataManager dataManager)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Recent
		};
	}

	// Token: 0x06002C4D RID: 11341 RVA: 0x0013E2B4 File Offset: 0x0013C6B4
	public static UIDataSourcePublicModelList ModelSearchResults(UIDataManager dataManager, string searchStr)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.SearchResults,
			searchStr = searchStr
		};
	}

	// Token: 0x06002C4E RID: 11342 RVA: 0x0013E2D8 File Offset: 0x0013C6D8
	public static UIDataSourcePublicModelList ListForCategory(UIDataManager dataManager, int categoryID)
	{
		return new UIDataSourcePublicModelList(dataManager)
		{
			listType = BWU2UModelListType.Category,
			categoryID = categoryID
		};
	}

	// Token: 0x06002C4F RID: 11343 RVA: 0x0013E2FB File Offset: 0x0013C6FB
	public override string GetPlayButtonMessage()
	{
		return "CommunityModelPreview";
	}

	// Token: 0x06002C50 RID: 11344 RVA: 0x0013E302 File Offset: 0x0013C702
	public bool AllPagesLoaded()
	{
		return this.allPagesLoaded;
	}

	// Token: 0x06002C51 RID: 11345 RVA: 0x0013E30A File Offset: 0x0013C70A
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		this.nextPage = 1;
		this.LoadFromDataManager();
	}

	// Token: 0x06002C52 RID: 11346 RVA: 0x0013E325 File Offset: 0x0013C725
	public override void RequestFullData(string itemID)
	{
		base.loadState = UIDataSource.LoadState.Loading;
		BWU2UModelDataManager.Instance.LoadFullModelFromRemote(itemID, this);
	}

	// Token: 0x06002C53 RID: 11347 RVA: 0x0013E33A File Offset: 0x0013C73A
	private void CurrentUserDataChanged()
	{
		base.ClearData();
		this.LoadFromDataManager();
	}

	// Token: 0x06002C54 RID: 11348 RVA: 0x0013E348 File Offset: 0x0013C748
	private void LoadFromDataManager()
	{
		BWUserDataManager.Instance.LoadUserData();
		base.loadState = UIDataSource.LoadState.Loading;
		switch (this.listType)
		{
		case BWU2UModelListType.Recent:
			BWU2UModelDataManager.Instance.LoadRecent(this.nextPage, this);
			break;
		case BWU2UModelListType.Popular:
			BWU2UModelDataManager.Instance.LoadBestSellers(this.nextPage, this);
			break;
		case BWU2UModelListType.SearchResults:
			BWU2UModelDataManager.Instance.LoadSearchResults(this.searchStr, this.nextPage, this);
			break;
		case BWU2UModelListType.Category:
			BWU2UModelDataManager.Instance.LoadCategory(this.categoryID, this.nextPage, this);
			break;
		case BWU2UModelListType.User:
			BWU2UModelDataManager.Instance.LoadUserPublicModels(this.userIDStr, this.nextPage, this);
			break;
		default:
			base.loadState = UIDataSource.LoadState.Failed;
			break;
		}
	}

	// Token: 0x06002C55 RID: 11349 RVA: 0x0013E418 File Offset: 0x0013C818
	public void DataManagerLoadedModelList(List<BWU2UModel> modelList, int nextPage, bool allPagesLoaded)
	{
		foreach (BWU2UModel bwu2UModel in modelList)
		{
			if (!base.Keys.Contains(bwu2UModel.modelID))
			{
				if (!BWUserDataManager.Instance.HasReportedModel(bwu2UModel.modelID))
				{
					Dictionary<string, string> value = bwu2UModel.AttributesForMenuUI();
					base.Keys.Add(bwu2UModel.modelID);
					base.Data.Add(bwu2UModel.modelID, value);
				}
			}
		}
		this.nextPage = nextPage;
		this.allPagesLoaded = allPagesLoaded;
		base.SetDataTimestamp();
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C56 RID: 11350 RVA: 0x0013E4E8 File Offset: 0x0013C8E8
	public void DataManagerFailedToLoadList()
	{
		base.loadState = UIDataSource.LoadState.Failed;
		base.NotifyListeners();
	}

	// Token: 0x06002C57 RID: 11351 RVA: 0x0013E4F8 File Offset: 0x0013C8F8
	public void DataManagerLoadedModel(BWU2UModel model)
	{
		if (!base.Keys.Contains(model.modelID))
		{
			base.Keys.Add(model.modelID);
		}
		base.Data[model.modelID] = model.AttributesForMenuUI();
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C58 RID: 11352 RVA: 0x0013E550 File Offset: 0x0013C950
	public void DataManagerFailedToLoadModel()
	{
		base.loadState = UIDataSource.LoadState.Failed;
		base.NotifyListeners();
	}

	// Token: 0x06002C59 RID: 11353 RVA: 0x0013E55F File Offset: 0x0013C95F
	public override bool CanExpand()
	{
		return base.loadState == UIDataSource.LoadState.Loaded && !this.allPagesLoaded;
	}

	// Token: 0x06002C5A RID: 11354 RVA: 0x0013E579 File Offset: 0x0013C979
	public override void Expand()
	{
		if (!this.allPagesLoaded)
		{
			this.LoadFromDataManager();
		}
	}

	// Token: 0x04002532 RID: 9522
	private bool allPagesLoaded;

	// Token: 0x04002533 RID: 9523
	private int categoryID;

	// Token: 0x04002534 RID: 9524
	private string userIDStr;

	// Token: 0x04002535 RID: 9525
	private string searchStr;

	// Token: 0x04002536 RID: 9526
	private bool hideUsersOwnWorlds;

	// Token: 0x04002537 RID: 9527
	private bool hideAlreadyPurchasedByUser;

	// Token: 0x04002538 RID: 9528
	private BWU2UModelListType listType;
}
