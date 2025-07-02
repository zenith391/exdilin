using System;
using System.Collections.Generic;
using SimpleJSON;

public class BWSocialUser
{
	public int userID { get; private set; }

	public string username { get; private set; }

	public int userStatus { get; private set; }

	public DateTime startedFollowingAt { get; private set; }

	public int relationship { get; private set; }

	public string profileImageUrl { get; private set; }

	public BWSocialUser(JObject json)
	{
		UpdateFromJson(json);
	}

	public void UpdateFromJson(JObject json)
	{
		userID = BWJsonHelpers.PropertyIfExists(userID, "user_id", json);
		username = BWJsonHelpers.PropertyIfExists(username, "username", json);
		userStatus = BWJsonHelpers.PropertyIfExists(userStatus, "user_status", json);
		startedFollowingAt = BWJsonHelpers.PropertyIfExists(startedFollowingAt, "started_following_at", json);
		relationship = BWJsonHelpers.PropertyIfExists(relationship, "relationship", json);
		profileImageUrl = BWJsonHelpers.PropertyIfExists(profileImageUrl, "profile_image_url", json);
	}

	public Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["id"] = userID.ToString();
		dictionary["username"] = ((!string.IsNullOrEmpty(username)) ? username : "Unnamed Blockster");
		if (string.IsNullOrEmpty(profileImageUrl))
		{
			dictionary["profile_image"] = $"{BWEnvConfig.AWS_S3_BASE_URL}/profiles/{userID.ToString()}/approved.png";
		}
		else
		{
			dictionary["profile_image"] = profileImageUrl;
		}
		dictionary["is_blocksworld_premium"] = Util.IsPremiumUserStatus(userStatus).ToString();
		string empty = string.Empty;
		DateTime utcNow = DateTime.UtcNow;
		empty = ((utcNow.Date == startedFollowingAt.Date) ? "Since today!" : ((!(utcNow.AddDays(-1.0).Date == startedFollowingAt.Date)) ? ("Since " + startedFollowingAt.ToShortDateString()) : "Since yesterday"));
		dictionary["following_since"] = empty;
		return dictionary;
	}
}
