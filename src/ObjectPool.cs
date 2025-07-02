using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
	private static Dictionary<GameObject, List<GameObject>> _pool = new Dictionary<GameObject, List<GameObject>>();

	public static void Remove(GameObject prefab)
	{
		if (_pool.ContainsKey(prefab))
		{
			foreach (GameObject item in _pool[prefab])
			{
				if (item != null)
				{
					Object.Destroy(item);
				}
			}
		}
		_pool.Remove(prefab);
	}

	public static PooledObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (_pool.ContainsKey(prefab) && _pool[prefab].Count != 0)
		{
			return GetFromPool(prefab, position, rotation);
		}
		GameObject gameObject = Object.Instantiate(prefab, position, rotation);
		Blocksworld.DisableRenderer(gameObject);
		return new PooledObject(prefab, gameObject);
	}

	public static void Return(GameObject prefab, GameObject obj)
	{
		if (!_pool.ContainsKey(prefab))
		{
			_pool[prefab] = new List<GameObject>();
		}
		_pool[prefab].Add(obj);
	}

	private static PooledObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		List<GameObject> list = _pool[prefab];
		GameObject gameObject = list[list.Count - 1];
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		list.RemoveAt(list.Count - 1);
		return new PooledObject(prefab, gameObject);
	}
}
