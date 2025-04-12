using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200025D RID: 605
public class OnScreenLog
{
	// Token: 0x06001B59 RID: 7001 RVA: 0x000C6BC1 File Offset: 0x000C4FC1
	public static void Clear()
	{
		OnScreenLog.items.Clear();
	}

	// Token: 0x06001B5A RID: 7002 RVA: 0x000C6BD0 File Offset: 0x000C4FD0
	public static void Update()
	{
		if (OnScreenLog.items.Count > 0)
		{
			float time = Time.time;
			OnScreenLog.items.RemoveAll((OnScreenLogItem item) => item.startTime + item.duration < time);
		}
		if (OnScreenLog.items.Count != OnScreenLog.lastItemCount)
		{
			OnScreenLog.Refresh();
			OnScreenLog.lastItemCount = OnScreenLog.items.Count;
		}
	}

	// Token: 0x06001B5B RID: 7003 RVA: 0x000C6C40 File Offset: 0x000C5040
	public static void AddLogItem(string text, float duration = 5f, bool log = false)
	{
		OnScreenLogItem item = new OnScreenLogItem(text, duration);
		OnScreenLog.items.Insert(0, item);
		if (log)
		{
			BWLog.Info(text);
		}
		OnScreenLog.Refresh();
	}

	// Token: 0x06001B5C RID: 7004 RVA: 0x000C6C74 File Offset: 0x000C5074
	private static void Refresh()
	{
		string text = string.Empty;
		if (OnScreenLog.items.Count > 0)
		{
			for (int i = 0; i < OnScreenLog.items.Count; i++)
			{
				text += OnScreenLog.items[i].text;
				text += "\n";
			}
		}
		Blocksworld.UI.UpdateOnScreenLog(text);
	}

	// Token: 0x04001735 RID: 5941
	private static List<OnScreenLogItem> items = new List<OnScreenLogItem>();

	// Token: 0x04001736 RID: 5942
	private static int lastItemCount = 0;
}
