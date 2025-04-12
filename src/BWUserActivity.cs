using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C4 RID: 964
public class BWUserActivity
{
	// Token: 0x06002A56 RID: 10838 RVA: 0x001348D8 File Offset: 0x00132CD8
	public BWUserActivity(JObject json)
	{
		this.type = BWJsonHelpers.PropertyIfExists(this.type, "type", json);
		this.timestamp = BWJsonHelpers.PropertyIfExists(this.timestamp, "timestamp", json);
		this.followedUserId = BWJsonHelpers.PropertyIfExists(this.followedUserId, "follow_target_id", json);
		this.followedUsername = BWJsonHelpers.PropertyIfExists(this.followedUsername, "follow_target_username", json);
		this.followedUserStatus = BWJsonHelpers.PropertyIfExists(this.followedUserStatus, "follow_target_user_status", json);
        // added by Exdilin
        this.followedProfileImageUrl = BWJsonHelpers.PropertyIfExists(this.followedProfileImageUrl, "follow_target_profile_image_url", json);
	}

	// Token: 0x06002A57 RID: 10839 RVA: 0x00134960 File Offset: 0x00132D60
	public static BWUserActivity CreateFromJson(JObject json)
	{
		int num = 0;
		num = BWJsonHelpers.PropertyIfExists(num, "type", json);
		BWUserActivity result;
		if (num == 104 || num == 108)
		{
			result = new BWUserActivityProfile(json);
		}
		else if (BWUserActivity.IsModelActivityType(num))
		{
			result = new BWUserActivityModel(json);
		}
		else if (BWUserActivity.IsWorldActivityType(num))
		{
			result = new BWUserActivityWorld(json);
		}
		else if (BWUserActivity.IsWorldLeaderboardActivityType(num))
		{
			result = new BWUserActivityWorldLeaderboard(json);
		}
		else
		{
			result = new BWUserActivity(json);
		}
		return result;
	}

	// Token: 0x06002A58 RID: 10840 RVA: 0x001349E8 File Offset: 0x00132DE8
	public virtual Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		BWUserActivityDefinition bwuserActivityDefinition = BWUserActivityDefinition.userActivitiesByID[this.type];
		if (bwuserActivityDefinition != null)
		{
			dictionary["description"] = bwuserActivityDefinition.textDescription;
			dictionary["activity_image_url"] = bwuserActivityDefinition.iconUrl;
		}
		if (this.followedUserId != 0)
		{
			dictionary["followed_user_id"] = this.followedUserId.ToString();
			dictionary["followed_username"] = ((!string.IsNullOrEmpty(this.followedUsername)) ? this.followedUsername : "Unnamed Blockster");
            //
            if (string.IsNullOrEmpty(this.followedProfileImageUrl))
            {
                dictionary["followed_user_image_url"] = string.Format("{0}/profiles/{1}/approved.png", BWEnvConfig.AWS_S3_BASE_URL, this.followedUserId);
            }
            else
            {
                dictionary["followed_user_image_url"] = this.followedProfileImageUrl;
            }
            //
        }
		return dictionary;
	}

	// Token: 0x06002A59 RID: 10841 RVA: 0x00134AAC File Offset: 0x00132EAC
	private static bool IsModelActivityType(int activityType)
	{
		return activityType > 300 && activityType < 400;
	}

	// Token: 0x06002A5A RID: 10842 RVA: 0x00134AC4 File Offset: 0x00132EC4
	private static bool IsWorldActivityType(int activityType)
	{
		return activityType > 200 && activityType < 250;
	}

	// Token: 0x06002A5B RID: 10843 RVA: 0x00134ADC File Offset: 0x00132EDC
	private static bool IsWorldLeaderboardActivityType(int activityType)
	{
		return activityType > 250 && activityType < 300;
	}

	// Token: 0x04002441 RID: 9281
	public const int ACCOUNT_CREATED = 101;

	// Token: 0x04002442 RID: 9282
	public const int ACCOUNT_BANNED = 102;

	// Token: 0x04002443 RID: 9283
	public const int ACCOUNT_UNBANNED = 103;

	// Token: 0x04002444 RID: 9284
	public const int ACCOUNT_UPDATED = 104;

	// Token: 0x04002445 RID: 9285
	public const int ACCOUNT_ACHIEVEMENT_ACKNOWLEDGED = 105;

	// Token: 0x04002446 RID: 9286
	public const int ACCOUNT_LUCKY_PRIZE_WHEEL_WIN = 106;

	// Token: 0x04002447 RID: 9287
	public const int ACCOUNT_BUILDING_SET_PURCHASED = 107;

	// Token: 0x04002448 RID: 9288
	public const int ACCOUNT_LINKED = 108;

	// Token: 0x04002449 RID: 9289
	public const int WORLD_PUBLISHED = 201;

	// Token: 0x0400244A RID: 9290
	public const int WORLD_UPDATED = 202;

	// Token: 0x0400244B RID: 9291
	public const int WORLD_GOT_ELITE = 203;

	// Token: 0x0400244C RID: 9292
	public const int WORLD_GOT_FEATURED = 204;

	// Token: 0x0400244D RID: 9293
	public const int WORLD_LIKE = 205;

	// Token: 0x0400244E RID: 9294
	public const int WORLD_BOOKMARK = 206;

	// Token: 0x0400244F RID: 9295
	public const int WORLD_FIVE_STARRED = 207;

	// Token: 0x04002450 RID: 9296
	public const int WORLD_LEADERBOARD_AUTHOR_FIRST_TIME = 251;

	// Token: 0x04002451 RID: 9297
	public const int WORLD_LEADERBOARD_AUTHOR_NEW_BEST = 252;

	// Token: 0x04002452 RID: 9298
	public const int WORLD_LEADERBOARD_PLAYER_PLACED = 253;

	// Token: 0x04002453 RID: 9299
	public const int WORLD_LEADERBOARD_PLAYER_NEW_BEST = 254;

	// Token: 0x04002454 RID: 9300
	public const int MODEL_PUBLISHED = 301;

	// Token: 0x04002455 RID: 9301
	public const int MODEL_UPDATED = 302;

	// Token: 0x04002456 RID: 9302
	public const int MODEL_GOT_ACHIEVEMENT = 303;

	// Token: 0x04002457 RID: 9303
	public const int MODEL_PURCHASED = 304;

	// Token: 0x04002458 RID: 9304
	public const int MODEL_WISHLIST = 305;

	// Token: 0x04002459 RID: 9305
	public const int MODEL_BOOKMARK = 306;

	// Token: 0x0400245A RID: 9306
	public const int MODEL_FIVE_STARRED = 307;

	// Token: 0x0400245B RID: 9307
	public int type;

	// Token: 0x0400245C RID: 9308
	public DateTime timestamp;

	// Token: 0x0400245D RID: 9309
	public int followedUserId;

	// Token: 0x0400245E RID: 9310
	public string followedUsername;

	// Token: 0x0400245F RID: 9311
	public int followedUserStatus;

    // added by Exdilin
    public string followedProfileImageUrl;
}
