using System.Collections.Generic;

public class UIDataSourcePendingPayouts : UIDataSource
{
	public UIDataSourcePendingPayouts(UIDataManager manager)
		: base(manager)
	{
	}

	public override void Refresh()
	{
		ClearData();
		foreach (BWPendingPayout pendingPayout in BWPendingPayouts.pendingPayouts)
		{
			string refID = pendingPayout.refID;
			Dictionary<string, string> value = pendingPayout.AttributesForMenuUI();
			base.Keys.Add(refID);
			base.Data.Add(refID, value);
		}
		base.Info["totalCoins"] = BWPendingPayouts.TotalCoins().ToString();
		base.loadState = LoadState.Loaded;
	}
}
