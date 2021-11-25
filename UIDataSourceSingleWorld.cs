using System;
using System.Collections.Generic;

// Token: 0x020003F2 RID: 1010
public class UIDataSourceSingleWorld : UIDataSource
{
	// Token: 0x06002C68 RID: 11368 RVA: 0x0013EA99 File Offset: 0x0013CE99
	public UIDataSourceSingleWorld(UIDataManager manager, string worldID) : base(manager)
	{
		this.worldID = worldID;
	}

	// Token: 0x06002C69 RID: 11369 RVA: 0x0013EAA9 File Offset: 0x0013CEA9
	public override void Refresh()
	{
		base.ClearData();
		BWUserDataManager.Instance.LoadUserData();
		base.loadState = UIDataSource.LoadState.Loading;
		BWRemoteWorldsDataManager.Instance.LoadWorldToDataSource(this.worldID, this);
	}

	// Token: 0x06002C6A RID: 11370 RVA: 0x0013EAD4 File Offset: 0x0013CED4
	public void OnWorldLoad(BWWorld world)
	{
		if (world == null)
		{
			base.loadState = UIDataSource.LoadState.Failed;
			return;
		}
		base.Keys.Add(this.worldID);
		Dictionary<string, string> dictionary = world.AttributesForMenuUI();
		dictionary.Add("bookmarked", BWUserDataManager.Instance.HasBookmarkedWorld(world.worldID).ToString());
		base.Data[this.worldID] = dictionary;
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
	}

	// Token: 0x06002C6B RID: 11371 RVA: 0x0013EB4F File Offset: 0x0013CF4F
	public override string GetPlayButtonMessage()
	{
		return "PlayWorld";
	}

	// Token: 0x0400253C RID: 9532
	private string worldID;
}
