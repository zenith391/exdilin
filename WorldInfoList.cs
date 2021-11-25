using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x0200036A RID: 874
public class WorldInfoList
{
	// Token: 0x14000013 RID: 19
	// (add) Token: 0x060026AD RID: 9901 RVA: 0x0011D464 File Offset: 0x0011B864
	// (remove) Token: 0x060026AE RID: 9902 RVA: 0x0011D49C File Offset: 0x0011B89C
	public event WorldListChangedEventHandler WorldListChanged;

	// Token: 0x060026AF RID: 9903 RVA: 0x0011D4D4 File Offset: 0x0011B8D4
	public void LoadCurrentUserWorlds()
	{
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/worlds_for_teleport");
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			this._isLoadingWorldList = false;
			List<JObject> arrayValue = responseJson["worlds"].ArrayValue;
			if (arrayValue != null)
			{
				foreach (JObject json in arrayValue)
				{
					WorldInfo worldInfo = new WorldInfo(json);
					if (worldInfo != null && !(worldInfo.id == WorldSession.current.worldId))
					{
						if (!this.elements.ContainsKey(worldInfo.id))
						{
							if (string.IsNullOrEmpty(worldInfo.authorUserName))
							{
								worldInfo.authorUserName = WorldSession.current.config.currentUserUsername;
							}
							this.sortedWorldInfos.Add(worldInfo);
							this.elements.Add(worldInfo.id, worldInfo);
						}
					}
				}
			}
			this.sortedWorldInfos.Reverse();
			this.OnWorldListChanged(EventArgs.Empty);
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			this._isLoadingWorldList = false;
			this._didFailToLoad = true;
			BWLog.Info(string.Concat(new object[]
			{
				"Failed to load user worlds ",
				error.message,
				" status ",
				error.httpStatusCode
			}));
			this.OnWorldListChanged(EventArgs.Empty);
		};
		bwapirequestBase.Send();
		this._isLoadingWorldList = true;
	}

	// Token: 0x060026B0 RID: 9904 RVA: 0x0011D528 File Offset: 0x0011B928
	public void AddInfoForWorld(string worldId)
	{
		if (this.elements.ContainsKey(worldId))
		{
			return;
		}
		string path = string.Format("/api/v1/worlds/{0}/basic_info", worldId);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson == null)
			{
				BWLog.Error("Bad response getting world info for world id " + worldId);
			}
			else
			{
				WorldInfo worldInfo = new WorldInfo(responseJson);
				if (!this.elements.ContainsKey(worldInfo.id))
				{
					this.elements.Add(worldInfo.id, worldInfo);
					this.sortedWorldInfos.Add(worldInfo);
					this.OnWorldListChanged(EventArgs.Empty);
				}
			}
			this._isLoadingWorldList = false;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.message);
			this._isLoadingWorldList = false;
		};
		bwapirequestBase.Send();
		this._isLoadingWorldList = true;
	}

	// Token: 0x060026B1 RID: 9905 RVA: 0x0011D5B3 File Offset: 0x0011B9B3
	public bool IsLoadingWorldList()
	{
		return this._isLoadingWorldList;
	}

	// Token: 0x060026B2 RID: 9906 RVA: 0x0011D5BB File Offset: 0x0011B9BB
	public string StatusString()
	{
		if (this._isLoadingWorldList)
		{
			return "Loading worlds...";
		}
		if (this._didFailToLoad)
		{
			return "Error!  Please check your network connection and try again.";
		}
		if (this.elements.Count == 0)
		{
			return "No worlds available.";
		}
		return string.Empty;
	}

	// Token: 0x060026B3 RID: 9907 RVA: 0x0011D5FC File Offset: 0x0011B9FC
	public WorldInfo GetWorldWithId(string worldId)
	{
		WorldInfo result = null;
		this.elements.TryGetValue(worldId, out result);
		return result;
	}

	// Token: 0x060026B4 RID: 9908 RVA: 0x0011D61C File Offset: 0x0011BA1C
	public void Clear()
	{
		foreach (WorldInfo worldInfo in this.elements.Values)
		{
			worldInfo.DestroyThumbnailImages();
		}
		this.elements.Clear();
	}

	// Token: 0x060026B5 RID: 9909 RVA: 0x0011D688 File Offset: 0x0011BA88
	private void OnWorldListChanged(EventArgs e)
	{
		if (this.WorldListChanged != null)
		{
			this.WorldListChanged(this, e);
		}
	}

	// Token: 0x040021E5 RID: 8677
	private Dictionary<string, WorldInfo> elements = new Dictionary<string, WorldInfo>();

	// Token: 0x040021E6 RID: 8678
	public List<WorldInfo> sortedWorldInfos = new List<WorldInfo>();

	// Token: 0x040021E7 RID: 8679
	private bool _isLoadingWorldList;

	// Token: 0x040021E8 RID: 8680
	public bool _didFailToLoad;
}
