using System.Collections.Generic;

public class UIDataSourceWorldTemplates : UIDataSource
{
	public UIDataSourceWorldTemplates(UIDataManager dataManager)
		: base(dataManager)
	{
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearData();
		if (BWUser.currentUser == null)
		{
			base.loadState = LoadState.Failed;
			return;
		}
		if (BWUser.currentUser.worldTemplates.Count == 0)
		{
			BWLog.Info("No world templates");
		}
		for (int i = 0; i < BWUser.currentUser.worldTemplates.Count; i++)
		{
			BWWorldTemplate bWWorldTemplate = BWUser.currentUser.worldTemplates[i];
			base.Keys.Add(bWWorldTemplate.title);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("title", bWWorldTemplate.title);
			dictionary.Add("thumbnail_url", bWWorldTemplate.smallImageURL);
			dictionary.Add("screenshot_url", bWWorldTemplate.largeImageURL);
			base.Data.Add(bWWorldTemplate.title, dictionary);
		}
		base.loadState = LoadState.Loaded;
	}

	public override string GetPlayButtonMessage()
	{
		return "NewWorldFromTemplate";
	}

	public static List<string> ExpectedImageUrlsForUI()
	{
		return new List<string> { "thumbnail_url", "screenshot_url" };
	}

	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string> { "title" };
	}
}
