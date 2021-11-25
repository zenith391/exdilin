using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003F6 RID: 1014
public class UIDataSourceUserProfile : UIDataSource
{
	// Token: 0x06002C80 RID: 11392 RVA: 0x0013F627 File Offset: 0x0013DA27
	public UIDataSourceUserProfile(UIDataManager dataManager, string userIdStr) : base(dataManager)
	{
		int.TryParse(userIdStr, out this.userID);
		this.user = new BWUser();
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(new CurrentUserDataChangedEventHandler(this.CurrentUserDataChanged));
	}

	// Token: 0x06002C81 RID: 11393 RVA: 0x0013F660 File Offset: 0x0013DA60
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		if (this.userID == 0)
		{
			BWLog.Error("Invalid userID");
			return;
		}
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", string.Format("/api/v1/users/{0}/basic_info", this.userID.ToString()));
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this.user.LoadFromJson(responseJson);
			string text = this.user.userID.ToString();
			Dictionary<string, string> dictionary = this.user.AttributesForUserProfileUI();
			this.currentUserIsFollowing = BWUserDataManager.Instance.CurrentUserIsFollowing(this.user.userID);
			dictionary.Add("currentUserIsFollowing", this.currentUserIsFollowing.ToString());
			base.Keys.Add(text);
			base.Data.Add(text, dictionary);
			base.loadState = UIDataSource.LoadState.Loaded;
			base.NotifyListeners();
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
			base.loadState = UIDataSource.LoadState.Failed;
		};
		bwapirequestBase.Send();
	}

	// Token: 0x06002C82 RID: 11394 RVA: 0x0013F6E4 File Offset: 0x0013DAE4
	private void CurrentUserDataChanged()
	{
		if (this.user == null)
		{
			return;
		}
		if (base.Data == null || !base.Data.ContainsKey(this.user.userID.ToString()))
		{
			return;
		}
		this.currentUserIsFollowing = BWUserDataManager.Instance.CurrentUserIsFollowing(this.user.userID);
		base.Data[this.user.userID.ToString()]["currentUserIsFollowing"] = this.currentUserIsFollowing.ToString();
		base.NotifyListeners();
	}

	// Token: 0x06002C83 RID: 11395 RVA: 0x0013F792 File Offset: 0x0013DB92
	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWUser.expectedImageUrlsForUI;
	}

	// Token: 0x06002C84 RID: 11396 RVA: 0x0013F799 File Offset: 0x0013DB99
	public static List<string> ExpectedDataKeysForUI()
	{
		return BWUser.expectedDataKeysForUI;
	}

	// Token: 0x04002544 RID: 9540
	public int userID;

	// Token: 0x04002545 RID: 9541
	public BWUser user;

	// Token: 0x04002546 RID: 9542
	private bool currentUserIsFollowing;
}
