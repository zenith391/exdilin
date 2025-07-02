using System.Collections.Generic;
using SimpleJSON;

public class BWUserActivityWorldLeaderboard : BWUserActivityWorld
{
	private int worldLeaderboardRank;

	private string description;

	public BWUserActivityWorldLeaderboard(JObject json)
		: base(json)
	{
		worldLeaderboardRank = BWJsonHelpers.PropertyIfExists(worldLeaderboardRank, "world_leaderboard_rank", json);
		bool flag = IsLeaderboardWorldAuthor();
		bool flag2 = IsImprovedLeaderboardResult();
		if (flag)
		{
			if (flag2)
			{
				description = "New best time!";
			}
			else
			{
				description = "Posted a time!";
			}
			return;
		}
		int num = worldLeaderboardRank % 10;
		string arg = ((worldLeaderboardRank / 10 == 1) ? "th" : (num switch
		{
			1 => "st", 
			2 => "nd", 
			3 => "rd", 
			_ => "th", 
		}));
		if (flag2)
		{
			description = string.Format((worldLeaderboardRank > 10) ? "New best! Now {0}{1}" : "New best! Now {0}{1}!", worldLeaderboardRank, arg);
		}
		else
		{
			description = string.Format((worldLeaderboardRank > 10) ? "Placed {0}{1}" : "Got {0}{1} place!", worldLeaderboardRank, arg);
		}
	}

	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		dictionary["description"] = description;
		return dictionary;
	}

	public bool IsImprovedLeaderboardResult()
	{
		if (type != 252)
		{
			return type == 254;
		}
		return true;
	}

	public bool IsLeaderboardWorldAuthor()
	{
		if (type != 251)
		{
			return type == 252;
		}
		return true;
	}
}
