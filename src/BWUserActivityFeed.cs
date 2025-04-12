using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003CB RID: 971
public class BWUserActivityFeed
{
	// Token: 0x06002A69 RID: 10857 RVA: 0x001351E8 File Offset: 0x001335E8
	public BWUserActivityFeed(JObject json, bool forCurrentUser)
	{
		this.forCurrentUser = forCurrentUser;
		this.UpdateFromJson(json);
	}

	// Token: 0x170001F4 RID: 500
	// (get) Token: 0x06002A6A RID: 10858 RVA: 0x001351FE File Offset: 0x001335FE
	// (set) Token: 0x06002A6B RID: 10859 RVA: 0x00135206 File Offset: 0x00133606
	public int followersCount { get; private set; }

	// Token: 0x170001F5 RID: 501
	// (get) Token: 0x06002A6C RID: 10860 RVA: 0x0013520F File Offset: 0x0013360F
	// (set) Token: 0x06002A6D RID: 10861 RVA: 0x00135217 File Offset: 0x00133617
	public DateTime dateLastActive { get; private set; }

	// Token: 0x170001F6 RID: 502
	// (get) Token: 0x06002A6E RID: 10862 RVA: 0x00135220 File Offset: 0x00133620
	// (set) Token: 0x06002A6F RID: 10863 RVA: 0x00135228 File Offset: 0x00133628
	public int subscriptionTier { get; private set; }

	// Token: 0x170001F7 RID: 503
	// (get) Token: 0x06002A70 RID: 10864 RVA: 0x00135231 File Offset: 0x00133631
	// (set) Token: 0x06002A71 RID: 10865 RVA: 0x00135239 File Offset: 0x00133639
	public List<BWUserActivity> activityList { get; private set; }

	// Token: 0x170001F8 RID: 504
	// (get) Token: 0x06002A72 RID: 10866 RVA: 0x00135242 File Offset: 0x00133642
	// (set) Token: 0x06002A73 RID: 10867 RVA: 0x0013524A File Offset: 0x0013364A
	public List<int> activitySectionIndexList { get; private set; }

	// Token: 0x06002A74 RID: 10868 RVA: 0x00135254 File Offset: 0x00133654
	public void UpdateFromJson(JObject json)
	{
		this.followersCount = BWJsonHelpers.PropertyIfExists(this.followersCount, "followers_count", json);
		this.dateLastActive = BWJsonHelpers.PropertyIfExists(this.dateLastActive, "last_transaction_at", json);
		this.subscriptionTier = BWJsonHelpers.PropertyIfExists(this.subscriptionTier, "subscription_tier", json);
		this.activityList = new List<BWUserActivity>();
		if (json.ContainsKey("user_activity"))
		{
			foreach (JObject jobject in json["user_activity"].ArrayValue)
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

	// Token: 0x06002A75 RID: 10869 RVA: 0x00135434 File Offset: 0x00133834
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

	// Token: 0x0400247E RID: 9342
	private bool forCurrentUser;
}
