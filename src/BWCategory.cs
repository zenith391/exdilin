using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020003A1 RID: 929
internal class BWCategory
{
	// Token: 0x06002869 RID: 10345 RVA: 0x0012A21C File Offset: 0x0012861C
	internal BWCategory(JObject json)
	{
		this.categoryID = BWJsonHelpers.PropertyIfExists(this.categoryID, "id", json);
		this.hidden = BWJsonHelpers.PropertyIfExists(this.hidden, "hidden", json);
		this.index = BWJsonHelpers.PropertyIfExists(this.index, "index", json);
		this.name = BWJsonHelpers.PropertyIfExists(this.name, "name", json);
		this.imageURL440 = BWJsonHelpers.PropertyIfExists(this.imageURL440, "image_urls_for_sizes", "440x440", json);
		this.imageURL220 = BWJsonHelpers.PropertyIfExists(this.imageURL220, "image_urls_for_sizes", "220x220", json);
	}

	// Token: 0x1700019D RID: 413
	// (get) Token: 0x0600286A RID: 10346 RVA: 0x0012A2C3 File Offset: 0x001286C3
	// (set) Token: 0x0600286B RID: 10347 RVA: 0x0012A2CA File Offset: 0x001286CA
	internal static List<BWCategory> modelCategories { get; private set; }

	// Token: 0x1700019E RID: 414
	// (get) Token: 0x0600286C RID: 10348 RVA: 0x0012A2D2 File Offset: 0x001286D2
	// (set) Token: 0x0600286D RID: 10349 RVA: 0x0012A2D9 File Offset: 0x001286D9
	internal static List<BWCategory> worldCategories { get; private set; }

	// Token: 0x1700019F RID: 415
	// (get) Token: 0x0600286E RID: 10350 RVA: 0x0012A2E1 File Offset: 0x001286E1
	// (set) Token: 0x0600286F RID: 10351 RVA: 0x0012A2E8 File Offset: 0x001286E8
	internal static List<BWCategory> visibleWorldCategories { get; private set; }

	// Token: 0x170001A0 RID: 416
	// (get) Token: 0x06002870 RID: 10352 RVA: 0x0012A2F0 File Offset: 0x001286F0
	// (set) Token: 0x06002871 RID: 10353 RVA: 0x0012A2F7 File Offset: 0x001286F7
	internal static List<BWCategory> worldCategoriesForPlayMenu { get; private set; }

	// Token: 0x170001A1 RID: 417
	// (get) Token: 0x06002872 RID: 10354 RVA: 0x0012A2FF File Offset: 0x001286FF
	// (set) Token: 0x06002873 RID: 10355 RVA: 0x0012A307 File Offset: 0x00128707
	internal int categoryID { get; private set; }

	// Token: 0x170001A2 RID: 418
	// (get) Token: 0x06002874 RID: 10356 RVA: 0x0012A310 File Offset: 0x00128710
	// (set) Token: 0x06002875 RID: 10357 RVA: 0x0012A318 File Offset: 0x00128718
	internal bool hidden { get; private set; }

	// Token: 0x170001A3 RID: 419
	// (get) Token: 0x06002876 RID: 10358 RVA: 0x0012A321 File Offset: 0x00128721
	// (set) Token: 0x06002877 RID: 10359 RVA: 0x0012A329 File Offset: 0x00128729
	internal int index { get; private set; }

	// Token: 0x170001A4 RID: 420
	// (get) Token: 0x06002878 RID: 10360 RVA: 0x0012A332 File Offset: 0x00128732
	// (set) Token: 0x06002879 RID: 10361 RVA: 0x0012A33A File Offset: 0x0012873A
	internal string name { get; private set; }

	// Token: 0x170001A5 RID: 421
	// (get) Token: 0x0600287A RID: 10362 RVA: 0x0012A343 File Offset: 0x00128743
	// (set) Token: 0x0600287B RID: 10363 RVA: 0x0012A34B File Offset: 0x0012874B
	internal string imageURL440 { get; private set; }

