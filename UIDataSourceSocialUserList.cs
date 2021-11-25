using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003F3 RID: 1011
public class UIDataSourceSocialUserList : UIDataSource
{
	// Token: 0x06002C6C RID: 11372 RVA: 0x0013EB56 File Offset: 0x0013CF56
	public UIDataSourceSocialUserList(UIDataManager manager) : base(manager)
	{
	}

	// Token: 0x06002C6D RID: 11373 RVA: 0x0013EB6A File Offset: 0x0013CF6A
	public UIDataSourceSocialUserList(string apiPath, UIDataManager manager) : base(manager)
	{
		this.apiPath = apiPath;
	}

	// Token: 0x06002C6E RID: 11374 RVA: 0x0013EB88 File Offset: 0x0013CF88
	public static UIDataSourceSocialUserList FollowersListForCurrentUser(UIDataManager manager)
	{
		UIDataSourceSocialUserList uidataSourceSocialUserList = new UIDataSourceSocialUserList(manager);
		uidataSourceSocialUserList.loadFromCurrentUserFollowers = true;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(uidataSourceSocialUserList.Refresh));
		return uidataSourceSocialUserList;
	}

	// Token: 0x06002C6F RID: 11375 RVA: 0x0013EBBC File Offset: 0x0013CFBC
	public static UIDataSourceSocialUserList FollowedUserListForCurrentUser(UIDataManager manager)
	{
		UIDataSourceSocialUserList uidataSourceSocialUserList = new UIDataSourceSocialUserList(manager);
		uidataSourceSocialUserList.loadFromCurrentUserFollowedUsers = true;
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(uidataSourceSocialUserList.Refresh));
		return uidataSourceSocialUserList;
	}

	// Token: 0x06002C70 RID: 11376 RVA: 0x0013EBEF File Offset: 0x0013CFEF
	public static UIDataSourceSocialUserList FollowersListForUser(UIDataManager manager, string userID)
	{
		return new UIDataSourceSocialUserList(string.Format("/api/v1/user/{0}/followers", userID), manager);
	}

	// Token: 0x06002C71 RID: 11377 RVA: 0x0013EC02 File Offset: 0x0013D002
	public static UIDataSourceSocialUserList FollowedUserListForUser(UIDataManager manager, string userID)
	{
		return new UIDataSourceSocialUserList(string.Format("/api/v1/user/{0}/followed_users", userID), manager);
	}

	// Token: 0x06002C72 RID: 11378 RVA: 0x0013EC18 File Offset: 0x0013D018
	public override void Refresh()
	{
		base.ClearData();
		base.loadState = UIDataSource.LoadState.Loading;
		if (this.loadFromCurrentUserFollowers)
		{
			if (BWUserDataManager.Instance.followers == null)
			{
				BWUserDataManager.Instance.LoadFollowers();
				return;
			}
			foreach (BWSocialUser bwsocialUser in BWUserDataManager.Instance.followers)
			{
				string text = bwsocialUser.userID.ToString();
				Dictionary<string, string> value = bwsocialUser.AttributesForMenuUI();
				base.Keys.Add(text);
				base.Data.Add(text, value);
			}
			base.Info.Add("count", BWUserDataManager.Instance.followers.Count.ToString());
			base.Info.Add("empty", (BWUserDataManager.Instance.followers.Count == 0).ToString());
			base.loadState = UIDataSource.LoadState.Loaded;
		}
		else if (this.loadFromCurrentUserFollowedUsers)
		{
			if (BWUserDataManager.Instance.followedUsers == null)
			{
				BWUserDataManager.Instance.LoadFollowedUsers();
				return;
			}
			foreach (BWSocialUser bwsocialUser2 in BWUserDataManager.Instance.followedUsers)
			{
				string text2 = bwsocialUser2.userID.ToString();
				Dictionary<string, string> value2 = bwsocialUser2.AttributesForMenuUI();
				base.Keys.Add(text2);
				base.Data.Add(text2, value2);
			}
			base.Info.Add("count", BWUserDataManager.Instance.followedUsers.Count.ToString());
			base.Info.Add("empty", (BWUserDataManager.Instance.followedUsers.Count == 0).ToString());
			base.loadState = UIDataSource.LoadState.Loaded;
		}
		else
		{
			base.ClearData();
			base.loadState = UIDataSource.LoadState.Loading;
			BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", this.apiPath);
			bwapirequestBase.onSuccess = delegate(JObject responseJson)
			{
				List<JObject> arrayValue = responseJson["attrs_for_follow_users"].ArrayValue;
				if (arrayValue != null)
				{
					foreach (JObject json in arrayValue)
					{
						BWSocialUser bwsocialUser3 = new BWSocialUser(json);
						string text3 = bwsocialUser3.userID.ToString();
						Dictionary<string, string> value3 = bwsocialUser3.AttributesForMenuUI();
						base.Keys.Add(text3);
						base.Data.Add(text3, value3);
					}
				}
				base.Info.Add("count", base.Keys.Count.ToString());
				base.Info.Add("empty", (base.Keys.Count == 0).ToString());
				base.loadState = UIDataSource.LoadState.Loaded;
			};
			bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
			{
				base.loadState = UIDataSource.LoadState.Failed;
			};
			bwapirequestBase.SendOwnerCoroutine(this.dataManager);
		}
	}

	// Token: 0x0400253D RID: 9533
	private string apiPath = string.Empty;

	// Token: 0x0400253E RID: 9534
	private bool loadFromCurrentUserFollowers;

	// Token: 0x0400253F RID: 9535
	private bool loadFromCurrentUserFollowedUsers;
}
