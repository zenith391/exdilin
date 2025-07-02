using System.Collections;
using System.Collections.Generic;

public class UIDataSourceWorldListConcatenated : UIDataSourceWorldList
{
	private UIDataSourceWorldList worldList1;

	private UIDataSourceWorldList worldList2;

	public UIDataSourceWorldListConcatenated(UIDataManager dataManager, UIDataSourceWorldList list1, UIDataSourceWorldList list2)
		: base(dataManager)
	{
		worldList1 = list1;
		worldList2 = list2;
	}

	public override void Refresh()
	{
		ClearData();
		dataManager.StartCoroutine(RefreshConcatenated());
	}

	private IEnumerator RefreshConcatenated()
	{
		BWLog.Info(worldList1.ToString());
		base.loadState = LoadState.Loading;
		worldList1.Refresh();
		while (worldList1.loadState == LoadState.Loading)
		{
			yield return null;
		}
		if (worldList1.loadState == LoadState.Loaded && worldList1.AllPagesLoaded())
		{
			worldList2.Refresh();
			while (worldList2.loadState == LoadState.Loading)
			{
				yield return null;
			}
			if (worldList1.IsDataLoaded() && worldList2.IsDataLoaded())
			{
				AddDataFromWorldLists();
				base.loadState = LoadState.Loaded;
				NotifyListeners();
			}
			else
			{
				base.loadState = LoadState.Failed;
			}
		}
		else if (worldList1.loadState == LoadState.Loaded)
		{
			base.Keys = new List<string>();
			base.Data = new Dictionary<string, Dictionary<string, string>>();
			base.Keys.AddRange(worldList1.Keys);
			foreach (KeyValuePair<string, Dictionary<string, string>> datum in worldList1.Data)
			{
				base.Data.Add(datum.Key, datum.Value);
			}
			base.loadState = LoadState.Loaded;
			NotifyListeners();
		}
		else
		{
			base.loadState = LoadState.Failed;
		}
	}

	private void AddDataFromWorldLists()
	{
		base.Keys = new List<string>();
		base.Data = new Dictionary<string, Dictionary<string, string>>();
		base.Keys.AddRange(worldList1.Keys);
		base.Keys.AddRange(worldList2.Keys);
		foreach (KeyValuePair<string, Dictionary<string, string>> datum in worldList1.Data)
		{
			base.Data.Add(datum.Key, datum.Value);
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> datum2 in worldList2.Data)
		{
			base.Data.Add(datum2.Key, datum2.Value);
		}
	}

	public new bool AllPagesLoaded()
	{
		if (worldList1.AllPagesLoaded())
		{
			return worldList2.AllPagesLoaded();
		}
		return false;
	}

	public override bool CanExpand()
	{
		if (!worldList1.CanExpand())
		{
			return worldList2.CanExpand();
		}
		return true;
	}

	public override void Expand()
	{
		if (!worldList1.AllPagesLoaded())
		{
			worldList1.DoRequest(delegate
			{
				if (worldList1.AllPagesLoaded())
				{
					Expand();
				}
				else
				{
					AddDataFromWorldLists();
					NotifyListeners();
				}
			});
		}
		else if (!worldList2.AllPagesLoaded())
		{
			worldList2.DoRequest(delegate
			{
				AddDataFromWorldLists();
				NotifyListeners();
			});
		}
	}

	public override int PagesLoaded()
	{
		return worldList1.PagesLoaded() + worldList2.PagesLoaded();
	}

	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}
}
