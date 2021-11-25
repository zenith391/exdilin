using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003A9 RID: 937
public class BWNewsFeed
{
	// Token: 0x060028CA RID: 10442 RVA: 0x0012C530 File Offset: 0x0012A930
	public BWNewsFeed(JObject json)
	{
		this.UpdateFromJson(json);
	}

	// Token: 0x170001B2 RID: 434
	// (get) Token: 0x060028CB RID: 10443 RVA: 0x0012C53F File Offset: 0x0012A93F
	// (set) Token: 0x060028CC RID: 10444 RVA: 0x0012C547 File Offset: 0x0012A947
	public int followersCount { get; private set; }

	// Token: 0x170001B3 RID: 435
	// (get) Token: 0x060028CD RID: 10445 RVA: 0x0012C550 File Offset: 0x0012A950
	// (set) Token: 0x060028CE RID: 10446 RVA: 0x0012C558 File Offset: 0x0012A958
	public DateTime dateLastActive { get; private set; }

	// Token: 0x170001B4 RID: 436
	// (get) Token: 0x060028CF RID: 10447 RVA: 0x0012C561 File Offset: 0x0012A961
	// (set) Token: 0x060028D0 RID: 10448 RVA: 0x0012C569 File Offset: 0x0012A969
	public int subscriptionTier { get; private set; }

	// Token: 0x170001B5 RID: 437
	// (get) Token: 0x060028D1 RID: 10449 RVA: 0x0012C572 File Offset: 0x0012A972
	// (set) Token: 0x060028D2 RID: 10450 RVA: 0x0012C57A File Offset: 0x0012A97A
	public List<BWUserActivity> activityList { get; private set; }

	// Token: 0x170001B6 RID: 438
	// (get) Token: 0x060028D3 RID: 10451 RVA: 0x0012C583 File Offset: 0x0012A983
	// (set) Token: 0x060028D4 RID: 10452 RVA: 0x0012C58B File Offset: 0x0012A98B
	public List<int> activitySectionIndexList { get; private set; }

	// Token: 0x060028D5 RID: 10453 RVA: 0x0012C594 File Offset: 0x0012A994
	public void UpdateFromJson(JObject json)
	{
		this.activityList = new List<BWUserActivity>();
		if (json.ContainsKey("news_feed"))
		{
			foreach (JObject jobject in json["news_feed"].ArrayValue)
			{
				JObject json2 = JSONDecoder.Decode(jobject.StringValue);
				this.activityList.Add(BWUserActivity.CreateFromJson(json2));
			}
		}
		for (int i = this.activityList.Count - 1; i >= 0; i--)
		{
			if (!this.IsUserActivityShown(this.activityList[i]))
			{
				this.activityList.RemoveAt(i);
			}
		}
		this.activitySectionIndexList = new List<int>();
		if (this.activityList.Count == 0)
		{
			return;
		}
		this.activitySectionIndexList.Add(0);
		DateTime timestamp = this.activityList[0].timestamp;
		for (int j = 0; j < this.activityList.Count; j++)
		{
			BWUserActivity bwuserActivity = this.activityList[j];
			if (timestamp.Date.CompareTo(bwuserActivity.timestamp.Date) != 0)
			{
				this.activitySectionIndexList.Add(j);
				timestamp = bwuserActivity.timestamp;
				if (this.activitySectionIndexList.Count > this.activityList.Count / 5)
				{
					break;
				}
			}
		}
	}

	// Token: 0x060028D6 RID: 10454 RVA: 0x0012C730 File Offset: 0x0012AB30
	private bool IsUserActivityShown(BWUserActivity userActivity)
	{
		if (userActivity == null)
		{
			return false;
		}
		BWUserActivityDefinition bwuserActivityDefinition = null;
		if (!BWUserActivityDefinition.userActivitiesByID.TryGetValue(userActivity.type, out bwuserActivityDefinition))
		{
			Debug.Log("unknown user activity type: " + userActivity.type);
			return false;
		}
		if (userActivity.type == 105 || userActivity.type == 107 || userActivity.type == 106)
		{
			Debug.Log("not showing ios activity: " + userActivity.type);
			return false;
		}
		return !this.forCurrentUser || true;
	}

	// Token: 0x0400238F RID: 9103
	private bool forCurrentUser;
}
