using System.Collections.Generic;
using SimpleJSON;

internal class BWCategory
{
	internal static BWCategory blocksworldOfficialCategory;

	internal static BWCategory leaderboardCategory;

	internal static BWCategory featuredCategory;

	private static Dictionary<int, BWCategory> modelCategoryByID;

	private static Dictionary<int, BWCategory> worldCategoryByID;

	internal static List<BWCategory> modelCategories { get; private set; }

	internal static List<BWCategory> worldCategories { get; private set; }

	internal static List<BWCategory> visibleWorldCategories { get; private set; }

	internal static List<BWCategory> worldCategoriesForPlayMenu { get; private set; }

	internal int categoryID { get; private set; }

	internal bool hidden { get; private set; }

	internal int index { get; private set; }

	internal string name { get; private set; }

	internal string imageURL440 { get; private set; }

	internal string imageURL220 { get; private set; }

	internal BWCategory(JObject json)
	{
		categoryID = BWJsonHelpers.PropertyIfExists(categoryID, "id", json);
		hidden = BWJsonHelpers.PropertyIfExists(hidden, "hidden", json);
		index = BWJsonHelpers.PropertyIfExists(index, "index", json);
		name = BWJsonHelpers.PropertyIfExists(name, "name", json);
		imageURL440 = BWJsonHelpers.PropertyIfExists(imageURL440, "image_urls_for_sizes", "440x440", json);
		imageURL220 = BWJsonHelpers.PropertyIfExists(imageURL220, "image_urls_for_sizes", "220x220", json);
	}

	internal static void LoadContentCategories(JObject ccJson)
	{
		modelCategories = new List<BWCategory>();
		worldCategories = new List<BWCategory>();
		visibleWorldCategories = new List<BWCategory>();
		BWJsonHelpers.AddForEachInArray(modelCategories, "model_categories", ccJson, (JObject json) => new BWCategory(json));
		BWJsonHelpers.AddForEachInArray(worldCategories, "world_categories", ccJson, (JObject json) => new BWCategory(json));
		modelCategoryByID = new Dictionary<int, BWCategory>();
		worldCategoryByID = new Dictionary<int, BWCategory>();
		modelCategories.ForEach(delegate(BWCategory mc)
		{
			modelCategoryByID[mc.categoryID] = mc;
		});
		worldCategories.ForEach(delegate(BWCategory wc)
		{
			if (!wc.hidden)
			{
				visibleWorldCategories.Add(wc);
			}
			if (wc.categoryID == 1)
			{
				blocksworldOfficialCategory = wc;
			}
			else if (wc.categoryID == 5)
			{
				featuredCategory = wc;
			}
			else if (wc.categoryID == 3)
			{
				leaderboardCategory = wc;
			}
			worldCategoryByID[wc.categoryID] = wc;
		});
		worldCategoriesForPlayMenu = new List<BWCategory>();
		visibleWorldCategories.ForEach(delegate(BWCategory wc)
		{
			if (wc != featuredCategory && wc != blocksworldOfficialCategory)
			{
				worldCategoriesForPlayMenu.Add(wc);
			}
		});
		worldCategoriesForPlayMenu.Sort((BWCategory x, BWCategory y) => x.index.CompareTo(y.index));
	}

	internal static string GetModelCategoryName(int catID)
	{
		if (modelCategoryByID != null && modelCategoryByID.ContainsKey(catID))
		{
			return modelCategoryByID[catID].name;
		}
		return string.Empty;
	}

	internal static string GetWorldCategoryName(int catID)
	{
		if (worldCategoryByID != null && worldCategoryByID.ContainsKey(catID))
		{
			return worldCategoryByID[catID].name;
		}
		return string.Empty;
	}
}
