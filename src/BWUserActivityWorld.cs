using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003C8 RID: 968
public class BWUserActivityWorld : BWUserActivity
{
	// Token: 0x06002A61 RID: 10849 RVA: 0x00134D80 File Offset: 0x00133180
	public BWUserActivityWorld(JObject json) : base(json)
	{
		this.worldAchievementTitle = BWJsonHelpers.PropertyIfExists(this.worldAchievementTitle, "world_achievement_title", json);
		this.worldID = BWJsonHelpers.IDPropertyAsStringIfExists(this.worldID, "world_id", json);
		this.imageUrl = BWJsonHelpers.PropertyIfExists(this.imageUrl, "world_image_urls_for_sizes", "220x220", json);
		this.worldTitle = BWJsonHelpers.PropertyIfExists(this.worldTitle, "world_title", json);
	}

	// Token: 0x06002A62 RID: 10850 RVA: 0x00134DF8 File Offset: 0x001331F8
	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		if (!string.IsNullOrEmpty(this.worldAchievementTitle))
		{
			dictionary["description"] = this.worldAchievementTitle;
		}
		else if (!string.IsNullOrEmpty(this.worldTitle))
		{
			dictionary["description"] = this.worldTitle;
		}
		dictionary["world_id"] = this.worldID;
		dictionary["title"] = this.worldTitle;
		dictionary["image_url"] = this.imageUrl;
		dictionary["world_achievement_title"] = this.worldAchievementTitle;
		dictionary["button_message"] = "ShowWorldDetail";
		dictionary["message_id"] = this.worldID;
		return dictionary;
	}

	// Token: 0x0400246A RID: 9322
	public string worldAchievementTitle;

	// Token: 0x0400246B RID: 9323
	public string worldID;

	// Token: 0x0400246C RID: 9324
	public string imageUrl;

	// Token: 0x0400246D RID: 9325
	public string worldTitle;
}
