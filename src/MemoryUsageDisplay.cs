using System;
using System.Collections.Generic;
using UnityEngine;

public class MemoryUsageDisplay : MonoBehaviour
{
	private class TypeCount : IComparable
	{
		public string typeName;

		public int count;

		public int CompareTo(object obj)
		{
			TypeCount typeCount = obj as TypeCount;
			return -1 * count.CompareTo(typeCount.count);
		}
	}

	private List<int> totalObjectCountHistory = new List<int>();

	private List<int> gameObjectCountHistory = new List<int>();

	private List<int> materialCountHistory = new List<int>();

	private List<int> textureCountHistory = new List<int>();

	private List<int> meshCountHistory = new List<int>();

	private List<int> componentCountHistory = new List<int>();

	private List<int> otherCountHistory = new List<int>();

	private List<int> totalMemoryHistory = new List<int>();

	private List<List<TypeCount>> genericTypeHistory = new List<List<TypeCount>>();

	private const int maxTypesToDisplay = 8;

	private const int maxTypesToTrack = 50;

	private bool includeScene = true;

	private bool includeResources;

	private int updateFreq = 1;

	private bool autoUpdate;

	private float nextUpdate = -1f;

	private bool haveData;

	private static MemoryUsageDisplay _instance;

	private int totalObjectCount;

	private int gameObjectCount;

	private int materialCount;

	private int meshCount;

	private int componentCount;

	private int textureCount;

	private int otherCount;

	private Dictionary<string, int> typeDict = new Dictionary<string, int>();

