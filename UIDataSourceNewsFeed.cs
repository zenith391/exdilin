using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003EC RID: 1004
public class UIDataSourceNewsFeed : UIDataSource
{
	// Token: 0x06002C43 RID: 11331 RVA: 0x0013DF66 File Offset: 0x0013C366
	public UIDataSourceNewsFeed(UIDataManager manager) : base(manager)
	{
	}

	// Token: 0x06002C44 RID: 11332 RVA: 0x0013DF70 File Offset: 0x0013C370
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		base.SectionStartIndicies = new List<int>();
		base.loadState = UIDataSource.LoadState.Loading;
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/news_feed");
		bwapirequestBase.onSuccess = delegate(JObject respJson)
		{
			base.loadState = UIDataSource.LoadState.Loaded;
			BWNewsFeed bwnewsFeed = new BWNewsFeed(respJson);
			int num = 0;
			for (int i = 0; i < bwnewsFeed.activityList.Count; i++)
			{
				BWUserActivity bwuserActivity = bwnewsFeed.activityList[i];
				if (num < 5 && i < bwnewsFeed.activitySectionIndexList.Count && i == bwnewsFeed.activitySectionIndexList[num])
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
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			base.loadState = UIDataSource.LoadState.Failed;
		};
		bwapirequestBase.SendOwnerCoroutine(this.dataManager);
	}
}
