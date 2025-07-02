using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWUserActivityDefinition
{
	public static List<BWUserActivityDefinition> userActivities;

	public static Dictionary<int, BWUserActivityDefinition> userActivitiesByID;

	public static Dictionary<string, BWUserActivityDefinition> userActivitiesByInternalIdentifier;

	public string internalIdentifier;

	public int settingsMask;

	public string textCategory;

	public string textDescription;

	public int userActivityId;

	public string iconUrl;

	public BWUserActivityDefinition(JObject json)
	{
		internalIdentifier = BWJsonHelpers.PropertyIfExists(internalIdentifier, "internal_identifier", json);
		userActivityId = BWJsonHelpers.PropertyIfExists(userActivityId, "id", json);
		settingsMask = BWJsonHelpers.PropertyIfExists(settingsMask, "settings_mask", json);
		textCategory = BWJsonHelpers.PropertyIfExists(textCategory, "category", json);
		textDescription = BWJsonHelpers.PropertyIfExists(textDescription, "description", json);
		iconUrl = BWJsonHelpers.PropertyIfExists(iconUrl, "icon_urls_for_sizes", "190x190", json);
		iconUrl = $"{BWEnvConfig.AWS_S3_BASE_URL}/{iconUrl}.png";
	}

	public static void LoadUserActivityDefinitions()
	{
		string text = Resources.Load<TextAsset>("user_activities").text;
		JObject jObject = JSONDecoder.Decode(text);
		userActivities = new List<BWUserActivityDefinition>();
		userActivitiesByID = new Dictionary<int, BWUserActivityDefinition>();
		userActivitiesByInternalIdentifier = new Dictionary<string, BWUserActivityDefinition>();
		foreach (JObject item in jObject.ArrayValue)
		{
			BWUserActivityDefinition bWUserActivityDefinition = new BWUserActivityDefinition(item);
			userActivities.Add(bWUserActivityDefinition);
			userActivitiesByID[bWUserActivityDefinition.userActivityId] = bWUserActivityDefinition;
			userActivitiesByInternalIdentifier[bWUserActivityDefinition.internalIdentifier] = bWUserActivityDefinition;
		}
	}
}
