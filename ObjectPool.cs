using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200025C RID: 604
public static class ObjectPool
{
	// Token: 0x06001B53 RID: 6995 RVA: 0x000C6A4C File Offset: 0x000C4E4C
	public static void Remove(GameObject prefab)
	{
		if (ObjectPool._pool.ContainsKey(prefab))
		{
			foreach (GameObject gameObject in ObjectPool._pool[prefab])
			{
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
		ObjectPool._pool.Remove(prefab);
	}

	// Token: 0x06001B54 RID: 6996 RVA: 0x000C6AD4 File Offset: 0x000C4ED4
	public static PooledObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (ObjectPool._pool.ContainsKey(prefab) && ObjectPool._pool[prefab].Count != 0)
		{
			return ObjectPool.GetFromPool(prefab, position, rotation);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
		Blocksworld.DisableRenderer(gameObject);
		return new PooledObject(prefab, gameObject);
	}

	// Token: 0x06001B55 RID: 6997 RVA: 0x000C6B25 File Offset: 0x000C4F25
	public static void Return(GameObject prefab, GameObject obj)
	{
		if (!ObjectPool._pool.ContainsKey(prefab))
		{
			ObjectPool._pool[prefab] = new List<GameObject>();
		}
		ObjectPool._pool[prefab].Add(obj);
	}

	// Token: 0x06001B56 RID: 6998 RVA: 0x000C6B58 File Offset: 0x000C4F58
	private static PooledObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		List<GameObject> list = ObjectPool._pool[prefab];
		GameObject gameObject = list[list.Count - 1];
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		list.RemoveAt(list.Count - 1);
		return new PooledObject(prefab, gameObject);
	}

	// Token: 0x04001734 RID: 5940
	private static Dictionary<GameObject, List<GameObject>> _pool = new Dictionary<GameObject, List<GameObject>>();
}
