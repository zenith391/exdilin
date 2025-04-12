using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003CA RID: 970
public class BWUserActivityDefinition
{
	// Token: 0x06002A67 RID: 10855 RVA: 0x00135064 File Offset: 0x00133464
	public BWUserActivityDefinition(JObject json)
	{
		this.internalIdentifier = BWJsonHelpers.PropertyIfExists(this.internalIdentifier, "internal_identifier", json);
		this.userActivityId = BWJsonHelpers.PropertyIfExists(this.userActivityId, "id", json);
		this.settingsMask = BWJsonHelpers.PropertyIfExists(this.settingsMask, "settings_mask", json);
		this.textCategory = BWJsonHelpers.PropertyIfExists(this.textCategory, "category", json);
		this.textDescription = BWJsonHelpers.PropertyIfExists(this.textDescription, "description", json);
		this.iconUrl = BWJsonHelpers.PropertyIfExists(this.iconUrl, "icon_urls_for_sizes", "190x190", json);
		this.iconUrl = string.Format("{0}/{1}.png", BWEnvConfig.AWS_S3_BASE_URL, this.iconUrl);
	}

	// Token: 0x06002A68 RID: 10856 RVA: 0x00135124 File Offset: 0x00133524
	public static void LoadUserActivityDefinitions()
	{
		string text = Resources.Load<TextAsset>("user_activities").text;
		JObject jobject = JSONDecoder.Decode(text);
		BWUserActivityDefinition.userActivities = new List<BWUserActivityDefinition>();
		BWUserActivityDefinition.userActivitiesByID = new Dictionary<int, BWUserActivityDefinition>();
		BWUserActivityDefinition.userActivitiesByInternalIdentifier = new Dictionary<string, BWUserActivityDefinition>();
		foreach (JObject json in jobject.ArrayValue)
		{
			BWUserActivityDefinition bwuserActivityDefinition = new BWUserActivityDefinition(json);
			BWUserActivityDefinition.userActivities.Add(bwuserActivityDefinition);
			BWUserActivityDefinition.userActivitiesByID[bwuserActivityDefinition.userActivityId] = bwuserActivityDefinition;
			BWUserActivityDefinition.userActivitiesByInternalIdentifier[bwuserActivityDefinition.internalIdentifier] = bwuserActivityDefinition;
		}
	}

	// Token: 0x04002470 RID: 9328
	public static List<BWUserActivityDefinition> userActivities;

	// Token: 0x04002471 RID: 9329
	public static Dictionary<int, BWUserActivityDefinition> userActivitiesByID;

	// Token: 0x04002472 RID: 9330
	public static Dictionary<string, BWUserActivityDefinition> userActivitiesByInternalIdentifier;

	// Token: 0x04002473 RID: 9331
	public string internalIdentifier;

	// Token: 0x04002474 RID: 9332
	public int settingsMask;

	// Token: 0x04002475 RID: 9333
	public string textCategory;

	// Token: 0x04002476 RID: 9334
	public string textDescription;

	// Token: 0x04002477 RID: 9335
	public int userActivityId;

	// Token: 0x04002478 RID: 9336
	public string iconUrl;
}