	public static MemoryUsageDisplay Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Create();
			}
			return _instance;
		}
	}

	private static MemoryUsageDisplay Create()
	{
		GameObject gameObject = new GameObject();
		MemoryUsageDisplay memoryUsageDisplay = gameObject.AddComponent<MemoryUsageDisplay>();
		memoryUsageDisplay.enabled = false;
		return memoryUsageDisplay;
	}

	public void GetCurrentMemoryUsage()
	{
		totalObjectCount = 0;
		gameObjectCount = 0;
		materialCount = 0;
		meshCount = 0;
		componentCount = 0;
		textureCount = 0;
		otherCount = 0;
		typeDict = new Dictionary<string, int>();
		HashSet<UnityEngine.Object> hashSet = new HashSet<UnityEngine.Object>();
		if (includeScene)
		{
			UnityEngine.Object[] objectArray = UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
			GetMemoryUseFromObjects(objectArray, hashSet);
		}
		if (includeResources)
		{
			UnityEngine.Object[] objectArray2 = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
			GetMemoryUseFromObjects(objectArray2, hashSet);
		}
		totalObjectCount = hashSet.Count;
		totalObjectCountHistory.Insert(0, totalObjectCount);
		gameObjectCountHistory.Insert(0, gameObjectCount);
		materialCountHistory.Insert(0, materialCount);
		meshCountHistory.Insert(0, meshCount);
		componentCountHistory.Insert(0, componentCount);
		textureCountHistory.Insert(0, textureCount);
		otherCountHistory.Insert(0, otherCount);
		List<TypeCount> list = new List<TypeCount>();
		int num = 0;
		foreach (KeyValuePair<string, int> item in typeDict)
		{
			TypeCount typeCount = new TypeCount
			{
				typeName = item.Key,
				count = item.Value
			};
			if (list.Count < 50 || typeCount.count > num)
			{
				list.Add(typeCount);
				list.Sort();
				num = list[0].count;
			}
		}
		genericTypeHistory.Insert(0, list);
		haveData = true;
	}

	private void GetMemoryUseFromObjects(UnityEngine.Object[] objectArray, HashSet<UnityEngine.Object> objectSet)
	{
		foreach (UnityEngine.Object obj in objectArray)
		{
			if (objectSet.Contains(obj))
			{
				continue;
			}
			objectSet.Add(obj);
			if (obj is Texture2D || obj.GetType().IsSubclassOf(typeof(Texture)))
			{
				textureCount++;
			}
			else if (obj is GameObject)
			{
				gameObjectCount++;
			}
			else if (obj is Material)
			{
				materialCount++;
			}
			else if (obj is Mesh)
			{
				meshCount++;
			}
			else if (typeDict.Count <= 50)
			{
				string key = obj.GetType().ToString();
				int value = 0;
				if (!typeDict.TryGetValue(key, out value))
				{
					typeDict[key] = 1;
				}
				else
				{
					typeDict[key] = value + 1;
				}
			}
			else
			{
				otherCount++;
			}
		}
	}

	public void Show()
	{
		if (!haveData)
		{
			SetBaseline();
		}
		base.enabled = true;
	}

	public void Hide()
	{
		base.enabled = false;
	}

	private void OnEnable()
	{
		nextUpdate = Time.time + (float)updateFreq;
	}

	private void OnDisable()
	{
		nextUpdate = -1f;
	}

	private void GCSize()
	{
		totalMemoryHistory.Insert(0, (int)GC.GetTotalMemory(forceFullCollection: true));
	}

	private void SetBaseline()
	{
		totalObjectCountHistory = new List<int>();
		gameObjectCountHistory = new List<int>();
		materialCountHistory = new List<int>();
		textureCountHistory = new List<int>();
		meshCountHistory = new List<int>();
		componentCountHistory = new List<int>();
		otherCountHistory = new List<int>();
		totalMemoryHistory = new List<int>();
		genericTypeHistory = new List<List<TypeCount>>();
		GetCurrentMemoryUsage();
	}

	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.MEMORY_USAGE))
		{
			GetCurrentMemoryUsage();
		}
		if (autoUpdate)
		{
			if (nextUpdate < 0f)
			{
				GetCurrentMemoryUsage();
				nextUpdate = Time.time + (float)updateFreq;
			}
			if (Time.time >= nextUpdate)
			{
				GetCurrentMemoryUsage();
				nextUpdate = Time.time + (float)updateFreq;
			}
		}
	}

	public void OnGUI()
	{
		GUILayout.BeginArea(new Rect(100f, 100f, 1000f, 700f));
		ShowCount("Total objects", totalObjectCountHistory);
		ShowCount("Game Objects", gameObjectCountHistory);
		ShowCount("Materials", materialCountHistory);
		ShowCount("Textures", textureCountHistory);
		ShowCount("Components", componentCountHistory);
		ShowCount("Meshes", meshCountHistory);
		for (int i = 0; i < 8; i++)
		{
			if (genericTypeHistory.Count == 0)
			{
				continue;
			}
			List<int> list = new List<int>();
			if (genericTypeHistory[0].Count <= i)
			{
				continue;
			}
			TypeCount typeCount = genericTypeHistory[0][i];
			list.Add(typeCount.count);
			if (genericTypeHistory.Count > 1)
			{
				for (int j = 0; j < genericTypeHistory[1].Count; j++)
				{
					TypeCount typeCount2 = genericTypeHistory[1][j];
					if (typeCount2.typeName == typeCount.typeName)
					{
						list.Add(typeCount2.count);
					}
				}
				List<TypeCount> list2 = genericTypeHistory[genericTypeHistory.Count - 1];
				for (int k = 0; k < list2.Count; k++)
				{
					TypeCount typeCount3 = list2[k];
					if (typeCount3.typeName == typeCount.typeName)
					{
						list.Add(typeCount3.count);
					}
				}
			}
			ShowCount(typeCount.typeName, list);
		}
		ShowCount("Other", otherCountHistory);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Update", GUILayout.Width(50f), GUILayout.Height(40f)))
		{
			GetCurrentMemoryUsage();
		}
		if (GUILayout.Button("SetBaseline", GUILayout.Width(200f), GUILayout.Height(40f)))
		{
			SetBaseline();
		}
		autoUpdate = GUILayout.Toggle(autoUpdate, "auto update", GUILayout.Width(100f), GUILayout.Height(40f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		includeScene = GUILayout.Toggle(includeScene, "include scene", GUILayout.Width(100f), GUILayout.Height(40f));
		includeResources = GUILayout.Toggle(includeResources, "include resources", GUILayout.Width(100f), GUILayout.Height(40f));
		GUILayout.EndHorizontal();
		GUILayout.Label(string.Empty);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("GC size", GUILayout.Width(80f), GUILayout.Height(40f)))
		{
			GCSize();
		}
		if (GUILayout.Button("Unload Unused Assets", GUILayout.Width(200f), GUILayout.Height(40f)))
		{
			Resources.UnloadUnusedAssets();
		}
		GUILayout.EndHorizontal();
		ShowCount("Usage", totalMemoryHistory);
		GUILayout.EndArea();
	}

	private void ShowCount(string label, List<int> history)
	{
		if (history.Count != 0)
		{
			GUILayout.BeginHorizontal(GUILayout.MaxWidth(700f));
			GUILayout.Label(label, GUILayout.Width(250f));
			GUILayout.Label(" : " + history[0], GUILayout.Width(100f));
			if (history.Count > 1)
			{
				int change = history[0] - history[1];
				ShowChange(change);
				int change2 = history[0] - history[history.Count - 1];
				ShowChange(change2);
			}
			GUILayout.EndHorizontal();
		}
	}

	private void ShowChange(int change)
	{
		string text = string.Empty;
		if (change > 0)
		{
			GUI.contentColor = Color.red;
			text = "+";
		}
		else if (change < 0)
		{
			GUI.contentColor = Color.green;
		}
		GUILayout.Label(" " + text + change, GUILayout.Width(80f));
		GUI.contentColor = Color.white;
	}
}