	// Token: 0x170001A6 RID: 422
	// (get) Token: 0x0600287C RID: 10364 RVA: 0x0012A354 File Offset: 0x00128754
	// (set) Token: 0x0600287D RID: 10365 RVA: 0x0012A35C File Offset: 0x0012875C
	internal string imageURL220 { get; private set; }

	// Token: 0x0600287E RID: 10366 RVA: 0x0012A368 File Offset: 0x00128768
	internal static void LoadContentCategories(JObject ccJson)
	{
		BWCategory.modelCategories = new List<BWCategory>();
		BWCategory.worldCategories = new List<BWCategory>();
		BWCategory.visibleWorldCategories = new List<BWCategory>();
		BWJsonHelpers.AddForEachInArray<BWCategory>(BWCategory.modelCategories, "model_categories", ccJson, (JObject json) => new BWCategory(json));
		BWJsonHelpers.AddForEachInArray<BWCategory>(BWCategory.worldCategories, "world_categories", ccJson, (JObject json) => new BWCategory(json));
		BWCategory.modelCategoryByID = new Dictionary<int, BWCategory>();
		BWCategory.worldCategoryByID = new Dictionary<int, BWCategory>();
		BWCategory.modelCategories.ForEach(delegate(BWCategory mc)
		{
			BWCategory.modelCategoryByID[mc.categoryID] = mc;
		});
		BWCategory.worldCategories.ForEach(delegate(BWCategory wc)
		{
			if (!wc.hidden)
			{
				BWCategory.visibleWorldCategories.Add(wc);
			}
			if (wc.categoryID == 1)
			{
				BWCategory.blocksworldOfficialCategory = wc;
			}
			else if (wc.categoryID == 5)
			{
				BWCategory.featuredCategory = wc;
			}
			else if (wc.categoryID == 3)
			{
				BWCategory.leaderboardCategory = wc;
			}
			BWCategory.worldCategoryByID[wc.categoryID] = wc;
		});
		BWCategory.worldCategoriesForPlayMenu = new List<BWCategory>();
		BWCategory.visibleWorldCategories.ForEach(delegate(BWCategory wc)
		{
			if (wc != BWCategory.featuredCategory && wc != BWCategory.blocksworldOfficialCategory)
			{
				BWCategory.worldCategoriesForPlayMenu.Add(wc);
			}
		});
		BWCategory.worldCategoriesForPlayMenu.Sort((BWCategory x, BWCategory y) => x.index.CompareTo(y.index));
	}

	// Token: 0x0600287F RID: 10367 RVA: 0x0012A4A7 File Offset: 0x001288A7
	internal static string GetModelCategoryName(int catID)
	{
		if (BWCategory.modelCategoryByID != null && BWCategory.modelCategoryByID.ContainsKey(catID))
		{
			return BWCategory.modelCategoryByID[catID].name;
		}
		return string.Empty;
	}

	// Token: 0x06002880 RID: 10368 RVA: 0x0012A4D9 File Offset: 0x001288D9
	internal static string GetWorldCategoryName(int catID)
	{
		if (BWCategory.worldCategoryByID != null && BWCategory.worldCategoryByID.ContainsKey(catID))
		{
			return BWCategory.worldCategoryByID[catID].name;
		}
		return string.Empty;
	}

	// Token: 0x0400235B RID: 9051
	internal static BWCategory blocksworldOfficialCategory;

	// Token: 0x0400235C RID: 9052
	internal static BWCategory leaderboardCategory;

	// Token: 0x0400235D RID: 9053
	internal static BWCategory featuredCategory;

	// Token: 0x0400235E RID: 9054
	private static Dictionary<int, BWCategory> modelCategoryByID;

	// Token: 0x0400235F RID: 9055
	private static Dictionary<int, BWCategory> worldCategoryByID;
}
