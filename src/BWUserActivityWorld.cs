using System.Collections.Generic;
using SimpleJSON;

public class BWUserActivityWorld : BWUserActivity
{
	public string worldAchievementTitle;

	public string worldID;

	public string imageUrl;

	public string worldTitle;

	public BWUserActivityWorld(JObject json)
		: base(json)
	{
		worldAchievementTitle = BWJsonHelpers.PropertyIfExists(worldAchievementTitle, "world_achievement_title", json);
		worldID = BWJsonHelpers.IDPropertyAsStringIfExists(worldID, "world_id", json);
		imageUrl = BWJsonHelpers.PropertyIfExists(imageUrl, "world_image_urls_for_sizes", "220x220", json);
		worldTitle = BWJsonHelpers.PropertyIfExists(worldTitle, "world_title", json);
	}

	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		if (!string.IsNullOrEmpty(worldAchievementTitle))
		{
			dictionary["description"] = worldAchievementTitle;
		}
		else if (!string.IsNullOrEmpty(worldTitle))
		{
			dictionary["description"] = worldTitle;
		}
		dictionary["world_id"] = worldID;
		dictionary["title"] = worldTitle;
		dictionary["image_url"] = imageUrl;
		dictionary["world_achievement_title"] = worldAchievementTitle;
		dictionary["button_message"] = "ShowWorldDetail";
		dictionary["message_id"] = worldID;
		return dictionary;
	}
}
