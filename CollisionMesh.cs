using System;
using UnityEngine;

// Token: 0x0200011A RID: 282
public class CollisionMesh
{
	// Token: 0x060013B9 RID: 5049 RVA: 0x00087F49 File Offset: 0x00086349
	public CollisionMesh(Triangle[] triangles)
	{
		this.Triangles = triangles;
		this.RecalculateAABB();
	}

	// Token: 0x060013BA RID: 5050 RVA: 0x00087F60 File Offset: 0x00086360
	public CollisionMesh(Transform transform, CollisionMesh prefabMesh)
	{
		this.localRot = transform.localEulerAngles;
		this.Triangles = new Triangle[prefabMesh.Triangles.Length];
		for (int i = 0; i < this.Triangles.Length; i++)
		{
			Triangle triangle = prefabMesh.Triangles[i];
			Vector3 vector = transform.TransformPoint(triangle.V1);
			Vector3 v = transform.TransformPoint(triangle.V2);
			Vector3 v2 = transform.TransformPoint(triangle.V3);
			Vector3 vector2 = transform.TransformDirection(triangle.P.normal);
			float distance = -Vector3.Dot(vector, vector2);
			Plane plane = default(Plane);
			plane.distance = distance;
			plane.normal = vector2;
			this.Triangles[i] = new Triangle(vector, v, v2, plane);
		}
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		Vector3 position = transform.position;
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				ref Vector3 ptr = ref position;
				int index;
				position[index = j] = ptr[index] + localToWorldMatrix[j, k] * prefabMesh.AABB.center[k];
				ptr = ref zero;
				int index2;
				zero[index2 = j] = ptr[index2] + Mathf.Abs(localToWorldMatrix[j, k]) * prefabMesh.AABB.size[k];
			}
		}
		this.AABB.center = position;
		this.AABB.size = zero;
	}

	// Token: 0x060013BB RID: 5051 RVA: 0x0008811C File Offset: 0x0008651C
	public void RecalculateAABB()
	{
		this.AABB = new Bounds(this.Triangles[0].V1, Vector3.zero);
		foreach (Triangle triangle in this.Triangles)
		{
			this.AABB.Encapsulate(triangle.V1);
			this.AABB.Encapsulate(triangle.V2);
			this.AABB.Encapsulate(triangle.V3);
		}
	}

	// Token: 0x04000F7A RID: 3962
	public Vector3 localRot;

	// Token: 0x04000F7B RID: 3963
	public readonly Triangle[] Triangles;

	// Token: 0x04000F7C RID: 3964
	public Bounds AABB;
}
