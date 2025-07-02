using System;
using System.Collections.Generic;
using SimpleJSON;

public class UIDataSourceNewsFeed : UIDataSource
{
	public UIDataSourceNewsFeed(UIDataManager manager)
		: base(manager)
	{
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		base.SectionStartIndicies = new List<int>();
		base.loadState = LoadState.Loading;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/news_feed");
		bWAPIRequestBase.onSuccess = delegate(JObject respJson)
		{
			base.loadState = LoadState.Loaded;
			BWNewsFeed bWNewsFeed = new BWNewsFeed(respJson);
			int num = 0;
			for (int i = 0; i < bWNewsFeed.activityList.Count; i++)
			{
				BWUserActivity bWUserActivity = bWNewsFeed.activityList[i];
				if (num < 5 && i < bWNewsFeed.activitySectionIndexList.Count && i == bWNewsFeed.activitySectionIndexList[num])
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
		};
		bWAPIRequestBase.onFailure = delegate
		{
			base.loadState = LoadState.Failed;
		};
		bWAPIRequestBase.SendOwnerCoroutine(dataManager);
	}
}
