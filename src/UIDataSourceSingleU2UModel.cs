using System.Collections.Generic;

public class UIDataSourceSingleU2UModel : UIDataSource
{
	private string modelID;

	public UIDataSourceSingleU2UModel(UIDataManager manager, string modelID)
		: base(manager)
	{
		this.modelID = modelID;
	}

	public override void Refresh()
	{
		ClearData();
		base.loadState = LoadState.Loading;
		BWU2UModelDataManager.Instance.LoadModelFromRemote(modelID, DataManagerLoadedModel);
	}

	public void DataManagerLoadedModel(BWU2UModel model)
	{
		if (model == null)
		{
			base.loadState = LoadState.Failed;
			return;
		}
		base.Keys.Add(modelID);
		Dictionary<string, string> value = model.AttributesForMenuUI();
		base.Data[modelID] = value;
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	public override string GetPlayButtonMessage()
	{
		return "CommunityModelPreview";
	}
}
