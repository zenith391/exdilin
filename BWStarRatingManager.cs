using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003BA RID: 954
public class BWStarRatingManager : MonoBehaviour
{
	// Token: 0x170001C7 RID: 455
	// (get) Token: 0x0600299B RID: 10651 RVA: 0x00131053 File Offset: 0x0012F453
	public static BWStarRatingManager Instance
	{
		get
		{
			return BWStarRatingManager.instance;
		}
	}

	// Token: 0x0600299C RID: 10652 RVA: 0x0013105A File Offset: 0x0012F45A
	public void Awake()
	{
		if (BWStarRatingManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.remoteWorldRatingsFromCurrentUser = new Dictionary<string, int>();
		this.remoteModelRatingsFromCurrentUser = new Dictionary<string, int>();
		BWStarRatingManager.instance = this;
	}

	// Token: 0x0600299D RID: 10653 RVA: 0x00131094 File Offset: 0x0012F494
	public int UserRatingForWorld(string worldID)
	{
		if (this.remoteWorldRatingsFromCurrentUser.ContainsKey(worldID))
		{
			return this.remoteWorldRatingsFromCurrentUser[worldID];
		}
		return -1;
	}

	// Token: 0x0600299E RID: 10654 RVA: 0x001310B5 File Offset: 0x0012F4B5
	public int UserRatingForModel(string modelID)
	{
		if (this.remoteModelRatingsFromCurrentUser.ContainsKey(modelID))
		{
			return this.remoteModelRatingsFromCurrentUser[modelID];
		}
		return -1;
	}

	// Token: 0x0600299F RID: 10655 RVA: 0x001310D8 File Offset: 0x0012F4D8
	public IEnumerator GetUserRatingForWorld(string worldID, BWStarRatingInfo ratingInfo)
	{
		if (string.IsNullOrEmpty(worldID))
		{
			yield break;
		}
		if (this.remoteWorldRatingsFromCurrentUser.ContainsKey(worldID))
		{
			ratingInfo.currentUserRating = this.remoteWorldRatingsFromCurrentUser[worldID];
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase request = BW.API.CreateRequest("GET", string.Format("/api/v1/current_user/world_star_rating/{0}", worldID));
		request.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				this.remoteWorldRatingsFromCurrentUser[worldID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			apiDone = true;
		};
		request.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060029A0 RID: 10656 RVA: 0x00131104 File Offset: 0x0012F504
	public IEnumerator GetUserRatingForModel(string modelID, BWStarRatingInfo ratingInfo)
	{
		if (string.IsNullOrEmpty(modelID))
		{
			yield break;
		}
		if (this.remoteModelRatingsFromCurrentUser.ContainsKey(modelID))
		{
			ratingInfo.currentUserRating = this.remoteModelRatingsFromCurrentUser[modelID];
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase request = BW.API.CreateRequest("GET", string.Format("/api/v1/current_user/u2u_model_star_rating/{0}", modelID));
		request.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				this.remoteModelRatingsFromCurrentUser[modelID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			apiDone = true;
		};
		request.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060029A1 RID: 10657 RVA: 0x00131130 File Offset: 0x0012F530
	public IEnumerator RateWorld(string worldID, int rating, BWStarRatingInfo ratingInfo)
	{
		if (rating < 1 || rating > 5)
		{
			BWLog.Error("Invalid world rating " + rating);
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase request = BW.API.CreateRequest("POST", string.Format("/api/v1/current_user/world_star_rating/{0}", worldID));
		request.AddParam("stars", rating.ToString());
		request.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				this.remoteWorldRatingsFromCurrentUser[worldID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			apiDone = true;
		};
		request.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060029A2 RID: 10658 RVA: 0x00131160 File Offset: 0x0012F560
	public IEnumerator RateModel(string modelID, int rating, BWStarRatingInfo ratingInfo)
	{
		if (rating < 1 || rating > 5)
		{
			BWLog.Error("Invalid model rating " + rating);
			yield break;
		}
		bool apiDone = false;
		BWAPIRequestBase request = BW.API.CreateRequest("POST", string.Format("/api/v1/current_user/u2u_model_star_rating/{0}", modelID));
		request.AddParam("stars", rating.ToString());
		request.onSuccess = delegate(JObject resp)
		{
			if (resp.ContainsKey("star_rating"))
			{
				int intValue = resp["star_rating"].IntValue;
				this.remoteWorldRatingsFromCurrentUser[modelID] = intValue;
				ratingInfo.currentUserRating = intValue;
			}
			if (resp.ContainsKey("average_star_rating"))
			{
				float floatValue = resp["average_star_rating"].FloatValue;
				ratingInfo.averageRating = floatValue;
			}
			apiDone = true;
		};
		request.onFailure = delegate(BWAPIRequestError error)
		{
			apiDone = true;
		};
		request.SendOwnerCoroutine(this);
		while (!apiDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x040023DF RID: 9183
	private static BWStarRatingManager instance;

	// Token: 0x040023E0 RID: 9184
	private Dictionary<string, int> remoteWorldRatingsFromCurrentUser;

	// Token: 0x040023E1 RID: 9185
	private Dictionary<string, int> remoteModelRatingsFromCurrentUser;
}
