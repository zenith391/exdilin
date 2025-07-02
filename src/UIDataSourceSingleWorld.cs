using System.Collections.Generic;

public class UIDataSourceSingleWorld : UIDataSource
{
	private string worldID;

	public UIDataSourceSingleWorld(UIDataManager manager, string worldID)
		: base(manager)
	{
		this.worldID = worldID;
	}

	public override void Refresh()
	{
		ClearData();
		BWUserDataManager.Instance.LoadUserData();
		base.loadState = LoadState.Loading;
		BWRemoteWorldsDataManager.Instance.LoadWorldToDataSource(worldID, this);
	}

	public void OnWorldLoad(BWWorld world)
	{
		if (world == null)
		{
			base.loadState = LoadState.Failed;
			return;
		}
		base.Keys.Add(worldID);
		Dictionary<string, string> dictionary = world.AttributesForMenuUI();
		dictionary.Add("bookmarked", BWUserDataManager.Instance.HasBookmarkedWorld(world.worldID).ToString());
		base.Data[worldID] = dictionary;
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}
}
