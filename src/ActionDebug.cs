using System.Collections.Generic;
using UnityEngine;

public class ActionDebug
{
	private static Dictionary<string, List<string>> categoryMessages = new Dictionary<string, List<string>>();

	private static Dictionary<string, int> categoryCounters = new Dictionary<string, int>();

	private static Dictionary<GameObject, List<string>> objectLabels = new Dictionary<GameObject, List<string>>();

	public static void IncrementCategoryCounter(string category, int increment = 1)
	{
		int num = 0;
		if (categoryCounters.ContainsKey(category))
		{
			num = categoryCounters[category];
		}
		num += increment;
		categoryCounters[category] = num;
	}

	public static void AddMessage(string category, string message, bool unique = false)
	{
		List<string> list;
		if (categoryMessages.ContainsKey(category))
		{
			list = categoryMessages[category];
		}
		else
		{
			list = new List<string>();
			categoryMessages[category] = list;
		}
		if (!unique || !list.Contains(message))
		{
			list.Add(message);
		}
	}

	public static void AddObjectLabel(GameObject go, string label)
	{
		if (!objectLabels.TryGetValue(go, out var value))
		{
			value = new List<string>();
			objectLabels[go] = value;
		}
		value.Add(label);
	}

	public static void Init()
	{
	}

	public static void Reset()
	{
		categoryMessages.Clear();
		categoryCounters.Clear();
		objectLabels.Clear();
	}
}
