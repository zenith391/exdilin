using System;
using SimpleJSON;

// Token: 0x020003C5 RID: 965
public class BWUserActivityAchievement : BWUserActivity
{
	// Token: 0x06002A5C RID: 10844 RVA: 0x00134AF4 File Offset: 0x00132EF4
	public BWUserActivityAchievement(JObject json) : base(json)
	{
		this.achievementID = BWJsonHelpers.PropertyIfExists(this.achievementID, "achievement_id", json);
		this.achievementTitle = BWJsonHelpers.PropertyIfExists(this.achievementTitle, "achievement_title", json);
	}

	// Token: 0x04002460 RID: 9312
	public int achievementID;

	// Token: 0x04002461 RID: 9313
	public string achievementTitle;
}
