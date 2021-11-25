using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200011E RID: 286
public static class CollisionVolumes
{
	// Token: 0x060013DC RID: 5084 RVA: 0x000896D8 File Offset: 0x00087AD8
	public static void TranslateMeshes(CollisionMesh[] meshes, Vector3 offset)
	{
		foreach (CollisionMesh collisionMesh in meshes)
		{
			CollisionMesh collisionMesh2 = collisionMesh;
			collisionMesh2.AABB.center = collisionMesh2.AABB.center + offset;
			for (int j = 0; j < collisionMesh.Triangles.Length; j++)
			{
				collisionMesh.Triangles[j] = CollisionVolumes.TranslateTriangle(collisionMesh.Triangles[j], offset);
			}
		}
	}

	// Token: 0x060013DD RID: 5085 RVA: 0x00089758 File Offset: 0x00087B58
	private static Triangle TranslateTriangle(Triangle tri, Vector3 offset)
	{
		Plane p = tri.P;
		Vector3 v = tri.V1 + offset;
		Vector3 v2 = tri.V2 + offset;
		Vector3 v3 = tri.V3 + offset;
		p.distance = -Vector3.Dot(p.normal, tri.V1 + offset);
		return new Triangle(v, v2, v3, p);
	}

	// Token: 0x060013DE RID: 5086 RVA: 0x000897C4 File Offset: 0x00087BC4
	public static void Remove(GameObject prefab)
	{
		if (prefab == null)
		{
			return;
		}
		foreach (PrefabCollisionData key in new List<PrefabCollisionData>(CollisionVolumes.PrefabMeshes.Keys))
		{
			if (key.PrefabID == prefab.GetInstanceID())
			{
				CollisionVolumes.PrefabMeshes.Remove(key);
			}
		}
		ObjectPool.Remove(prefab);
	}

