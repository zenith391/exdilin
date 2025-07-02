using System.Collections.Generic;

public class UIDataSourcePurchasedModelList : UIDataSource
{
	public UIDataSourcePurchasedModelList(UIDataManager manager)
		: base(manager)
	{
	}

	public override void Refresh()
	{
		base.loadState = LoadState.Loading;
		LoadFromDataManager();
	}

	public void LoadFromDataManager()
	{
		List<BWU2UModel> list = BWU2UModelDataManager.Instance.PurchasedU2UModels();
		ClearData();
		foreach (BWU2UModel item in list)
		{
			base.Keys.Add(item.modelID);
			Dictionary<string, string> value = item.AttributesForMenuUI();
			base.Data.Add(item.modelID, value);
		}
		base.loadState = LoadState.Loaded;
	}

	public void DataManagerFailedToLoadModelList()
	{
		base.loadState = LoadState.Failed;
	}
}
