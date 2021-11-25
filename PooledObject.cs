using System;
using UnityEngine;

// Token: 0x0200025B RID: 603
public class PooledObject : IDisposable
{
	// Token: 0x06001B51 RID: 6993 RVA: 0x000C6A12 File Offset: 0x000C4E12
	public PooledObject(GameObject prefab, GameObject gameObject)
	{
		this._prefab = prefab;
		this.GameObject = gameObject;
	}

	// Token: 0x06001B52 RID: 6994 RVA: 0x000C6A28 File Offset: 0x000C4E28
	public void Dispose()
	{
		if (this.GameObject != null)
		{
			ObjectPool.Return(this._prefab, this.GameObject);
		}
	}

	// Token: 0x04001732 RID: 5938
	public readonly GameObject GameObject;

	// Token: 0x04001733 RID: 5939
	private readonly GameObject _prefab;
}