	// Token: 0x060013DF RID: 5087 RVA: 0x00089854 File Offset: 0x00087C54
	public static void FromPrefab(GameObject prefab, Transform transform, Vector3 scale, ref CollisionMesh[] meshes)
	{
		PrefabCollisionData key = new PrefabCollisionData(prefab, scale);
		CollisionMesh[] array;
		if (!CollisionVolumes.PrefabMeshes.TryGetValue(key, out array))
		{
			using (PooledObject pooledObject = ObjectPool.Get(prefab, Vector3.zero, Quaternion.identity))
			{
				GameObject gameObject = pooledObject.GameObject;
				gameObject.transform.localScale = scale;
				array = new CollisionMesh[gameObject.transform.childCount];
				int num = 0;
				IEnumerator enumerator = gameObject.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						array[num++] = CollisionVolumes.GenerateCollisionMesh(transform2.gameObject);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
		}
		CollisionVolumes.PrefabMeshes[key] = array;
		if (meshes == null)
		{
			meshes = new CollisionMesh[array.Length];
		}
		CollisionVolumes.TransformMeshes(transform, array, ref meshes);
	}

	// Token: 0x060013E0 RID: 5088 RVA: 0x00089964 File Offset: 0x00087D64
	private static void TransformMeshes(Transform transform, CollisionMesh[] prefabMeshes, ref CollisionMesh[] meshes)
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i] = new CollisionMesh(transform, prefabMeshes[i]);
		}
	}

	// Token: 0x060013E1 RID: 5089 RVA: 0x00089994 File Offset: 0x00087D94
	public static CollisionMesh GenerateCollisionMesh(GameObject volumePrefab)
	{
		string name = volumePrefab.name;
		if (name != null)
		{
			if (name == "Quad")
			{
				return CollisionVolumes.GenerateQuadGlueVolume(volumePrefab, 0.2f);
			}
			if (name == "Quad Locked")
			{
				return CollisionVolumes.GenerateQuadGlueVolume(volumePrefab, 0f);
			}
			if (name == "Tri")
			{
				return CollisionVolumes.GenerateTriGlueVolume(volumePrefab, 0.2f);
			}
			if (name == "Tri Locked")
			{
				return CollisionVolumes.GenerateTriGlueVolume(volumePrefab, 0f);
			}
			if (name == "Box")
			{
				return CollisionVolumes.GenerateBoxShapeVolume(volumePrefab);
			}
			if (name == "Wedge")
			{
				return CollisionVolumes.GenerateWedgeShapeVolume(volumePrefab);
			}
		}
		return CollisionVolumes.GenerateMeshVolume(volumePrefab);
	}

	// Token: 0x060013E2 RID: 5090 RVA: 0x00089A58 File Offset: 0x00087E58
	public static CollisionMesh GenerateMeshVolume(GameObject volumePrefab)
	{
		MeshFilter component = volumePrefab.GetComponent<MeshFilter>();
		SkinnedMeshRenderer component2 = volumePrefab.GetComponent<SkinnedMeshRenderer>();
		Mesh mesh = null;
		if (component != null)
		{
			mesh = component.sharedMesh;
		}
		else if (component2 != null)
		{
			mesh = component2.sharedMesh;
		}
		if (mesh == null)
		{
			return null;
		}
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Triangle[] array = new Triangle[triangles.Length / 3];
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			array[i] = new Triangle(volumePrefab.transform.TransformPoint(vertices[triangles[i * 3]]), volumePrefab.transform.TransformPoint(vertices[triangles[i * 3 + 1]]), volumePrefab.transform.TransformPoint(vertices[triangles[i * 3 + 2]]));
		}
		return new CollisionMesh(array);
	}

	// Token: 0x060013E3 RID: 5091 RVA: 0x00089B5C File Offset: 0x00087F5C
	private static CollisionMesh GenerateQuadGlueVolume(GameObject volumePrefab, float inwardOffset = 0.2f)
	{
		Triangle[] array = new Triangle[12];
		Transform transform = volumePrefab.transform;
		Vector3 lossyScale = transform.lossyScale;
		Vector3 position = transform.position;
		Vector3 vector = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0f));
		Vector3 vector3 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0f));
		Vector3 vector4 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0f));
		ScaleLockOptions component = volumePrefab.GetComponent<ScaleLockOptions>();
		if (null != component)
		{
			if (component.lockX)
			{
				lossyScale.x = transform.localScale.x;
			}
			if (component.lockY)
			{
				lossyScale.y = transform.localScale.y;
			}
			if (component.lockZ)
			{
				lossyScale.z = transform.localScale.z;
			}
			Bounds bounds = new Bounds(vector, Vector3.zero);
			bounds.Encapsulate(vector2);
			bounds.Encapsulate(vector3);
			bounds.Encapsulate(vector4);
			Vector3 lhs = position + transform.rotation * Vector3.Scale(lossyScale, new Vector3(0.5f, 0.5f, 0f));
			Vector3 rhs = position + transform.rotation * Vector3.Scale(lossyScale, new Vector3(-0.5f, -0.5f, 0f));
			Vector3 vector5 = Vector3.Max(lhs, rhs) - Vector3.Min(lhs, rhs);
			if (component.lockX)
			{
				if (bounds.center.x < position.x)
				{
					if (vector.x >= position.x)
					{
						vector.x = bounds.min.x + vector5.x;
					}
					if (vector2.x >= position.x)
					{
						vector2.x = bounds.min.x + vector5.x;
					}
					if (vector3.x >= position.x)
					{
						vector3.x = bounds.min.x + vector5.x;
					}
					if (vector4.x >= position.x)
					{
						vector4.x = bounds.min.x + vector5.x;
					}
				}
				else
				{
					if (vector.x < position.x)
					{
						vector.x = bounds.max.x - vector5.x;
					}
					if (vector2.x < position.x)
					{
						vector2.x = bounds.max.x - vector5.x;
					}
					if (vector3.x < position.x)
					{
						vector3.x = bounds.max.x - vector5.x;
					}
					if (vector4.x < position.x)
					{
						vector4.x = bounds.max.x - vector5.x;
					}
				}
			}
			if (component.lockY)
			{
				if (bounds.center.y < position.y)
				{
					if (vector.y >= position.y)
					{
						vector.y = bounds.min.y + vector5.y;
					}
					if (vector2.y >= position.y)
					{
						vector2.y = bounds.min.y + vector5.y;
					}
					if (vector3.y >= position.y)
					{
						vector3.y = bounds.min.y + vector5.y;
					}
					if (vector4.y >= position.y)
					{
						vector4.y = bounds.min.y + vector5.y;
					}
				}
				else
				{
					if (vector.y < position.y)
					{
						vector.y = bounds.max.y - vector5.y;
					}
					if (vector2.y < position.y)
					{
						vector2.y = bounds.max.y - vector5.y;
					}
					if (vector3.y < position.y)
					{
						vector3.y = bounds.max.y - vector5.y;
					}
					if (vector4.y < position.y)
					{
						vector4.y = bounds.max.y - vector5.y;
					}
				}
			}
			if (component.lockZ)
			{
				if (bounds.center.z < position.z)
				{
					if (vector.z >= position.z)
					{
						vector.z = bounds.min.z + vector5.z;
					}
					if (vector2.z >= position.z)
					{
						vector2.z = bounds.min.z + vector5.z;
					}
					if (vector3.z >= position.z)
					{
						vector3.z = bounds.min.z + vector5.z;
					}
					if (vector4.z >= position.z)
					{
						vector4.z = bounds.min.z + vector5.z;
					}
				}
				else
				{
					if (vector.z < position.z)
					{
						vector.z = bounds.max.z - vector5.z;
					}
					if (vector2.z < position.z)
					{
						vector2.z = bounds.max.z - vector5.z;
					}
					if (vector3.z < position.z)
					{
						vector3.z = bounds.max.z - vector5.z;
					}
					if (vector4.z < position.z)
					{
						vector4.z = bounds.max.z - vector5.z;
					}
				}
			}
		}
		Vector3 normalized = (vector2 - vector3).normalized;
		Vector3 normalized2 = (vector - vector2).normalized;
		Vector3 normalized3 = Vector3.Cross(vector3 - vector2, vector3 - vector).normalized;
		Vector3 b = inwardOffset * normalized;
		Vector3 b2 = inwardOffset * normalized2;
		Vector3 b3 = 0.01f * normalized3;
		array[0] = new Triangle(vector2 - b + b2 + b3, vector - b - b2 + b3, vector3 + b + b2 + b3);
		array[1] = new Triangle(vector3 + b + b2 + b3, vector - b - b2 + b3, vector4 + b - b2 + b3);
		array[2] = new Triangle(vector3 + b + b2 - b3, vector - b - b2 - b3, vector2 - b + b2 - b3);
		array[3] = new Triangle(vector4 + b - b2 - b3, vector - b - b2 - b3, vector3 + b + b2 - b3);
		array[4] = new Triangle(vector - b - b2 - b3, vector - b - b2 + b3, vector2 - b + b2 - b3);
		array[5] = new Triangle(vector - b - b2 + b3, vector2 - b + b2 + b3, vector2 - b + b2 - b3);
		array[6] = new Triangle(vector4 + b - b2 - b3, vector3 + b + b2 - b3, vector4 + b - b2 + b3);
		array[7] = new Triangle(vector4 + b - b2 + b3, vector3 + b + b2 - b3, vector3 + b + b2 + b3);
		array[8] = new Triangle(vector - b - b2 - b3, vector4 + b - b2 + b3, vector - b - b2 + b3);
		array[9] = new Triangle(vector4 + b - b2 + b3, vector - b - b2 - b3, vector4 + b - b2 - b3);
		array[10] = new Triangle(vector2 - b + b2 - b3, vector2 - b + b2 + b3, vector3 + b + b2 + b3);
		array[11] = new Triangle(vector3 + b + b2 + b3, vector3 + b + b2 - b3, vector2 - b + b2 - b3);
		return new CollisionMesh(array);
	}

	// Token: 0x060013E4 RID: 5092 RVA: 0x0008A6C4 File Offset: 0x00088AC4
	private static CollisionMesh GenerateTriGlueVolume(GameObject volumePrefab, float inwardOffset = 0.2f)
	{
		Triangle[] array = new Triangle[8];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0f, -0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0f, -0.5f, -0.5f));
		Vector3 a = transform.TransformPoint(new Vector3(0f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector).normalized;
		Vector3 normalized2 = (a - vector).normalized;
		Vector3 normalized3 = (a - vector2).normalized;
		Vector3 normalized4 = (normalized + normalized2).normalized;
		Vector3 normalized5 = (-normalized + normalized3).normalized;
		Vector3 normalized6 = (-normalized3 - normalized2).normalized;
		Vector3 normalized7 = Vector3.Cross(normalized2, normalized3).normalized;
		Vector3 b = 0.01f * normalized7;
		array[0] = new Triangle(vector2 + inwardOffset * normalized5 + b, a + inwardOffset * normalized6 + b, vector + inwardOffset * normalized4 + b);
		array[1] = new Triangle(vector2 + inwardOffset * normalized5 - b, vector + inwardOffset * normalized4 - b, a + inwardOffset * normalized6 - b);
		array[2] = new Triangle(vector2 + inwardOffset * normalized5 - b, vector2 + inwardOffset * normalized5 + b, vector + inwardOffset * normalized4 - b);
		array[3] = new Triangle(vector + inwardOffset * normalized4 - b, vector2 + inwardOffset * normalized5 + b, vector + inwardOffset * normalized4 + b);
		array[4] = new Triangle(vector2 + inwardOffset * normalized5 - b, a + inwardOffset * normalized6 + b, vector2 + inwardOffset * normalized5 + b);
		array[5] = new Triangle(a + inwardOffset * normalized6 + b, vector2 + inwardOffset * normalized5 - b, a + inwardOffset * normalized6 - b);
		array[6] = new Triangle(vector + inwardOffset * normalized4 + b, a + inwardOffset * normalized6 - b, vector + inwardOffset * normalized4 - b);
		array[7] = new Triangle(a + inwardOffset * normalized6 - b, vector + inwardOffset * normalized4 + b, a + inwardOffset * normalized6 + b);
		return new CollisionMesh(array);
	}

	// Token: 0x060013E5 RID: 5093 RVA: 0x0008AA60 File Offset: 0x00088E60
	private static CollisionMesh GenerateBoxShapeVolume(GameObject volumePrefab)
	{
		Triangle[] array = new Triangle[12];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.5f));
		Vector3 vector3 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.5f));
		Vector3 a = transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0.5f));
		Vector3 a2 = transform.TransformPoint(new Vector3(0.5f, 0.5f, -0.5f));
		Vector3 a3 = transform.TransformPoint(new Vector3(0.5f, -0.5f, -0.5f));
		Vector3 a4 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f));
		Vector3 a5 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector3).normalized;
		Vector3 normalized2 = (vector - vector2).normalized;
		Vector3 normalized3 = Vector3.Cross(vector3 - vector2, vector3 - vector).normalized;
		Vector3 b = 0.1f * normalized;
		Vector3 b2 = 0.1f * normalized2;
		Vector3 b3 = 0.1f * normalized3;
		array[0] = new Triangle(a2 - b - b2 + b3, a3 - b + b2 + b3, a5 + b - b2 + b3);
		array[1] = new Triangle(a5 + b - b2 + b3, a3 - b + b2 + b3, a4 + b + b2 + b3);
		array[2] = new Triangle(vector - b - b2 - b3, a + b - b2 - b3, vector2 - b + b2 - b3);
		array[3] = new Triangle(a + b - b2 - b3, vector3 + b + b2 - b3, vector2 - b + b2 - b3);
		array[4] = new Triangle(vector - b - b2 - b3, vector2 - b + b2 - b3, a2 - b - b2 + b3);
		array[5] = new Triangle(vector2 - b + b2 - b3, a3 - b + b2 + b3, a2 - b - b2 + b3);
		array[6] = new Triangle(a + b - b2 - b3, a5 + b - b2 + b3, vector3 + b + b2 - b3);
		array[7] = new Triangle(vector3 + b + b2 - b3, a5 + b - b2 + b3, a4 + b + b2 + b3);
		array[8] = new Triangle(vector - b - b2 - b3, a2 - b - b2 + b3, a + b - b2 - b3);
		array[9] = new Triangle(a2 - b - b2 + b3, a5 + b - b2 + b3, a + b - b2 - b3);
		array[10] = new Triangle(vector2 - b + b2 - b3, vector3 + b + b2 - b3, a3 - b + b2 + b3);
		array[11] = new Triangle(a3 - b + b2 + b3, vector3 + b + b2 - b3, a4 + b + b2 + b3);
		return new CollisionMesh(array);
	}

	// Token: 0x060013E6 RID: 5094 RVA: 0x0008AFD4 File Offset: 0x000893D4
	private static CollisionMesh GenerateWedgeShapeVolume(GameObject volumePrefab)
	{
		Triangle[] array = new Triangle[8];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0.5f, -0.5f, -0.5f));
		Vector3 a = transform.TransformPoint(new Vector3(0.5f, 0.5f, -0.5f));
		Vector3 a2 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.5f));
		Vector3 a3 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f));
		Vector3 a4 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector).normalized;
		Vector3 normalized2 = (a - vector).normalized;
		Vector3 normalized3 = (a - vector2).normalized;
		Vector3 normalized4 = (normalized + normalized2).normalized;
		Vector3 normalized5 = (-normalized + normalized3).normalized;
		Vector3 normalized6 = (-normalized3 - normalized2).normalized;
		Vector3 normalized7 = Vector3.Cross(normalized2, normalized3).normalized;
		Vector3 b = 0.1f * normalized7;
		array[0] = new Triangle(vector2 + 0.1f * normalized5 - b, a + 0.1f * normalized6 - b, vector + 0.1f * normalized4 - b);
		array[1] = new Triangle(a3 + 0.1f * normalized5 + b, a2 + 0.1f * normalized4 + b, a4 + 0.1f * normalized6 + b);
		array[2] = new Triangle(vector2 + 0.1f * normalized5 - b, vector + 0.1f * normalized4 - b, a3 + 0.1f * normalized5 + b);
		array[3] = new Triangle(vector + 0.1f * normalized4 - b, a2 + 0.1f * normalized4 + b, a3 + 0.1f * normalized5 + b);
		array[4] = new Triangle(vector2 + 0.1f * normalized5 - b, a3 + 0.1f * normalized5 + b, a4 + 0.1f * normalized6 + b);
		array[5] = new Triangle(a4 + 0.1f * normalized6 + b, a + 0.1f * normalized6 - b, vector2 + 0.1f * normalized5 - b);
		array[6] = new Triangle(a2 + 0.1f * normalized4 + b, vector + 0.1f * normalized4 - b, a + 0.1f * normalized6 - b);
		array[7] = new Triangle(a + 0.1f * normalized6 - b, a4 + 0.1f * normalized6 + b, a2 + 0.1f * normalized4 + b);
		return new CollisionMesh(array);
	}

	// Token: 0x04000F84 RID: 3972
	public const float lenExtrude = 0.01f;

	// Token: 0x04000F85 RID: 3973
	private const float lenInwardOffset = 0.2f;

	// Token: 0x04000F86 RID: 3974
	public const float shapeShrink = 0.1f;

	// Token: 0x04000F87 RID: 3975
	public static Dictionary<PrefabCollisionData, CollisionMesh[]> PrefabMeshes = new Dictionary<PrefabCollisionData, CollisionMesh[]>();
}
