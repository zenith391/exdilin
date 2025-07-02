using SimpleJSON;

public static class Leaderboard_API
{
	public static void LoadFromRemote(Leaderboard.LoadFromRemoteSuccessHandler success, Leaderboard.LoadFromRemoteFailureHandler failure)
	{
		string worldId = WorldSession.current.worldId;
		string path = "/api/v1/world_leaderboards/" + worldId;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			Leaderboard leaderboard = Leaderboard.BuildWithJsonObj(resp["leaderboard"]);
			success(leaderboard);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			reportErrorMessage(error, failure);
		};
		bWAPIRequestBase.Send();
	}

	public static void ReportNewTimeRemote(float newTime, Leaderboard.ReportNewTimeRemoteSuccessHandler success, Leaderboard.LoadFromRemoteFailureHandler failure)
	{
		string worldId = WorldSession.current.worldId;
		int newTimeMs = Leaderboard.TimeToMs(newTime);
		string path = "/api/v1/world_leaderboards/" + worldId + "/plays";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", path);
		bWAPIRequestBase.AddParam("time_ms", newTimeMs.ToString());
		bWAPIRequestBase.AddParam("digest", Leaderboard.EncryptDigest(worldId, newTimeMs));
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			Leaderboard leaderboard = Leaderboard.BuildWithJsonObj(resp["leaderboard"]);
			bool booleanValue = resp["time_improved"].BooleanValue;
			success(leaderboard, booleanValue);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			reportErrorMessage(error, failure);
		};
		bWAPIRequestBase.Send();
	}

	private static void reportErrorMessage(BWAPIRequestError error, Leaderboard.LoadFromRemoteFailureHandler failure)
	{
		string text = null;
		JObject responseBodyJson = error.responseBodyJson;
		if (responseBodyJson != null && responseBodyJson.ContainsKey("error"))
		{
			string stringValue = responseBodyJson["error"].StringValue;
			if (stringValue == "world_has_no_win_condition")
			{
				text = "This world no longer has a leadeboard.";
			}
			if (stringValue == "world_is_not_visible_in_community")
			{
				text = "Sorry, this world is not available anymore.";
			}
		}
		if (string.IsNullOrEmpty(text) && error.httpStatusCode == 404)
		{
			text = "Sorry, this world is not available anymore.";
		}
		if (string.IsNullOrEmpty(text))
		{
			text = error.title + "\n" + error.message;
		}
		failure(text);
	}
}
