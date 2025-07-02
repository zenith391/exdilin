using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class BWRemoteWorldsDataManager : MonoBehaviour
{
	private List<string> cachedWorldIDs = new List<string>();

	private Dictionary<string, BWWorld> cachedWorldData = new Dictionary<string, BWWorld>();

	private const int maxSize = 128;

	private static BWRemoteWorldsDataManager instance;

	public static BWRemoteWorldsDataManager Instance => instance;

	public void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public void LoadWorld(string worldID, Action<BWWorld> completion)
	{
		if (cachedWorldData.ContainsKey(worldID))
		{
			completion?.Invoke(cachedWorldData[worldID]);
		}
		else
		{
			LoadFromAPI(worldID, completion);
		}
	}

	public void LoadWorldToDataSource(string worldID, UIDataSourceSingleWorld toDataSource)
	{
		LoadWorld(worldID, delegate(BWWorld world)
		{
			toDataSource.OnWorldLoad(world);
		});
	}

	private void LoadFromAPI(string worldID, Action<BWWorld> completion)
	{
		string path = $"/api/v1/worlds/{worldID}";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject json = responseJson["world"];
			BWWorld bWWorld = new BWWorld(json);
			if (bWWorld != null && !cachedWorldData.ContainsKey(worldID))
			{
				cachedWorldIDs.Add(worldID);
				cachedWorldData.Add(worldID, bWWorld);
				if (cachedWorldIDs.Count >= 128)
				{
					string key = cachedWorldIDs[0];
					cachedWorldIDs.RemoveAt(0);
					cachedWorldData.Remove(key);
				}
			}
			if (completion != null)
			{
				completion(bWWorld);
			}
		};
		bWAPIRequestBase.SendOwnerCoroutine(this);
	}
}
