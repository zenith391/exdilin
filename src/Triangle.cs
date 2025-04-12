using System;
using UnityEngine;

// Token: 0x0200011B RID: 283
public struct Triangle
{
	// Token: 0x060013BC RID: 5052 RVA: 0x000881A8 File Offset: 0x000865A8
	public Triangle(Vector3[] points)
	{
		this.V1 = points[0];
		this.V2 = points[1];
		this.V3 = points[2];
		this.P = new Plane(this.V1, this.V2, this.V3);
	}

	// Token: 0x060013BD RID: 5053 RVA: 0x00088208 File Offset: 0x00086608
	public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		this.V1 = v1;
		this.V2 = v2;
		this.V3 = v3;
		this.P = new Plane(this.V1, this.V2, this.V3);
	}

	// Token: 0x060013BE RID: 5054 RVA: 0x0008823C File Offset: 0x0008663C
	public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Plane plane)
	{
		this.V1 = v1;
		this.V2 = v2;
		this.V3 = v3;
		this.P = plane;
	}

	// Token: 0x04000F7D RID: 3965
	public readonly Vector3 V1;

	// Token: 0x04000F7E RID: 3966
	public readonly Vector3 V2;

	// Token: 0x04000F7F RID: 3967
	public readonly Vector3 V3;

	// Token: 0x04000F80 RID: 3968
	public readonly Plane P;
}
