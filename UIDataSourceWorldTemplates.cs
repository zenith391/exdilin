using System;
using System.Collections.Generic;

// Token: 0x020003FA RID: 1018
public class UIDataSourceWorldTemplates : UIDataSource
{
	// Token: 0x06002CAA RID: 11434 RVA: 0x0014050F File Offset: 0x0013E90F
	public UIDataSourceWorldTemplates(UIDataManager dataManager) : base(dataManager)
	{
	}

	// Token: 0x06002CAB RID: 11435 RVA: 0x00140518 File Offset: 0x0013E918
	public override void Refresh()
	{
		base.Refresh();
		base.ClearData();
		if (BWUser.currentUser == null)
		{
			base.loadState = UIDataSource.LoadState.Failed;
			return;
		}
		if (BWUser.currentUser.worldTemplates.Count == 0)
		{
			BWLog.Info("No world templates");
		}
		for (int i = 0; i < BWUser.currentUser.worldTemplates.Count; i++)
		{
			BWWorldTemplate bwworldTemplate = BWUser.currentUser.worldTemplates[i];
			base.Keys.Add(bwworldTemplate.title);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("title", bwworldTemplate.title);
			dictionary.Add("thumbnail_url", bwworldTemplate.smallImageURL);
			dictionary.Add("screenshot_url", bwworldTemplate.largeImageURL);
			base.Data.Add(bwworldTemplate.title, dictionary);
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}

	// Token: 0x06002CAC RID: 11436 RVA: 0x001405F5 File Offset: 0x0013E9F5
	public override string GetPlayButtonMessage()
	{
		return "NewWorldFromTemplate";
	}

	// Token: 0x06002CAD RID: 11437 RVA: 0x001405FC File Offset: 0x0013E9FC
	public static List<string> ExpectedImageUrlsForUI()
	{
		return new List<string>
		{
			"thumbnail_url",
			"screenshot_url"
		};
	}

	// Token: 0x06002CAE RID: 11438 RVA: 0x00140628 File Offset: 0x0013EA28
	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string>
		{
			"title"
		};
	}
}
