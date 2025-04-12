using System;
using System.Collections.Generic;
using System.IO;

// Token: 0x020003E4 RID: 996
public class UIDataSourceCurrentUser : UIDataSource
{
	// Token: 0x06002C0F RID: 11279 RVA: 0x0013CE10 File Offset: 0x0013B210
	public UIDataSourceCurrentUser(UIDataManager dataManager, BWUser user) : base(dataManager)
	{
		this.currentUser = user;
		BWUserDataManager.Instance.AddListener(new ProfileChangedEventHandler(this.OnUserProfileChanged));
	}

	// Token: 0x06002C10 RID: 11280 RVA: 0x0013CE38 File Offset: 0x0013B238
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		string text = this.currentUser.userID.ToString();
		base.Keys.Add(text);
		string value = BWFilesystem.FileProtocolPrefixStr + Path.Combine(BWFilesystem.CurrentUserProfileWorldFolder, "profile.png");
		Dictionary<string, string> value2 = new Dictionary<string, string>
		{
			{
				"id",
				this.currentUser.userID.ToString()
			},
			{
				"coins",
				this.currentUser.coins.ToString()
			},
			{
				"username",
				this.currentUser.username
			},
			{
				"profileImageUrl",
				value
			},
			{
				"iosAccountLinkAvailable",
				this.currentUser.iosAccountLinkAvailable.ToString()
			}
		};
		base.Data.Add(text, value2);
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C11 RID: 11281 RVA: 0x0013CF4B File Offset: 0x0013B34B
	private void OnUserProfileChanged()
	{
		this.Refresh();
	}

	// Token: 0x06002C12 RID: 11282 RVA: 0x0013CF53 File Offset: 0x0013B353
	public override string GetPlayButtonMessage()
	{
		return "LoadProfileWorld";
	}

	// Token: 0x06002C13 RID: 11283 RVA: 0x0013CF5A File Offset: 0x0013B35A
	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWUser.expectedImageUrlsForUI;
	}

	// Token: 0x06002C14 RID: 11284 RVA: 0x0013CF61 File Offset: 0x0013B361
	public static List<string> ExpectedDataKeysForUI()
	{
		return BWUser.expectedDataKeysForUI;
	}

	// Token: 0x04002527 RID: 9511
	private BWUser currentUser;
}
