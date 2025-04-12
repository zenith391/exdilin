using System;
using System.Collections;
using System.Collections.Generic;

// Token: 0x020003DF RID: 991
public class UIDataSource
{
	// Token: 0x06002BE4 RID: 11236 RVA: 0x0013C719 File Offset: 0x0013AB19
	public UIDataSource(UIDataManager manager)
	{
		this.dataManager = manager;
	}

	// Token: 0x1400001F RID: 31
	// (add) Token: 0x06002BE5 RID: 11237 RVA: 0x0013C730 File Offset: 0x0013AB30
	// (remove) Token: 0x06002BE6 RID: 11238 RVA: 0x0013C768 File Offset: 0x0013AB68
	private event UIDataLoadedEventHandler onLoaded;

	// Token: 0x1700023C RID: 572
	// (get) Token: 0x06002BE7 RID: 11239 RVA: 0x0013C79E File Offset: 0x0013AB9E
	// (set) Token: 0x06002BE8 RID: 11240 RVA: 0x0013C7A6 File Offset: 0x0013ABA6
	public List<string> Keys { get; protected set; }

	// Token: 0x1700023D RID: 573
	// (get) Token: 0x06002BE9 RID: 11241 RVA: 0x0013C7AF File Offset: 0x0013ABAF
	// (set) Token: 0x06002BEA RID: 11242 RVA: 0x0013C7B7 File Offset: 0x0013ABB7
	public Dictionary<string, Dictionary<string, string>> Data { get; protected set; }

	// Token: 0x1700023E RID: 574
	// (get) Token: 0x06002BEB RID: 11243 RVA: 0x0013C7C0 File Offset: 0x0013ABC0
	// (set) Token: 0x06002BEC RID: 11244 RVA: 0x0013C7C8 File Offset: 0x0013ABC8
	public Dictionary<string, string> Info { get; protected set; }

	// Token: 0x1700023F RID: 575
	// (get) Token: 0x06002BED RID: 11245 RVA: 0x0013C7D1 File Offset: 0x0013ABD1
	// (set) Token: 0x06002BEE RID: 11246 RVA: 0x0013C7D9 File Offset: 0x0013ABD9
	public Dictionary<string, int> PanelOverrides { get; protected set; }

	// Token: 0x17000240 RID: 576
	// (get) Token: 0x06002BEF RID: 11247 RVA: 0x0013C7E2 File Offset: 0x0013ABE2
	// (set) Token: 0x06002BF0 RID: 11248 RVA: 0x0013C7EA File Offset: 0x0013ABEA
	public List<int> SectionStartIndicies { get; protected set; }

	// Token: 0x17000241 RID: 577
	// (get) Token: 0x06002BF1 RID: 11249 RVA: 0x0013C7F3 File Offset: 0x0013ABF3
	// (set) Token: 0x06002BF2 RID: 11250 RVA: 0x0013C7FB File Offset: 0x0013ABFB
	public int expectedSize { get; protected set; }

	// Token: 0x17000242 RID: 578
	// (get) Token: 0x06002BF3 RID: 11251 RVA: 0x0013C804 File Offset: 0x0013AC04
	// (set) Token: 0x06002BF4 RID: 11252 RVA: 0x0013C80C File Offset: 0x0013AC0C
	public UIDataSource.LoadState loadState { get; protected set; }

	// Token: 0x06002BF5 RID: 11253 RVA: 0x0013C815 File Offset: 0x0013AC15
	public void LoadIfNeeded()
	{
		this.dataManager.StartCoroutine(this.LoadIfNeededCoroutine());
	}

	// Token: 0x06002BF6 RID: 11254 RVA: 0x0013C82C File Offset: 0x0013AC2C
	private IEnumerator LoadIfNeededCoroutine()
	{
		if (this.loadState == UIDataSource.LoadState.Loading)
		{
			yield break;
		}
		if (this.loadState == UIDataSource.LoadState.Loaded)
		{
			if (!this.DataIsStale())
			{
				yield return null;
				this.NotifyListeners(new List<string>());
				yield break;
			}
			this.loadState = UIDataSource.LoadState.Loading;
			this.Refresh();
		}
		else if (this.loadState == UIDataSource.LoadState.Unloaded)
		{
			this.loadState = UIDataSource.LoadState.Loading;
			this.Refresh();
		}
		while (this.loadState == UIDataSource.LoadState.Loading)
		{
			yield return null;
		}
		if (this.loadState == UIDataSource.LoadState.Loaded)
		{
			this.NotifyListeners(this.Keys);
		}
		yield break;
	}

	// Token: 0x06002BF7 RID: 11255 RVA: 0x0013C847 File Offset: 0x0013AC47
	protected void SetDataTimestamp()
	{
		this.dataTimestamp = DateTime.UtcNow;
	}

	// Token: 0x06002BF8 RID: 11256 RVA: 0x0013C854 File Offset: 0x0013AC54
	private bool DataIsStale()
	{
		return this.timeOut > 0f && (float)(DateTime.UtcNow - this.dataTimestamp).Seconds > this.timeOut;
	}

