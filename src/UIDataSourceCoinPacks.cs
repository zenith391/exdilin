using System.Collections.Generic;

public class UIDataSourceCoinPacks : UIDataSource
{
	public UIDataSourceCoinPacks(UIDataManager manager)
		: base(manager)
	{
		LoadCoinPacks();
	}

	private void LoadCoinPacks()
	{
		ClearData();
		foreach (BWSteamIAPCoinPack coinPack in BWSteamIAPManager.Instance.GetCoinPacks())
		{
			string internalIdentifier = coinPack.internalIdentifier;
			Dictionary<string, string> value = coinPack.AttributesForMenuUI();
			base.Keys.Add(internalIdentifier);
			base.Data.Add(internalIdentifier, value);
		}
		base.loadState = LoadState.Loaded;
	}
}
