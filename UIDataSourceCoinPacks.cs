using System;
using System.Collections.Generic;

// Token: 0x020003E3 RID: 995
public class UIDataSourceCoinPacks : UIDataSource
{
	// Token: 0x06002C0D RID: 11277 RVA: 0x0013CD70 File Offset: 0x0013B170
	public UIDataSourceCoinPacks(UIDataManager manager) : base(manager)
	{
		this.LoadCoinPacks();
	}

	// Token: 0x06002C0E RID: 11278 RVA: 0x0013CD80 File Offset: 0x0013B180
	private void LoadCoinPacks()
	{
		base.ClearData();
		foreach (BWSteamIAPCoinPack bwsteamIAPCoinPack in BWSteamIAPManager.Instance.GetCoinPacks())
		{
			string internalIdentifier = bwsteamIAPCoinPack.internalIdentifier;
			Dictionary<string, string> value = bwsteamIAPCoinPack.AttributesForMenuUI();
			base.Keys.Add(internalIdentifier);
			base.Data.Add(internalIdentifier, value);
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}
}
