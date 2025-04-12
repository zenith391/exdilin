using System;
using SimpleJSON;

// Token: 0x020001B2 RID: 434
public static class Leaderboard_API
{
	// Token: 0x060017AC RID: 6060 RVA: 0x000A6A4C File Offset: 0x000A4E4C
	public static void LoadFromRemote(Leaderboard.LoadFromRemoteSuccessHandler success, Leaderboard.LoadFromRemoteFailureHandler failure)
	{
		string worldId = WorldSession.current.worldId;
		string path = "/api/v1/world_leaderboards/" + worldId;
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			Leaderboard leaderboard = Leaderboard.BuildWithJsonObj(resp["leaderboard"]);
			success(leaderboard);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Leaderboard_API.reportErrorMessage(error, failure);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x060017AD RID: 6061 RVA: 0x000A6AC0 File Offset: 0x000A4EC0
	public static void ReportNewTimeRemote(float newTime, Leaderboard.ReportNewTimeRemoteSuccessHandler success, Leaderboard.LoadFromRemoteFailureHandler failure)
	{
		string worldId = WorldSession.current.worldId;
		int newTimeMs = Leaderboard.TimeToMs(newTime);
		string path = "/api/v1/world_leaderboards/" + worldId + "/plays";
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("POST", path);
		bwapirequestBase.AddParam("time_ms", newTimeMs.ToString());
		bwapirequestBase.AddParam("digest", Leaderboard.EncryptDigest(worldId, newTimeMs));
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			Leaderboard leaderboard = Leaderboard.BuildWithJsonObj(resp["leaderboard"]);
			bool booleanValue = resp["time_improved"].BooleanValue;
			success(leaderboard, booleanValue);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			Leaderboard_API.reportErrorMessage(error, failure);
		};
		bwapirequestBase.Send();
	}

	// Token: 0x060017AE RID: 6062 RVA: 0x000A6B70 File Offset: 0x000A4F70
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
