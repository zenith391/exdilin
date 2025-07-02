using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDataSourceExampleTemplates : UIDataSource
{
	public UIDataSourceExampleTemplates(UIDataManager dataManager)
		: base(dataManager)
	{
	}

	public override void Refresh()
	{
		Debug.Log("Refreshing example data");
		base.loadState = LoadState.Loading;
		if (Application.isEditor && !Application.isPlaying)
		{
			GenerateData();
			base.loadState = LoadState.Loaded;
			NotifyListeners();
		}
		else
		{
			dataManager.StartCoroutine(GenerateDataCoroutine());
		}
	}

	private IEnumerator GenerateDataCoroutine()
	{
		GenerateData();
		yield return new WaitForSeconds(0.1f);
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	private void GenerateData()
	{
		ClearData();
		string[] array = new string[10] { "Artic Circle", "Asteroid Alley", "Blockster Bay", "Blocktropolis", "Endless Expanse", "Havoc Island", "Ice Moon Alpha", "Kingdom of Blockshire", "Lost Isles", "The Land of Oz" };
		for (int i = 0; i < array.Length; i++)
		{
			string text = (100 + i).ToString();
			base.Keys.Add(text);
			base.Data.Add(text, CreateExampleTemplate(array[i]));
		}
	}

	private Dictionary<string, string> CreateExampleTemplate(string title)
	{
		string key = "title";
		string key2 = "thumbnail_url";
		string key3 = "screenshot_url";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary[key] = title;
		string value = (dictionary[key2] = Application.streamingAssetsPath + "/WorldTemplateImages/" + title.Replace(" ", string.Empty) + ".png");
		dictionary[key3] = value;
		return dictionary;
	}

	private IEnumerator NotifyListenersAfterDelay()
	{
		yield return new WaitForSeconds(0.1f);
		NotifyListeners();
	}
}
