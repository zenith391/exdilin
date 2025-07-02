using System.Collections.Generic;
using SimpleJSON;

public class UIDataSourceSocialUserList : UIDataSource
{
	private string apiPath = string.Empty;

	private bool loadFromCurrentUserFollowers;

	private bool loadFromCurrentUserFollowedUsers;

	public UIDataSourceSocialUserList(UIDataManager manager)
		: base(manager)
	{
	}

	public UIDataSourceSocialUserList(string apiPath, UIDataManager manager)
		: base(manager)
	{
		this.apiPath = apiPath;
	}

	public static UIDataSourceSocialUserList FollowersListForCurrentUser(UIDataManager manager)
	{
		UIDataSourceSocialUserList uIDataSourceSocialUserList = new UIDataSourceSocialUserList(manager);
		uIDataSourceSocialUserList.loadFromCurrentUserFollowers = true;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(uIDataSourceSocialUserList.Refresh);
		return uIDataSourceSocialUserList;
	}

	public static UIDataSourceSocialUserList FollowedUserListForCurrentUser(UIDataManager manager)
	{
		UIDataSourceSocialUserList uIDataSourceSocialUserList = new UIDataSourceSocialUserList(manager);
		uIDataSourceSocialUserList.loadFromCurrentUserFollowedUsers = true;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(uIDataSourceSocialUserList.Refresh);
		return uIDataSourceSocialUserList;
	}

	public static UIDataSourceSocialUserList FollowersListForUser(UIDataManager manager, string userID)
	{
		return new UIDataSourceSocialUserList($"/api/v1/user/{userID}/followers", manager);
	}

	public static UIDataSourceSocialUserList FollowedUserListForUser(UIDataManager manager, string userID)
	{
		return new UIDataSourceSocialUserList($"/api/v1/user/{userID}/followed_users", manager);
	}

	public override void Refresh()
	{
		ClearData();
		base.loadState = LoadState.Loading;
		if (loadFromCurrentUserFollowers)
		{
			if (BWUserDataManager.Instance.followers == null)
			{
				BWUserDataManager.Instance.LoadFollowers();
				return;
			}
			foreach (BWSocialUser follower in BWUserDataManager.Instance.followers)
			{
				string text = follower.userID.ToString();
				Dictionary<string, string> value = follower.AttributesForMenuUI();
				base.Keys.Add(text);
				base.Data.Add(text, value);
			}
			base.Info.Add("count", BWUserDataManager.Instance.followers.Count.ToString());
			base.Info.Add("empty", (BWUserDataManager.Instance.followers.Count == 0).ToString());
			base.loadState = LoadState.Loaded;
			return;
		}
		if (loadFromCurrentUserFollowedUsers)
		{
			if (BWUserDataManager.Instance.followedUsers == null)
			{
				BWUserDataManager.Instance.LoadFollowedUsers();
				return;
			}
			foreach (BWSocialUser followedUser in BWUserDataManager.Instance.followedUsers)
			{
				string text2 = followedUser.userID.ToString();
				Dictionary<string, string> value2 = followedUser.AttributesForMenuUI();
				base.Keys.Add(text2);
				base.Data.Add(text2, value2);
			}
			base.Info.Add("count", BWUserDataManager.Instance.followedUsers.Count.ToString());
			base.Info.Add("empty", (BWUserDataManager.Instance.followedUsers.Count == 0).ToString());
			base.loadState = LoadState.Loaded;
			return;
		}
		ClearData();
		base.loadState = LoadState.Loading;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", apiPath);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject item in arrayValue)
				{
					BWSocialUser bWSocialUser = new BWSocialUser(item);
					string text3 = bWSocialUser.userID.ToString();
					Dictionary<string, string> value3 = bWSocialUser.AttributesForMenuUI();
					base.Keys.Add(text3);
					base.Data.Add(text3, value3);
				}
			}
			base.Info.Add("count", base.Keys.Count.ToString());
			base.Info.Add("empty", (base.Keys.Count == 0).ToString());
			base.loadState = LoadState.Loaded;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			base.loadState = LoadState.Failed;
		};
		bWAPIRequestBase.SendOwnerCoroutine(dataManager);
	}
}
