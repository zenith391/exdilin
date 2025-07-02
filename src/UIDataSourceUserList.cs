using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDataSourceUserList : UIDataSource
{
	private List<int> userIds;

	private List<UIDataSourceUserProfile> userDataSources;

	public UIDataSourceUserList(UIDataManager dataManager)
		: base(dataManager)
	{
	}

	public static UIDataSourceUserList UserListForPlayerSpotlight(UIDataManager dataManager)
	{
		UIDataSourceUserList uIDataSourceUserList = new UIDataSourceUserList(dataManager);
		uIDataSourceUserList.userIds = new List<int>();
		switch (BWEnvConfig.BLOCKSWORLD_ENVIRONMENT)
		{
		case "prod_demo":
		case "production":
			uIDataSourceUserList.userIds.Add(3213492);
			uIDataSourceUserList.userIds.Add(4454170);
			uIDataSourceUserList.userIds.Add(3469622);
			uIDataSourceUserList.userIds.Add(3848409);
			break;
		}
		uIDataSourceUserList.userDataSources = new List<UIDataSourceUserProfile>();
		foreach (int userId in uIDataSourceUserList.userIds)
		{
			uIDataSourceUserList.userDataSources.Add(new UIDataSourceUserProfile(dataManager, userId.ToString()));
		}
		return uIDataSourceUserList;
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		dataManager.StartCoroutine(LoadChildDataSources());
	}

	private IEnumerator LoadChildDataSources()
	{
		foreach (UIDataSourceUserProfile userDataSource in userDataSources)
		{
			userDataSource.Refresh();
		}
		while (!userDataSources.TrueForAll((UIDataSourceUserProfile d) => d.IsDataLoaded()))
		{
			yield return null;
		}
		foreach (UIDataSourceUserProfile userDataSource2 in userDataSources)
		{
			BWUser user = userDataSource2.user;
			Debug.Log("Adding user to user list: " + user.username);
			base.Keys.Add(user.userID.ToString());
			base.Data.Add(user.userID.ToString(), user.AttributesForUserProfileUI());
		}
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWUser.expectedImageUrlsForUI;
	}

	public static List<string> ExpectedDataKeysForUI()
	{
		return BWUser.expectedDataKeysForUI;
	}
}
