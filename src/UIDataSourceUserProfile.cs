using System.Collections.Generic;
using SimpleJSON;

public class UIDataSourceUserProfile : UIDataSource
{
	public int userID;

	public BWUser user;

	private bool currentUserIsFollowing;

	public UIDataSourceUserProfile(UIDataManager dataManager, string userIdStr)
		: base(dataManager)
	{
		int.TryParse(userIdStr, out userID);
		user = new BWUser();
		BWUserDataManager.Instance.AddCurrentUserDataChangedListener(CurrentUserDataChanged);
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		if (userID == 0)
		{
			BWLog.Error("Invalid userID");
			return;
		}
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/users/{userID.ToString()}/basic_info");
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			user.LoadFromJson(responseJson);
			string text = user.userID.ToString();
			Dictionary<string, string> dictionary = user.AttributesForUserProfileUI();
			currentUserIsFollowing = BWUserDataManager.Instance.CurrentUserIsFollowing(user.userID);
			dictionary.Add("currentUserIsFollowing", currentUserIsFollowing.ToString());
			base.Keys.Add(text);
			base.Data.Add(text, dictionary);
			base.loadState = LoadState.Loaded;
			NotifyListeners();
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
			base.loadState = LoadState.Failed;
		};
		bWAPIRequestBase.Send();
	}

	private void CurrentUserDataChanged()
	{
		if (user != null && base.Data != null && base.Data.ContainsKey(user.userID.ToString()))
		{
			currentUserIsFollowing = BWUserDataManager.Instance.CurrentUserIsFollowing(user.userID);
			base.Data[user.userID.ToString()]["currentUserIsFollowing"] = currentUserIsFollowing.ToString();
			NotifyListeners();
		}
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
