using System;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;

public class Leaderboard
{
	public delegate void LoadFromRemoteSuccessHandler(Leaderboard leaderboard);

	public delegate void ReportNewTimeRemoteSuccessHandler(Leaderboard leaderboard, bool timeImproved);

	public delegate void LoadFromRemoteFailureHandler(string errorMessage);

	public LeaderboardType leaderboardType;

	public int authorId;

	public string authorUsername;

	public int authorStatus;

	public int authorPlayCount;

	public float authorTime;

	public LeaderboardRecord[] records;

	public LeaderboardRecord[] periodicRecords;

	public static int TimeToMs(float time)
	{
		return (int)Math.Floor(time * 1000f);
	}

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

	public static Leaderboard BuildWithJsonStr(string leaderboardJsonStr)
	{
		JObject json = JSONDecoder.Decode(leaderboardJsonStr);
		return BuildWithJsonObj(json);
	}

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
			JObject jObject = arrayValue[i];
			leaderboard.records[i].userId = jObject["user_id"].IntValue;
			leaderboard.records[i].username = Util.FixNonAscii(jObject["user_username"].StringValue);
			leaderboard.records[i].userProfileImageUrl = jObject["user_profile_image_url"].StringValue;
			leaderboard.records[i].userStatus = jObject["user_status"].IntValue;
			leaderboard.records[i].rank = jObject["rank"].IntValue;
			leaderboard.records[i].time = jObject["best_time_ms"].FloatValue / 1000f;
			leaderboard.records[i].playCount = jObject["play_count"].IntValue;
		}
		List<JObject> arrayValue2 = json["periodic_records"].ArrayValue;
		leaderboard.periodicRecords = new LeaderboardRecord[arrayValue2.Count];
		for (int j = 0; j < arrayValue2.Count; j++)
		{
			JObject jObject2 = arrayValue2[j];
			leaderboard.periodicRecords[j].userId = jObject2["user_id"].IntValue;
			leaderboard.periodicRecords[j].username = Util.FixNonAscii(jObject2["user_username"].StringValue);
			leaderboard.periodicRecords[j].userProfileImageUrl = jObject2["user_profile_image_url"].StringValue;
			leaderboard.periodicRecords[j].userStatus = jObject2["user_status"].IntValue;
			leaderboard.periodicRecords[j].rank = jObject2["rank"].IntValue;
			leaderboard.periodicRecords[j].time = jObject2["best_time_ms"].FloatValue / 1000f;
			leaderboard.periodicRecords[j].playCount = 0;
		}
		return leaderboard;
	}
}
