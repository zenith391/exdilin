using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWUserActivityFeed
{
	private bool forCurrentUser;

	public int followersCount { get; private set; }

	public DateTime dateLastActive { get; private set; }

	public int subscriptionTier { get; private set; }

	public List<BWUserActivity> activityList { get; private set; }

	public List<int> activitySectionIndexList { get; private set; }

	public BWUserActivityFeed(JObject json, bool forCurrentUser)
	{
		this.forCurrentUser = forCurrentUser;
		UpdateFromJson(json);
	}

	public void UpdateFromJson(JObject json)
	{
		followersCount = BWJsonHelpers.PropertyIfExists(followersCount, "followers_count", json);
		dateLastActive = BWJsonHelpers.PropertyIfExists(dateLastActive, "last_transaction_at", json);
		subscriptionTier = BWJsonHelpers.PropertyIfExists(subscriptionTier, "subscription_tier", json);
		activityList = new List<BWUserActivity>();
		if (json.ContainsKey("user_activity"))
		{
			foreach (JObject item in json["user_activity"].ArrayValue)
			{
				JObject json2 = JSONDecoder.Decode(item.StringValue);
				activityList.Add(BWUserActivity.CreateFromJson(json2));
			}
		}
		for (int num = activityList.Count - 1; num >= 0; num--)
		{
			if (!IsUserActivityShown(activityList[num]))
			{
				activityList.RemoveAt(num);
			}
		}
		activitySectionIndexList = new List<int>();
		if (activityList.Count == 0)
		{
			return;
		}
		activitySectionIndexList.Add(0);
		DateTime timestamp = activityList[0].timestamp;
		for (int i = 0; i < activityList.Count; i++)
		{
			BWUserActivity bWUserActivity = activityList[i];
			if (timestamp.Date.CompareTo(bWUserActivity.timestamp.Date) != 0)
			{
				activitySectionIndexList.Add(i);
				timestamp = bWUserActivity.timestamp;
				if (activitySectionIndexList.Count > activityList.Count / 5)
				{
					break;
				}
			}
		}
	}

	private bool IsUserActivityShown(BWUserActivity userActivity)
	{
		if (userActivity == null)
		{
			return false;
		}
		BWUserActivityDefinition value = null;
		if (!BWUserActivityDefinition.userActivitiesByID.TryGetValue(userActivity.type, out value))
		{
			Debug.Log("unknown user activity type: " + userActivity.type);
			return false;
		}
		if (userActivity.type == 105 || userActivity.type == 107 || userActivity.type == 106)
		{
			Debug.Log("not showing ios activity: " + userActivity.type);
			return false;
		}
		if (forCurrentUser)
		{
			return true;
		}
		return true;
	}
}
