using System;
using System.Collections.Generic;

// Token: 0x020003ED RID: 1005
public class UIDataSourcePendingPayouts : UIDataSource
{
	// Token: 0x06002C47 RID: 11335 RVA: 0x0013E181 File Offset: 0x0013C581
	public UIDataSourcePendingPayouts(UIDataManager manager) : base(manager)
	{
	}

	// Token: 0x06002C48 RID: 11336 RVA: 0x0013E18C File Offset: 0x0013C58C
	public override void Refresh()
	{
		base.ClearData();
		foreach (BWPendingPayout bwpendingPayout in BWPendingPayouts.pendingPayouts)
		{
			string refID = bwpendingPayout.refID;
			Dictionary<string, string> value = bwpendingPayout.AttributesForMenuUI();
			base.Keys.Add(refID);
			base.Data.Add(refID, value);
		}
		base.Info["totalCoins"] = BWPendingPayouts.TotalCoins().ToString();
		base.loadState = UIDataSource.LoadState.Loaded;
	}
}