	// Token: 0x06002BF9 RID: 11257 RVA: 0x0013C894 File Offset: 0x0013AC94
	public virtual void Refresh()
	{
	}

	// Token: 0x06002BFA RID: 11258 RVA: 0x0013C896 File Offset: 0x0013AC96
	public virtual bool CanExpand()
	{
		return false;
	}

	// Token: 0x06002BFB RID: 11259 RVA: 0x0013C899 File Offset: 0x0013AC99
	public virtual void Expand()
	{
	}

	// Token: 0x06002BFC RID: 11260 RVA: 0x0013C89B File Offset: 0x0013AC9B
	public virtual int PagesLoaded()
	{
		return this.nextPage;
	}

	// Token: 0x06002BFD RID: 11261 RVA: 0x0013C8A3 File Offset: 0x0013ACA3
	public virtual void RequestFullData(string itemID)
	{
	}

	// Token: 0x06002BFE RID: 11262 RVA: 0x0013C8A5 File Offset: 0x0013ACA5
	public bool IsDataLoaded()
	{
		return this.loadState == UIDataSource.LoadState.Loaded;
	}

	// Token: 0x06002BFF RID: 11263 RVA: 0x0013C8B0 File Offset: 0x0013ACB0
	public bool DataLoadFailed()
	{
		return this.loadState == UIDataSource.LoadState.Failed;
	}

	// Token: 0x06002C00 RID: 11264 RVA: 0x0013C8BB File Offset: 0x0013ACBB
	public void ClearData()
	{
		this.Keys = new List<string>();
		this.Data = new Dictionary<string, Dictionary<string, string>>();
		this.Info = new Dictionary<string, string>();
		this.PanelOverrides = new Dictionary<string, int>();
		this.loadState = UIDataSource.LoadState.Unloaded;
	}

	// Token: 0x06002C01 RID: 11265 RVA: 0x0013C8F0 File Offset: 0x0013ACF0
	public virtual void OverwriteData(string id, string dataKey, string newValueStr)
	{
		if (this.loadState != UIDataSource.LoadState.Loaded)
		{
			return;
		}
		if (!this.Data.ContainsKey(id))
		{
			return;
		}
		Dictionary<string, string> dictionary = this.Data[id];
		if (!dictionary.ContainsKey(dataKey))
		{
			return;
		}
		dictionary[dataKey] = newValueStr;
		this.NotifyListeners(new List<string>
		{
			dataKey
		});
	}

	// Token: 0x06002C02 RID: 11266 RVA: 0x0013C952 File Offset: 0x0013AD52
	public virtual void BackupData(string id)
	{
	}

	// Token: 0x06002C03 RID: 11267 RVA: 0x0013C954 File Offset: 0x0013AD54
	public virtual string GetPlayButtonMessage()
	{
		return null;
	}

	// Token: 0x06002C04 RID: 11268 RVA: 0x0013C957 File Offset: 0x0013AD57
	public void AddListener(UIDataLoadedEventHandler listener)
	{
		this.onLoaded -= listener;
		this.onLoaded += listener;
	}

	// Token: 0x06002C05 RID: 11269 RVA: 0x0013C967 File Offset: 0x0013AD67
	public void RemoveListener(UIDataLoadedEventHandler listener)
	{
		this.onLoaded -= listener;
	}

	// Token: 0x06002C06 RID: 11270 RVA: 0x0013C970 File Offset: 0x0013AD70
	public void ClearListeners()
	{
		this.onLoaded = null;
	}

	// Token: 0x06002C07 RID: 11271 RVA: 0x0013C979 File Offset: 0x0013AD79
	protected void NotifyListeners()
	{
		this.NotifyListeners(this.Keys);
	}

	// Token: 0x06002C08 RID: 11272 RVA: 0x0013C987 File Offset: 0x0013AD87
	protected void NotifyListeners(List<string> modifiedKeys)
	{
		if (this.onLoaded != null)
		{
			this.onLoaded(modifiedKeys);
		}
	}

	// Token: 0x04002513 RID: 9491
	public string dataType;

	// Token: 0x04002514 RID: 9492
	public string dataSubtype;

	// Token: 0x0400251A RID: 9498
	protected UIDataManager dataManager;

	// Token: 0x0400251B RID: 9499
	protected float timeOut;

	// Token: 0x0400251C RID: 9500
	protected DateTime dataTimestamp;

	// Token: 0x0400251E RID: 9502
	public int nextPage = 1;

	// Token: 0x020003E0 RID: 992
	public enum LoadState
	{
		// Token: 0x04002521 RID: 9505
		Unloaded,
		// Token: 0x04002522 RID: 9506
		Loading,
		// Token: 0x04002523 RID: 9507
		Loaded,
		// Token: 0x04002524 RID: 9508
		Failed
	}
}
