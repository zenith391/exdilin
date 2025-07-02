using System.Collections.Generic;

public class UIDataSourceLocalModelList : UIDataSource
{
	public delegate bool ModelFilter(BWUserModel model);

	private ModelFilter filter;

	public UIDataSourceLocalModelList(UIDataManager dataManager)
		: base(dataManager)
	{
		BWUserModelsDataManager.Instance.AddListener(OnModelListChanged);
	}

	public static UIDataSourceLocalModelList CurrentUserUnpublishedModels(UIDataManager dataManager)
	{
		UIDataSourceLocalModelList uIDataSourceLocalModelList = new UIDataSourceLocalModelList(dataManager);
		uIDataSourceLocalModelList.filter = (BWUserModel model) => !model.isPublished;
		return uIDataSourceLocalModelList;
	}

	public static UIDataSourceLocalModelList CurrentUserPublishedModels(UIDataManager dataManager)
	{
		UIDataSourceLocalModelList uIDataSourceLocalModelList = new UIDataSourceLocalModelList(dataManager);
		uIDataSourceLocalModelList.filter = (BWUserModel model) => model.isPublished;
		return uIDataSourceLocalModelList;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (BWUserModelsDataManager.Instance.localModelsLoaded)
		{
			OnModelListChanged();
			base.loadState = LoadState.Loaded;
		}
		else
		{
			ClearData();
			BWUserModelsDataManager.Instance.LoadAllModelsMetadata();
		}
	}

	public override string GetPlayButtonMessage()
	{
		return "UserModelPreview";
	}

	public override void OverwriteData(string id, string dataKey, string newValueStr)
	{
		base.OverwriteData(id, dataKey, newValueStr);
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(id);
		if (modelWithLocalId.OverwriteMetadata(base.Data[id]))
		{
			BWUserModelsDataManager.Instance.SaveModelMetadata(modelWithLocalId);
			if (!modelWithLocalId.isPublished)
			{
				BWUserModelsDataManager.Instance.UpdateRemoteModel(modelWithLocalId);
			}
			BWUserModelsDataManager.Instance.NotifyListeners();
		}
	}

	private void OnModelListChanged()
	{
		ClearData();
		foreach (BWUserModel localModel in BWUserModelsDataManager.Instance.localModels)
		{
			if (filter(localModel))
			{
				if (string.IsNullOrEmpty(localModel.localID))
				{
					BWLog.Info("Invalid model data, ignoring");
					continue;
				}
				if (base.Keys.Contains(localModel.localID))
				{
					BWLog.Info("Duplicate modelID " + localModel.localID);
					continue;
				}
				Dictionary<string, string> dictionary = localModel.AttributesForMenuUI();
				dictionary["author_username"] = BWUser.currentUser.username;
				dictionary["author_id"] = BWUser.currentUser.userID.ToString();
				dictionary["author_profile_image_url"] = BWUser.currentUser.profileImageURL;
				dictionary["created_by_current_user"] = true.ToString();
				base.Keys.Add(localModel.localID);
				base.Data.Add(localModel.localID, dictionary);
			}
		}
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}
}
