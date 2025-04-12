using System;
using System.Collections.Generic;

// Token: 0x020003E2 RID: 994
public class UIDataSourceBlockShopSectionTitles : UIDataSource
{
	// Token: 0x06002C0C RID: 11276 RVA: 0x0013CCC4 File Offset: 0x0013B0C4
	public UIDataSourceBlockShopSectionTitles(UIDataManager manager, string shopCategoryID) : base(manager)
	{
		base.ClearData();
		int num = 1;
		foreach (string text in BWBlockShopData.GetSectionTitlesForCategory(shopCategoryID))
		{
			string text2 = num.ToString();
			base.Keys.Add(text);
			base.Data.Add(text, new Dictionary<string, string>
			{
				{
					"sectionTitle",
					text
				}
			});
			num++;
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}
}
