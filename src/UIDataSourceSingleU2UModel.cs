using System;
using System.Collections.Generic;
using UnityEngine.Events;

// Token: 0x020003F1 RID: 1009
public class UIDataSourceSingleU2UModel : UIDataSource
{
	// Token: 0x06002C64 RID: 11364 RVA: 0x0013EA04 File Offset: 0x0013CE04
	public UIDataSourceSingleU2UModel(UIDataManager manager, string modelID) : base(manager)
	{
		this.modelID = modelID;
	}

	// Token: 0x06002C65 RID: 11365 RVA: 0x0013EA14 File Offset: 0x0013CE14
	public override void Refresh()
	{
		base.ClearData();
		base.loadState = UIDataSource.LoadState.Loading;
		BWU2UModelDataManager.Instance.LoadModelFromRemote(this.modelID, new UnityAction<BWU2UModel>(this.DataManagerLoadedModel), null);
	}

	// Token: 0x06002C66 RID: 11366 RVA: 0x0013EA40 File Offset: 0x0013CE40
	public void DataManagerLoadedModel(BWU2UModel model)
	{
		if (model == null)
		{
			base.loadState = UIDataSource.LoadState.Failed;
			return;
		}
		base.Keys.Add(this.modelID);
		Dictionary<string, string> value = model.AttributesForMenuUI();
		base.Data[this.modelID] = value;
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C67 RID: 11367 RVA: 0x0013EA92 File Offset: 0x0013CE92
	public override string GetPlayButtonMessage()
	{
		return "CommunityModelPreview";
	}

	// Token: 0x0400253B RID: 9531
	private string modelID;
}
