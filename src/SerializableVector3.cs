using System;
using UnityEngine;

// Token: 0x02000247 RID: 583
[Serializable]
public class SerializableVector3
{
	// Token: 0x06001B00 RID: 6912 RVA: 0x000C5D22 File Offset: 0x000C4122
	public SerializableVector3(Vector3 v)
	{
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
	}

	// Token: 0x06001B01 RID: 6913 RVA: 0x000C5D51 File Offset: 0x000C4151
	public Vector3 VectorValue()
	{
		return new Vector3(this.x, this.y, this.z);
	}

	// Token: 0x06001B02 RID: 6914 RVA: 0x000C5D6A File Offset: 0x000C416A
	public void Set(Vector3 v)
	{
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
	}

	// Token: 0x040016D6 RID: 5846
	public float x;

	// Token: 0x040016D7 RID: 5847
	public float y;

	// Token: 0x040016D8 RID: 5848
	public float z;
}
