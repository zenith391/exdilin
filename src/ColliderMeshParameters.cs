using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200033B RID: 827
internal class ColliderMeshParameters
{
	// Token: 0x04001FD9 RID: 8153
	public ColliderTypes colliderType;

	// Token: 0x04001FDA RID: 8154
	public List<Collider> ourColliders;

	// Token: 0x04001FDB RID: 8155
	public float[] normalizedAreaWeights;

	// Token: 0x04001FDC RID: 8156
	public Mesh colliderMesh;

	// Token: 0x04001FDD RID: 8157
	public Vector3[] colliderMeshVertices;

	// Token: 0x04001FDE RID: 8158
	public int[] colliderMeshTriangles;

	// Token: 0x04001FDF RID: 8159
	public float ourRadius;

	// Token: 0x04001FE0 RID: 8160
	public Vector3 theSize;

	// Token: 0x04001FE1 RID: 8161
	public Vector3 localOffset;
}
