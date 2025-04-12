using System;
using System.Collections.Generic;

// Token: 0x020003EB RID: 1003
public class UIDataSourceModelCategories : UIDataSource
{
	// Token: 0x06002C40 RID: 11328 RVA: 0x0013DE28 File Offset: 0x0013C228
	public UIDataSourceModelCategories(UIDataManager dataManager) : base(dataManager)
	{
		base.ClearData();
		foreach (BWCategory bwcategory in BWCategory.modelCategories)
		{
			if (!bwcategory.hidden)
			{
				string text = bwcategory.categoryID.ToString();
				base.Keys.Add(text);
				base.Data.Add(text, new Dictionary<string, string>
				{
					{
						"categoryID",
						bwcategory.categoryID.ToString()
					},
					{
						"categoryName",
						bwcategory.name
					},
					{
						"imageUrl",
						bwcategory.imageURL440
					}
				});
			}
		}
		base.loadState = UIDataSource.LoadState.Loaded;
	}

	// Token: 0x06002C41 RID: 11329 RVA: 0x0013DF1C File Offset: 0x0013C31C
	public static List<string> ExpectedImageUrlsForUI()
	{
		return new List<string>
		{
			"imageUrl"
		};
	}

	// Token: 0x06002C42 RID: 11330 RVA: 0x0013DF3C File Offset: 0x0013C33C
	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string>
		{
			"categoryID",
			"categoryName"
		};
	}
}
