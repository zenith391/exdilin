using System.Collections.Generic;

public class UIDataSourceModelCategories : UIDataSource
{
	public UIDataSourceModelCategories(UIDataManager dataManager)
		: base(dataManager)
	{
		ClearData();
		foreach (BWCategory modelCategory in BWCategory.modelCategories)
		{
			if (!modelCategory.hidden)
			{
				string text = modelCategory.categoryID.ToString();
				base.Keys.Add(text);
				base.Data.Add(text, new Dictionary<string, string>
				{
					{
						"categoryID",
						modelCategory.categoryID.ToString()
					},
					{ "categoryName", modelCategory.name },
					{ "imageUrl", modelCategory.imageURL440 }
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
