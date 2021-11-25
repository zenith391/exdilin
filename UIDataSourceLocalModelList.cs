using System;
using System.Collections.Generic;

// Token: 0x020003E7 RID: 999
public class UIDataSourceLocalModelList : UIDataSource
{
	// Token: 0x06002C21 RID: 11297 RVA: 0x0013D632 File Offset: 0x0013BA32
	public UIDataSourceLocalModelList(UIDataManager dataManager) : base(dataManager)
	{
		BWUserModelsDataManager.Instance.AddListener(new ModelsListChangedEventHandler(this.OnModelListChanged));
	}

	// Token: 0x06002C22 RID: 11298 RVA: 0x0013D654 File Offset: 0x0013BA54
	public static UIDataSourceLocalModelList CurrentUserUnpublishedModels(UIDataManager dataManager)
	{
		UIDataSourceLocalModelList uidataSourceLocalModelList = new UIDataSourceLocalModelList(dataManager);
		uidataSourceLocalModelList.filter = ((BWUserModel model) => !model.isPublished);
		return uidataSourceLocalModelList;
	}

	// Token: 0x06002C23 RID: 11299 RVA: 0x0013D68C File Offset: 0x0013BA8C
	public static UIDataSourceLocalModelList CurrentUserPublishedModels(UIDataManager dataManager)
	{
		UIDataSourceLocalModelList uidataSourceLocalModelList = new UIDataSourceLocalModelList(dataManager);
		uidataSourceLocalModelList.filter = ((BWUserModel model) => model.isPublished);
		return uidataSourceLocalModelList;
	}

	// Token: 0x06002C24 RID: 11300 RVA: 0x0013D6C4 File Offset: 0x0013BAC4
	public override void Refresh()
	{
		base.Refresh();
		if (BWUserModelsDataManager.Instance.localModelsLoaded)
		{
			this.OnModelListChanged();
			base.loadState = UIDataSource.LoadState.Loaded;
		}
		else
		{
			base.ClearData();
			BWUserModelsDataManager.Instance.LoadAllModelsMetadata();
		}
	}

	// Token: 0x06002C25 RID: 11301 RVA: 0x0013D6FD File Offset: 0x0013BAFD
	public override string GetPlayButtonMessage()
	{
		return "UserModelPreview";
	}

	// Token: 0x06002C26 RID: 11302 RVA: 0x0013D704 File Offset: 0x0013BB04
	public override void OverwriteData(string id, string dataKey, string newValueStr)
	{
		base.OverwriteData(id, dataKey, newValueStr);
		BWUserModel modelWithLocalId = BWUserModelsDataManager.Instance.GetModelWithLocalId(id);
		bool flag = modelWithLocalId.OverwriteMetadata(base.Data[id]);
		if (flag)
		{
			BWUserModelsDataManager.Instance.SaveModelMetadata(modelWithLocalId);
			if (!modelWithLocalId.isPublished)
			{
				BWUserModelsDataManager.Instance.UpdateRemoteModel(modelWithLocalId);
			}
			BWUserModelsDataManager.Instance.NotifyListeners();
		}
	}

	// Token: 0x06002C27 RID: 11303 RVA: 0x0013D76C File Offset: 0x0013BB6C
	private void OnModelListChanged()
	{
		base.ClearData();
		foreach (BWUserModel bwuserModel in BWUserModelsDataManager.Instance.localModels)
		{
			if (this.filter(bwuserModel))
			{
				if (string.IsNullOrEmpty(bwuserModel.localID))
				{
					BWLog.Info("Invalid model data, ignoring");
				}
				else if (base.Keys.Contains(bwuserModel.localID))
				{
					BWLog.Info("Duplicate modelID " + bwuserModel.localID);
				}
				else
				{
					Dictionary<string, string> dictionary = bwuserModel.AttributesForMenuUI();
					dictionary["author_username"] = BWUser.currentUser.username;
					dictionary["author_id"] = BWUser.currentUser.userID.ToString();
					dictionary["author_profile_image_url"] = BWUser.currentUser.profileImageURL;
					dictionary["created_by_current_user"] = true.ToString();
					base.Keys.Add(bwuserModel.localID);
					base.Data.Add(bwuserModel.localID, dictionary);
				}
			}
		}
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x0400252B RID: 9515
	private UIDataSourceLocalModelList.ModelFilter filter;

	// Token: 0x020003E8 RID: 1000
	// (Invoke) Token: 0x06002C2B RID: 11307
	public delegate bool ModelFilter(BWUserModel model);
}
