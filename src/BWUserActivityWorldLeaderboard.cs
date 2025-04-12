using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C9 RID: 969
public class BWUserActivityWorldLeaderboard : BWUserActivityWorld
{
	// Token: 0x06002A63 RID: 10851 RVA: 0x00134EBC File Offset: 0x001332BC
	public BWUserActivityWorldLeaderboard(JObject json) : base(json)
	{
		this.worldLeaderboardRank = BWJsonHelpers.PropertyIfExists(this.worldLeaderboardRank, "world_leaderboard_rank", json);
		bool flag = this.IsLeaderboardWorldAuthor();
		bool flag2 = this.IsImprovedLeaderboardResult();
		if (flag)
		{
			if (flag2)
			{
				this.description = "New best time!";
			}
			else
			{
				this.description = "Posted a time!";
			}
		}
		else
		{
			int num = this.worldLeaderboardRank % 10;
			bool flag3 = this.worldLeaderboardRank / 10 == 1;
			string arg = (!flag3) ? ((num != 1) ? ((num != 2) ? ((num != 3) ? "th" : "rd") : "nd") : "st") : "th";
			if (flag2)
			{
				this.description = string.Format((this.worldLeaderboardRank > 10) ? "New best! Now {0}{1}" : "New best! Now {0}{1}!", this.worldLeaderboardRank, arg);
			}
			else
			{
				this.description = string.Format((this.worldLeaderboardRank > 10) ? "Placed {0}{1}" : "Got {0}{1} place!", this.worldLeaderboardRank, arg);
			}
		}
	}

	// Token: 0x06002A64 RID: 10852 RVA: 0x00134FF8 File Offset: 0x001333F8
	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		dictionary["description"] = this.description;
		return dictionary;
	}

	// Token: 0x06002A65 RID: 10853 RVA: 0x0013501E File Offset: 0x0013341E
	public bool IsImprovedLeaderboardResult()
	{
		return this.type == 252 || this.type == 254;
	}

	// Token: 0x06002A66 RID: 10854 RVA: 0x00135040 File Offset: 0x00133440
	public bool IsLeaderboardWorldAuthor()
	{
		return this.type == 251 || this.type == 252;
	}

	// Token: 0x0400246E RID: 9326
	private int worldLeaderboardRank;

	// Token: 0x0400246F RID: 9327
	private string description;
}
