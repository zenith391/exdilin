using System;
using UnityEngine;

// Token: 0x0200011D RID: 285
public struct PrefabCollisionData
{
	// Token: 0x060013D9 RID: 5081 RVA: 0x0008964E File Offset: 0x00087A4E
	public PrefabCollisionData(GameObject prefab, Vector3 scale)
	{
		this.PrefabID = prefab.GetInstanceID();
		this.Scale = scale;
	}

	// Token: 0x060013DA RID: 5082 RVA: 0x00089664 File Offset: 0x00087A64
	public override int GetHashCode()
	{
		return this.PrefabID ^ this.Scale.GetHashCode() << 16;
	}

	// Token: 0x060013DB RID: 5083 RVA: 0x00089690 File Offset: 0x00087A90
	public override bool Equals(object other)
	{
		if (!(other is PrefabCollisionData))
		{
			return false;
		}
		PrefabCollisionData prefabCollisionData = (PrefabCollisionData)other;
		return this.PrefabID == prefabCollisionData.PrefabID && this.Scale == prefabCollisionData.Scale;
	}

	// Token: 0x04000F82 RID: 3970
	public readonly int PrefabID;

	// Token: 0x04000F83 RID: 3971
	public readonly Vector3 Scale;
}
