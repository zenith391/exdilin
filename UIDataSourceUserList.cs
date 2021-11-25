using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003F5 RID: 1013
public class UIDataSourceUserList : UIDataSource
{
	// Token: 0x06002C7A RID: 11386 RVA: 0x0013F286 File Offset: 0x0013D686
	public UIDataSourceUserList(UIDataManager dataManager) : base(dataManager)
	{
	}

	// Token: 0x06002C7B RID: 11387 RVA: 0x0013F290 File Offset: 0x0013D690
	public static UIDataSourceUserList UserListForPlayerSpotlight(UIDataManager dataManager)
	{
		UIDataSourceUserList uidataSourceUserList = new UIDataSourceUserList(dataManager);
		uidataSourceUserList.userIds = new List<int>();
		string blocksworld_ENVIRONMENT = BWEnvConfig.BLOCKSWORLD_ENVIRONMENT;
		if (blocksworld_ENVIRONMENT != null)
		{
			if (!(blocksworld_ENVIRONMENT == "local"))
			{
				if (!(blocksworld_ENVIRONMENT == "develop"))
				{
					if (!(blocksworld_ENVIRONMENT == "staging"))
					{
						if (blocksworld_ENVIRONMENT == "prod_demo" || blocksworld_ENVIRONMENT == "production")
						{
							uidataSourceUserList.userIds.Add(3213492);
							uidataSourceUserList.userIds.Add(4454170);
							uidataSourceUserList.userIds.Add(3469622);
							uidataSourceUserList.userIds.Add(3848409);
						}
					}
				}
			}
		}
		uidataSourceUserList.userDataSources = new List<UIDataSourceUserProfile>();
		foreach (int num in uidataSourceUserList.userIds)
		{
			uidataSourceUserList.userDataSources.Add(new UIDataSourceUserProfile(dataManager, num.ToString()));
		}
		return uidataSourceUserList;
	}

	// Token: 0x06002C7C RID: 11388 RVA: 0x0013F3D8 File Offset: 0x0013D7D8
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		this.dataManager.StartCoroutine(this.LoadChildDataSources());
	}

	// Token: 0x06002C7D RID: 11389 RVA: 0x0013F3F8 File Offset: 0x0013D7F8
	private IEnumerator LoadChildDataSources()
	{
		foreach (UIDataSourceUserProfile uidataSourceUserProfile in this.userDataSources)
		{
			uidataSourceUserProfile.Refresh();
		}
		for (;;)
		{
			if (this.userDataSources.TrueForAll((UIDataSourceUserProfile d) => d.IsDataLoaded()))
			{
				break;
			}
			yield return null;
		}
		foreach (UIDataSourceUserProfile uidataSourceUserProfile2 in this.userDataSources)
		{
			BWUser user = uidataSourceUserProfile2.user;
			Debug.Log("Adding user to user list: " + user.username);
			base.Keys.Add(user.userID.ToString());
			base.Data.Add(user.userID.ToString(), user.AttributesForUserProfileUI());
		}
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
		yield break;
	}

	// Token: 0x06002C7E RID: 11390 RVA: 0x0013F413 File Offset: 0x0013D813
	public static List<string> ExpectedImageUrlsForUI()
	{
		return BWUser.expectedImageUrlsForUI;
	}

	// Token: 0x06002C7F RID: 11391 RVA: 0x0013F41A File Offset: 0x0013D81A
	public static List<string> ExpectedDataKeysForUI()
	{
		return BWUser.expectedDataKeysForUI;
	}

	// Token: 0x04002542 RID: 9538
	private List<int> userIds;

	// Token: 0x04002543 RID: 9539
	private List<UIDataSourceUserProfile> userDataSources;
}
