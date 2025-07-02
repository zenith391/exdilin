using System.Collections.Generic;
using UnityEngine;

public static class CollisionVolumes
{
	public const float lenExtrude = 0.01f;

	private const float lenInwardOffset = 0.2f;

	public const float shapeShrink = 0.1f;

	public static Dictionary<PrefabCollisionData, CollisionMesh[]> PrefabMeshes = new Dictionary<PrefabCollisionData, CollisionMesh[]>();

	public static void TranslateMeshes(CollisionMesh[] meshes, Vector3 offset)
	{
		foreach (CollisionMesh collisionMesh in meshes)
		{
			CollisionMesh collisionMesh2 = collisionMesh;
			collisionMesh2.AABB.center = collisionMesh2.AABB.center + offset;
			for (int j = 0; j < collisionMesh.Triangles.Length; j++)
			{
				collisionMesh.Triangles[j] = TranslateTriangle(collisionMesh.Triangles[j], offset);
			}
		}
	}

	private static Triangle TranslateTriangle(Triangle tri, Vector3 offset)
	{
		Plane p = tri.P;
		Vector3 v = tri.V1 + offset;
		Vector3 v2 = tri.V2 + offset;
		Vector3 v3 = tri.V3 + offset;
		p.distance = 0f - Vector3.Dot(p.normal, tri.V1 + offset);
		return new Triangle(v, v2, v3, p);
	}

	public static void Remove(GameObject prefab)
	{
		if (prefab == null)
		{
			return;
		}
		foreach (PrefabCollisionData item in new List<PrefabCollisionData>(PrefabMeshes.Keys))
		{
			if (item.PrefabID == prefab.GetInstanceID())
			{
				PrefabMeshes.Remove(item);
			}
		}
		ObjectPool.Remove(prefab);
	}

	public static void FromPrefab(GameObject prefab, Transform transform, Vector3 scale, ref CollisionMesh[] meshes)
	{
		PrefabCollisionData key = new PrefabCollisionData(prefab, scale);
		if (!PrefabMeshes.TryGetValue(key, out var value))
		{
			using PooledObject pooledObject = ObjectPool.Get(prefab, Vector3.zero, Quaternion.identity);
			GameObject gameObject = pooledObject.GameObject;
			gameObject.transform.localScale = scale;
			value = new CollisionMesh[gameObject.transform.childCount];
			int num = 0;
			foreach (object item in gameObject.transform)
			{
				Transform transform2 = (Transform)item;
				value[num++] = GenerateCollisionMesh(transform2.gameObject);
			}
		}
		PrefabMeshes[key] = value;
		if (meshes == null)
		{
			meshes = new CollisionMesh[value.Length];
		}
		TransformMeshes(transform, value, ref meshes);
	}

