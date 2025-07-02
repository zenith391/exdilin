using System.Collections.Generic;
using Blocks;
using UnityEngine;

public static class CollisionTest
{
	public static Dictionary<string, HashSet<string>> noShapeCollides = new Dictionary<string, HashSet<string>>();

	public static void ReadNoShapeCollides()
	{
		GameObject gameObject = Resources.Load("Shape Collision Matrix") as GameObject;
		ShapeCollisionMatrix component = gameObject.GetComponent<ShapeCollisionMatrix>();
		ShapeCollisionMatrixEntry[] entries = component.entries;
		foreach (ShapeCollisionMatrixEntry shapeCollisionMatrixEntry in entries)
		{
			string shapeCategory = shapeCollisionMatrixEntry.shapeCategory;
			string[] noCollides = shapeCollisionMatrixEntry.noCollides;
			foreach (string item in noCollides)
			{
				if (!noShapeCollides.TryGetValue(shapeCategory, out var value))
				{
					value = new HashSet<string>();
					noShapeCollides[shapeCategory] = value;
				}
				value.Add(item);
			}
		}
	}

	public static void DrawCollisionMesh(CollisionMesh mesh, Color color, float duration = 0f)
	{
		Triangle[] triangles = mesh.Triangles;
		foreach (Triangle tri in triangles)
		{
			DrawTriangle(tri, color, duration);
		}
	}

	public static void DrawCollisionMeshes(CollisionMesh[] meshes, Color color)
	{
		foreach (CollisionMesh mesh in meshes)
		{
			DrawCollisionMesh(mesh, color);
		}
	}

