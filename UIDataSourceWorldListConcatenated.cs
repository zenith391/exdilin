using System;
using System.Collections;
using System.Collections.Generic;

// Token: 0x020003F9 RID: 1017
public class UIDataSourceWorldListConcatenated : UIDataSourceWorldList
{
	// Token: 0x06002C9F RID: 11423 RVA: 0x00140031 File Offset: 0x0013E431
	public UIDataSourceWorldListConcatenated(UIDataManager dataManager, UIDataSourceWorldList list1, UIDataSourceWorldList list2) : base(dataManager)
	{
		this.worldList1 = list1;
		this.worldList2 = list2;
	}

	// Token: 0x06002CA0 RID: 11424 RVA: 0x00140048 File Offset: 0x0013E448
	public override void Refresh()
	{
		base.ClearData();
		this.dataManager.StartCoroutine(this.RefreshConcatenated());
	}

	// Token: 0x06002CA1 RID: 11425 RVA: 0x00140064 File Offset: 0x0013E464
	private IEnumerator RefreshConcatenated()
	{
		BWLog.Info(worldList1.ToString());
		base.loadState = UIDataSource.LoadState.Loading;
		this.worldList1.Refresh();
		while (this.worldList1.loadState == UIDataSource.LoadState.Loading)
		{
			yield return null;
		}
		if (this.worldList1.loadState == UIDataSource.LoadState.Loaded && this.worldList1.AllPagesLoaded())
		{
			this.worldList2.Refresh();
			while (this.worldList2.loadState == UIDataSource.LoadState.Loading)
			{
				yield return null;
			}
			if (this.worldList1.IsDataLoaded() && this.worldList2.IsDataLoaded())
			{
				this.AddDataFromWorldLists();
				base.loadState = UIDataSource.LoadState.Loaded;
				base.NotifyListeners();
			}
			else
			{
				base.loadState = UIDataSource.LoadState.Failed;
			}
		}
		else if (this.worldList1.loadState == UIDataSource.LoadState.Loaded)
		{
			base.Keys = new List<string>();
			base.Data = new Dictionary<string, Dictionary<string, string>>();
			base.Keys.AddRange(this.worldList1.Keys);
			foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in this.worldList1.Data)
			{
				base.Data.Add(keyValuePair.Key, keyValuePair.Value);
			}
			base.loadState = UIDataSource.LoadState.Loaded;
			base.NotifyListeners();
		}
		else
		{
			base.loadState = UIDataSource.LoadState.Failed;
		}
		yield break;
	}

	// Token: 0x06002CA2 RID: 11426 RVA: 0x00140080 File Offset: 0x0013E480
	private void AddDataFromWorldLists()
	{
		base.Keys = new List<string>();
		base.Data = new Dictionary<string, Dictionary<string, string>>();
		base.Keys.AddRange(this.worldList1.Keys);
		base.Keys.AddRange(this.worldList2.Keys);
		foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair in this.worldList1.Data)
		{
			base.Data.Add(keyValuePair.Key, keyValuePair.Value);
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair2 in this.worldList2.Data)
		{
			base.Data.Add(keyValuePair2.Key, keyValuePair2.Value);
		}
	}

	// Token: 0x06002CA3 RID: 11427 RVA: 0x00140198 File Offset: 0x0013E598
	public new bool AllPagesLoaded()
	{
		return this.worldList1.AllPagesLoaded() && this.worldList2.AllPagesLoaded();
	}

	// Token: 0x06002CA4 RID: 11428 RVA: 0x001401B8 File Offset: 0x0013E5B8
	public override bool CanExpand()
	{
		return this.worldList1.CanExpand() || this.worldList2.CanExpand();
	}

	// Token: 0x06002CA5 RID: 11429 RVA: 0x001401D8 File Offset: 0x0013E5D8
	public override void Expand()
	{
		if (!this.worldList1.AllPagesLoaded())
		{
			this.worldList1.DoRequest(delegate
			{
				if (this.worldList1.AllPagesLoaded())
				{
					this.Expand();
				}
				else
				{
					this.AddDataFromWorldLists();
					base.NotifyListeners();
				}
			});
		}
		else if (!this.worldList2.AllPagesLoaded())
		{
			this.worldList2.DoRequest(delegate
			{
				this.AddDataFromWorldLists();
				base.NotifyListeners();
			});
		}
	}

	// Token: 0x06002CA6 RID: 11430 RVA: 0x00140238 File Offset: 0x0013E638
	public override int PagesLoaded()
	{
		return this.worldList1.PagesLoaded() + this.worldList2.PagesLoaded();
	}

	// Token: 0x06002CA7 RID: 11431 RVA: 0x00140251 File Offset: 0x0013E651
	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}

	// Token: 0x0400254E RID: 9550
	private UIDataSourceWorldList worldList1;

	// Token: 0x0400254F RID: 9551
	private UIDataSourceWorldList worldList2;
}
