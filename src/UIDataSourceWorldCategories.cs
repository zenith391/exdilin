using System.Collections.Generic;

public class UIDataSourceWorldCategories : UIDataSource
{
	public UIDataSourceWorldCategories(UIDataManager dataManager)
		: base(dataManager)
	{
		ClearData();
		foreach (BWCategory item in BWCategory.worldCategoriesForPlayMenu)
		{
			if (!item.hidden)
			{
				string text = item.categoryID.ToString();
				base.Keys.Add(text);
				base.Data.Add(text, new Dictionary<string, string>
				{
					{
						"categoryID",
						item.categoryID.ToString()
					},
					{ "categoryName", item.name },
					{ "imageUrl", item.imageURL440 }
				});
			}
		}
		base.loadState = LoadState.Loaded;
	}

	public static List<string> ExpectedImageUrlsForUI()
	{
		return new List<string> { "imageUrl" };
	}

	public static List<string> ExpectedDataKeysForUI()
	{
		return new List<string> { "categoryID", "categoryName" };
	}
}
