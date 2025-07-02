using System;
using System.Collections;
using System.Collections.Generic;

public class UIDataSource
{
	public enum LoadState
	{
		Unloaded,
		Loading,
		Loaded,
		Failed
	}

	public string dataType;

	public string dataSubtype;

	protected UIDataManager dataManager;

	protected float timeOut;

	protected DateTime dataTimestamp;

	public int nextPage = 1;

	public List<string> Keys { get; protected set; }

	public Dictionary<string, Dictionary<string, string>> Data { get; protected set; }

	public Dictionary<string, string> Info { get; protected set; }

	public Dictionary<string, int> PanelOverrides { get; protected set; }

	public List<int> SectionStartIndicies { get; protected set; }

	public int expectedSize { get; protected set; }

	public LoadState loadState { get; protected set; }

	private event UIDataLoadedEventHandler onLoaded;

	public UIDataSource(UIDataManager manager)
	{
		dataManager = manager;
	}

	public void LoadIfNeeded()
	{
		dataManager.StartCoroutine(LoadIfNeededCoroutine());
	}

	private IEnumerator LoadIfNeededCoroutine()
	{
		if (loadState == LoadState.Loading)
		{
			yield break;
		}
		if (loadState == LoadState.Loaded)
		{
			if (!DataIsStale())
			{
				yield return null;
				NotifyListeners(new List<string>());
				yield break;
			}
			loadState = LoadState.Loading;
			Refresh();
		}
		else if (loadState == LoadState.Unloaded)
		{
			loadState = LoadState.Loading;
			Refresh();
		}
		while (loadState == LoadState.Loading)
		{
			yield return null;
		}
		if (loadState == LoadState.Loaded)
		{
			NotifyListeners(Keys);
		}
	}

	protected void SetDataTimestamp()
	{
		dataTimestamp = DateTime.UtcNow;
	}

	private bool DataIsStale()
	{
		if (timeOut > 0f)
		{
			return (float)(DateTime.UtcNow - dataTimestamp).Seconds > timeOut;
		}
		return false;
	}

	public virtual void Refresh()
	{
	}

	public virtual bool CanExpand()
	{
		return false;
	}

	public virtual void Expand()
	{
	}

	public virtual int PagesLoaded()
	{
		return nextPage;
	}

	public virtual void RequestFullData(string itemID)
	{
	}

	public bool IsDataLoaded()
	{
		return loadState == LoadState.Loaded;
	}

	public bool DataLoadFailed()
	{
		return loadState == LoadState.Failed;
	}

	public void ClearData()
	{
		Keys = new List<string>();
		Data = new Dictionary<string, Dictionary<string, string>>();
		Info = new Dictionary<string, string>();
		PanelOverrides = new Dictionary<string, int>();
		loadState = LoadState.Unloaded;
	}

	public virtual void OverwriteData(string id, string dataKey, string newValueStr)
	{
		if (loadState == LoadState.Loaded && Data.ContainsKey(id))
		{
			Dictionary<string, string> dictionary = Data[id];
			if (dictionary.ContainsKey(dataKey))
			{
				dictionary[dataKey] = newValueStr;
				NotifyListeners(new List<string> { dataKey });
			}
		}
	}

	public virtual void BackupData(string id)
	{
	}

	public virtual string GetPlayButtonMessage()
	{
		return null;
	}

	public void AddListener(UIDataLoadedEventHandler listener)
	{
		onLoaded -= listener;
		onLoaded += listener;
	}

	public void RemoveListener(UIDataLoadedEventHandler listener)
	{
		onLoaded -= listener;
	}

	public void ClearListeners()
	{
		this.onLoaded = null;
	}

	protected void NotifyListeners()
	{
		NotifyListeners(Keys);
	}

	protected void NotifyListeners(List<string> modifiedKeys)
	{
		if (this.onLoaded != null)
		{
			this.onLoaded(modifiedKeys);
		}
	}
}
