using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWStarRatingManager : MonoBehaviour
{
	private static BWStarRatingManager instance;

	private Dictionary<string, int> remoteWorldRatingsFromCurrentUser;

	private Dictionary<string, int> remoteModelRatingsFromCurrentUser;

	public static BWStarRatingManager Instance => instance;

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		remoteWorldRatingsFromCurrentUser = new Dictionary<string, int>();
		remoteModelRatingsFromCurrentUser = new Dictionary<string, int>();
		instance = this;
	}

	public int UserRatingForWorld(string worldID)
	{
		if (remoteWorldRatingsFromCurrentUser.ContainsKey(worldID))
		{
			return remoteWorldRatingsFromCurrentUser[worldID];
		}
		return -1;
	}

	public int UserRatingForModel(string modelID)
	{
		if (remoteModelRatingsFromCurrentUser.ContainsKey(modelID))
		{
			return remoteModelRatingsFromCurrentUser[modelID];
		}
		return -1;
	}

	public IEnumerator GetUserRatingForWorld(string worldID, BWStarRatingInfo ratingInfo)
	{
		if (string.IsNullOrEmpty(worldID))
		{
			yield break;
		}
		if (remoteWorldRatingsFromCurrentUser.ContainsKey(worldID))
		{
			ratingInfo.currentUserRating = remoteWorldRatingsFromCurrentUser[worldID];
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/current_user/world_star_rating/{worldID}");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				remoteWorldRatingsFromCurrentUser[worldID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			apiDone = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
	}

	public IEnumerator GetUserRatingForModel(string modelID, BWStarRatingInfo ratingInfo)
	{
		if (string.IsNullOrEmpty(modelID))
		{
			yield break;
		}
		if (remoteModelRatingsFromCurrentUser.ContainsKey(modelID))
		{
			ratingInfo.currentUserRating = remoteModelRatingsFromCurrentUser[modelID];
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", $"/api/v1/current_user/u2u_model_star_rating/{modelID}");
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				remoteModelRatingsFromCurrentUser[modelID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			apiDone = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
	}

	public IEnumerator RateWorld(string worldID, int rating, BWStarRatingInfo ratingInfo)
	{
		if (rating < 1 || rating > 5)
		{
			BWLog.Error("Invalid world rating " + rating);
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", $"/api/v1/current_user/world_star_rating/{worldID}");
		bWAPIRequestBase.AddParam("stars", rating.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				remoteWorldRatingsFromCurrentUser[worldID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			apiDone = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
	}

	public IEnumerator RateModel(string modelID, int rating, BWStarRatingInfo ratingInfo)
	{
		if (rating < 1 || rating > 5)
		{
			BWLog.Error("Invalid model rating " + rating);
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("POST", $"/api/v1/current_user/u2u_model_star_rating/{modelID}");
		bWAPIRequestBase.AddParam("stars", rating.ToString());
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				remoteWorldRatingsFromCurrentUser[modelID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		bWAPIRequestBase.onFailure = delegate
		{
			apiDone = true;
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
	}
}
