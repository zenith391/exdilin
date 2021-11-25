using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000330 RID: 816
public class GameObjectNameComparer : IComparer<GameObject>
{
	// Token: 0x06002520 RID: 9504 RVA: 0x0010F0FC File Offset: 0x0010D4FC
	public int Compare(GameObject a, GameObject b)
	{
		return a.name.CompareTo(b.name);
	}
}
