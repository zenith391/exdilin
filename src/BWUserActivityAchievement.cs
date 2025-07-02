using SimpleJSON;

public class BWUserActivityAchievement : BWUserActivity
{
	public int achievementID;

	public string achievementTitle;

	public BWUserActivityAchievement(JObject json)
		: base(json)
	{
		achievementID = BWJsonHelpers.PropertyIfExists(achievementID, "achievement_id", json);
		achievementTitle = BWJsonHelpers.PropertyIfExists(achievementTitle, "achievement_title", json);
	}
}
