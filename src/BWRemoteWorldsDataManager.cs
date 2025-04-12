using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x020003AF RID: 943
public class BWRemoteWorldsDataManager : MonoBehaviour
{
	// Token: 0x170001BC RID: 444
	// (get) Token: 0x06002903 RID: 10499 RVA: 0x0012D006 File Offset: 0x0012B406
	public static BWRemoteWorldsDataManager Instance
	{
		get
		{
			return BWRemoteWorldsDataManager.instance;
		}
	}

	// Token: 0x06002904 RID: 10500 RVA: 0x0012D00D File Offset: 0x0012B40D
	public void Awake()
	{
		if (BWRemoteWorldsDataManager.instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		BWRemoteWorldsDataManager.instance = this;
	}

	// Token: 0x06002905 RID: 10501 RVA: 0x0012D031 File Offset: 0x0012B431
	public void LoadWorld(string worldID, Action<BWWorld> completion)
	{
		if (this.cachedWorldData.ContainsKey(worldID))
		{
			if (completion != null)
			{
				completion(this.cachedWorldData[worldID]);
			}
			return;
		}
		this.LoadFromAPI(worldID, completion);
	}

	// Token: 0x06002906 RID: 10502 RVA: 0x0012D068 File Offset: 0x0012B468
	public void LoadWorldToDataSource(string worldID, UIDataSourceSingleWorld toDataSource)
	{
		this.LoadWorld(worldID, delegate(BWWorld world)
		{
			toDataSource.OnWorldLoad(world);
		});
	}

	// Token: 0x06002907 RID: 10503 RVA: 0x0012D098 File Offset: 0x0012B498
	private void LoadFromAPI(string worldID, Action<BWWorld> completion)
	{
		string path = string.Format("/api/v1/worlds/{0}", worldID);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Error(error.message);
		};
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
		{
			JObject json = responseJson["world"];
			BWWorld bwworld = new BWWorld(json);
			if (bwworld != null && !this.cachedWorldData.ContainsKey(worldID))
			{
				this.cachedWorldIDs.Add(worldID);
				this.cachedWorldData.Add(worldID, bwworld);
				if (this.cachedWorldIDs.Count >= 128)
				{
					string key = this.cachedWorldIDs[0];
					this.cachedWorldIDs.RemoveAt(0);
					this.cachedWorldData.Remove(key);
				}
			}
			if (completion != null)
			{
				completion(bwworld);
			}
		};
		bwapirequestBase.SendOwnerCoroutine(this);
	}

	// Token: 0x040023A4 RID: 9124
	private List<string> cachedWorldIDs = new List<string>();

	// Token: 0x040023A5 RID: 9125
	private Dictionary<string, BWWorld> cachedWorldData = new Dictionary<string, BWWorld>();

	// Token: 0x040023A6 RID: 9126
	private const int maxSize = 128;

	// Token: 0x040023A7 RID: 9127
	private static BWRemoteWorldsDataManager instance;
}
