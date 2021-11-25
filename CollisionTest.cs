using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200011C RID: 284
public static class CollisionTest
{
	// Token: 0x060013BF RID: 5055 RVA: 0x0008825C File Offset: 0x0008665C
	public static void ReadNoShapeCollides()
	{
		GameObject gameObject = Resources.Load("Shape Collision Matrix") as GameObject;
		ShapeCollisionMatrix component = gameObject.GetComponent<ShapeCollisionMatrix>();
		foreach (ShapeCollisionMatrixEntry shapeCollisionMatrixEntry in component.entries)
		{
			string shapeCategory = shapeCollisionMatrixEntry.shapeCategory;
			foreach (string item in shapeCollisionMatrixEntry.noCollides)
			{
				HashSet<string> hashSet;
				if (!CollisionTest.noShapeCollides.TryGetValue(shapeCategory, out hashSet))
				{
					hashSet = new HashSet<string>();
					CollisionTest.noShapeCollides[shapeCategory] = hashSet;
				}
				hashSet.Add(item);
			}
		}
	}

	// Token: 0x060013C0 RID: 5056 RVA: 0x00088308 File Offset: 0x00086708
	public static void DrawCollisionMesh(CollisionMesh mesh, Color color, float duration = 0f)
	{
		foreach (Triangle tri in mesh.Triangles)
		{
			CollisionTest.DrawTriangle(tri, color, duration);
		}
	}

	// Token: 0x060013C1 RID: 5057 RVA: 0x00088348 File Offset: 0x00086748
	public static void DrawCollisionMeshes(CollisionMesh[] meshes, Color color)
	{
		foreach (CollisionMesh mesh in meshes)
		{
			CollisionTest.DrawCollisionMesh(mesh, color, 0f);
		}
	}

