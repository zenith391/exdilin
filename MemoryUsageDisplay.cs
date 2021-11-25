using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000364 RID: 868
public class MemoryUsageDisplay : MonoBehaviour
{
	// Token: 0x17000178 RID: 376
	// (get) Token: 0x06002684 RID: 9860 RVA: 0x0011C23E File Offset: 0x0011A63E
	public static MemoryUsageDisplay Instance
	{
		get
		{
			if (MemoryUsageDisplay._instance == null)
			{
				MemoryUsageDisplay._instance = MemoryUsageDisplay.Create();
			}
			return MemoryUsageDisplay._instance;
		}
	}

	// Token: 0x06002685 RID: 9861 RVA: 0x0011C260 File Offset: 0x0011A660
	private static MemoryUsageDisplay Create()
	{
		GameObject gameObject = new GameObject();
		MemoryUsageDisplay memoryUsageDisplay = gameObject.AddComponent<MemoryUsageDisplay>();
		memoryUsageDisplay.enabled = false;
		return memoryUsageDisplay;
	}

	// Token: 0x06002686 RID: 9862 RVA: 0x0011C284 File Offset: 0x0011A684
	public void GetCurrentMemoryUsage()
	{
		this.totalObjectCount = 0;
		this.gameObjectCount = 0;
		this.materialCount = 0;
		this.meshCount = 0;
		this.componentCount = 0;
		this.textureCount = 0;
		this.otherCount = 0;
		this.typeDict = new Dictionary<string, int>();
		HashSet<UnityEngine.Object> hashSet = new HashSet<UnityEngine.Object>();
		if (this.includeScene)
		{
			UnityEngine.Object[] objectArray = UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
			this.GetMemoryUseFromObjects(objectArray, hashSet);
		}
		if (this.includeResources)
		{
			UnityEngine.Object[] objectArray2 = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
			this.GetMemoryUseFromObjects(objectArray2, hashSet);
		}
		this.totalObjectCount = hashSet.Count;
		this.totalObjectCountHistory.Insert(0, this.totalObjectCount);
		this.gameObjectCountHistory.Insert(0, this.gameObjectCount);
		this.materialCountHistory.Insert(0, this.materialCount);
		this.meshCountHistory.Insert(0, this.meshCount);
		this.componentCountHistory.Insert(0, this.componentCount);
		this.textureCountHistory.Insert(0, this.textureCount);
		this.otherCountHistory.Insert(0, this.otherCount);
		List<MemoryUsageDisplay.TypeCount> list = new List<MemoryUsageDisplay.TypeCount>();
		int num = 0;
		foreach (KeyValuePair<string, int> keyValuePair in this.typeDict)
		{
			MemoryUsageDisplay.TypeCount typeCount = new MemoryUsageDisplay.TypeCount
			{
				typeName = keyValuePair.Key,
				count = keyValuePair.Value
			};
			if (list.Count < 50 || typeCount.count > num)
			{
				list.Add(typeCount);
				list.Sort();
				num = list[0].count;
			}
		}
		this.genericTypeHistory.Insert(0, list);
		this.haveData = true;
	}

	// Token: 0x06002687 RID: 9863 RVA: 0x0011C454 File Offset: 0x0011A854
	private void GetMemoryUseFromObjects(UnityEngine.Object[] objectArray, HashSet<UnityEngine.Object> objectSet)
	{
		foreach (UnityEngine.Object @object in objectArray)
		{
			if (!objectSet.Contains(@object))
			{
				objectSet.Add(@object);
				if (@object is Texture2D || @object.GetType().IsSubclassOf(typeof(Texture)))
				{
					this.textureCount++;
				}
				else if (@object is GameObject)
				{
					this.gameObjectCount++;
				}
				else if (@object is Material)
				{
					this.materialCount++;
				}
				else if (@object is Mesh)
				{
					this.meshCount++;
				}
				else if (this.typeDict.Count <= 50)
				{
					string key = @object.GetType().ToString();
					int num = 0;
					if (!this.typeDict.TryGetValue(key, out num))
					{
						this.typeDict[key] = 1;
					}
					else
					{
						this.typeDict[key] = num + 1;
					}
				}
				else
				{
					this.otherCount++;
				}
			}
		}
	}

	// Token: 0x06002688 RID: 9864 RVA: 0x0011C58B File Offset: 0x0011A98B
	public void Show()
	{
		if (!this.haveData)
		{
			this.SetBaseline();
		}
		base.enabled = true;
	}

	// Token: 0x06002689 RID: 9865 RVA: 0x0011C5A5 File Offset: 0x0011A9A5
	public void Hide()
	{
		base.enabled = false;
	}

	// Token: 0x0600268A RID: 9866 RVA: 0x0011C5AE File Offset: 0x0011A9AE
	private void OnEnable()
	{
		this.nextUpdate = Time.time + (float)this.updateFreq;
	}

