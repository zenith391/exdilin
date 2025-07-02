using System.Collections.Generic;
using UnityEngine;

public class OnScreenLog
{
	private static List<OnScreenLogItem> items = new List<OnScreenLogItem>();

	private static int lastItemCount = 0;

	public static void Clear()
	{
		items.Clear();
	}

	public static void Update()
	{
		if (items.Count > 0)
		{
			float time = Time.time;
			items.RemoveAll((OnScreenLogItem item) => item.startTime + item.duration < time);
		}
		if (items.Count != lastItemCount)
		{
			Refresh();
			lastItemCount = items.Count;
		}
	}

	public static void AddLogItem(string text, float duration = 5f, bool log = false)
	{
		OnScreenLogItem item = new OnScreenLogItem(text, duration);
		items.Insert(0, item);
		if (log)
		{
			BWLog.Info(text);
		}
		Refresh();
	}

	private static void Refresh()
	{
		string text = string.Empty;
		if (items.Count > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				text += items[i].text;
				text += "\n";
			}
		}
		Blocksworld.UI.UpdateOnScreenLog(text);
	}
}