	// Token: 0x060013C2 RID: 5058 RVA: 0x0008837C File Offset: 0x0008677C
	public static int IndexOfGlueCollisionMesh(string blockType, string glueGOName)
	{
		int num = 0;
		if (!Blocksworld.glues.ContainsKey(blockType))
		{
			Blocksworld.LoadBlock(blockType);
		}
		IEnumerator enumerator = Blocksworld.glues[blockType].transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.gameObject.name == glueGOName)
				{
					return num;
				}
				num++;
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
		return -1;
	}

	// Token: 0x060013C3 RID: 5059 RVA: 0x00088424 File Offset: 0x00086824
	public static void DrawTriangle(Triangle tri, Color color, float duration = 0f)
	{
		Debug.DrawLine(tri.V1, tri.V2, color, duration);
		Debug.DrawLine(tri.V2, tri.V3, color, duration);
		Debug.DrawLine(tri.V3, tri.V1, color, duration);
	}

	// Token: 0x060013C4 RID: 5060 RVA: 0x00088470 File Offset: 0x00086870
	public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, -extents.y, extents.z), center + new Vector3(extents.x, -extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(-extents.x, extents.y, extents.z), center + new Vector3(-extents.x, extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(-extents.x, -extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, -extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, -extents.z), center + new Vector3(extents.x, -extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(-extents.x, extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(-extents.x, extents.y, -extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(-extents.x, extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, -extents.z), center + new Vector3(-extents.x, extents.y, -extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, -extents.y, extents.z), center + new Vector3(-extents.x, -extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, -extents.y, -extents.z), center + new Vector3(-extents.x, -extents.y, -extents.z), color, duration);
	}

	// Token: 0x060013C5 RID: 5061 RVA: 0x00088808 File Offset: 0x00086C08
	public static void PrintTriangle(Triangle tri)
	{
		string text = string.Concat(new object[]
		{
			"(",
			tri.V1.x,
			", ",
			tri.V1.y,
			", ",
			tri.V1.z,
			")"
		});
		string text2 = string.Concat(new object[]
		{
			"(",
			tri.V2.x,
			", ",
			tri.V2.y,
			", ",
			tri.V2.z,
			")"
		});
		string text3 = string.Concat(new object[]
		{
			"(",
			tri.V3.x,
			", ",
			tri.V3.y,
			", ",
			tri.V3.z,
			")"
		});
		BWLog.Info(string.Concat(new string[]
		{
			"[",
			text,
			", ",
			text2,
			", ",
			text3,
			"]"
		}));
	}

	// Token: 0x060013C6 RID: 5062 RVA: 0x000889B0 File Offset: 0x00086DB0
	private static bool ShapeCategoriesNoColliderOneDirection(string cat1, string cat2)
	{
		HashSet<string> hashSet;
		return CollisionTest.noShapeCollides.TryGetValue(cat1, out hashSet) && hashSet.Contains(cat2);
	}

	// Token: 0x060013C7 RID: 5063 RVA: 0x000889D8 File Offset: 0x00086DD8
	private static bool ShapeCategoriesNoCollide(string cat1, string cat2)
	{
		return CollisionTest.ShapeCategoriesNoColliderOneDirection(cat1, cat2) || CollisionTest.ShapeCategoriesNoColliderOneDirection(cat2, cat1);
	}

	// Token: 0x060013C8 RID: 5064 RVA: 0x000889F0 File Offset: 0x00086DF0
	public static bool Collision(Block b1, HashSet<Block> exclude = null)
	{
		Bounds shapeCollisionBounds = b1.GetShapeCollisionBounds();
		Vector3 center = shapeCollisionBounds.center;
		Collider[] array = Physics.OverlapSphere(center, shapeCollisionBounds.extents.magnitude + 1f);
		foreach (Collider collider in array)
		{
			if (!(collider.gameObject == b1.go))
			{
				Block block = BWSceneManager.FindBlock(collider.gameObject, true);
				if (block != null)
				{
					if (exclude == null || !exclude.Contains(block))
					{
						if (b1 != block && b1.ShapeMeshCanCollideWith(block))
						{
							HashSet<string> shapeCategories = b1.GetShapeCategories();
							HashSet<string> shapeCategories2 = block.GetShapeCategories();
							bool flag = false;
							foreach (string cat in shapeCategories)
							{
								foreach (string cat2 in shapeCategories2)
								{
									if (CollisionTest.ShapeCategoriesNoCollide(cat, cat2))
									{
										flag = true;
										break;
									}
								}
							}
							if (!flag)
							{
								HashSet<string> noShapeCollideClasses = b1.GetNoShapeCollideClasses();
								HashSet<string> noShapeCollideClasses2 = block.GetNoShapeCollideClasses();
								if (((!(b1 is BlockTerrain) && !noShapeCollideClasses.Contains("Terrain")) || (!(block is BlockTerrain) && !noShapeCollideClasses2.Contains("Terrain"))) && !noShapeCollideClasses.Overlaps(shapeCategories2))
								{
									if (CollisionTest.MultiMeshMeshTest(b1.shapeMeshes, block.shapeMeshes, true))
									{
										return true;
									}
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	// Token: 0x060013C9 RID: 5065 RVA: 0x00088BEC File Offset: 0x00086FEC
	public static int MultiMeshMeshTestCount(CollisionMesh[] l1, CollisionMesh[] l2, bool draw = false)
	{
		int num = 0;
		foreach (CollisionMesh m in l1)
		{
			foreach (CollisionMesh m2 in l2)
			{
				if (CollisionTest.MeshMeshTest(m, m2, draw))
				{
					num++;
				}
			}
		}
		return num;
	}

	// Token: 0x060013CA RID: 5066 RVA: 0x00088C4C File Offset: 0x0008704C
	public static bool MultiMeshMeshTest(CollisionMesh[] l1, CollisionMesh[] l2, bool draw = false)
	{
		foreach (CollisionMesh m in l1)
		{
			foreach (CollisionMesh m2 in l2)
			{
				if (CollisionTest.MeshMeshTest(m, m2, draw))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060013CB RID: 5067 RVA: 0x00088CA8 File Offset: 0x000870A8
	public static bool MultiMeshMeshTest2(CollisionMesh m1, CollisionMesh[] l2, bool draw = false)
	{
		foreach (CollisionMesh m2 in l2)
		{
			if (CollisionTest.MeshMeshTest(m1, m2, draw))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060013CC RID: 5068 RVA: 0x00088CE0 File Offset: 0x000870E0
	public static bool MeshMeshTest(CollisionMesh m1, CollisionMesh m2, bool draw = false)
	{
		if (m1.Triangles.Length == 0 || m2.Triangles.Length == 0)
		{
			return false;
		}
		if (!m1.AABB.Intersects(m2.AABB))
		{
			return false;
		}
		foreach (Triangle triangle in m1.Triangles)
		{
			if (CollisionTest.PointInConvexMesh(triangle.V1, m2) || CollisionTest.PointInConvexMesh(triangle.V2, m2) || CollisionTest.PointInConvexMesh(triangle.V3, m2))
			{
				return true;
			}
		}
		foreach (Triangle triangle2 in m2.Triangles)
		{
			if (CollisionTest.PointInConvexMesh(triangle2.V1, m1) || CollisionTest.PointInConvexMesh(triangle2.V2, m1) || CollisionTest.PointInConvexMesh(triangle2.V3, m1))
			{
				return true;
			}
		}
		foreach (Triangle t in m1.Triangles)
		{
			foreach (Triangle t2 in m2.Triangles)
			{
				if (CollisionTest.TriangleTriangleTest(t, t2))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060013CD RID: 5069 RVA: 0x00088E5C File Offset: 0x0008725C
	public static bool PointInConvexMesh(Vector3 point, CollisionMesh mesh)
	{
		foreach (Triangle triangle in mesh.Triangles)
		{
			if (triangle.P.GetDistanceToPoint(point) > 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060013CE RID: 5070 RVA: 0x00088EB0 File Offset: 0x000872B0
	public static bool TriangleTriangleTest(Triangle t1, Triangle t2)
	{
		if (CollisionTest.AnyPointsSame(t1, t2))
		{
			return true;
		}
		if (CollisionTest.AllSameSide(t1.P, t2))
		{
			return false;
		}
		if (CollisionTest.AllSameSide(t2.P, t1))
		{
			return false;
		}
		Vector3 b;
		Vector3 lhs;
		if (!CollisionTest.PlanePlaneIntersection(t1.P, t2.P, out b, out lhs))
		{
			return false;
		}
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		CollisionTest.SortTrianglePoints(t2.P, t1, out vector, out vector2, out vector3);
		Vector3 vector4;
		Vector3 vector5;
		Vector3 vector6;
		CollisionTest.SortTrianglePoints(t1.P, t2, out vector4, out vector5, out vector6);
		float num = Vector3.Dot(t2.P.normal, vector) + t2.P.distance;
		float num2 = Vector3.Dot(t2.P.normal, vector2) + t2.P.distance;
		float num3 = Vector3.Dot(t2.P.normal, vector3) + t2.P.distance;
		float num4 = Vector3.Dot(t1.P.normal, vector4) + t1.P.distance;
		float num5 = Vector3.Dot(t1.P.normal, vector5) + t1.P.distance;
		float num6 = Vector3.Dot(t1.P.normal, vector6) + t1.P.distance;
		float num7 = Vector3.Dot(lhs, vector - b);
		float num8 = Vector3.Dot(lhs, vector2 - b);
		float num9 = Vector3.Dot(lhs, vector3 - b);
		float num10 = Vector3.Dot(lhs, vector4 - b);
		float num11 = Vector3.Dot(lhs, vector5 - b);
		float num12 = Vector3.Dot(lhs, vector6 - b);
		float a = num7 + (num8 - num7) * (num / (num - num2));
		float b2 = num7 + (num9 - num7) * (num / (num - num3));
		float a2 = num10 + (num11 - num10) * (num4 / (num4 - num5));
		float b3 = num10 + (num12 - num10) * (num4 / (num4 - num6));
		float num13 = Mathf.Min(a, b2);
		float num14 = Mathf.Max(a, b2);
		float num15 = Mathf.Min(a2, b3);
		float num16 = Mathf.Max(a2, b3);
		return num15 <= num14 && num13 <= num16;
	}

	// Token: 0x060013CF RID: 5071 RVA: 0x0008911C File Offset: 0x0008751C
	private static bool AllSameSide(Plane plane, Triangle tri)
	{
		bool side = plane.GetSide(tri.V1);
		bool side2 = plane.GetSide(tri.V2);
		bool side3 = plane.GetSide(tri.V3);
		return side == side2 && side2 == side3;
	}

	// Token: 0x060013D0 RID: 5072 RVA: 0x00089164 File Offset: 0x00087564
	private static bool AnyPointsSame(Triangle t1, Triangle t2)
	{
		return CollisionTest.Vector3Approximately(t1.V1, t2.V1) || CollisionTest.Vector3Approximately(t1.V1, t2.V2) || CollisionTest.Vector3Approximately(t1.V1, t2.V3) || CollisionTest.Vector3Approximately(t1.V2, t2.V1) || CollisionTest.Vector3Approximately(t1.V2, t2.V2) || CollisionTest.Vector3Approximately(t1.V2, t2.V3) || CollisionTest.Vector3Approximately(t1.V3, t2.V1) || CollisionTest.Vector3Approximately(t1.V3, t2.V2) || CollisionTest.Vector3Approximately(t1.V3, t2.V3);
	}

	// Token: 0x060013D1 RID: 5073 RVA: 0x00089248 File Offset: 0x00087648
	private static bool Vector3Approximately(Vector3 v1, Vector3 v2)
	{
		float num = v1.x - v2.x;
		float num2 = v1.y - v2.y;
		float num3 = v1.z - v2.z;
		return num * num + num2 * num2 + num3 * num3 < 0.001f;
	}

	// Token: 0x060013D2 RID: 5074 RVA: 0x00089298 File Offset: 0x00087698
	private static bool PlanePlaneIntersection(Plane p1, Plane p2, out Vector3 out_origin, out Vector3 out_direction)
	{
		out_direction = Vector3.Cross(p1.normal, p2.normal);
		float sqrMagnitude = out_direction.sqrMagnitude;
		if (sqrMagnitude < 0.001f)
		{
			out_origin = new Vector3(0f, 0f, 0f);
			return false;
		}
		out_origin = Vector3.Cross(p2.distance * p1.normal - p1.distance * p2.normal, out_direction) / sqrMagnitude;
		return true;
	}

	// Token: 0x060013D3 RID: 5075 RVA: 0x0008932C File Offset: 0x0008772C
	private static void SortTrianglePoints(Plane p, Triangle t, out Vector3 out_v1, out Vector3 out_v2, out Vector3 out_v3)
	{
		bool side = p.GetSide(t.V1);
		bool side2 = p.GetSide(t.V2);
		bool side3 = p.GetSide(t.V3);
		if (side == side2)
		{
			out_v1 = t.V3;
			out_v2 = t.V1;
			out_v3 = t.V2;
		}
		else if (side == side3)
		{
			out_v1 = t.V2;
			out_v2 = t.V1;
			out_v3 = t.V3;
		}
		else
		{
			out_v1 = t.V1;
			out_v2 = t.V2;
			out_v3 = t.V3;
		}
	}

	// Token: 0x060013D4 RID: 5076 RVA: 0x000893F8 File Offset: 0x000877F8
	public static bool RayTriangleTest(Vector3 origin, Vector3 dir, Triangle t, out Vector3 point, out Vector3 normal)
	{
		Vector3 vector = origin + dir;
		point = vector;
		normal = Vector3.zero;
		Vector3 a = vector - origin;
		Vector3 vector2 = t.V1 - origin;
		Vector3 vector3 = t.V2 - origin;
		Vector3 vector4 = t.V3 - origin;
		float num = CollisionTest.ScalarTriple(a, vector4, vector3);
		if (num < 0f)
		{
			return false;
		}
		float num2 = CollisionTest.ScalarTriple(a, vector2, vector4);
		if (num2 < 0f)
		{
			return false;
		}
		float num3 = CollisionTest.ScalarTriple(a, vector3, vector2);
		if (num3 < 0f)
		{
			return false;
		}
		float num4 = num + num2 + num3;
		if (num4 > 1E-06f)
		{
			float num5 = 1f / num4;
			num *= num5;
			num2 *= num5;
			num3 *= num5;
			point = t.V1 * num + t.V2 * num2 + t.V3 * num3;
			normal = t.P.normal;
			return true;
		}
		return false;
	}

	// Token: 0x060013D5 RID: 5077 RVA: 0x0008952B File Offset: 0x0008792B
	public static float ScalarTriple(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Dot(a, Vector3.Cross(b, c));
	}

	// Token: 0x060013D6 RID: 5078 RVA: 0x0008953C File Offset: 0x0008793C
	public static bool RayMeshTest(Vector3 origin, Vector3 dir, CollisionMesh mesh)
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		foreach (Triangle t in mesh.Triangles)
		{
			if (CollisionTest.RayTriangleTest(origin, dir, t, out vector, out vector2))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060013D7 RID: 5079 RVA: 0x0008959C File Offset: 0x0008799C
	public static bool RayMeshTestClosest(Vector3 origin, Vector3 dir, CollisionMesh mesh, out Vector3 closestPoint, out Vector3 normal)
	{
		float num = 1E+09f;
		bool result = false;
		closestPoint = default(Vector3);
		normal = default(Vector3);
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		foreach (Triangle t in mesh.Triangles)
		{
			if (CollisionTest.RayTriangleTest(origin, dir, t, out vector, out vector2))
			{
				result = true;
				float sqrMagnitude = (origin - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					closestPoint = vector;
					normal = vector2;
				}
			}
		}
		return result;
	}

	// Token: 0x04000F81 RID: 3969
	public static Dictionary<string, HashSet<string>> noShapeCollides = new Dictionary<string, HashSet<string>>();
}