	// Token: 0x0600268B RID: 9867 RVA: 0x0011C5C3 File Offset: 0x0011A9C3
	private void OnDisable()
	{
		this.nextUpdate = -1f;
	}

	// Token: 0x0600268C RID: 9868 RVA: 0x0011C5D0 File Offset: 0x0011A9D0
	private void GCSize()
	{
		this.totalMemoryHistory.Insert(0, (int)GC.GetTotalMemory(true));
	}

	// Token: 0x0600268D RID: 9869 RVA: 0x0011C5E8 File Offset: 0x0011A9E8
	private void SetBaseline()
	{
		this.totalObjectCountHistory = new List<int>();
		this.gameObjectCountHistory = new List<int>();
		this.materialCountHistory = new List<int>();
		this.textureCountHistory = new List<int>();
		this.meshCountHistory = new List<int>();
		this.componentCountHistory = new List<int>();
		this.otherCountHistory = new List<int>();
		this.totalMemoryHistory = new List<int>();
		this.genericTypeHistory = new List<List<MemoryUsageDisplay.TypeCount>>();
		this.GetCurrentMemoryUsage();
	}

	// Token: 0x0600268E RID: 9870 RVA: 0x0011C660 File Offset: 0x0011AA60
	private void Update()
	{
		if (MappedInput.InputDown(MappableInput.MEMORY_USAGE))
		{
			this.GetCurrentMemoryUsage();
		}
		if (this.autoUpdate)
		{
			if (this.nextUpdate < 0f)
			{
				this.GetCurrentMemoryUsage();
				this.nextUpdate = Time.time + (float)this.updateFreq;
			}
			if (Time.time >= this.nextUpdate)
			{
				this.GetCurrentMemoryUsage();
				this.nextUpdate = Time.time + (float)this.updateFreq;
			}
		}
	}

