using System;
using System.Collections.Generic;
using SimpleJSON;

public class BWUserActivity
{
	public const int ACCOUNT_CREATED = 101;

	public const int ACCOUNT_BANNED = 102;

	public const int ACCOUNT_UNBANNED = 103;

	public const int ACCOUNT_UPDATED = 104;

	public const int ACCOUNT_ACHIEVEMENT_ACKNOWLEDGED = 105;

	public const int ACCOUNT_LUCKY_PRIZE_WHEEL_WIN = 106;

	public const int ACCOUNT_BUILDING_SET_PURCHASED = 107;

	public const int ACCOUNT_LINKED = 108;

	public const int WORLD_PUBLISHED = 201;

	public const int WORLD_UPDATED = 202;

	public const int WORLD_GOT_ELITE = 203;

	public const int WORLD_GOT_FEATURED = 204;

	public const int WORLD_LIKE = 205;

	public const int WORLD_BOOKMARK = 206;

	public const int WORLD_FIVE_STARRED = 207;

	public const int WORLD_LEADERBOARD_AUTHOR_FIRST_TIME = 251;

	public const int WORLD_LEADERBOARD_AUTHOR_NEW_BEST = 252;

	public const int WORLD_LEADERBOARD_PLAYER_PLACED = 253;

	public const int WORLD_LEADERBOARD_PLAYER_NEW_BEST = 254;

	public const int MODEL_PUBLISHED = 301;

	public const int MODEL_UPDATED = 302;

	public const int MODEL_GOT_ACHIEVEMENT = 303;

	public const int MODEL_PURCHASED = 304;

	public const int MODEL_WISHLIST = 305;

	public const int MODEL_BOOKMARK = 306;

	public const int MODEL_FIVE_STARRED = 307;

	public int type;

	public DateTime timestamp;

	public int followedUserId;

	public string followedUsername;

	public int followedUserStatus;

	public string followedProfileImageUrl;

	public BWUserActivity(JObject json)
	{
		type = BWJsonHelpers.PropertyIfExists(type, "type", json);
		timestamp = BWJsonHelpers.PropertyIfExists(timestamp, "timestamp", json);
		followedUserId = BWJsonHelpers.PropertyIfExists(followedUserId, "follow_target_id", json);
		followedUsername = BWJsonHelpers.PropertyIfExists(followedUsername, "follow_target_username", json);
		followedUserStatus = BWJsonHelpers.PropertyIfExists(followedUserStatus, "follow_target_user_status", json);
		followedProfileImageUrl = BWJsonHelpers.PropertyIfExists(followedProfileImageUrl, "follow_target_profile_image_url", json);
	}

	public static BWUserActivity CreateFromJson(JObject json)
	{
		int property = 0;
		property = BWJsonHelpers.PropertyIfExists(property, "type", json);
		if (property == 104 || property == 108)
		{
			return new BWUserActivityProfile(json);
		}
		if (IsModelActivityType(property))
		{
			return new BWUserActivityModel(json);
		}
		if (IsWorldActivityType(property))
		{
			return new BWUserActivityWorld(json);
		}
		if (IsWorldLeaderboardActivityType(property))
		{
			return new BWUserActivityWorldLeaderboard(json);
		}
		return new BWUserActivity(json);
	}

	public virtual Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		BWUserActivityDefinition bWUserActivityDefinition = BWUserActivityDefinition.userActivitiesByID[type];
		if (bWUserActivityDefinition != null)
		{
			dictionary["description"] = bWUserActivityDefinition.textDescription;
			dictionary["activity_image_url"] = bWUserActivityDefinition.iconUrl;
		}
		if (followedUserId != 0)
		{
			dictionary["followed_user_id"] = followedUserId.ToString();
			dictionary["followed_username"] = ((!string.IsNullOrEmpty(followedUsername)) ? followedUsername : "Unnamed Blockster");
			if (string.IsNullOrEmpty(followedProfileImageUrl))
			{
				dictionary["followed_user_image_url"] = $"{BWEnvConfig.AWS_S3_BASE_URL}/profiles/{followedUserId}/approved.png";
			}
			else
			{
				dictionary["followed_user_image_url"] = followedProfileImageUrl;
			}
		}
		return dictionary;
	}

	private static bool IsModelActivityType(int activityType)
	{
		if (activityType > 300)
		{
			return activityType < 400;
		}
		return false;
	}

	private static bool IsWorldActivityType(int activityType)
	{
		if (activityType > 200)
		{
			return activityType < 250;
		}
		return false;
	}

	private static bool IsWorldLeaderboardActivityType(int activityType)
	{
		if (activityType > 250)
		{
			return activityType < 300;
		}
		return false;
	}
}
