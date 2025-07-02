using System;
using UnityEngine;

public class PooledObject : IDisposable
{
	public readonly GameObject GameObject;

	private readonly GameObject _prefab;

	public PooledObject(GameObject prefab, GameObject gameObject)
	{
		_prefab = prefab;
		GameObject = gameObject;
	}

	public void Dispose()
	{
		if (GameObject != null)
		{
			ObjectPool.Return(_prefab, GameObject);
		}
	}
}
