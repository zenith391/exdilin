using System.Collections.Generic;

public class UIDataSourceBlockShopSectionTitles : UIDataSource
{
	public UIDataSourceBlockShopSectionTitles(UIDataManager manager, string shopCategoryID)
		: base(manager)
	{
		ClearData();
		int num = 1;
		foreach (string item in BWBlockShopData.GetSectionTitlesForCategory(shopCategoryID))
		{
			string text = num.ToString();
			base.Keys.Add(item);
			base.Data.Add(item, new Dictionary<string, string> { { "sectionTitle", item } });
			num++;
		}
		base.loadState = LoadState.Loaded;
	}
}
