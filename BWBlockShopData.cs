using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x0200039E RID: 926
public static class BWBlockShopData
{
	// Token: 0x06002861 RID: 10337 RVA: 0x00129E44 File Offset: 0x00128244
	private static void LoadData()
	{
		BlockItem.LoadBlockItemsFromResources();
		TextAsset textAsset = Resources.Load<TextAsset>("panel_slots_json");
		string text = textAsset.text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
		BWBlockShopData.shopData = new Dictionary<string, BWBlockShopCategory>();
		foreach (JObject json in arrayValue)
		{
			BWBlockShopCategory bwblockShopCategory = new BWBlockShopCategory(json);
			BWBlockShopData.shopData.Add(bwblockShopCategory.internalIdentifier, bwblockShopCategory);
		}
		Resources.UnloadAsset(textAsset);
	}

	// Token: 0x06002862 RID: 10338 RVA: 0x00129EE4 File Offset: 0x001282E4
	public static List<string> GetBlockShopCategoryIdentifiers()
	{
		if (BWBlockShopData.shopData == null)
		{
			BWBlockShopData.LoadData();
		}
		return new List<string>(BWBlockShopData.shopData.Keys);
	}

	// Token: 0x06002863 RID: 10339 RVA: 0x00129F04 File Offset: 0x00128304
	public static string GetTitleForCategory(string categoryIdentifier)
	{
		if (BWBlockShopData.shopData == null)
		{
			BWBlockShopData.LoadData();
		}
		return BWBlockShopData.shopData[categoryIdentifier].title;
	}

	// Token: 0x06002864 RID: 10340 RVA: 0x00129F28 File Offset: 0x00128328
	public static List<string> GetSectionTitlesForCategory(string shopCategory)
	{
		if (BWBlockShopData.shopData == null)
		{
			BWBlockShopData.LoadData();
		}
		List<string> titles = new List<string>();
		BWBlockShopData.shopData[shopCategory].sections.ForEach(delegate(BWBlockShopSection s)
		{
			titles.Add(s.sectionTitle);
		});
		return titles;
	}

	// Token: 0x06002865 RID: 10341 RVA: 0x00129F7C File Offset: 0x0012837C
	public static List<BlockItem> GetBlockItemsForShop(string shopCategory, string sectionTitle)
	{
		if (BWBlockShopData.shopData == null)
		{
			BWBlockShopData.LoadData();
		}
		foreach (BWBlockShopSection bwblockShopSection in BWBlockShopData.shopData[shopCategory].sections)
		{
			if (bwblockShopSection.sectionTitle == sectionTitle)
			{
				return bwblockShopSection.blockItems;
			}
		}
		return null;
	}

	// Token: 0x06002866 RID: 10342 RVA: 0x0012A00C File Offset: 0x0012840C
	public static string GetCategoryIDForTitle(string categoryTitle)
	{
		foreach (KeyValuePair<string, BWBlockShopCategory> keyValuePair in BWBlockShopData.shopData)
		{
			if (keyValuePair.Value.title == categoryTitle)
			{
				return keyValuePair.Key;
			}
		}
		return null;
	}

	// Token: 0x04002351 RID: 9041
	private static Dictionary<string, BWBlockShopCategory> shopData;
}
