using System;
using System.Reflection;
using UnityEngine;

public class ArrowPrefabData : MonoBehaviour
{
	public GameObject prefabBody;

	public GameObject prefabHead;

	public void Awake()
	{
		Type type = Type.GetType("Arrow");
		if (type == null)
		{
			type = Type.GetType("Arrow,Blocksworld");
		}
		MethodInfo method = type.GetMethod("Bootstrap");
		object[] parameters = new GameObject[1] { base.gameObject };
		method.Invoke(null, parameters);
	}
}
