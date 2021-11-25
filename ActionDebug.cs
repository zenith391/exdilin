using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class ActionDebug
{
	// Token: 0x060001C4 RID: 452 RVA: 0x0000A380 File Offset: 0x00008780
	public static void IncrementCategoryCounter(string category, int increment = 1)
	{
		int num = 0;
		if (ActionDebug.categoryCounters.ContainsKey(category))
		{
			num = ActionDebug.categoryCounters[category];
		}
		num += increment;
		ActionDebug.categoryCounters[category] = num;
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x0000A3BC File Offset: 0x000087BC
	public static void AddMessage(string category, string message, bool unique = false)
	{
		List<string> list;
		if (ActionDebug.categoryMessages.ContainsKey(category))
		{
			list = ActionDebug.categoryMessages[category];
		}
		else
		{
			list = new List<string>();
			ActionDebug.categoryMessages[category] = list;
		}
		if (!unique || !list.Contains(message))
		{
			list.Add(message);
		}
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x0000A418 File Offset: 0x00008818
	public static void AddObjectLabel(GameObject go, string label)
	{
		List<string> list;
		if (!ActionDebug.objectLabels.TryGetValue(go, out list))
		{
			list = new List<string>();
			ActionDebug.objectLabels[go] = list;
		}
		list.Add(label);
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x0000A450 File Offset: 0x00008850
	public static void Init()
	{
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x0000A452 File Offset: 0x00008852
	public static void Reset()
	{
		ActionDebug.categoryMessages.Clear();
		ActionDebug.categoryCounters.Clear();
		ActionDebug.objectLabels.Clear();
	}

	// Token: 0x040001C7 RID: 455
	private static Dictionary<string, List<string>> categoryMessages = new Dictionary<string, List<string>>();

	// Token: 0x040001C8 RID: 456
	private static Dictionary<string, int> categoryCounters = new Dictionary<string, int>();

	// Token: 0x040001C9 RID: 457
	private static Dictionary<GameObject, List<string>> objectLabels = new Dictionary<GameObject, List<string>>();
}
