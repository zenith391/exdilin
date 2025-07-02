using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDataSourceExample : UIDataSource
{
	private int size = 30;

	private int minValue = 100;

	private int maxValue = 1000;

	public UIDataSourceExample(UIDataManager dataManager, int size)
		: base(dataManager)
	{
		this.size = size;
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
		yield return new WaitForSeconds(0.3f);
		base.loadState = LoadState.Loaded;
		NotifyListeners();
	}

	private void GenerateData()
	{
		ClearData();
		int num = 10000;
		string[] array = new string[6] { "Dominos", "Donut", "Floaty Car", "Strawberry Jam", "TapBlock", "TiltRoll" };
		string[] array2 = new string[6] { "Niblett", "Shrike", "Leslie", "Zaius", "Krispy", "Very Long User Name" };
		while (base.Data.Count < size && num-- > 0)
		{
			int num2 = Random.Range(minValue, maxValue);
			string text = num2.ToString();
			if (!base.Keys.Contains(text))
			{
				base.Keys.Add(text);
				int num3 = num2 % array.Length;
				base.Data.Add(text, CreateExampleWorld(array[num3], array2[num3]));
			}
		}
	}

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
		string text = Application.streamingAssetsPath + "/ExampleWorlds/" + title.Replace(" ", string.Empty);
		string value = text + "_Large.jpg";
		string value2 = text + "_Small.jpg";
		dictionary[key4] = value;
		dictionary[key5] = value2;
		dictionary[key3] = "Type world description here";
		return dictionary;
	}

	private IEnumerator NotifyListenersAfterDelay()
	{
		yield return new WaitForSeconds(0.1f);
		NotifyListeners();
	}
}
