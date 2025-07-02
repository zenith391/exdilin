using System.Collections.Generic;
using System.IO;

public class UIDataSourceCurrentUser : UIDataSource
{
	private BWUser currentUser;

	public UIDataSourceCurrentUser(UIDataManager dataManager, BWUser user)
		: base(dataManager)
	{
		currentUser = user;
		BWUserDataManager.Instance.AddListener(OnUserProfileChanged);
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		string text = currentUser.userID.ToString();
		base.Keys.Add(text);
		string value = BWFilesystem.FileProtocolPrefixStr + Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profile.png");
		Dictionary<string, string> value2 = new Dictionary<string, string>
		{
			{
				"id",
				currentUser.userID.ToString()
			},
			{
				"coins",
				currentUser.coins.ToString()
			},
			{ "username", currentUser.username },
			{ "profileImageUrl", value },
			{
				"iosAccountLinkAvailable",
				currentUser.iosAccountLinkAvailable.ToString()
			}
		};
		base.Data.Add(text, value2);
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	private void OnUserProfileChanged()
	{
		Refresh();
	}

	public override string GetPlayButtonMessage()
	{
		return "LoadProfileWorld";
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