	private static void TransformMeshes(Transform transform, CollisionMesh[] prefabMeshes, ref CollisionMesh[] meshes)
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i] = new CollisionMesh(transform, prefabMeshes[i]);
		}
	}

	public static CollisionMesh GenerateCollisionMesh(GameObject volumePrefab)
	{
		return volumePrefab.name switch
		{
			"Quad" => GenerateQuadGlueVolume(volumePrefab), 
			"Quad Locked" => GenerateQuadGlueVolume(volumePrefab, 0f), 
			"Tri" => GenerateTriGlueVolume(volumePrefab), 
			"Tri Locked" => GenerateTriGlueVolume(volumePrefab, 0f), 
			"Box" => GenerateBoxShapeVolume(volumePrefab), 
			"Wedge" => GenerateWedgeShapeVolume(volumePrefab), 
			_ => GenerateMeshVolume(volumePrefab), 
		};
	}

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
		Vector3 vector6 = inwardOffset * normalized;
		Vector3 vector7 = inwardOffset * normalized2;
		Vector3 vector8 = 0.01f * normalized3;
		array[0] = new Triangle(vector2 - vector6 + vector7 + vector8, vector - vector6 - vector7 + vector8, vector3 + vector6 + vector7 + vector8);
		array[1] = new Triangle(vector3 + vector6 + vector7 + vector8, vector - vector6 - vector7 + vector8, vector4 + vector6 - vector7 + vector8);
		array[2] = new Triangle(vector3 + vector6 + vector7 - vector8, vector - vector6 - vector7 - vector8, vector2 - vector6 + vector7 - vector8);
		array[3] = new Triangle(vector4 + vector6 - vector7 - vector8, vector - vector6 - vector7 - vector8, vector3 + vector6 + vector7 - vector8);
		array[4] = new Triangle(vector - vector6 - vector7 - vector8, vector - vector6 - vector7 + vector8, vector2 - vector6 + vector7 - vector8);
		array[5] = new Triangle(vector - vector6 - vector7 + vector8, vector2 - vector6 + vector7 + vector8, vector2 - vector6 + vector7 - vector8);
		array[6] = new Triangle(vector4 + vector6 - vector7 - vector8, vector3 + vector6 + vector7 - vector8, vector4 + vector6 - vector7 + vector8);
		array[7] = new Triangle(vector4 + vector6 - vector7 + vector8, vector3 + vector6 + vector7 - vector8, vector3 + vector6 + vector7 + vector8);
		array[8] = new Triangle(vector - vector6 - vector7 - vector8, vector4 + vector6 - vector7 + vector8, vector - vector6 - vector7 + vector8);
		array[9] = new Triangle(vector4 + vector6 - vector7 + vector8, vector - vector6 - vector7 - vector8, vector4 + vector6 - vector7 - vector8);
		array[10] = new Triangle(vector2 - vector6 + vector7 - vector8, vector2 - vector6 + vector7 + vector8, vector3 + vector6 + vector7 + vector8);
		array[11] = new Triangle(vector3 + vector6 + vector7 + vector8, vector3 + vector6 + vector7 - vector8, vector2 - vector6 + vector7 - vector8);
		return new CollisionMesh(array);
	}

	private static CollisionMesh GenerateTriGlueVolume(GameObject volumePrefab, float inwardOffset = 0.2f)
	{
		Triangle[] array = new Triangle[8];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0f, -0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0f, -0.5f, -0.5f));
		Vector3 vector3 = transform.TransformPoint(new Vector3(0f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector).normalized;
		Vector3 normalized2 = (vector3 - vector).normalized;
		Vector3 normalized3 = (vector3 - vector2).normalized;
		Vector3 normalized4 = (normalized + normalized2).normalized;
		Vector3 normalized5 = (-normalized + normalized3).normalized;
		Vector3 normalized6 = (-normalized3 - normalized2).normalized;
		Vector3 normalized7 = Vector3.Cross(normalized2, normalized3).normalized;
		Vector3 vector4 = 0.01f * normalized7;
		array[0] = new Triangle(vector2 + inwardOffset * normalized5 + vector4, vector3 + inwardOffset * normalized6 + vector4, vector + inwardOffset * normalized4 + vector4);
		array[1] = new Triangle(vector2 + inwardOffset * normalized5 - vector4, vector + inwardOffset * normalized4 - vector4, vector3 + inwardOffset * normalized6 - vector4);
		array[2] = new Triangle(vector2 + inwardOffset * normalized5 - vector4, vector2 + inwardOffset * normalized5 + vector4, vector + inwardOffset * normalized4 - vector4);
		array[3] = new Triangle(vector + inwardOffset * normalized4 - vector4, vector2 + inwardOffset * normalized5 + vector4, vector + inwardOffset * normalized4 + vector4);
		array[4] = new Triangle(vector2 + inwardOffset * normalized5 - vector4, vector3 + inwardOffset * normalized6 + vector4, vector2 + inwardOffset * normalized5 + vector4);
		array[5] = new Triangle(vector3 + inwardOffset * normalized6 + vector4, vector2 + inwardOffset * normalized5 - vector4, vector3 + inwardOffset * normalized6 - vector4);
		array[6] = new Triangle(vector + inwardOffset * normalized4 + vector4, vector3 + inwardOffset * normalized6 - vector4, vector + inwardOffset * normalized4 - vector4);
		array[7] = new Triangle(vector3 + inwardOffset * normalized6 - vector4, vector + inwardOffset * normalized4 + vector4, vector3 + inwardOffset * normalized6 + vector4);
		return new CollisionMesh(array);
	}

	private static CollisionMesh GenerateBoxShapeVolume(GameObject volumePrefab)
	{
		Triangle[] array = new Triangle[12];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.5f));
		Vector3 vector3 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.5f));
		Vector3 vector4 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0.5f));
		Vector3 vector5 = transform.TransformPoint(new Vector3(0.5f, 0.5f, -0.5f));
		Vector3 vector6 = transform.TransformPoint(new Vector3(0.5f, -0.5f, -0.5f));
		Vector3 vector7 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f));
		Vector3 vector8 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector3).normalized;
		Vector3 normalized2 = (vector - vector2).normalized;
		Vector3 normalized3 = Vector3.Cross(vector3 - vector2, vector3 - vector).normalized;
		Vector3 vector9 = 0.1f * normalized;
		Vector3 vector10 = 0.1f * normalized2;
		Vector3 vector11 = 0.1f * normalized3;
		array[0] = new Triangle(vector5 - vector9 - vector10 + vector11, vector6 - vector9 + vector10 + vector11, vector8 + vector9 - vector10 + vector11);
		array[1] = new Triangle(vector8 + vector9 - vector10 + vector11, vector6 - vector9 + vector10 + vector11, vector7 + vector9 + vector10 + vector11);
		array[2] = new Triangle(vector - vector9 - vector10 - vector11, vector4 + vector9 - vector10 - vector11, vector2 - vector9 + vector10 - vector11);
		array[3] = new Triangle(vector4 + vector9 - vector10 - vector11, vector3 + vector9 + vector10 - vector11, vector2 - vector9 + vector10 - vector11);
		array[4] = new Triangle(vector - vector9 - vector10 - vector11, vector2 - vector9 + vector10 - vector11, vector5 - vector9 - vector10 + vector11);
		array[5] = new Triangle(vector2 - vector9 + vector10 - vector11, vector6 - vector9 + vector10 + vector11, vector5 - vector9 - vector10 + vector11);
		array[6] = new Triangle(vector4 + vector9 - vector10 - vector11, vector8 + vector9 - vector10 + vector11, vector3 + vector9 + vector10 - vector11);
		array[7] = new Triangle(vector3 + vector9 + vector10 - vector11, vector8 + vector9 - vector10 + vector11, vector7 + vector9 + vector10 + vector11);
		array[8] = new Triangle(vector - vector9 - vector10 - vector11, vector5 - vector9 - vector10 + vector11, vector4 + vector9 - vector10 - vector11);
		array[9] = new Triangle(vector5 - vector9 - vector10 + vector11, vector8 + vector9 - vector10 + vector11, vector4 + vector9 - vector10 - vector11);
		array[10] = new Triangle(vector2 - vector9 + vector10 - vector11, vector3 + vector9 + vector10 - vector11, vector6 - vector9 + vector10 + vector11);
		array[11] = new Triangle(vector6 - vector9 + vector10 + vector11, vector3 + vector9 + vector10 - vector11, vector7 + vector9 + vector10 + vector11);
		return new CollisionMesh(array);
	}

	private static CollisionMesh GenerateWedgeShapeVolume(GameObject volumePrefab)
	{
		Triangle[] array = new Triangle[8];
		Transform transform = volumePrefab.transform;
		Vector3 vector = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.5f));
		Vector3 vector2 = transform.TransformPoint(new Vector3(0.5f, -0.5f, -0.5f));
		Vector3 vector3 = transform.TransformPoint(new Vector3(0.5f, 0.5f, -0.5f));
		Vector3 vector4 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.5f));
		Vector3 vector5 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f));
		Vector3 vector6 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f));
		Vector3 normalized = (vector2 - vector).normalized;
		Vector3 normalized2 = (vector3 - vector).normalized;
		Vector3 normalized3 = (vector3 - vector2).normalized;
		Vector3 normalized4 = (normalized + normalized2).normalized;
		Vector3 normalized5 = (-normalized + normalized3).normalized;
		Vector3 normalized6 = (-normalized3 - normalized2).normalized;
		Vector3 normalized7 = Vector3.Cross(normalized2, normalized3).normalized;
		Vector3 vector7 = 0.1f * normalized7;
		array[0] = new Triangle(vector2 + 0.1f * normalized5 - vector7, vector3 + 0.1f * normalized6 - vector7, vector + 0.1f * normalized4 - vector7);
		array[1] = new Triangle(vector5 + 0.1f * normalized5 + vector7, vector4 + 0.1f * normalized4 + vector7, vector6 + 0.1f * normalized6 + vector7);
		array[2] = new Triangle(vector2 + 0.1f * normalized5 - vector7, vector + 0.1f * normalized4 - vector7, vector5 + 0.1f * normalized5 + vector7);
		array[3] = new Triangle(vector + 0.1f * normalized4 - vector7, vector4 + 0.1f * normalized4 + vector7, vector5 + 0.1f * normalized5 + vector7);
		array[4] = new Triangle(vector2 + 0.1f * normalized5 - vector7, vector5 + 0.1f * normalized5 + vector7, vector6 + 0.1f * normalized6 + vector7);
		array[5] = new Triangle(vector6 + 0.1f * normalized6 + vector7, vector3 + 0.1f * normalized6 - vector7, vector2 + 0.1f * normalized5 - vector7);
		array[6] = new Triangle(vector4 + 0.1f * normalized4 + vector7, vector + 0.1f * normalized4 - vector7, vector3 + 0.1f * normalized6 - vector7);
		array[7] = new Triangle(vector3 + 0.1f * normalized6 - vector7, vector6 + 0.1f * normalized6 + vector7, vector4 + 0.1f * normalized4 + vector7);
		return new CollisionMesh(array);
	}
}
