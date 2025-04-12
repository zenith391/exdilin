using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020003E5 RID: 997
public class UIDataSourceExample : UIDataSource
{
	// Token: 0x06002C15 RID: 11285 RVA: 0x0013CF68 File Offset: 0x0013B368
	public UIDataSourceExample(UIDataManager dataManager, int size) : base(dataManager)
	{
		this.size = size;
	}

	// Token: 0x06002C16 RID: 11286 RVA: 0x0013CF94 File Offset: 0x0013B394
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

	// Token: 0x06002C17 RID: 11287 RVA: 0x0013CFF0 File Offset: 0x0013B3F0
	private IEnumerator GenerateDataCoroutine()
	{
		this.GenerateData();
		yield return new WaitForSeconds(0.3f);
		base.loadState = UIDataSource.LoadState.Loaded;
		base.NotifyListeners();
		yield break;
	}

	// Token: 0x06002C18 RID: 11288 RVA: 0x0013D00C File Offset: 0x0013B40C
	private void GenerateData()
	{
		base.ClearData();
		int num = 10000;
		string[] array = new string[]
		{
			"Dominos",
			"Donut",
			"Floaty Car",
			"Strawberry Jam",
			"TapBlock",
			"TiltRoll"
		};
		string[] array2 = new string[]
		{
			"Niblett",
			"Shrike",
			"Leslie",
			"Zaius",
			"Krispy",
			"Very Long User Name"
		};
		while (base.Data.Count < this.size && num-- > 0)
		{
			int num2 = UnityEngine.Random.Range(this.minValue, this.maxValue);
			string text = num2.ToString();
			if (!base.Keys.Contains(text))
			{
				base.Keys.Add(text);
				int num3 = num2 % array.Length;
				base.Data.Add(text, this.CreateExampleWorld(array[num3], array2[num3]));
			}
		}
	}

	// Token: 0x06002C19 RID: 11289 RVA: 0x0013D11C File Offset: 0x0013B51C
	private Dictionary<string, string> CreateExampleWorld(string title, string author)
	{
		string key = "author_username";
		string key2 = "title";
		string key3 = "description";
		string key4 = "screenshot_url";
		string key5 = "thumbnail_url";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary[key2] = title;
		dictionary[key] = author;
		string str = Application.streamingAssetsPath + "/ExampleWorlds/" + title.Replace(" ", string.Empty);
		string value = str + "_Large.jpg";
		string value2 = str + "_Small.jpg";
		dictionary[key4] = value;
		dictionary[key5] = value2;
		dictionary[key3] = "Type world description here";
		return dictionary;
	}

	// Token: 0x06002C1A RID: 11290 RVA: 0x0013D1C4 File Offset: 0x0013B5C4
	private IEnumerator NotifyListenersAfterDelay()
	{
		yield return new WaitForSeconds(0.1f);
		base.NotifyListeners();
		yield break;
	}

	// Token: 0x04002528 RID: 9512
	private int size = 30;

	// Token: 0x04002529 RID: 9513
	private int minValue = 100;

	// Token: 0x0400252A RID: 9514
	private int maxValue = 1000;
}
