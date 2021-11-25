using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003F4 RID: 1012
public class UIDataSourceUserActivity : UIDataSource
{
	// Token: 0x06002C75 RID: 11381 RVA: 0x0013EFC5 File Offset: 0x0013D3C5
	public UIDataSourceUserActivity(UIDataManager manager, string userID) : base(manager)
	{
		this.userIDStr = userID;
	}

	// Token: 0x06002C76 RID: 11382 RVA: 0x0013EFD8 File Offset: 0x0013D3D8
	public static UIDataSourceUserActivity UserActivityForCurrentUser(UIDataManager manager)
	{
		return new UIDataSourceUserActivity(manager, BWUser.currentUser.userID.ToString())
		{
			forCurrentUser = true
		};
	}

	// Token: 0x06002C77 RID: 11383 RVA: 0x0013F00C File Offset: 0x0013D40C
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		base.SectionStartIndicies = new List<int>();
		base.loadState = UIDataSource.LoadState.Loading;
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", string.Format("/api/v1/user/{0}/recent_activity", this.userIDStr));
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			base.loadState = UIDataSource.LoadState.Loaded;
			BWUserActivityFeed bwuserActivityFeed = new BWUserActivityFeed(respJson, this.forCurrentUser);
			base.Info.Add("followers_count", bwuserActivityFeed.followersCount.ToString());
			int num = 0;
			for (int i = 0; i < bwuserActivityFeed.activityList.Count; i++)
			{
				BWUserActivity bwuserActivity = bwuserActivityFeed.activityList[i];
				if (num < 5 && i < bwuserActivityFeed.activitySectionIndexList.Count && i == bwuserActivityFeed.activitySectionIndexList[num])
				{
					base.SectionStartIndicies.Add(num + i);
					num++;
					string text = string.Empty;
					DateTime utcNow = DateTime.UtcNow;
					if (utcNow.Date == bwuserActivity.timestamp.Date)
					{
						text = "Today";
					}
					else if (utcNow.Date.AddDays(-1.0) == bwuserActivity.timestamp.Date)
					{
						text = "Yesterday";
					}
					else if (num == 5)
					{
						text = bwuserActivity.timestamp.ToLongDateString() + " and earlier";
					}
					else
					{
						text = bwuserActivity.timestamp.ToLongDateString();
					}
					Dictionary<string, string> value = new Dictionary<string, string>
					{
						{
							"title",
							text
						}
					};
					base.Keys.Add(text);
					base.Data.Add(text, value);
					base.PanelOverrides[text] = 0;
				}
				string text2 = i.ToString();
				base.Keys.Add(text2);
				Dictionary<string, string> value2 = bwuserActivity.AttributesForMenuUI();
				base.Data.Add(text2, value2);
			}
			base.Info.Add("empty", (base.Keys.Count == 0).ToString());
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			base.loadState = UIDataSource.LoadState.Failed;
		};
		bwapirequestBase.SendOwnerCoroutine(this.dataManager);
	}

	// Token: 0x04002540 RID: 9536
	private string userIDStr;

	// Token: 0x04002541 RID: 9537
	private bool forCurrentUser;
}