	// Token: 0x0600268F RID: 9871 RVA: 0x0011C6DC File Offset: 0x0011AADC
	public void OnGUI()
	{
		GUILayout.BeginArea(new Rect(100f, 100f, 1000f, 700f));
		this.ShowCount("Total objects", this.totalObjectCountHistory);
		this.ShowCount("Game Objects", this.gameObjectCountHistory);
		this.ShowCount("Materials", this.materialCountHistory);
		this.ShowCount("Textures", this.textureCountHistory);
		this.ShowCount("Components", this.componentCountHistory);
		this.ShowCount("Meshes", this.meshCountHistory);
		for (int i = 0; i < 8; i++)
		{
			if (this.genericTypeHistory.Count != 0)
			{
				List<int> list = new List<int>();
				if (this.genericTypeHistory[0].Count > i)
				{
					MemoryUsageDisplay.TypeCount typeCount = this.genericTypeHistory[0][i];
					list.Add(typeCount.count);
					if (this.genericTypeHistory.Count > 1)
					{
						for (int j = 0; j < this.genericTypeHistory[1].Count; j++)
						{
							MemoryUsageDisplay.TypeCount typeCount2 = this.genericTypeHistory[1][j];
							if (typeCount2.typeName == typeCount.typeName)
							{
								list.Add(typeCount2.count);
							}
						}
						List<MemoryUsageDisplay.TypeCount> list2 = this.genericTypeHistory[this.genericTypeHistory.Count - 1];
						for (int k = 0; k < list2.Count; k++)
						{
							MemoryUsageDisplay.TypeCount typeCount3 = list2[k];
							if (typeCount3.typeName == typeCount.typeName)
							{
								list.Add(typeCount3.count);
							}
						}
					}
					this.ShowCount(typeCount.typeName, list);
				}
			}
		}
		this.ShowCount("Other", this.otherCountHistory);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		if (GUILayout.Button("Update", new GUILayoutOption[]
		{
			GUILayout.Width(50f),
			GUILayout.Height(40f)
		}))
		{
			this.GetCurrentMemoryUsage();
		}
		if (GUILayout.Button("SetBaseline", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(40f)
		}))
		{
			this.SetBaseline();
		}
		this.autoUpdate = GUILayout.Toggle(this.autoUpdate, "auto update", new GUILayoutOption[]
		{
			GUILayout.Width(100f),
			GUILayout.Height(40f)
		});
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		this.includeScene = GUILayout.Toggle(this.includeScene, "include scene", new GUILayoutOption[]
		{
			GUILayout.Width(100f),
			GUILayout.Height(40f)
		});
		this.includeResources = GUILayout.Toggle(this.includeResources, "include resources", new GUILayoutOption[]
		{
			GUILayout.Width(100f),
			GUILayout.Height(40f)
		});
		GUILayout.EndHorizontal();
		GUILayout.Label(string.Empty, new GUILayoutOption[0]);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		if (GUILayout.Button("GC size", new GUILayoutOption[]
		{
			GUILayout.Width(80f),
			GUILayout.Height(40f)
		}))
		{
			this.GCSize();
		}
		if (GUILayout.Button("Unload Unused Assets", new GUILayoutOption[]
		{
			GUILayout.Width(200f),
			GUILayout.Height(40f)
		}))
		{
			Resources.UnloadUnusedAssets();
		}
		GUILayout.EndHorizontal();
		this.ShowCount("Usage", this.totalMemoryHistory);
		GUILayout.EndArea();
	}

	// Token: 0x06002690 RID: 9872 RVA: 0x0011CA8C File Offset: 0x0011AE8C
	private void ShowCount(string label, List<int> history)
	{
		if (history.Count == 0)
		{
			return;
		}
		GUILayout.BeginHorizontal(new GUILayoutOption[]
		{
			GUILayout.MaxWidth(700f)
		});
		GUILayout.Label(label, new GUILayoutOption[]
		{
			GUILayout.Width(250f)
		});
		GUILayout.Label(" : " + history[0], new GUILayoutOption[]
		{
			GUILayout.Width(100f)
		});
		if (history.Count > 1)
		{
			int change = history[0] - history[1];
			this.ShowChange(change);
			int change2 = history[0] - history[history.Count - 1];
			this.ShowChange(change2);
		}
		GUILayout.EndHorizontal();
	}

	// Token: 0x06002691 RID: 9873 RVA: 0x0011CB4C File Offset: 0x0011AF4C
	private void ShowChange(int change)
	{
		string arg = string.Empty;
		if (change > 0)
		{
			GUI.contentColor = Color.red;
			arg = "+";
		}
		else if (change < 0)
		{
			GUI.contentColor = Color.green;
		}
		GUILayout.Label(" " + arg + change, new GUILayoutOption[]
		{
			GUILayout.Width(80f)
		});
		GUI.contentColor = Color.white;
	}

	// Token: 0x040021B3 RID: 8627
	private List<int> totalObjectCountHistory = new List<int>();

	// Token: 0x040021B4 RID: 8628
	private List<int> gameObjectCountHistory = new List<int>();

	// Token: 0x040021B5 RID: 8629
	private List<int> materialCountHistory = new List<int>();

	// Token: 0x040021B6 RID: 8630
	private List<int> textureCountHistory = new List<int>();

	// Token: 0x040021B7 RID: 8631
	private List<int> meshCountHistory = new List<int>();

	// Token: 0x040021B8 RID: 8632
	private List<int> componentCountHistory = new List<int>();

	// Token: 0x040021B9 RID: 8633
	private List<int> otherCountHistory = new List<int>();

	// Token: 0x040021BA RID: 8634
	private List<int> totalMemoryHistory = new List<int>();

	// Token: 0x040021BB RID: 8635
	private List<List<MemoryUsageDisplay.TypeCount>> genericTypeHistory = new List<List<MemoryUsageDisplay.TypeCount>>();

	// Token: 0x040021BC RID: 8636
	private const int maxTypesToDisplay = 8;

	// Token: 0x040021BD RID: 8637
	private const int maxTypesToTrack = 50;

	// Token: 0x040021BE RID: 8638
	private bool includeScene = true;

	// Token: 0x040021BF RID: 8639
	private bool includeResources;

	// Token: 0x040021C0 RID: 8640
	private int updateFreq = 1;

	// Token: 0x040021C1 RID: 8641
	private bool autoUpdate;

	// Token: 0x040021C2 RID: 8642
	private float nextUpdate = -1f;

	// Token: 0x040021C3 RID: 8643
	private bool haveData;

	// Token: 0x040021C4 RID: 8644
	private static MemoryUsageDisplay _instance;

	// Token: 0x040021C5 RID: 8645
	private int totalObjectCount;

	// Token: 0x040021C6 RID: 8646
	private int gameObjectCount;

	// Token: 0x040021C7 RID: 8647
	private int materialCount;

	// Token: 0x040021C8 RID: 8648
	private int meshCount;

	// Token: 0x040021C9 RID: 8649
	private int componentCount;

	// Token: 0x040021CA RID: 8650
	private int textureCount;

	// Token: 0x040021CB RID: 8651
	private int otherCount;

	// Token: 0x040021CC RID: 8652
	private Dictionary<string, int> typeDict = new Dictionary<string, int>();

	// Token: 0x02000365 RID: 869
	private class TypeCount : IComparable
	{
		// Token: 0x06002693 RID: 9875 RVA: 0x0011CBC8 File Offset: 0x0011AFC8
		public int CompareTo(object obj)
		{
			MemoryUsageDisplay.TypeCount typeCount = obj as MemoryUsageDisplay.TypeCount;
			return -1 * this.count.CompareTo(typeCount.count);
		}

		// Token: 0x040021CD RID: 8653
		public string typeName;

		// Token: 0x040021CE RID: 8654
		public int count;
	}
}
