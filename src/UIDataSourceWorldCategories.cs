using System;
using System.Collections.Generic;

// Token: 0x020003F7 RID: 1015
public class UIDataSourceWorldCategories : UIDataSource
{
	// Token: 0x06002C87 RID: 11399 RVA: 0x0013F850 File Offset: 0x0013DC50
	public UIDataSourceWorldCategories(UIDataManager dataManager) : base(dataManager)
	{
		base.ClearData();
		foreach (BWCategory bwcategory in BWCategory.worldCategoriesForPlayMenu)
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

	// Token: 0x06002C88 RID: 11400 RVA: 0x0013F944 File Offset: 0x0013DD44
	public static List<string> ExpectedImageUrlsForUI()
	{
		return new List<string>
		{
			"imageUrl"
		};
	}

	// Token: 0x06002C89 RID: 11401 RVA: 0x0013F964 File Offset: 0x0013DD64
	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string>
		{
			"categoryID",
			"categoryName"
		};
	}
}
