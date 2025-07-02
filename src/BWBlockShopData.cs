using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public static class BWBlockShopData
{
	private static Dictionary<string, BWBlockShopCategory> shopData;

	private static void LoadData()
	{
		BlockItem.LoadBlockItemsFromResources();
		TextAsset textAsset = Resources.Load<TextAsset>("panel_slots_json");
		string text = textAsset.text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
		shopData = new Dictionary<string, BWBlockShopCategory>();
		foreach (JObject item in arrayValue)
		{
			BWBlockShopCategory bWBlockShopCategory = new BWBlockShopCategory(item);
			shopData.Add(bWBlockShopCategory.internalIdentifier, bWBlockShopCategory);
		}
		Resources.UnloadAsset(textAsset);
	}

	public static List<string> GetBlockShopCategoryIdentifiers()
	{
		if (shopData == null)
		{
			LoadData();
		}
		return new List<string>(shopData.Keys);
	}

	public static string GetTitleForCategory(string categoryIdentifier)
	{
		if (shopData == null)
		{
			LoadData();
		}
		return shopData[categoryIdentifier].title;
	}

	public static List<string> GetSectionTitlesForCategory(string shopCategory)
	{
		if (shopData == null)
		{
			LoadData();
		}
		List<string> titles = new List<string>();
		shopData[shopCategory].sections.ForEach(delegate(BWBlockShopSection s)
		{
			titles.Add(s.sectionTitle);
		});
		return titles;
	}

	public static List<BlockItem> GetBlockItemsForShop(string shopCategory, string sectionTitle)
	{
		if (shopData == null)
		{
			LoadData();
		}
		foreach (BWBlockShopSection section in shopData[shopCategory].sections)
		{
			if (section.sectionTitle == sectionTitle)
			{
				return section.blockItems;
			}
		}
		return null;
	}

	public static string GetCategoryIDForTitle(string categoryTitle)
	{
		foreach (KeyValuePair<string, BWBlockShopCategory> shopDatum in shopData)
		{
			if (shopDatum.Value.title == categoryTitle)
			{
				return shopDatum.Key;
			}
		}
		return null;
	}
}
