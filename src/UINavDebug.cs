using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINavDebug : MonoBehaviour
{
	public Text historyT;

	public Text futureT;

	private UISceneInfo[] historyArray;

	private UISceneInfo[] futureArray;

	private void Awake()
	{
	}

	public void UpdateHistory(Stack<UISceneInfo> history, Stack<UISceneInfo> future)
	{
		historyArray = history.ToArray();
		futureArray = future.ToArray();
		historyT.text = string.Empty;
		UISceneInfo[] array = historyArray;
		foreach (UISceneInfo uISceneInfo in array)
		{
			Text text = historyT;
			text.text = text.text + uISceneInfo.path + "\n";
		}
		UISceneInfo[] array2 = futureArray;
		foreach (UISceneInfo uISceneInfo2 in array2)
		{
			Text text2 = futureT;
			text2.text = text2.text + uISceneInfo2.path + "\n";
		}
	}
}
