using System;
using System.Collections.Generic;

// Token: 0x020003EF RID: 1007
public class UIDataSourcePurchasedModelList : UIDataSource
{
	// Token: 0x06002C5B RID: 11355 RVA: 0x0013E58C File Offset: 0x0013C98C
	public UIDataSourcePurchasedModelList(UIDataManager manager) : base(manager)
	{
	}

	// Token: 0x06002C5C RID: 11356 RVA: 0x0013E595 File Offset: 0x0013C995
	public override void Refresh()
	{
		base.loadState = UIDataSource.LoadState.Loading;
		this.LoadFromDataManager();
	}

	// Token: 0x06002C5D RID: 11357 RVA: 0x0013E5A4 File Offset: 0x0013C9A4
	public void LoadFromDataManager()
	{
		List<BWU2UModel> list = BWU2UModelDataManager.Instance.PurchasedU2UModels();
		base.ClearData();
		foreach (BWU2UModel bwu2UModel in list)
		{
			base.Keys.Add(bwu2UModel.modelID);
			Dictionary<string, string> value = bwu2UModel.AttributesForMenuUI();
			base.Data.Add(bwu2UModel.modelID, value);
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}

	// Token: 0x06002C5E RID: 11358 RVA: 0x0013E638 File Offset: 0x0013CA38
	public void DataManagerFailedToLoadModelList()
	{
		base.loadState = UIDataSource.LoadState.Failed;
	}
}
