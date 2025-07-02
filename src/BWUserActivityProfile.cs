using System.Collections.Generic;
using SimpleJSON;

public class BWUserActivityProfile : BWUserActivity
{
	public int userId;

	public int userStatus;

	public string profileImageUrl;

	public BWUserActivityProfile(JObject json)
		: base(json)
	{
		userId = BWJsonHelpers.PropertyIfExists(userId, "user_id", json);
		userStatus = BWJsonHelpers.PropertyIfExists(userStatus, "user_status", json);
		profileImageUrl = $"{BWEnvConfig.AWS_S3_BASE_URL}/profiles/{userId}/approved.png";
	}

	public override Dictionary<string, string> AttributesForMenuUI()
	{
		Dictionary<string, string> dictionary = base.AttributesForMenuUI();
		dictionary["user_id"] = userId.ToString();
		dictionary["user_status"] = userStatus.ToString();
		dictionary["image_url"] = profileImageUrl;
		return dictionary;
	}
}
