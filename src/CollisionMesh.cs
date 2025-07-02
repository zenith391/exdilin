using UnityEngine;

public class CollisionMesh
{
	public Vector3 localRot;

	public readonly Triangle[] Triangles;

	public Bounds AABB;

	public CollisionMesh(Triangle[] triangles)
	{
		Triangles = triangles;
		RecalculateAABB();
	}

	public CollisionMesh(Transform transform, CollisionMesh prefabMesh)
	{
		localRot = transform.localEulerAngles;
		Triangles = new Triangle[prefabMesh.Triangles.Length];
		for (int i = 0; i < Triangles.Length; i++)
		{
			Triangle triangle = prefabMesh.Triangles[i];
			Vector3 vector = transform.TransformPoint(triangle.V1);
			Vector3 v = transform.TransformPoint(triangle.V2);
			Vector3 v2 = transform.TransformPoint(triangle.V3);
			Vector3 vector2 = transform.TransformDirection(triangle.P.normal);
			float distance = 0f - Vector3.Dot(vector, vector2);
			Plane plane = new Plane
			{
				distance = distance,
				normal = vector2
			};
			Triangles[i] = new Triangle(vector, v, v2, plane);
		}
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		Vector3 position = transform.position;
		Vector3 zero = Vector3.zero;
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				position[j] += localToWorldMatrix[j, k] * prefabMesh.AABB.center[k];
				zero[j] += Mathf.Abs(localToWorldMatrix[j, k]) * prefabMesh.AABB.size[k];
			}
		}
		AABB.center = position;
		AABB.size = zero;
	}

	public void RecalculateAABB()
	{
		AABB = new Bounds(Triangles[0].V1, Vector3.zero);
		Triangle[] triangles = Triangles;
		for (int i = 0; i < triangles.Length; i++)
		{
			Triangle triangle = triangles[i];
			AABB.Encapsulate(triangle.V1);
			AABB.Encapsulate(triangle.V2);
			AABB.Encapsulate(triangle.V3);
		}
	}
}
