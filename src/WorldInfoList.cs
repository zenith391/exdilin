using System;
using System.Collections.Generic;
using SimpleJSON;

public class WorldInfoList
{
	private Dictionary<string, WorldInfo> elements = new Dictionary<string, WorldInfo>();

	public List<WorldInfo> sortedWorldInfos = new List<WorldInfo>();

	private bool _isLoadingWorldList;

	public bool _didFailToLoad;

	public event WorldListChangedEventHandler WorldListChanged;

	public void LoadCurrentUserWorlds()
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/worlds_for_teleport");
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			_isLoadingWorldList = false;
			List<JObject> arrayValue = responseJson["worlds"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject item in arrayValue)
				{
					WorldInfo worldInfo = new WorldInfo(item);
					if (worldInfo != null && !(worldInfo.id == WorldSession.current.worldId) && !elements.ContainsKey(worldInfo.id))
					{
						if (string.IsNullOrEmpty(worldInfo.authorUserName))
						{
							worldInfo.authorUserName = WorldSession.current.config.currentUserUsername;
						}
						sortedWorldInfos.Add(worldInfo);
						elements.Add(worldInfo.id, worldInfo);
					}
				}
			}
			sortedWorldInfos.Reverse();
			OnWorldListChanged(EventArgs.Empty);
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			_isLoadingWorldList = false;
			_didFailToLoad = true;
			BWLog.Info("Failed to load user worlds " + error.message + " status " + error.httpStatusCode);
			OnWorldListChanged(EventArgs.Empty);
		};
		bWAPIRequestBase.Send();
		_isLoadingWorldList = true;
	}

	public void AddInfoForWorld(string worldId)
	{
		if (elements.ContainsKey(worldId))
		{
			return;
		}
		string path = $"/api/v1/worlds/{worldId}/basic_info";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson == null)
			{
				BWLog.Error("Bad response getting world info for world id " + worldId);
			}
			else
			{
				WorldInfo worldInfo = new WorldInfo(responseJson);
				if (!elements.ContainsKey(worldInfo.id))
				{
					elements.Add(worldInfo.id, worldInfo);
					sortedWorldInfos.Add(worldInfo);
					OnWorldListChanged(EventArgs.Empty);
				}
			}
			_isLoadingWorldList = false;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.message);
			_isLoadingWorldList = false;
		};
		bWAPIRequestBase.Send();
		_isLoadingWorldList = true;
	}

	public bool IsLoadingWorldList()
	{
		return _isLoadingWorldList;
	}

	public string StatusString()
	{
		if (_isLoadingWorldList)
		{
			return "Loading worlds...";
		}
		if (_didFailToLoad)
		{
			return "Error!  Please check your network connection and try again.";
		}
		if (elements.Count == 0)
		{
			return "No worlds available.";
		}
		return string.Empty;
	}

	public WorldInfo GetWorldWithId(string worldId)
	{
		WorldInfo value = null;
		elements.TryGetValue(worldId, out value);
		return value;
	}

	public void Clear()
	{
		foreach (WorldInfo value in elements.Values)
		{
			value.DestroyThumbnailImages();
		}
		elements.Clear();
	}

	private void OnWorldListChanged(EventArgs e)
	{
		if (this.WorldListChanged != null)
		{
			this.WorldListChanged(this, e);
		}
	}
}