	public static int IndexOfGlueCollisionMesh(string blockType, string glueGOName)
	{
		int num = 0;
		if (!Blocksworld.glues.ContainsKey(blockType))
		{
			Blocksworld.LoadBlock(blockType);
		}
		foreach (object item in Blocksworld.glues[blockType].transform)
		{
			Transform transform = (Transform)item;
			if (transform.gameObject.name == glueGOName)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static void DrawTriangle(Triangle tri, Color color, float duration = 0f)
	{
		Debug.DrawLine(tri.V1, tri.V2, color, duration);
		Debug.DrawLine(tri.V2, tri.V3, color, duration);
		Debug.DrawLine(tri.V3, tri.V1, color, duration);
	}

	public static void DrawBounds(Bounds bounds, Color color, float duration = 0f)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, 0f - extents.y, extents.z), center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(0f - extents.x, extents.y, extents.z), center + new Vector3(0f - extents.x, extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(0f - extents.x, 0f - extents.y, extents.z), center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(extents.x, 0f - extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, 0f - extents.z), center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(0f - extents.x, extents.y, extents.z), center + new Vector3(0f - extents.x, 0f - extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(0f - extents.x, extents.y, 0f - extents.z), center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, extents.z), center + new Vector3(0f - extents.x, extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, extents.y, 0f - extents.z), center + new Vector3(0f - extents.x, extents.y, 0f - extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, 0f - extents.y, extents.z), center + new Vector3(0f - extents.x, 0f - extents.y, extents.z), color, duration);
		Debug.DrawLine(center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), color, duration);
	}

	public static void PrintTriangle(Triangle tri)
	{
		string text = "(" + tri.V1.x + ", " + tri.V1.y + ", " + tri.V1.z + ")";
		string text2 = "(" + tri.V2.x + ", " + tri.V2.y + ", " + tri.V2.z + ")";
		string text3 = "(" + tri.V3.x + ", " + tri.V3.y + ", " + tri.V3.z + ")";
		BWLog.Info("[" + text + ", " + text2 + ", " + text3 + "]");
	}

	private static bool ShapeCategoriesNoColliderOneDirection(string cat1, string cat2)
	{
		if (noShapeCollides.TryGetValue(cat1, out var value))
		{
			return value.Contains(cat2);
		}
		return false;
	}

	private static bool ShapeCategoriesNoCollide(string cat1, string cat2)
	{
		if (!ShapeCategoriesNoColliderOneDirection(cat1, cat2))
		{
			return ShapeCategoriesNoColliderOneDirection(cat2, cat1);
		}
		return true;
	}

	public static bool Collision(Block b1, HashSet<Block> exclude = null)
	{
		Bounds shapeCollisionBounds = b1.GetShapeCollisionBounds();
		Vector3 center = shapeCollisionBounds.center;
		Collider[] array = Physics.OverlapSphere(center, shapeCollisionBounds.extents.magnitude + 1f);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.gameObject == b1.go)
			{
				continue;
			}
			Block block = BWSceneManager.FindBlock(collider.gameObject, checkChildGos: true);
			if (block == null || (exclude != null && exclude.Contains(block)) || b1 == block || !b1.ShapeMeshCanCollideWith(block))
			{
				continue;
			}
			HashSet<string> shapeCategories = b1.GetShapeCategories();
			HashSet<string> shapeCategories2 = block.GetShapeCategories();
			bool flag = false;
			foreach (string item in shapeCategories)
			{
				foreach (string item2 in shapeCategories2)
				{
					if (ShapeCategoriesNoCollide(item, item2))
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
				if (((!(b1 is BlockTerrain) && !noShapeCollideClasses.Contains("Terrain")) || (!(block is BlockTerrain) && !noShapeCollideClasses2.Contains("Terrain"))) && !noShapeCollideClasses.Overlaps(shapeCategories2) && MultiMeshMeshTest(b1.shapeMeshes, block.shapeMeshes, draw: true))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int MultiMeshMeshTestCount(CollisionMesh[] l1, CollisionMesh[] l2, bool draw = false)
	{
		int num = 0;
		foreach (CollisionMesh m in l1)
		{
			foreach (CollisionMesh m2 in l2)
			{
				if (MeshMeshTest(m, m2, draw))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool MultiMeshMeshTest(CollisionMesh[] l1, CollisionMesh[] l2, bool draw = false)
	{
		foreach (CollisionMesh m in l1)
		{
			foreach (CollisionMesh m2 in l2)
			{
				if (MeshMeshTest(m, m2, draw))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool MultiMeshMeshTest2(CollisionMesh m1, CollisionMesh[] l2, bool draw = false)
	{
		foreach (CollisionMesh m2 in l2)
		{
			if (MeshMeshTest(m1, m2, draw))
			{
				return true;
			}
		}
		return false;
	}

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
		Triangle[] triangles = m1.Triangles;
		for (int i = 0; i < triangles.Length; i++)
		{
			Triangle triangle = triangles[i];
			if (PointInConvexMesh(triangle.V1, m2) || PointInConvexMesh(triangle.V2, m2) || PointInConvexMesh(triangle.V3, m2))
			{
				return true;
			}
		}
		Triangle[] triangles2 = m2.Triangles;
		for (int j = 0; j < triangles2.Length; j++)
		{
			Triangle triangle2 = triangles2[j];
			if (PointInConvexMesh(triangle2.V1, m1) || PointInConvexMesh(triangle2.V2, m1) || PointInConvexMesh(triangle2.V3, m1))
			{
				return true;
			}
		}
		Triangle[] triangles3 = m1.Triangles;
		foreach (Triangle t in triangles3)
		{
			Triangle[] triangles4 = m2.Triangles;
			foreach (Triangle t2 in triangles4)
			{
				if (TriangleTriangleTest(t, t2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool PointInConvexMesh(Vector3 point, CollisionMesh mesh)
	{
		Triangle[] triangles = mesh.Triangles;
		for (int i = 0; i < triangles.Length; i++)
		{
			Triangle triangle = triangles[i];
			if (triangle.P.GetDistanceToPoint(point) > 0f)
			{
				return false;
			}
		}
		return true;
	}

	public static bool TriangleTriangleTest(Triangle t1, Triangle t2)
	{
		if (AnyPointsSame(t1, t2))
		{
			return true;
		}
		if (AllSameSide(t1.P, t2))
		{
			return false;
		}
		if (AllSameSide(t2.P, t1))
		{
			return false;
		}
		if (!PlanePlaneIntersection(t1.P, t2.P, out var out_origin, out var out_direction))
		{
			return false;
		}
		SortTrianglePoints(t2.P, t1, out var out_v, out var out_v2, out var out_v3);
		SortTrianglePoints(t1.P, t2, out var out_v4, out var out_v5, out var out_v6);
		float num = Vector3.Dot(t2.P.normal, out_v) + t2.P.distance;
		float num2 = Vector3.Dot(t2.P.normal, out_v2) + t2.P.distance;
		float num3 = Vector3.Dot(t2.P.normal, out_v3) + t2.P.distance;
		float num4 = Vector3.Dot(t1.P.normal, out_v4) + t1.P.distance;
		float num5 = Vector3.Dot(t1.P.normal, out_v5) + t1.P.distance;
		float num6 = Vector3.Dot(t1.P.normal, out_v6) + t1.P.distance;
		float num7 = Vector3.Dot(out_direction, out_v - out_origin);
		float num8 = Vector3.Dot(out_direction, out_v2 - out_origin);
		float num9 = Vector3.Dot(out_direction, out_v3 - out_origin);
		float num10 = Vector3.Dot(out_direction, out_v4 - out_origin);
		float num11 = Vector3.Dot(out_direction, out_v5 - out_origin);
		float num12 = Vector3.Dot(out_direction, out_v6 - out_origin);
		float a = num7 + (num8 - num7) * (num / (num - num2));
		float b = num7 + (num9 - num7) * (num / (num - num3));
		float a2 = num10 + (num11 - num10) * (num4 / (num4 - num5));
		float b2 = num10 + (num12 - num10) * (num4 / (num4 - num6));
		float num13 = Mathf.Min(a, b);
		float num14 = Mathf.Max(a, b);
		float num15 = Mathf.Min(a2, b2);
		float num16 = Mathf.Max(a2, b2);
		if (num15 <= num14)
		{
			return num13 <= num16;
		}
		return false;
	}

	private static bool AllSameSide(Plane plane, Triangle tri)
	{
		bool side = plane.GetSide(tri.V1);
		bool side2 = plane.GetSide(tri.V2);
		bool side3 = plane.GetSide(tri.V3);
		if (side == side2)
		{
			return side2 == side3;
		}
		return false;
	}

	private static bool AnyPointsSame(Triangle t1, Triangle t2)
	{
		if (!Vector3Approximately(t1.V1, t2.V1) && !Vector3Approximately(t1.V1, t2.V2) && !Vector3Approximately(t1.V1, t2.V3) && !Vector3Approximately(t1.V2, t2.V1) && !Vector3Approximately(t1.V2, t2.V2) && !Vector3Approximately(t1.V2, t2.V3) && !Vector3Approximately(t1.V3, t2.V1) && !Vector3Approximately(t1.V3, t2.V2))
		{
			return Vector3Approximately(t1.V3, t2.V3);
		}
		return true;
	}

	private static bool Vector3Approximately(Vector3 v1, Vector3 v2)
	{
		float num = v1.x - v2.x;
		float num2 = v1.y - v2.y;
		float num3 = v1.z - v2.z;
		return num * num + num2 * num2 + num3 * num3 < 0.001f;
	}

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

	public static bool RayTriangleTest(Vector3 origin, Vector3 dir, Triangle t, out Vector3 point, out Vector3 normal)
	{
		Vector3 vector = (point = origin + dir);
		normal = Vector3.zero;
		Vector3 a = vector - origin;
		Vector3 vector2 = t.V1 - origin;
		Vector3 vector3 = t.V2 - origin;
		Vector3 vector4 = t.V3 - origin;
		float num = ScalarTriple(a, vector4, vector3);
		if (num < 0f)
		{
			return false;
		}
		float num2 = ScalarTriple(a, vector2, vector4);
		if (num2 < 0f)
		{
			return false;
		}
		float num3 = ScalarTriple(a, vector3, vector2);
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

	public static float ScalarTriple(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Dot(a, Vector3.Cross(b, c));
	}

	public static bool RayMeshTest(Vector3 origin, Vector3 dir, CollisionMesh mesh)
	{
		Vector3 point = default(Vector3);
		Vector3 normal = default(Vector3);
		Triangle[] triangles = mesh.Triangles;
		foreach (Triangle t in triangles)
		{
			if (RayTriangleTest(origin, dir, t, out point, out normal))
			{
				return true;
			}
		}
		return false;
	}

	public static bool RayMeshTestClosest(Vector3 origin, Vector3 dir, CollisionMesh mesh, out Vector3 closestPoint, out Vector3 normal)
	{
		float num = 1E+09f;
		bool result = false;
		closestPoint = default(Vector3);
		normal = default(Vector3);
		Vector3 point = default(Vector3);
		Vector3 normal2 = default(Vector3);
		Triangle[] triangles = mesh.Triangles;
		foreach (Triangle t in triangles)
		{
			if (RayTriangleTest(origin, dir, t, out point, out normal2))
			{
				result = true;
				float sqrMagnitude = (origin - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					closestPoint = point;
					normal = normal2;
				}
			}
		}
		return result;
	}
}
