using System;
using System.Collections.Generic;
using SimpleJSON;

public class UIDataSourceUserActivity : UIDataSource
{
	private string userIDStr;

	private bool forCurrentUser;

	public UIDataSourceUserActivity(UIDataManager manager, string userID)
		: base(manager)
	{
		userIDStr = userID;
	}

	public static UIDataSourceUserActivity UserActivityForCurrentUser(UIDataManager manager)
	{
		return new UIDataSourceUserActivity(manager, BWUser.currentUser.userID.ToString())
		{
			forCurrentUser = true
		};
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		base.SectionStartIndicies = new List<int>();
		base.loadState = LoadState.Loading;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/user/{userIDStr}/recent_activity");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			base.loadState = LoadState.Loaded;
			BWUserActivityFeed bWUserActivityFeed = new BWUserActivityFeed(respJson, forCurrentUser);
			base.Info.Add("followers_count", bWUserActivityFeed.followersCount.ToString());
			int num = 0;
			for (int i = 0; i < bWUserActivityFeed.activityList.Count; i++)
			{
				BWUserActivity bWUserActivity = bWUserActivityFeed.activityList[i];
				if (num < 5 && i < bWUserActivityFeed.activitySectionIndexList.Count && i == bWUserActivityFeed.activitySectionIndexList[num])
				{
					base.SectionStartIndicies.Add(num + i);
					num++;
					string empty = string.Empty;
					DateTime utcNow = DateTime.UtcNow;
					empty = ((utcNow.Date == bWUserActivity.timestamp.Date) ? "Today" : ((utcNow.Date.AddDays(-1.0) == bWUserActivity.timestamp.Date) ? "Yesterday" : ((num != 5) ? bWUserActivity.timestamp.ToLongDateString() : (bWUserActivity.timestamp.ToLongDateString() + " and earlier"))));
					Dictionary<string, string> value = new Dictionary<string, string> { { "title", empty } };
					base.Keys.Add(empty);
					base.Data.Add(empty, value);
					base.PanelOverrides[empty] = 0;
				}
				string text = i.ToString();
				base.Keys.Add(text);
				Dictionary<string, string> value2 = bWUserActivity.AttributesForMenuUI();
				base.Data.Add(text, value2);
			}
			base.Info.Add("empty", (base.Keys.Count == 0).ToString());
		};
		bWAPIRequestBase.onFailure = delegate
		{
			base.loadState = LoadState.Failed;
		};
		bWAPIRequestBase.SendOwnerCoroutine(dataManager);
	}
}
