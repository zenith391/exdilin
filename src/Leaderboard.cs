using System;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;

// Token: 0x020001AD RID: 429
public class Leaderboard
{
	// Token: 0x0600179C RID: 6044 RVA: 0x000A6693 File Offset: 0x000A4A93
	public static int TimeToMs(float time)
	{
		return (int)Math.Floor((double)(time * 1000f));
	}

	// Token: 0x0600179D RID: 6045 RVA: 0x000A66A4 File Offset: 0x000A4AA4
	public static string EncryptDigest(string worldId, int newTimeMs)
	{
		string text = worldId + "--" + newTimeMs;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			stringBuilder.Append(text[i] ^ "H7FN3epG7Q0rHzQ304h0AuMpjxjTX3RMy8FssU1P9VWsGOjLQw4Bb0lVbT9Iy3m7"[i % "H7FN3epG7Q0rHzQ304h0AuMpjxjTX3RMy8FssU1P9VWsGOjLQw4Bb0lVbT9Iy3m7".Length]);
		}
		return stringBuilder.ToString();
	}

	// Token: 0x0600179E RID: 6046 RVA: 0x000A670C File Offset: 0x000A4B0C
	public static Leaderboard BuildWithJsonStr(string leaderboardJsonStr)
	{
		JObject json = JSONDecoder.Decode(leaderboardJsonStr);
		return Leaderboard.BuildWithJsonObj(json);
	}

	// Token: 0x0600179F RID: 6047 RVA: 0x000A6728 File Offset: 0x000A4B28
	public static Leaderboard BuildWithJsonObj(JObject json)
	{
		Leaderboard leaderboard = new Leaderboard();
		leaderboard.leaderboardType = (LeaderboardType)json["leaderboard_type"].IntValue;
		leaderboard.authorId = json["author_id"].IntValue;
		leaderboard.authorUsername = Util.FixNonAscii(json["author_username"].StringValue);
		leaderboard.authorStatus = json["author_status"].IntValue;
		leaderboard.authorPlayCount = json["author_play_count"].IntValue;
		leaderboard.authorTime = json["author_best_time_ms"].FloatValue / 1000f;
		leaderboard.authorUsername = Util.FixNonAscii(leaderboard.authorUsername);
		List<JObject> arrayValue = json["records"].ArrayValue;
		leaderboard.records = new LeaderboardRecord[arrayValue.Count];
		for (int i = 0; i < arrayValue.Count; i++)
		{
			JObject jobject = arrayValue[i];
			leaderboard.records[i].userId = jobject["user_id"].IntValue;
			leaderboard.records[i].username = Util.FixNonAscii(jobject["user_username"].StringValue);
			leaderboard.records[i].userProfileImageUrl = jobject["user_profile_image_url"].StringValue;
			leaderboard.records[i].userStatus = jobject["user_status"].IntValue;
			leaderboard.records[i].rank = jobject["rank"].IntValue;
			leaderboard.records[i].time = jobject["best_time_ms"].FloatValue / 1000f;
			leaderboard.records[i].playCount = jobject["play_count"].IntValue;
		}
		List<JObject> arrayValue2 = json["periodic_records"].ArrayValue;
		leaderboard.periodicRecords = new LeaderboardRecord[arrayValue2.Count];
		for (int j = 0; j < arrayValue2.Count; j++)
		{
			JObject jobject2 = arrayValue2[j];
			leaderboard.periodicRecords[j].userId = jobject2["user_id"].IntValue;
			leaderboard.periodicRecords[j].username = Util.FixNonAscii(jobject2["user_username"].StringValue);
			leaderboard.periodicRecords[j].userProfileImageUrl = jobject2["user_profile_image_url"].StringValue;
			leaderboard.periodicRecords[j].userStatus = jobject2["user_status"].IntValue;
			leaderboard.periodicRecords[j].rank = jobject2["rank"].IntValue;
			leaderboard.periodicRecords[j].time = jobject2["best_time_ms"].FloatValue / 1000f;
			leaderboard.periodicRecords[j].playCount = 0;
		}
		return leaderboard;
	}

	// Token: 0x0400128D RID: 4749
	public LeaderboardType leaderboardType;

	// Token: 0x0400128E RID: 4750
	public int authorId;

	// Token: 0x0400128F RID: 4751
	public string authorUsername;

	// Token: 0x04001290 RID: 4752
	public int authorStatus;

	// Token: 0x04001291 RID: 4753
	public int authorPlayCount;

	// Token: 0x04001292 RID: 4754
	public float authorTime;

	// Token: 0x04001293 RID: 4755
	public LeaderboardRecord[] records;

	// Token: 0x04001294 RID: 4756
	public LeaderboardRecord[] periodicRecords;

	// Token: 0x020001AE RID: 430
	// (Invoke) Token: 0x060017A1 RID: 6049
	public delegate void LoadFromRemoteSuccessHandler(Leaderboard leaderboard);

	// Token: 0x020001AF RID: 431
	// (Invoke) Token: 0x060017A5 RID: 6053
	public delegate void ReportNewTimeRemoteSuccessHandler(Leaderboard leaderboard, bool timeImproved);

	// Token: 0x020001B0 RID: 432
	// (Invoke) Token: 0x060017A9 RID: 6057
	public delegate void LoadFromRemoteFailureHandler(string errorMessage);
}
