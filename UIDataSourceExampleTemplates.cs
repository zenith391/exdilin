using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003E6 RID: 998
public class UIDataSourceExampleTemplates : UIDataSource
{
	// Token: 0x06002C1B RID: 11291 RVA: 0x0013D326 File Offset: 0x0013B726
	public UIDataSourceExampleTemplates(UIDataManager dataManager) : base(dataManager)
	{
	}

	// Token: 0x06002C1C RID: 11292 RVA: 0x0013D330 File Offset: 0x0013B730
	public override void Refresh()
	{
		Debug.Log("Refreshing example data");
		base.loadState = UIDataSource.LoadState.Loading;
		if (Application.isEditor && !Application.isPlaying)
		{
			this.GenerateData();
			base.loadState = UIDataSource.LoadState.Loaded;
			base.NotifyListeners();
		}
		else
		{
			this.dataManager.StartCoroutine(this.GenerateDataCoroutine());
		}
	}

	// Token: 0x06002C1D RID: 11293 RVA: 0x0013D38C File Offset: 0x0013B78C
	private IEnumerator GenerateDataCoroutine()
	{
		this.GenerateData();
		yield return new WaitForSeconds(0.1f);
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
		yield break;
	}

	// Token: 0x06002C1E RID: 11294 RVA: 0x0013D3A8 File Offset: 0x0013B7A8
	private void GenerateData()
	{
		base.ClearData();
		string[] array = new string[]
		{
			"Artic Circle",
			"Asteroid Alley",
			"Blockster Bay",
			"Blocktropolis",
			"Endless Expanse",
			"Havoc Island",
			"Ice Moon Alpha",
			"Kingdom of Blockshire",
			"Lost Isles",
			"The Land of Oz"
		};
		for (int i = 0; i < array.Length; i++)
		{
			string text = (100 + i).ToString();
			base.Keys.Add(text);
			base.Data.Add(text, this.CreateExampleTemplate(array[i]));
		}
	}

	// Token: 0x06002C1F RID: 11295 RVA: 0x0013D468 File Offset: 0x0013B868
	private Dictionary<string, string> CreateExampleTemplate(string title)
	{
		string key = "title";
		string key2 = "thumbnail_url";
		string key3 = "screenshot_url";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary[key] = title;
		string value = Application.streamingAssetsPath + "/WorldTemplateImages/" + title.Replace(" ", string.Empty) + ".png";
		dictionary[key2] = value;
		dictionary[key3] = value;
		return dictionary;
	}

	// Token: 0x06002C20 RID: 11296 RVA: 0x0013D4D0 File Offset: 0x0013B8D0
	private IEnumerator NotifyListenersAfterDelay()
	{
		yield return new WaitForSeconds(0.1f);
		base.NotifyListeners();
		yield break;
	}
}
