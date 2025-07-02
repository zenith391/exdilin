using System.Collections.Generic;
using UnityEngine;

public class GameObjectNameComparer : IComparer<GameObject>
{
	public int Compare(GameObject a, GameObject b)
	{
		return a.name.CompareTo(b.name);
	}
}
