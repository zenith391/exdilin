using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Blocks;
using UnityEngine;

public static class Util
{
	private class TypeCounter
	{
		public string name;

		public int count;
	}

	private enum UserStatus
	{
		STATUS_NONE = 0,
		STATUS_PREMIUM = 1,
		STATUS_STEAM = 2,
		STATUS_EARLY_ACCESS = 4,
		STATUS_MODERATOR = 8,
		STATUS_LINDEN = 0x10,
		STATUS_TIER1_MEMBER = 0x20,
		STATUS_TIER2_MEMBER = 0x40,
		STATUS_TIER3_MEMBER = 0x80,
		STATUS_IOS_IPAD = 0x100,
		STATUS_IOS_IPHONE = 0x200
	}

	public const float epsilon = 0.001f;

	public static Vector3 nullVector3 = new Vector3((float)Math.PI * -10000f, (float)Math.PI * -10000f, (float)Math.PI * -10000f);

	public const int CAMERA_CHECK_LAYER_MASK = 261119;

	private static List<Vector3> lineStart = new List<Vector3>();

	private static List<Vector3> lineEnd = new List<Vector3>();

	private static List<Color> lineColor = new List<Color>();

	private static Vector3 drawPos = Vector3.zero;

	private static Quaternion drawRot = Quaternion.identity;

	private static bool drawScreenCoords = false;

	public static TileResultCode Not(TileResultCode v)
	{
		return v switch
		{
			TileResultCode.True => TileResultCode.False, 
			TileResultCode.False => TileResultCode.True, 
			_ => TileResultCode.Delayed, 
		};
	}

	public static Color Color(float r, float g, float b, float a = 255f)
	{
		float num = 255f;
		return new Color(num * r, num * g, num * b, num * a);
	}

	public static bool IsNullVector3(Vector3 v)
	{
		return (v - nullVector3).sqrMagnitude < 0.0001f;
	}

	public static bool IsNullVector3Component(float value, int index)
	{
		return Mathf.Abs(value - nullVector3[index]) < 0.0001f;
	}

	public static Vector3 Round(Vector3 v)
	{
		v.Set(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
		return v;
	}

	public static Vector3 Round2(Vector3 v)
	{
		v.Set(Mathf.Round(2f * v.x) / 2f, Mathf.Round(2f * v.y) / 2f, Mathf.Round(2f * v.z) / 2f);
		return v;
	}

	public static Vector3 RoundDirection(Vector3 v)
	{
		Vector3[] array = new Vector3[6]
		{
			Vector3.right,
			-Vector3.right,
			Vector3.up,
			-Vector3.up,
			Vector3.forward,
			-Vector3.forward
		};
		Vector3 result = Vector3.zero;
		float num = 1000f;
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = Vector3.Angle(v, array[i]);
			if (num2 < num)
			{
				num = num2;
				result = array[i];
			}
		}
		return result;
	}

	public static Vector3 Abs(Vector3 v)
	{
		v.Set(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		return v;
	}

	public static float MaxAbsWithSign(float a, float b)
	{
		if (Mathf.Abs(a) > Mathf.Abs(b))
		{
			return a;
		}
		return b;
	}

	public static float MinComponent(Vector3 v)
	{
		if (v.x < v.y && v.x < v.z)
		{
			return v.x;
		}
		if (v.y < v.z)
		{
			return v.y;
		}
		return v.z;
	}

	public static float MaxComponent(Vector3 v)
	{
		if (v.x > v.y && v.x > v.z)
		{
			return v.x;
		}
		if (v.y > v.z)
		{
			return v.y;
		}
		return v.z;
	}

	public static float MeanAbs(Vector3 v)
	{
		return (Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z)) / 3f;
	}

	public static float AngleBetween(Vector3 v1, Vector3 v2, Vector3 up)
	{
		return Vector3.Angle(v1, v2) * (float)LeftOf(v1, v2, up);
	}

	public static int LeftOf(Vector3 v1, Vector3 v2, Vector3 up)
	{
		return Math.Sign(Vector3.Dot(Vector3.Cross(v1, v2), up));
	}

	public static Vector3 ProjectOntoPlane(Vector3 vec, Vector3 planeNormal)
	{
		return vec - Vector3.Dot(vec, planeNormal) * planeNormal;
	}

	public static Vector3 ProjectScreenPointOnWorldPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 screenPoint)
	{
		Ray ray = Blocksworld.CameraScreenPointToRay(screenPoint * NormalizedScreen.scale);
		new Plane(planeNormal, planePoint).Raycast(ray, out var enter);
		return ray.GetPoint(enter);
	}

	public static Vector3 ProjectScreenPointOnWorldAxis(Vector3 planePoint, Vector3 planeNormal, Vector3 axis, Vector3 screenPoint)
	{
		Plane plane = new Plane(planeNormal, planePoint);
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(screenPoint * NormalizedScreen.scale);
		plane.Raycast(ray, out var enter);
		Vector3 point = ray.GetPoint(enter);
		return Vector3.Project(point, axis);
	}

	public static Vector3 ProjectScreenPointOnWorldAxis(Vector3 axisPoint, Vector3 axis, Vector3 screenPoint)
	{
		Plane plane;
		if (!(axis == Vector3.up))
		{
			plane = ((axis == Vector3.right) ? ((!(Blocksworld.cameraForward.y > -0.5f)) ? new Plane(Vector3.up, axisPoint) : new Plane(Vector3.forward, axisPoint)) : ((!(Blocksworld.cameraForward.y > -0.5f)) ? new Plane(Vector3.up, axisPoint) : new Plane(Vector3.right, axisPoint)));
		}
		else
		{
			Vector3 inNormal = Vector3.Cross(Vector3.up, Blocksworld.cameraRight);
			plane = new Plane(inNormal, axisPoint);
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(screenPoint * NormalizedScreen.scale);
		plane.Raycast(ray, out var enter);
		Vector3 vector = ray.GetPoint(enter) - axisPoint;
		vector.Scale(axis);
		return axisPoint + vector;
	}

	public static Vector3 WorldToScreenPoint(Vector3 worldPos, bool z)
	{
		Vector3 result = Blocksworld.mainCamera.WorldToScreenPoint(worldPos) / NormalizedScreen.scale;
		if (!z)
		{
			result.Set(result.x, result.y, 0f);
		}
		return result;
	}

	public static Vector3 WorldToScreenPointSafe(Vector3 worldPos)
	{
		Vector3 vector = Blocksworld.mainCamera.WorldToScreenPoint(worldPos) / NormalizedScreen.scale;
		return vector * Mathf.Sign(vector.z);
	}

	public static Vector3 ClampToScreen(Vector3 screenPos)
	{
		float x = Mathf.Clamp(screenPos.x, 0f, NormalizedScreen.width);
		float y = Mathf.Clamp(screenPos.y, 0f, NormalizedScreen.height);
		return new Vector3(x, y, screenPos.z);
	}

	public static float WorldToScreenScale(Vector3 atWorldPos)
	{
		float num = 10f;
		Vector3 vector = Blocksworld.mainCamera.WorldToScreenPoint(atWorldPos - 0.5f * num * Blocksworld.cameraRight) / NormalizedScreen.scale;
		Vector3 vector2 = Blocksworld.mainCamera.WorldToScreenPoint(atWorldPos + 0.5f * num * Blocksworld.cameraRight) / NormalizedScreen.scale;
		float magnitude = (vector2 - vector).magnitude;
		return magnitude / num;
	}

	public static float ScreenToWorldScale(Vector3 atWorldPos)
	{
		float magnitude = Vector3.Project(atWorldPos - Blocksworld.cameraPosition, Blocksworld.cameraForward).magnitude;
		float num = 100f;
		Vector3 vector = Blocksworld.mainCamera.ScreenToWorldPoint(new Vector3(0f, 0f, magnitude));
		Vector3 vector2 = Blocksworld.mainCamera.ScreenToWorldPoint(new Vector3(num * NormalizedScreen.scale, 0f, magnitude));
		float magnitude2 = (vector2 - vector).magnitude;
		return magnitude2 / num;
	}

	public static void DisableRenderer(GameObject parent)
	{
		parent.GetComponent<Renderer>().enabled = false;
		foreach (object item in parent.transform)
		{
			Transform transform = (Transform)item;
			transform.GetComponent<Renderer>().enabled = false;
		}
	}

	public static Mesh CopyMesh(Mesh mesh)
	{
		return new Mesh
		{
			vertices = mesh.vertices,
			triangles = mesh.triangles,
			uv = mesh.uv,
			normals = mesh.normals,
			colors = mesh.colors,
			tangents = mesh.tangents
		};
	}

	public static string Spaces(int len)
	{
		string text = string.Empty;
		for (int i = 0; i < len; i++)
		{
			text += " ";
		}
		return text;
	}

	public static string ListToString(List<object> objs)
	{
		string text = string.Empty;
		for (int i = 0; i < objs.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += objs[i].ToString();
		}
		return text;
	}

	public static string GameObjectListToString(List<GameObject> gos)
	{
		string text = string.Empty;
		for (int i = 0; i < gos.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += gos[i].name;
		}
		return text;
	}

	public static string BlockListToString(List<Block> blocks)
	{
		string text = string.Empty;
		for (int i = 0; i < blocks.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += blocks[i].go.name;
		}
		return text;
	}

	public static string StringListToString(List<string> strings)
	{
		string text = string.Empty;
		for (int i = 0; i < strings.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += strings[i];
		}
		return text;
	}

	public static bool SphereSphereTest(Vector3 p1, float r1, Vector3 p2, float r2)
	{
		Vector3 vector = p2 - p1;
		return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z < r1 * r1 + r2 * r2;
	}

	public static Vector3 ComputeCenter(IEnumerable<Block> blocks, bool usePlayModeCenter = false)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		bool flag = false;
		foreach (Block block in blocks)
		{
			if (usePlayModeCenter && (block.GetMass() <= 0f || block.IsRuntimeInvisible()))
			{
				BlockAbstractWheel blockAbstractWheel = block as BlockAbstractWheel;
				BlockAbstractMotor blockAbstractMotor = block as BlockAbstractMotor;
				if (blockAbstractWheel == null && blockAbstractMotor == null)
				{
					continue;
				}
			}
			flag = true;
			Vector3 vector = ((!usePlayModeCenter) ? block.GetCenter() : block.GetPlayModeCenter());
			Quaternion rotation = block.go.transform.rotation;
			Vector3 scale = block.GetScale();
			lhs = Vector3.Min(lhs, vector + rotation * (0.5f * scale));
			lhs = Vector3.Min(lhs, vector - rotation * (0.5f * scale));
			lhs2 = Vector3.Max(lhs2, vector + rotation * (0.5f * scale));
			lhs2 = Vector3.Max(lhs2, vector - rotation * (0.5f * scale));
		}
		if (!flag && usePlayModeCenter)
		{
			return ComputeCenter(blocks);
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	public static Vector3 ComputeCenter(Vector3[] points)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		for (int i = 0; i < points.Length; i++)
		{
			lhs = Vector3.Min(lhs, points[i]);
			lhs2 = Vector3.Max(lhs2, points[i]);
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	public static Vector3 ComputeCenter(Vector2[] points)
	{
		Vector2 lhs = new Vector2(9999999f, 9999999f);
		Vector2 lhs2 = new Vector2(-9999999f, -9999999f);
		for (int i = 0; i < points.Length; i++)
		{
			lhs = Vector2.Min(lhs, points[i]);
			lhs2 = Vector2.Max(lhs2, points[i]);
		}
		return new Vector2(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f);
	}

	public static Vector3 ComputeCenter(Transform parent)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		foreach (object item in parent)
		{
			Transform transform = (Transform)item;
			lhs = Vector3.Min(lhs, transform.position);
			lhs2 = Vector3.Max(lhs2, transform.position);
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	public static Vector3 ComputeCenter(List<GameObject> gos)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		for (int i = 0; i < gos.Count; i++)
		{
			GameObject gameObject = gos[i];
			lhs = Vector3.Min(lhs, gameObject.transform.position);
			lhs2 = Vector3.Max(lhs2, gameObject.transform.position);
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	public static Vector3 ComputeCenter(List<Chunk> chunks)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			lhs = Vector3.Min(lhs, chunk.GetPosition());
			lhs2 = Vector3.Max(lhs2, chunk.GetPosition());
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	public static Vector3 ComputePositionMean(List<Chunk> chunks)
	{
		Vector3 vector = default(Vector3);
		int num = 0;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			Vector3 position = chunk.GetPosition();
			vector += position;
			num++;
		}
		if (num > 0)
		{
			return vector / num;
		}
		return vector;
	}

	public static Bounds ComputeBounds(List<Block> blocks)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		result.center = ComputeCenter(blocks);
		result.size = Vector3.zero;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			result.Encapsulate(block.go.transform.position - block.go.transform.rotation * (0.5f * block.GetScale()));
			result.Encapsulate(block.go.transform.position + block.go.transform.rotation * (0.5f * block.GetScale()));
		}
		return result;
	}

	public static Bounds ComputeBoundsWithSize(List<Block> blocks, bool ignoreInvisible = true)
	{
		Bounds result = default(Bounds);
		bool flag = true;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if ((!(block is BlockPosition) && !(block is BlockEmitter) && !(block is BlockVolumeBlock) && !block.IsRuntimeInvisible()) || !ignoreInvisible)
			{
				Bounds bounds = new Bounds(block.go.transform.position, Abs(block.go.transform.rotation * block.size));
				if (flag)
				{
					result = bounds;
					flag = false;
				}
				else
				{
					result.Encapsulate(bounds);
				}
			}
		}
		if (flag)
		{
			for (int j = 0; j < blocks.Count; j++)
			{
				Block block2 = blocks[j];
				Bounds bounds2 = new Bounds(block2.go.transform.position, Abs(block2.go.transform.rotation * block2.size));
				if (flag)
				{
					result = bounds2;
				}
				else
				{
					result.Encapsulate(bounds2);
				}
			}
		}
		return result;
	}

	public static Bounds ComputeBoundsDetailed(List<Block> blocks)
	{
		Bounds result = default(Bounds);
		bool flag = true;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			Collider component = block.go.GetComponent<Collider>();
			if (component != null)
			{
				Bounds bounds = component.bounds;
				if (flag)
				{
					result = bounds;
					flag = false;
				}
				else
				{
					result.Encapsulate(bounds);
				}
			}
		}
		return result;
	}

	public static Bounds ComputeBoundsCustom(List<Block> blocks, Func<Block, Bounds> computer)
	{
		Bounds result = default(Bounds);
		bool flag = true;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block arg = blocks[i];
			Bounds bounds = computer(arg);
			if (flag)
			{
				result = bounds;
				flag = false;
			}
			else
			{
				result.Encapsulate(bounds);
			}
		}
		return result;
	}

	public static Vector2[] ConvexHull(Vector2[] s)
	{
		List<int> list = new List<int>();
		int num = 0;
		for (int i = 1; i < s.Length; i++)
		{
			if (s[i].x < s[num].x)
			{
				num = i;
			}
		}
		int num2 = s.Length * s.Length;
		int num3 = 0;
		do
		{
			list.Add(num);
			int num4 = 0;
			for (int j = 1; j < s.Length; j++)
			{
				Vector2 vector = s[j] - s[list[num3]];
				Vector2 vector2 = s[num4] - s[list[num3]];
				float num5 = vector.x * (0f - vector2.y) + vector.y * vector2.x;
				if (num4 == num || num5 < 0f)
				{
					num4 = j;
				}
			}
			num3++;
			num = num4;
			if (num4 == list[0])
			{
				Vector2[] array = new Vector2[list.Count];
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = s[list[k]];
				}
				return array;
			}
		}
		while (num3 <= num2);
		return null;
	}

	public static void DrawVertexNormals(GameObject go, Color color)
	{
		Mesh mesh = go.GetComponent<MeshFilter>().mesh;
		Vector3[] normals = mesh.normals;
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 start = go.transform.TransformPoint(vertices[i]);
			Vector3 end = go.transform.TransformPoint(vertices[i] + normals[i]);
			Debug.DrawLine(start, end, color);
		}
	}

	public static void PrintEverything(bool absolutelyEverything)
	{
		Dictionary<string, TypeCounter> dictionary = new Dictionary<string, TypeCounter>();
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(UnityEngine.Object));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object obj in array2)
		{
			string text = obj.GetType().ToString();
			if (!dictionary.ContainsKey(text))
			{
				dictionary[text] = new TypeCounter
				{
					name = text,
					count = 1
				};
			}
			else
			{
				dictionary[text].count++;
			}
		}
		List<TypeCounter> list = new List<TypeCounter>();
		foreach (TypeCounter value in dictionary.Values)
		{
			list.Add(value);
		}
		list.Sort((TypeCounter y, TypeCounter x) => x.count.CompareTo(y.count));
		string text2 = "Total everything " + array.Length + "\n";
		foreach (TypeCounter item in list)
		{
			string text3 = text2;
			text2 = text3 + item.name + " " + item.count + "\n";
		}
		BWLog.Info(text2);
		if (!absolutelyEverything)
		{
			return;
		}
		foreach (TypeCounter item2 in list)
		{
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			UnityEngine.Object[] array3 = array;
			foreach (UnityEngine.Object obj2 in array3)
			{
				if (obj2.GetType().ToString() == item2.name)
				{
					if (dictionary2.ContainsKey(obj2.name))
					{
						dictionary2[obj2.name]++;
					}
					else
					{
						dictionary2.Add(obj2.name, 1);
					}
				}
			}
			string text4 = "Objects of type: " + item2.name + "\n\n";
			foreach (string key in dictionary2.Keys)
			{
				string text5 = text4;
				text4 = text5 + "  " + dictionary2[key] + " by name:   " + key + "\n";
			}
			BWLog.Info(text4);
		}
	}

	public static void SetDrawTransform(Vector3 pos, Quaternion rot, bool screenCoords = false)
	{
		drawPos = pos;
		drawRot = rot;
		drawScreenCoords = screenCoords;
	}

	public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
	{
		if (drawScreenCoords)
		{
			p1 = Blocksworld.mainCamera.ScreenToWorldPoint(p1);
			p2 = Blocksworld.mainCamera.ScreenToWorldPoint(p2);
		}
		lineStart.Add(drawPos + drawRot * p1);
		lineEnd.Add(drawPos + drawRot * p2);
		lineColor.Add(color);
	}

	public static bool IsClear()
	{
		return lineStart.Count == 0;
	}

	public static void ClearDraw()
	{
		lineStart.Clear();
		lineEnd.Clear();
		lineColor.Clear();
	}

	public static void Update()
	{
		for (int i = 0; i < lineStart.Count; i++)
		{
			Debug.DrawLine(lineStart[i], lineEnd[i], lineColor[i]);
		}
	}

	public static void DrawCircle(Vector3 center, Quaternion rot, float radius, Color color)
	{
		int num = 32;
		for (int i = 0; i < num; i++)
		{
			float f = (float)(i * 2) * (float)Math.PI / (float)num;
			float f2 = (float)((i + 1) * 2) * (float)Math.PI / (float)num;
			DrawLine(center + rot * (radius * Mathf.Cos(f) * Vector3.right) + rot * (radius * Mathf.Sin(f) * Vector3.up), center + rot * (radius * Mathf.Cos(f2) * Vector3.right) + rot * (radius * Mathf.Sin(f2) * Vector3.up), color);
		}
	}

	public static void DrawRectangle(Vector3 center, Quaternion rot, float width, float height, Color color)
	{
		Vector3 vector = center + rot * new Vector3(-0.5f * width, -0.5f * height, 0f);
		Vector3 vector2 = center + rot * new Vector3(0.5f * width, -0.5f * height, 0f);
		Vector3 vector3 = center + rot * new Vector3(0.5f * width, 0.5f * height, 0f);
		Vector3 vector4 = center + rot * new Vector3(-0.5f * width, 0.5f * height, 0f);
		DrawLine(vector, vector2, color);
		DrawLine(vector2, vector3, color);
		DrawLine(vector3, vector4, color);
		DrawLine(vector4, vector, color);
	}

	public static void DrawBox(Vector3 center, Quaternion rot, Vector3 size, Color color)
	{
		DrawRectangle(center - 0.5f * size.x * (rot * Vector3.right), rot * Quaternion.Euler(0f, 90f, 0f), size.z, size.y, color);
		DrawRectangle(center + 0.5f * size.x * (rot * Vector3.right), rot * Quaternion.Euler(0f, 90f, 0f), size.z, size.y, color);
		DrawRectangle(center - 0.5f * size.y * (rot * Vector3.up), rot * Quaternion.Euler(90f, 0f, 0f), size.x, size.z, color);
		DrawRectangle(center + 0.5f * size.y * (rot * Vector3.up), rot * Quaternion.Euler(90f, 0f, 0f), size.x, size.z, color);
	}

	public static void DrawMesh(Mesh mesh, Color color)
	{
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			DrawLine(vertices[triangles[i * 3]], vertices[triangles[i * 3 + 1]], color);
			DrawLine(vertices[triangles[i * 3 + 1]], vertices[triangles[i * 3 + 2]], color);
			DrawLine(vertices[triangles[i * 3 + 2]], vertices[triangles[i * 3]], color);
		}
		Vector3[] normals = mesh.normals;
		for (int j = 0; j < normals.Length; j++)
		{
			DrawLine(vertices[j], vertices[j] + 0.1f * normals[j], color);
		}
	}

	public static void SetVanishingPoint(Camera cam, Vector2 perspectiveOffset)
	{
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		float num = 2f * cam.nearClipPlane / projectionMatrix.m00;
		float num2 = 2f * cam.nearClipPlane / projectionMatrix.m11;
		float num3 = (0f - num) / 2f - perspectiveOffset.x;
		float right = num3 + num;
		float num4 = (0f - num2) / 2f - perspectiveOffset.y;
		float top = num4 + num2;
		cam.projectionMatrix = PerspectiveOffCenter(num3, right, num4, top, cam.nearClipPlane, cam.farClipPlane);
	}

	public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		float value = 2f * near / (right - left);
		float value2 = 2f * near / (top - bottom);
		float value3 = (right + left) / (right - left);
		float value4 = (top + bottom) / (top - bottom);
		float value5 = (0f - (far + near)) / (far - near);
		float value6 = (0f - 2f * far * near) / (far - near);
		float value7 = -1f;
		return new Matrix4x4
		{
			[0, 0] = value,
			[0, 1] = 0f,
			[0, 2] = value3,
			[0, 3] = 0f,
			[1, 0] = 0f,
			[1, 1] = value2,
			[1, 2] = value4,
			[1, 3] = 0f,
			[2, 0] = 0f,
			[2, 1] = 0f,
			[2, 2] = value5,
			[2, 3] = value6,
			[3, 0] = 0f,
			[3, 1] = 0f,
			[3, 2] = value7,
			[3, 3] = 0f
		};
	}

	public static Matrix4x4 GetOrthographicMatrix(float l, float r, float b, float t, float n, float f)
	{
		return new Matrix4x4
		{
			[0, 0] = 2f / (r - l),
			[0, 1] = 0f,
			[0, 2] = 0f,
			[0, 3] = 0f,
			[1, 0] = 0f,
			[1, 1] = 2f / (t - b),
			[1, 2] = 0f,
			[1, 3] = 0f,
			[2, 0] = 0f,
			[2, 1] = 0f,
			[2, 2] = -1f / (f - n),
			[2, 3] = 0f,
			[3, 0] = (0f - (r + l)) / (r - l),
			[3, 1] = (0f - (t + b)) / (t - b),
			[3, 2] = (0f - n) / (f - n),
			[3, 3] = 1f
		};
	}

	public static void LogArray<T>(IEnumerable<T> objs)
	{
		BWLog.Info(ArrayToString(objs));
	}

	public static string ArrayToString<T>(IEnumerable<T> objs)
	{
		string text = "[ ";
		bool flag = true;
		foreach (T obj in objs)
		{
			if (!flag)
			{
				text += ", ";
			}
			text += obj.ToString();
			flag = false;
		}
		return text + " ]";
	}

	public static void DrawBounds(Bounds b, Color c, float duration = 0f)
	{
		CollisionTest.DrawBounds(b, c, duration);
	}

	public static byte[] RenderScreenshotForCoverImage()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		float num = 1024f;
		float num2 = 768f;
		float num3 = num / num2;
		float num4 = mainCamera.pixelWidth;
		float num5 = mainCamera.pixelHeight;
		float num6 = num4 / num5;
		int num7 = 0;
		int num8 = 0;
		float num9;
		float num10;
		if (num6 >= num3)
		{
			num2 = Mathf.Min(num2, num5);
			num = num2 * num3;
			num9 = num2;
			num10 = num2 * num6;
			num7 = (int)((num10 - num) / 2f);
		}
		else
		{
			num = Mathf.Min(num, num4);
			num2 = num / num3;
			num10 = num;
			num9 = num / num6;
			num8 = (int)((num9 - num2) / 2f);
		}
		int num11 = (int)num;
		int num12 = (int)num2;
		Texture2D texture2D = new Texture2D(num11, num12, TextureFormat.RGB24, mipmap: false);
		if (texture2D == null)
		{
			return null;
		}
		RenderTexture temporary = RenderTexture.GetTemporary((int)num10, (int)num9, 24);
		if (temporary == null)
		{
			UnityEngine.Object.Destroy(texture2D);
			return null;
		}
		mainCamera.targetTexture = temporary;
		mainCamera.Render();
		mainCamera.targetTexture = null;
		RenderTexture.active = temporary;
		texture2D.ReadPixels(new Rect(num7, num8, num11, num12), 0, 0, recalculateMipMaps: false);
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] result = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	public static byte[] RenderFullScreenScreenshot()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		float num = Screen.width;
		float num2 = Screen.height;
		int num3 = (int)num;
		int num4 = (int)num2;
		Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.ARGB32, mipmap: false);
		if (texture2D == null)
		{
			return null;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(num3, num4, 24);
		if (temporary == null)
		{
			UnityEngine.Object.Destroy(texture2D);
			return null;
		}
		mainCamera.targetTexture = temporary;
		mainCamera.Render();
		mainCamera.targetTexture = null;
		RenderTexture.active = temporary;
		texture2D.ReadPixels(new Rect(0f, 0f, num3, num4), 0, 0, recalculateMipMaps: false);
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] result = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	public static bool CameraVisibilityCheck(Vector3 from, Vector3 to, HashSet<Block> exempt = null, bool occludeByBroken = true, CameraVisibilityCheckInfo info = null)
	{
		Vector3 vector = to - from;
		float num = vector.magnitude;
		RaycastHit[] array = Physics.RaycastAll(from, vector.normalized, num, 261119);
		float num2 = 999999f;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			float magnitude = (raycastHit.point - from).magnitude;
			if (!(magnitude < num2))
			{
				continue;
			}
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
			if (block is BlockTerrain || (block != null && !block.isTransparent && !block.isTreasure && block != Blocksworld.worldOceanBlock && (occludeByBroken || !block.broken) && (exempt == null || !exempt.Contains(block))))
			{
				if (block.didFix || (block.chunk != null && block.chunk.go != null && block.chunk.approxSizeMaxComponent > 4.5f))
				{
					num2 = magnitude;
				}
			}
			else if (exempt != null && exempt.Contains(block))
			{
				num = magnitude;
				num2 = magnitude;
			}
		}
		bool result = num2 >= num;
		if (info != null)
		{
			info.closestDist = num2;
		}
		return result;
	}

	public static bool PointWithinTerrain(Vector3 point, bool fixedAsTerrain = false)
	{
		int hitsUp;
		int hitsDown;
		return PointWithinTerrain(point, out hitsUp, out hitsDown, fixedAsTerrain);
	}

	public static bool PointWithinTerrain(Vector3 point, out int hitsUp, out int hitsDown, bool fixedAsTerrain = false)
	{
		hitsUp = 0;
		hitsDown = 0;
		if (Blocksworld.worldOceanBlock != null && Blocksworld.worldOceanBlock.isSolid && Blocksworld.worldOceanBlock.GetWaterBounds().Contains(point))
		{
			hitsDown = 1;
			return true;
		}
		RaycastHit[] array = Physics.RaycastAll(point, Vector3.up);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject);
			if ((block is BlockTerrain blockTerrain && blockTerrain.IsSolidTerrain()) || (fixedAsTerrain && block != null && block.didFix && !(block is BlockSky) && !raycastHit.collider.isTrigger))
			{
				hitsUp++;
			}
		}
		float num = 10000f;
		RaycastHit[] array2 = Physics.RaycastAll(point + Vector3.up * num, -Vector3.up, num);
		for (int j = 0; j < array2.Length; j++)
		{
			RaycastHit raycastHit2 = array2[j];
			Block block2 = BWSceneManager.FindBlock(raycastHit2.collider.gameObject);
			if ((block2 is BlockTerrain blockTerrain2 && blockTerrain2.IsSolidTerrain()) || (fixedAsTerrain && block2 != null && block2.didFix && !(block2 is BlockSky) && !raycastHit2.collider.isTrigger))
			{
				hitsDown++;
			}
		}
		int num2 = hitsUp - hitsDown;
		bool flag = num2 == 0;
		return !flag;
	}

	public static T[] CopyArray<T>(T[] array)
	{
		T[] array2 = new T[array.Length];
		Array.Copy(array, 0, array2, 0, array.Length);
		return array2;
	}

	public static float Mod(float x, float y)
	{
		return x - y * Mathf.Floor(x / y);
	}

	public static void SmartSort(RaycastHit[] hits, Vector3 testPos)
	{
		int num = hits.Length;
		if (num < 2)
		{
			return;
		}
		if (num < 20)
		{
			for (int i = 0; i < num - 1; i++)
			{
				int num2 = i;
				for (int j = i + 1; j < num; j++)
				{
					if ((hits[j].point - testPos).sqrMagnitude < (hits[num2].point - testPos).sqrMagnitude)
					{
						num2 = j;
					}
				}
				RaycastHit raycastHit = hits[i];
				hits[i] = hits[num2];
				hits[num2] = raycastHit;
			}
		}
		else
		{
			Array.Sort(hits, new RaycastDistanceComparer(testPos));
		}
	}

	public static int GetIntArg(object[] args, int index, int defaultValue)
	{
		if (args.Length > index)
		{
			return (int)args[index];
		}
		return defaultValue;
	}

	public static bool GetIntBooleanArg(object[] args, int index, bool defaultValue)
	{
		if (args.Length > index)
		{
			return (int)args[index] != 0;
		}
		return defaultValue;
	}

	public static float GetFloatArg(object[] args, int index, float defaultValue)
	{
		if (args.Length > index)
		{
			return (float)args[index];
		}
		return defaultValue;
	}

	public static string GetStringArg(object[] args, int index, string defaultValue)
	{
		if (args.Length > index)
		{
			return (string)args[index];
		}
		return defaultValue;
	}

	public static string GetStringArgSafe(object[] args, int index, string defaultValue)
	{
		if (args.Length > index)
		{
			return args[index].ToString();
		}
		return defaultValue;
	}

	public static object GetEnumArg(object[] args, int index, string defaultValue, Type enumType)
	{
		string stringArg = GetStringArg(args, index, defaultValue);
		try
		{
			return Enum.Parse(enumType, stringArg);
		}
		catch (Exception ex)
		{
			BWLog.Error("Could not parse enum '" + defaultValue + "' with type " + enumType.Name + " " + ex.StackTrace);
			return Enum.GetValues(enumType).GetValue(0);
		}
	}

	public static Vector3 GetVector3Arg(object[] args, int index, Vector3 defaultValue)
	{
		if (args.Length > index)
		{
			return (Vector3)args[index];
		}
		return defaultValue;
	}

	public static void UnparentTransformSafely(Transform t)
	{
		Collider component = t.GetComponent<Collider>();
		if (component != null && component.enabled)
		{
			component.enabled = false;
			component.enabled = true;
		}
		t.parent = null;
	}

	public static bool IsLayer(this GameObject go, Layer layer)
	{
		return go.layer == (int)layer;
	}

	public static void SetLayerRaw(GameObject go, int layer, bool recursive = false)
	{
		go.layer = layer;
		if (!recursive)
		{
			return;
		}
		foreach (object item in go.transform)
		{
			Transform transform = (Transform)item;
			SetLayerRaw(transform.gameObject, layer, recursive);
		}
	}

	public static void SetLayer(this GameObject go, Layer layer, bool recursive = false)
	{
		go.layer = (int)layer;
		if (!recursive)
		{
			return;
		}
		foreach (object item in go.transform)
		{
			Transform transform = (Transform)item;
			transform.gameObject.SetLayer(layer, recursive: true);
		}
	}

	public static float FastAngle(Vector3 v1, Vector3 v2)
	{
		return Mathf.Acos(Mathf.Clamp(Vector3.Dot(v1, v2), -1f, 1f)) * 57.29578f;
	}

	public static bool FloatsClose(float f1, float f2)
	{
		return Mathf.Abs(f1 - f2) <= 0.001f;
	}

	public static bool FloatsClose(float f1, float f2, float d)
	{
		return Mathf.Abs(f1 - f2) <= d;
	}

	public static bool ColorsClose(Color pixelColor, Color backgroundColor, float d = 0.01f)
	{
		if (FloatsClose(pixelColor.r, backgroundColor.r, d) && FloatsClose(pixelColor.g, backgroundColor.g, d))
		{
			return FloatsClose(pixelColor.b, backgroundColor.b, d);
		}
		return false;
	}

	public static void MakeColorTransparent(Color[] pixels, Color backgroundColor)
	{
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
			if (ColorsClose(color, backgroundColor))
			{
				color.a = 0f;
				pixels[i] = color;
			}
		}
	}

	public static void RemoveBackground(string path, Color backgroundColor)
	{
		byte[] data = File.ReadAllBytes(path);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipmap: false);
		texture2D.LoadImage(data);
		Color[] pixels = texture2D.GetPixels();
		MakeColorTransparent(pixels, backgroundColor);
		texture2D.SetPixels(pixels);
		File.WriteAllBytes(path, texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
	}

	public static void SetGridMeshSize(Mesh mesh, float w, float h, float r, float yOffset = 0f)
	{
		Vector3[] vertices = mesh.vertices;
		vertices[7].x = 0f;
		vertices[7].y = yOffset;
		vertices[5].x = r;
		vertices[5].y = yOffset;
		vertices[1].x = w - r;
		vertices[1].y = yOffset;
		vertices[0].x = w;
		vertices[0].y = yOffset;
		vertices[3].x = w;
		vertices[3].y = r + yOffset;
		vertices[2].x = w - r;
		vertices[2].y = r + yOffset;
		vertices[4].x = r;
		vertices[4].y = r + yOffset;
		vertices[6].x = 0f;
		vertices[6].y = r + yOffset;
		vertices[8].x = 0f;
		vertices[8].y = h - r + yOffset;
		vertices[9].x = r;
		vertices[9].y = h - r + yOffset;
		vertices[10].x = w - r;
		vertices[10].y = h - r + yOffset;
		vertices[11].x = w;
		vertices[11].y = h - r + yOffset;
		vertices[13].x = w;
		vertices[13].y = h + yOffset;
		vertices[12].x = w - r;
		vertices[12].y = h + yOffset;
		vertices[14].x = r;
		vertices[14].y = h + yOffset;
		vertices[15].x = 0f;
		vertices[15].y = h + yOffset;
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
	}

	public static Vector3 CalculateTileOffset(Tile t)
	{
		return t.tileObject.CalculateTileOffset();
	}

	public static List<List<Tile>> CopyTiles(List<List<Tile>> tiles)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<Tile> tile in tiles)
		{
			List<Tile> list2 = new List<Tile>();
			foreach (Tile item in tile)
			{
				list2.Add(item.Clone());
			}
			list.Add(list2);
		}
		return list;
	}

	public static Vector3 ComputeMean(this List<Vector3> points)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < points.Count; i++)
		{
			zero += points[i];
		}
		if (points.Count == 0)
		{
			return zero;
		}
		return zero / points.Count;
	}

	public static string GetLongTagName(string tag)
	{
		string result = tag;
		if (int.TryParse(tag, out var result2) && result2 >= 0 && result2 < Tile.tagNames.Length)
		{
			result = Tile.tagNames[result2];
		}
		return result;
	}

	public static float GetJumpForcePerFrame(float height, float mass, int maxJumpFrames)
	{
		float num = (float)maxJumpFrames * Blocksworld.fixedDeltaTime;
		float num2 = Mathf.Sqrt(19.64f * height);
		float num3 = num2 / num;
		float num4 = num3 * num * num * 0.5f;
		num2 = Mathf.Sqrt(19.64f * (height + num4));
		num3 = num2 / num;
		return mass * num3 * Mathf.Clamp(1f + height * 0.03f, 1.25f, 1.3f);
	}

	public static bool CheckNumFishy(this Vector3 v, string prefix = "")
	{
		for (int i = 0; i < 3; i++)
		{
			float f = v[i];
			if (float.IsNaN(f))
			{
				BWLog.Info(prefix + "NaN[" + i + "]");
				return true;
			}
			if (float.IsInfinity(f))
			{
				BWLog.Info(prefix + "Inf[" + i + "]");
				return true;
			}
		}
		return false;
	}

	public static string GetBlockIconPath(string blockName, Dictionary<string, string> cache = null)
	{
		string value = null;
		string text = blockName + " HD.png";
		if (cache != null && cache.TryGetValue(text, out value))
		{
			return value;
		}
		string path = Tile.iconBasePath + "/Icons";
		string[] directories = Directory.GetDirectories(path);
		string[] array = directories;
		foreach (string path2 in array)
		{
			string[] files = Directory.GetFiles(path2);
			string[] array2 = files;
			foreach (string text2 in array2)
			{
				string fileName = Path.GetFileName(text2);
				if (fileName == text)
				{
					value = text2;
					if (cache == null)
					{
						break;
					}
				}
				if (cache != null)
				{
					cache[fileName] = text2;
				}
			}
		}
		return value;
	}

	public static void AddGroupedTilesToBlockList(List<Block> blockList)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		foreach (Block item in blockList.FindAll((Block b) => b is BlockGrouped))
		{
			BlockGroup blockGroup = ((BlockGrouped)item).group;
			if (blockGroup != null)
			{
				hashSet.UnionWith(((BlockGrouped)item).group.GetBlocks());
			}
		}
		hashSet.ExceptWith(blockList);
		blockList.AddRange(hashSet);
	}

	public static void DoWithFilesRecursive(string startDir, Action<string> action, int limit = 10000)
	{
		List<string> list = new List<string>();
		list.Add(startDir);
		int num = 0;
		while (list.Count > 0)
		{
			string path = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			string[] files = Directory.GetFiles(path);
			string[] directories = Directory.GetDirectories(path);
			for (int i = 0; i < files.Length; i++)
			{
				num++;
				if (num > limit)
				{
					BWLog.Info("Hit limit " + limit);
					return;
				}
				string fullPath = Path.GetFullPath(files[i]);
				if (!Directory.Exists(fullPath))
				{
					action(files[i]);
				}
			}
			string[] array = directories;
			foreach (string item in array)
			{
				list.Add(item);
			}
		}
	}

	public static string GetSignalName(int i)
	{
		return Convert.ToChar(i + 65).ToString();
	}

	private static string GetTagName(Tile t)
	{
		return GetTagName(t.gaf);
	}

	private static string GetTagName(GAF gaf)
	{
		string text = (string)gaf.Args[0];
		if (int.TryParse(text, out var result) && result >= 0 && result < 100)
		{
			return string.Empty + Convert.ToChar(result + 65);
		}
		return text;
	}

	public static bool RectTransformContains(Transform t, Vector3 screenPoint)
	{
		Vector2 screenPoint2 = NormalizedScreen.scale * screenPoint;
		Camera guiCamera = Blocksworld.guiCamera;
		return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)t, screenPoint2, guiCamera);
	}

	public static Vector3 CenterOfRectTransform(Transform t)
	{
		Vector3[] array = new Vector3[4];
		((RectTransform)t).GetWorldCorners(array);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 4; i++)
		{
			zero += array[i] * 0.25f;
		}
		return zero;
	}

	public static bool HasNoAngle(float rot)
	{
		rot = Mathf.Abs(rot);
		if ((!(rot > -1f) || !(rot < 1f)) && (!(rot > 89f) || !(rot < 91f)) && (!(rot > 179f) || !(rot < 181f)))
		{
			if (rot > 269f)
			{
				return rot < 271f;
			}
			return false;
		}
		return true;
	}

	public static Rect GetWorldRectForRectTransform(RectTransform rt)
	{
		Vector3[] array = new Vector3[4];
		rt.GetWorldCorners(array);
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		for (int i = 0; i < 4; i++)
		{
			num = Mathf.Min(num, array[i].x);
			num2 = Mathf.Min(num2, array[i].y);
			num3 = Mathf.Max(num3, array[i].x);
			num4 = Mathf.Max(num4, array[i].y);
		}
		return new Rect(num, num2, num3 - num, num4 - num2);
	}

	public static bool IncludeNonProductionReadyBlockItems()
	{
		if (BWEnvConfig.Flags.ContainsKey("INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS"))
		{
			return BWEnvConfig.Flags["INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS"];
		}
		return false;
	}

	public static string ObfuscateSourceForUser(string source, int userID)
	{
		StringBuilder stringBuilder = new StringBuilder(source.Length);
		stringBuilder.Append("bw");
		return MangleForUser(stringBuilder, source, userID);
	}

	public static string UnobfuscateSourceForUser(string source, int userID)
	{
		if (source[0] == 'b' && source[1] == 'w')
		{
			StringBuilder sb = new StringBuilder(source.Length);
			return MangleForUser(sb, source.Substring(2), userID);
		}
		return source;
	}

	public static string MangleForUser(StringBuilder sb, string source, int userID)
	{
		string text = "bw-source +cipher" + userID;
		for (int i = 0; i < source.Length; i++)
		{
			sb.Append((char)(source[i] ^ text[i % text.Length]));
		}
		return sb.ToString();
	}

	public static string FixNonAscii(string inputStr)
	{
		if (string.IsNullOrEmpty(inputStr))
		{
			return string.Empty;
		}
		return Regex.Replace(inputStr, "[^\\u0020-\\u007E]+", "?");
	}

	public static bool IsBlocksworldOfficialUser(int user_id)
	{
		if (BWEnvConfig.API_BASE_URL == "https://blocksworld-api.lindenlab.com")
		{
			return user_id == 24;
		}
		return false;
	}

	public static bool IsIOSExclusiveUserStatus(int user_status)
	{
		if (IsIOSUserStatus(user_status))
		{
			return !IsSteamUserStatus(user_status);
		}
		return false;
	}

	public static bool IsIOSUserStatus(int user_status)
	{
		if (user_status > 1)
		{
			return (user_status & 0x300) != 0;
		}
		return true;
	}

	public static bool IsPremiumUserStatus(int user_status)
	{
		if (user_status >= 0)
		{
			return (user_status & 1) != 0;
		}
		return false;
	}

	public static bool IsSteamUserStatus(int user_status)
	{
		if (user_status > 0)
		{
			return (user_status & 2) != 0;
		}
		return false;
	}

	public static int PremiumMembershipTier(int user_status)
	{
		if (user_status > 0)
		{
			if ((user_status & 0x80) != 0)
			{
				return 3;
			}
			if ((user_status & 0x40) != 0)
			{
				return 2;
			}
			if ((user_status & 0x20) != 0)
			{
				return 1;
			}
		}
		return 0;
	}
}
