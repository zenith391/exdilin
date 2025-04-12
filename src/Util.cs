using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Blocks;
using UnityEngine;

// Token: 0x0200032D RID: 813
public static class Util
{
	// Token: 0x060024AA RID: 9386 RVA: 0x0010BB62 File Offset: 0x00109F62
	public static TileResultCode Not(TileResultCode v)
	{
		if (v == TileResultCode.True)
		{
			return TileResultCode.False;
		}
		if (v != TileResultCode.False)
		{
			return TileResultCode.Delayed;
		}
		return TileResultCode.True;
	}

	// Token: 0x060024AB RID: 9387 RVA: 0x0010BB7C File Offset: 0x00109F7C
	public static Color Color(float r, float g, float b, float a = 255f)
	{
        float num = 255f / 1f;
		return new Color(num * r, num * g, num * b, num * a);
	}

	// Token: 0x060024AC RID: 9388 RVA: 0x0010BBA0 File Offset: 0x00109FA0
	public static bool IsNullVector3(Vector3 v)
	{
		return (v - Util.nullVector3).sqrMagnitude < 0.0001f;
	}

	// Token: 0x060024AD RID: 9389 RVA: 0x0010BBC7 File Offset: 0x00109FC7
	public static bool IsNullVector3Component(float value, int index)
	{
		return Mathf.Abs(value - Util.nullVector3[index]) < 0.0001f;
	}

	// Token: 0x060024AE RID: 9390 RVA: 0x0010BBE2 File Offset: 0x00109FE2
	public static Vector3 Round(Vector3 v)
	{
		v.Set(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
		return v;
	}

	// Token: 0x060024AF RID: 9391 RVA: 0x0010BC10 File Offset: 0x0010A010
	public static Vector3 Round2(Vector3 v)
	{
		v.Set(Mathf.Round(2f * v.x) / 2f, Mathf.Round(2f * v.y) / 2f, Mathf.Round(2f * v.z) / 2f);
		return v;
	}

	// Token: 0x060024B0 RID: 9392 RVA: 0x0010BC70 File Offset: 0x0010A070
	public static Vector3 RoundDirection(Vector3 v)
	{
		Vector3[] array = new Vector3[]
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

	// Token: 0x060024B1 RID: 9393 RVA: 0x0010BD46 File Offset: 0x0010A146
	public static Vector3 Abs(Vector3 v)
	{
		v.Set(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		return v;
	}

	// Token: 0x060024B2 RID: 9394 RVA: 0x0010BD74 File Offset: 0x0010A174
	public static float MaxAbsWithSign(float a, float b)
	{
		if (Mathf.Abs(a) > Mathf.Abs(b))
		{
			return a;
		}
		return b;
	}

	// Token: 0x060024B3 RID: 9395 RVA: 0x0010BD8C File Offset: 0x0010A18C
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

	// Token: 0x060024B4 RID: 9396 RVA: 0x0010BDEC File Offset: 0x0010A1EC
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

	// Token: 0x060024B5 RID: 9397 RVA: 0x0010BE49 File Offset: 0x0010A249
	public static float MeanAbs(Vector3 v)
	{
		return (Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z)) / 3f;
	}

	// Token: 0x060024B6 RID: 9398 RVA: 0x0010BE77 File Offset: 0x0010A277
	public static float AngleBetween(Vector3 v1, Vector3 v2, Vector3 up)
	{
		return Vector3.Angle(v1, v2) * (float)Util.LeftOf(v1, v2, up);
	}

	// Token: 0x060024B7 RID: 9399 RVA: 0x0010BE8A File Offset: 0x0010A28A
	public static int LeftOf(Vector3 v1, Vector3 v2, Vector3 up)
	{
		return Math.Sign(Vector3.Dot(Vector3.Cross(v1, v2), up));
	}

	// Token: 0x060024B8 RID: 9400 RVA: 0x0010BE9E File Offset: 0x0010A29E
	public static Vector3 ProjectOntoPlane(Vector3 vec, Vector3 planeNormal)
	{
		return vec - Vector3.Dot(vec, planeNormal) * planeNormal;
	}

	// Token: 0x060024B9 RID: 9401 RVA: 0x0010BEB4 File Offset: 0x0010A2B4
	public static Vector3 ProjectScreenPointOnWorldPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 screenPoint)
	{
		Ray ray = Blocksworld.CameraScreenPointToRay(screenPoint * NormalizedScreen.scale);
		Plane plane = new Plane(planeNormal, planePoint);
		float distance;
		plane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	// Token: 0x060024BA RID: 9402 RVA: 0x0010BEF0 File Offset: 0x0010A2F0
	public static Vector3 ProjectScreenPointOnWorldAxis(Vector3 planePoint, Vector3 planeNormal, Vector3 axis, Vector3 screenPoint)
	{
		Plane plane = new Plane(planeNormal, planePoint);
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(screenPoint * NormalizedScreen.scale);
		float distance;
		plane.Raycast(ray, out distance);
		Vector3 point = ray.GetPoint(distance);
		return Vector3.Project(point, axis);
	}

	// Token: 0x060024BB RID: 9403 RVA: 0x0010BF38 File Offset: 0x0010A338
	public static Vector3 ProjectScreenPointOnWorldAxis(Vector3 axisPoint, Vector3 axis, Vector3 screenPoint)
	{
		Plane plane;
		if (axis == Vector3.up)
		{
			Vector3 inNormal = Vector3.Cross(Vector3.up, Blocksworld.cameraRight);
			plane = new Plane(inNormal, axisPoint);
		}
		else if (axis == Vector3.right)
		{
			if (Blocksworld.cameraForward.y > -0.5f)
			{
				plane = new Plane(Vector3.forward, axisPoint);
			}
			else
			{
				plane = new Plane(Vector3.up, axisPoint);
			}
		}
		else if (Blocksworld.cameraForward.y > -0.5f)
		{
			plane = new Plane(Vector3.right, axisPoint);
		}
		else
		{
			plane = new Plane(Vector3.up, axisPoint);
		}
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(screenPoint * NormalizedScreen.scale);
		float distance;
		plane.Raycast(ray, out distance);
		Vector3 b = ray.GetPoint(distance) - axisPoint;
		b.Scale(axis);
		return axisPoint + b;
	}

	// Token: 0x060024BC RID: 9404 RVA: 0x0010C030 File Offset: 0x0010A430
	public static Vector3 WorldToScreenPoint(Vector3 worldPos, bool z)
	{
		Vector3 result = Blocksworld.mainCamera.WorldToScreenPoint(worldPos) / NormalizedScreen.scale;
		if (!z)
		{
			result.Set(result.x, result.y, 0f);
		}
		return result;
	}

	// Token: 0x060024BD RID: 9405 RVA: 0x0010C074 File Offset: 0x0010A474
	public static Vector3 WorldToScreenPointSafe(Vector3 worldPos)
	{
		Vector3 a = Blocksworld.mainCamera.WorldToScreenPoint(worldPos) / NormalizedScreen.scale;
		return a * Mathf.Sign(a.z);
	}

	// Token: 0x060024BE RID: 9406 RVA: 0x0010C0AC File Offset: 0x0010A4AC
	public static Vector3 ClampToScreen(Vector3 screenPos)
	{
		float x = Mathf.Clamp(screenPos.x, 0f, (float)NormalizedScreen.width);
		float y = Mathf.Clamp(screenPos.y, 0f, (float)NormalizedScreen.height);
		return new Vector3(x, y, screenPos.z);
	}

	// Token: 0x060024BF RID: 9407 RVA: 0x0010C0F8 File Offset: 0x0010A4F8
	public static float WorldToScreenScale(Vector3 atWorldPos)
	{
		float num = 10f;
		Vector3 b = Blocksworld.mainCamera.WorldToScreenPoint(atWorldPos - 0.5f * num * Blocksworld.cameraRight) / NormalizedScreen.scale;
		Vector3 a = Blocksworld.mainCamera.WorldToScreenPoint(atWorldPos + 0.5f * num * Blocksworld.cameraRight) / NormalizedScreen.scale;
		float magnitude = (a - b).magnitude;
		return magnitude / num;
	}

	// Token: 0x060024C0 RID: 9408 RVA: 0x0010C178 File Offset: 0x0010A578
	public static float ScreenToWorldScale(Vector3 atWorldPos)
	{
		float magnitude = Vector3.Project(atWorldPos - Blocksworld.cameraPosition, Blocksworld.cameraForward).magnitude;
		float num = 100f;
		Vector3 b = Blocksworld.mainCamera.ScreenToWorldPoint(new Vector3(0f, 0f, magnitude));
		Vector3 a = Blocksworld.mainCamera.ScreenToWorldPoint(new Vector3(num * NormalizedScreen.scale, 0f, magnitude));
		float magnitude2 = (a - b).magnitude;
		return magnitude2 / num;
	}

	// Token: 0x060024C1 RID: 9409 RVA: 0x0010C1FC File Offset: 0x0010A5FC
	public static void DisableRenderer(GameObject parent)
	{
		parent.GetComponent<Renderer>().enabled = false;
		IEnumerator enumerator = parent.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.GetComponent<Renderer>().enabled = false;
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

	// Token: 0x060024C2 RID: 9410 RVA: 0x0010C274 File Offset: 0x0010A674
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

	// Token: 0x060024C3 RID: 9411 RVA: 0x0010C2D0 File Offset: 0x0010A6D0
	public static string Spaces(int len)
	{
		string text = string.Empty;
		for (int i = 0; i < len; i++)
		{
			text += " ";
		}
		return text;
	}

	// Token: 0x060024C4 RID: 9412 RVA: 0x0010C304 File Offset: 0x0010A704
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

	// Token: 0x060024C5 RID: 9413 RVA: 0x0010C358 File Offset: 0x0010A758
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

	// Token: 0x060024C6 RID: 9414 RVA: 0x0010C3AC File Offset: 0x0010A7AC
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

	// Token: 0x060024C7 RID: 9415 RVA: 0x0010C404 File Offset: 0x0010A804
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

	// Token: 0x060024C8 RID: 9416 RVA: 0x0010C450 File Offset: 0x0010A850
	public static bool SphereSphereTest(Vector3 p1, float r1, Vector3 p2, float r2)
	{
		Vector3 vector = p2 - p1;
		return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z < r1 * r1 + r2 * r2;
	}

	// Token: 0x060024C9 RID: 9417 RVA: 0x0010C4A0 File Offset: 0x0010A8A0
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
			Vector3 a = (!usePlayModeCenter) ? block.GetCenter() : block.GetPlayModeCenter();
			Quaternion rotation = block.go.transform.rotation;
			Vector3 scale = block.GetScale();
			lhs = Vector3.Min(lhs, a + rotation * (0.5f * scale));
			lhs = Vector3.Min(lhs, a - rotation * (0.5f * scale));
			lhs2 = Vector3.Max(lhs2, a + rotation * (0.5f * scale));
			lhs2 = Vector3.Max(lhs2, a - rotation * (0.5f * scale));
		}
		if (!flag && usePlayModeCenter)
		{
			return Util.ComputeCenter(blocks, false);
		}
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	// Token: 0x060024CA RID: 9418 RVA: 0x0010C69C File Offset: 0x0010AA9C
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

	// Token: 0x060024CB RID: 9419 RVA: 0x0010C76C File Offset: 0x0010AB6C
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

	// Token: 0x060024CC RID: 9420 RVA: 0x0010C81C File Offset: 0x0010AC1C
	public static Vector3 ComputeCenter(Transform parent)
	{
		Vector3 lhs = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 lhs2 = new Vector3(-9999999f, -9999999f, -9999999f);
		IEnumerator enumerator = parent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				lhs = Vector3.Min(lhs, transform.position);
				lhs2 = Vector3.Max(lhs2, transform.position);
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
		return new Vector3(lhs.x + (lhs2.x - lhs.x) / 2f, lhs.y + (lhs2.y - lhs.y) / 2f, lhs.z + (lhs2.z - lhs.z) / 2f);
	}

	// Token: 0x060024CD RID: 9421 RVA: 0x0010C91C File Offset: 0x0010AD1C
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

	// Token: 0x060024CE RID: 9422 RVA: 0x0010C9F4 File Offset: 0x0010ADF4
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

	// Token: 0x060024CF RID: 9423 RVA: 0x0010CAC4 File Offset: 0x0010AEC4
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
		return (num <= 0) ? vector : (vector / (float)num);
	}

	// Token: 0x060024D0 RID: 9424 RVA: 0x0010CB24 File Offset: 0x0010AF24
	public static Bounds ComputeBounds(List<Block> blocks)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		result.center = Util.ComputeCenter(blocks, false);
		result.size = Vector3.zero;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			result.Encapsulate(block.go.transform.position - block.go.transform.rotation * (0.5f * block.GetScale()));
			result.Encapsulate(block.go.transform.position + block.go.transform.rotation * (0.5f * block.GetScale()));
		}
		return result;
	}

	// Token: 0x060024D1 RID: 9425 RVA: 0x0010CC00 File Offset: 0x0010B000
	public static Bounds ComputeBoundsWithSize(List<Block> blocks, bool ignoreInvisible = true)
	{
		Bounds result = default(Bounds);
		bool flag = true;
		for (int i = 0; i < blocks.Count; i++)
		{
			Block block = blocks[i];
			if ((!(block is BlockPosition) && !(block is BlockEmitter) && !(block is BlockVolumeBlock) && !block.IsRuntimeInvisible()) || !ignoreInvisible)
			{
				Bounds bounds = new Bounds(block.go.transform.position, Util.Abs(block.go.transform.rotation * block.size));
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
				Bounds bounds2 = new Bounds(block2.go.transform.position, Util.Abs(block2.go.transform.rotation * block2.size));
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

	// Token: 0x060024D2 RID: 9426 RVA: 0x0010CD3C File Offset: 0x0010B13C
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

	// Token: 0x060024D3 RID: 9427 RVA: 0x0010CDB0 File Offset: 0x0010B1B0
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

	// Token: 0x060024D4 RID: 9428 RVA: 0x0010CE0C File Offset: 0x0010B20C
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
		for (;;)
		{
			list.Add(num);
			int num4 = 0;
			for (int j = 1; j < s.Length; j++)
			{
				Vector2 vector = s[j] - s[list[num3]];
				Vector2 vector2 = s[num4] - s[list[num3]];
				float num5 = vector.x * -vector2.y + vector.y * vector2.x;
				if (num4 == num || num5 < 0f)
				{
					num4 = j;
				}
			}
			num3++;
			num = num4;
			if (num4 == list[0])
			{
				break;
			}
			if (num3 > num2)
			{
				goto Block_7;
			}
		}
		Vector2[] array = new Vector2[list.Count];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = s[list[k]];
		}
		return array;
		Block_7:
		return null;
	}

	// Token: 0x060024D5 RID: 9429 RVA: 0x0010CF78 File Offset: 0x0010B378
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

	// Token: 0x060024D6 RID: 9430 RVA: 0x0010D000 File Offset: 0x0010B400
	public static void PrintEverything(bool absolutelyEverything)
	{
		Dictionary<string, Util.TypeCounter> dictionary = new Dictionary<string, Util.TypeCounter>();
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(UnityEngine.Object));
		foreach (UnityEngine.Object @object in array)
		{
			string text = @object.GetType().ToString();
			if (!dictionary.ContainsKey(text))
			{
				dictionary[text] = new Util.TypeCounter
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
		List<Util.TypeCounter> list = new List<Util.TypeCounter>();
		foreach (Util.TypeCounter item in dictionary.Values)
		{
			list.Add(item);
		}
		list.Sort((Util.TypeCounter y, Util.TypeCounter x) => x.count.CompareTo(y.count));
		string text2 = "Total everything " + array.Length + "\n";
		foreach (Util.TypeCounter typeCounter in list)
		{
			string text3 = text2;
			text2 = string.Concat(new object[]
			{
				text3,
				typeCounter.name,
				" ",
				typeCounter.count,
				"\n"
			});
		}
		BWLog.Info(text2);
		if (!absolutelyEverything)
		{
			return;
		}
		foreach (Util.TypeCounter typeCounter2 in list)
		{
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			foreach (UnityEngine.Object object2 in array)
			{
				if (object2.GetType().ToString() == typeCounter2.name)
				{
					if (dictionary2.ContainsKey(object2.name))
					{
						string text3;
						Dictionary<string, int> dictionary3;
						(dictionary3 = dictionary2)[text3 = object2.name] = dictionary3[text3] + 1;
					}
					else
					{
						dictionary2.Add(object2.name, 1);
					}
				}
			}
			string text4 = "Objects of type: " + typeCounter2.name + "\n\n";
			foreach (string text5 in dictionary2.Keys)
			{
				string text6 = text4;
				text4 = string.Concat(new object[]
				{
					text6,
					"  ",
					dictionary2[text5],
					" by name:   ",
					text5,
					"\n"
				});
			}
			BWLog.Info(text4);
		}
	}

	// Token: 0x060024D7 RID: 9431 RVA: 0x0010D368 File Offset: 0x0010B768
	public static void SetDrawTransform(Vector3 pos, Quaternion rot, bool screenCoords = false)
	{
		Util.drawPos = pos;
		Util.drawRot = rot;
		Util.drawScreenCoords = screenCoords;
	}

	// Token: 0x060024D8 RID: 9432 RVA: 0x0010D37C File Offset: 0x0010B77C
	public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
	{
		if (Util.drawScreenCoords)
		{
			p1 = Blocksworld.mainCamera.ScreenToWorldPoint(p1);
			p2 = Blocksworld.mainCamera.ScreenToWorldPoint(p2);
		}
		Util.lineStart.Add(Util.drawPos + Util.drawRot * p1);
		Util.lineEnd.Add(Util.drawPos + Util.drawRot * p2);
		Util.lineColor.Add(color);
	}

	// Token: 0x060024D9 RID: 9433 RVA: 0x0010D3F6 File Offset: 0x0010B7F6
	public static bool IsClear()
	{
		return Util.lineStart.Count == 0;
	}

	// Token: 0x060024DA RID: 9434 RVA: 0x0010D405 File Offset: 0x0010B805
	public static void ClearDraw()
	{
		Util.lineStart.Clear();
		Util.lineEnd.Clear();
		Util.lineColor.Clear();
	}

	// Token: 0x060024DB RID: 9435 RVA: 0x0010D428 File Offset: 0x0010B828
	public static void Update()
	{
		for (int i = 0; i < Util.lineStart.Count; i++)
		{
			Debug.DrawLine(Util.lineStart[i], Util.lineEnd[i], Util.lineColor[i]);
		}
	}

	// Token: 0x060024DC RID: 9436 RVA: 0x0010D478 File Offset: 0x0010B878
	public static void DrawCircle(Vector3 center, Quaternion rot, float radius, Color color)
	{
		int num = 32;
		for (int i = 0; i < num; i++)
		{
			float f = (float)(i * 2) * 3.14159274f / (float)num;
			float f2 = (float)((i + 1) * 2) * 3.14159274f / (float)num;
			Util.DrawLine(center + rot * (radius * Mathf.Cos(f) * Vector3.right) + rot * (radius * Mathf.Sin(f) * Vector3.up), center + rot * (radius * Mathf.Cos(f2) * Vector3.right) + rot * (radius * Mathf.Sin(f2) * Vector3.up), color);
		}
	}

	// Token: 0x060024DD RID: 9437 RVA: 0x0010D534 File Offset: 0x0010B934
	public static void DrawRectangle(Vector3 center, Quaternion rot, float width, float height, Color color)
	{
		Vector3 vector = center + rot * new Vector3(-0.5f * width, -0.5f * height, 0f);
		Vector3 vector2 = center + rot * new Vector3(0.5f * width, -0.5f * height, 0f);
		Vector3 vector3 = center + rot * new Vector3(0.5f * width, 0.5f * height, 0f);
		Vector3 vector4 = center + rot * new Vector3(-0.5f * width, 0.5f * height, 0f);
		Util.DrawLine(vector, vector2, color);
		Util.DrawLine(vector2, vector3, color);
		Util.DrawLine(vector3, vector4, color);
		Util.DrawLine(vector4, vector, color);
	}

	// Token: 0x060024DE RID: 9438 RVA: 0x0010D5FC File Offset: 0x0010B9FC
	public static void DrawBox(Vector3 center, Quaternion rot, Vector3 size, Color color)
	{
		Util.DrawRectangle(center - 0.5f * size.x * (rot * Vector3.right), rot * Quaternion.Euler(0f, 90f, 0f), size.z, size.y, color);
		Util.DrawRectangle(center + 0.5f * size.x * (rot * Vector3.right), rot * Quaternion.Euler(0f, 90f, 0f), size.z, size.y, color);
		Util.DrawRectangle(center - 0.5f * size.y * (rot * Vector3.up), rot * Quaternion.Euler(90f, 0f, 0f), size.x, size.z, color);
		Util.DrawRectangle(center + 0.5f * size.y * (rot * Vector3.up), rot * Quaternion.Euler(90f, 0f, 0f), size.x, size.z, color);
	}

	// Token: 0x060024DF RID: 9439 RVA: 0x0010D750 File Offset: 0x0010BB50
	public static void DrawMesh(Mesh mesh, Color color)
	{
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			Util.DrawLine(vertices[triangles[i * 3]], vertices[triangles[i * 3 + 1]], color);
			Util.DrawLine(vertices[triangles[i * 3 + 1]], vertices[triangles[i * 3 + 2]], color);
			Util.DrawLine(vertices[triangles[i * 3 + 2]], vertices[triangles[i * 3]], color);
		}
		Vector3[] normals = mesh.normals;
		for (int j = 0; j < normals.Length; j++)
		{
			Util.DrawLine(vertices[j], vertices[j] + 0.1f * normals[j], color);
		}
	}

	// Token: 0x060024E0 RID: 9440 RVA: 0x0010D858 File Offset: 0x0010BC58
	public static void SetVanishingPoint(Camera cam, Vector2 perspectiveOffset)
	{
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
		float num = 2f * cam.nearClipPlane / projectionMatrix.m00;
		float num2 = 2f * cam.nearClipPlane / projectionMatrix.m11;
		float num3 = -num / 2f - perspectiveOffset.x;
		float right = num3 + num;
		float num4 = -num2 / 2f - perspectiveOffset.y;
		float top = num4 + num2;
		cam.projectionMatrix = Util.PerspectiveOffCenter(num3, right, num4, top, cam.nearClipPlane, cam.farClipPlane);
	}

	// Token: 0x060024E1 RID: 9441 RVA: 0x0010D8E4 File Offset: 0x0010BCE4
	public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		float value = 2f * near / (right - left);
		float value2 = 2f * near / (top - bottom);
		float value3 = (right + left) / (right - left);
		float value4 = (top + bottom) / (top - bottom);
		float value5 = -(far + near) / (far - near);
		float value6 = -(2f * far * near) / (far - near);
		float value7 = -1f;
		Matrix4x4 result = default(Matrix4x4);
		result[0, 0] = value;
		result[0, 1] = 0f;
		result[0, 2] = value3;
		result[0, 3] = 0f;
		result[1, 0] = 0f;
		result[1, 1] = value2;
		result[1, 2] = value4;
		result[1, 3] = 0f;
		result[2, 0] = 0f;
		result[2, 1] = 0f;
		result[2, 2] = value5;
		result[2, 3] = value6;
		result[3, 0] = 0f;
		result[3, 1] = 0f;
		result[3, 2] = value7;
		result[3, 3] = 0f;
		return result;
	}

	// Token: 0x060024E2 RID: 9442 RVA: 0x0010DA18 File Offset: 0x0010BE18
	public static Matrix4x4 GetOrthographicMatrix(float l, float r, float b, float t, float n, float f)
	{
		Matrix4x4 result = default(Matrix4x4);
		result[0, 0] = 2f / (r - l);
		result[0, 1] = 0f;
		result[0, 2] = 0f;
		result[0, 3] = 0f;
		result[1, 0] = 0f;
		result[1, 1] = 2f / (t - b);
		result[1, 2] = 0f;
		result[1, 3] = 0f;
		result[2, 0] = 0f;
		result[2, 1] = 0f;
		result[2, 2] = -1f / (f - n);
		result[2, 3] = 0f;
		result[3, 0] = -(r + l) / (r - l);
		result[3, 1] = -(t + b) / (t - b);
		result[3, 2] = -n / (f - n);
		result[3, 3] = 1f;
		return result;
	}

	// Token: 0x060024E3 RID: 9443 RVA: 0x0010DB26 File Offset: 0x0010BF26
	public static void LogArray<T>(IEnumerable<T> objs)
	{
		BWLog.Info(Util.ArrayToString<T>(objs));
	}

	// Token: 0x060024E4 RID: 9444 RVA: 0x0010DB34 File Offset: 0x0010BF34
	public static string ArrayToString<T>(IEnumerable<T> objs)
	{
		string str = "[ ";
		bool flag = true;
		foreach (T t in objs)
		{
			if (!flag)
			{
				str += ", ";
			}
			str += t.ToString();
			flag = false;
		}
		return str + " ]";
	}

	// Token: 0x060024E5 RID: 9445 RVA: 0x0010DBBC File Offset: 0x0010BFBC
	public static void DrawBounds(Bounds b, Color c, float duration = 0f)
	{
		CollisionTest.DrawBounds(b, c, duration);
	}

	// Token: 0x060024E6 RID: 9446 RVA: 0x0010DBC8 File Offset: 0x0010BFC8
	public static byte[] RenderScreenshotForCoverImage()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		float width = 1024f;
		float height = 768f;
		float ratio = width / height;
		float camWidth = (float)mainCamera.pixelWidth;
		float camHeight = (float)mainCamera.pixelHeight;
		float camRatio = camWidth / camHeight;
		int num7 = 0;
		int num8 = 0;
		float num9;
		float num10;
		if (camRatio >= ratio)
		{
			height = Mathf.Min(height, camHeight);
			width = height * ratio;
			num9 = height;
			num10 = height * camRatio;
			num7 = (int)((num10 - width) / 2f);
		}
		else
		{
			width = Mathf.Min(width, camWidth);
			height = width / ratio;
			num10 = width;
			num9 = width / camRatio;
			num8 = (int)((num9 - height) / 2f);
		}
		int num11 = (int) width;
		int num12 = (int) height;
		Texture2D texture2D = new Texture2D(num11, num12, TextureFormat.RGB24, false);
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
		texture2D.ReadPixels(new Rect((float)num7, (float)num8, (float)num11, (float)num12), 0, 0, false);
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] result = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	// Token: 0x060024E7 RID: 9447 RVA: 0x0010DD04 File Offset: 0x0010C104
	public static byte[] RenderFullScreenScreenshot()
	{
		Camera mainCamera = Blocksworld.mainCamera;
		float num = (float)Screen.width;
		float num2 = (float)Screen.height;
		int num3 = (int)num;
		int num4 = (int)num2;
		Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.ARGB32, false);
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
		texture2D.ReadPixels(new Rect(0f, 0f, (float)num3, (float)num4), 0, 0, false);
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(temporary);
		byte[] result = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	// Token: 0x060024E8 RID: 9448 RVA: 0x0010DDC4 File Offset: 0x0010C1C4
	public static bool CameraVisibilityCheck(Vector3 from, Vector3 to, HashSet<Block> exempt = null, bool occludeByBroken = true, CameraVisibilityCheckInfo info = null)
	{
		Vector3 vector = to - from;
		float num = vector.magnitude;
		RaycastHit[] array = Physics.RaycastAll(from, vector.normalized, num, 261119);
		float num2 = 999999f;
		foreach (RaycastHit raycastHit in array)
		{
			float magnitude = (raycastHit.point - from).magnitude;
			if (magnitude < num2)
			{
				Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
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
		}
		bool result = num2 >= num;
		if (info != null)
		{
			info.closestDist = num2;
		}
		return result;
	}

	// Token: 0x060024E9 RID: 9449 RVA: 0x0010DF34 File Offset: 0x0010C334
	public static bool PointWithinTerrain(Vector3 point, bool fixedAsTerrain = false)
	{
		int num;
		int num2;
		return Util.PointWithinTerrain(point, out num, out num2, fixedAsTerrain);
	}

	// Token: 0x060024EA RID: 9450 RVA: 0x0010DF4C File Offset: 0x0010C34C
	public static bool PointWithinTerrain(Vector3 point, out int hitsUp, out int hitsDown, bool fixedAsTerrain = false)
	{
		hitsUp = 0;
		hitsDown = 0;
		if (Blocksworld.worldOceanBlock != null && Blocksworld.worldOceanBlock.isSolid && Blocksworld.worldOceanBlock.GetWaterBounds().Contains(point))
		{
			hitsDown = 1;
			return true;
		}
		foreach (RaycastHit raycastHit in Physics.RaycastAll(point, Vector3.up))
		{
			Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
			BlockTerrain blockTerrain = block as BlockTerrain;
			if ((blockTerrain != null && blockTerrain.IsSolidTerrain()) || (fixedAsTerrain && block != null && block.didFix && !(block is BlockSky) && !raycastHit.collider.isTrigger))
			{
				hitsUp++;
			}
		}
		float num = 10000f;
		foreach (RaycastHit raycastHit2 in Physics.RaycastAll(point + Vector3.up * num, -Vector3.up, num))
		{
			Block block2 = BWSceneManager.FindBlock(raycastHit2.collider.gameObject, false);
			BlockTerrain blockTerrain2 = block2 as BlockTerrain;
			if ((blockTerrain2 != null && blockTerrain2.IsSolidTerrain()) || (fixedAsTerrain && block2 != null && block2.didFix && !(block2 is BlockSky) && !raycastHit2.collider.isTrigger))
			{
				hitsDown++;
			}
		}
		int num2 = hitsUp - hitsDown;
		bool flag = num2 == 0;
		return !flag;
	}

	// Token: 0x060024EB RID: 9451 RVA: 0x0010E100 File Offset: 0x0010C500
	public static T[] CopyArray<T>(T[] array)
	{
		T[] array2 = new T[array.Length];
		Array.Copy(array, 0, array2, 0, array.Length);
		return array2;
	}

	// Token: 0x060024EC RID: 9452 RVA: 0x0010E123 File Offset: 0x0010C523
	public static float Mod(float x, float y)
	{
		return x - y * Mathf.Floor(x / y);
	}

	// Token: 0x060024ED RID: 9453 RVA: 0x0010E134 File Offset: 0x0010C534
	public static void SmartSort(RaycastHit[] hits, Vector3 testPos)
	{
		int num = hits.Length;
		if (num >= 2)
		{
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
				Array.Sort<RaycastHit>(hits, new RaycastDistanceComparer(testPos));
			}
		}
	}

	// Token: 0x060024EE RID: 9454 RVA: 0x0010E20B File Offset: 0x0010C60B
	public static int GetIntArg(object[] args, int index, int defaultValue)
	{
		return (args.Length <= index) ? defaultValue : ((int)args[index]);
	}

	// Token: 0x060024EF RID: 9455 RVA: 0x0010E224 File Offset: 0x0010C624
	public static bool GetIntBooleanArg(object[] args, int index, bool defaultValue)
	{
		return (args.Length <= index) ? defaultValue : ((int)args[index] != 0);
	}

	// Token: 0x060024F0 RID: 9456 RVA: 0x0010E243 File Offset: 0x0010C643
	public static float GetFloatArg(object[] args, int index, float defaultValue)
	{
		return (args.Length <= index) ? defaultValue : ((float)args[index]);
	}

	// Token: 0x060024F1 RID: 9457 RVA: 0x0010E25C File Offset: 0x0010C65C
	public static string GetStringArg(object[] args, int index, string defaultValue)
	{
		return (args.Length <= index) ? defaultValue : ((string)args[index]);
	}

	// Token: 0x060024F2 RID: 9458 RVA: 0x0010E275 File Offset: 0x0010C675
	public static string GetStringArgSafe(object[] args, int index, string defaultValue)
	{
		return (args.Length <= index) ? defaultValue : args[index].ToString();
	}

	// Token: 0x060024F3 RID: 9459 RVA: 0x0010E290 File Offset: 0x0010C690
	public static object GetEnumArg(object[] args, int index, string defaultValue, Type enumType)
	{
		string stringArg = Util.GetStringArg(args, index, defaultValue);
		object result;
		try
		{
			result = Enum.Parse(enumType, stringArg);
		}
		catch (Exception ex)
		{
			BWLog.Error(string.Concat(new string[]
			{
				"Could not parse enum '",
				defaultValue,
				"' with type ",
				enumType.Name,
				" ",
				ex.StackTrace
			}));
			result = Enum.GetValues(enumType).GetValue(0);
		}
		return result;
	}

	// Token: 0x060024F4 RID: 9460 RVA: 0x0010E318 File Offset: 0x0010C718
	public static Vector3 GetVector3Arg(object[] args, int index, Vector3 defaultValue)
	{
		return (args.Length <= index) ? defaultValue : ((Vector3)args[index]);
	}

	// Token: 0x060024F5 RID: 9461 RVA: 0x0010E334 File Offset: 0x0010C734
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

	// Token: 0x060024F6 RID: 9462 RVA: 0x0010E374 File Offset: 0x0010C774
	public static bool IsLayer(this GameObject go, Layer layer)
	{
		return go.layer == (int)layer;
	}

	// Token: 0x060024F7 RID: 9463 RVA: 0x0010E380 File Offset: 0x0010C780
	public static void SetLayerRaw(GameObject go, int layer, bool recursive = false)
	{
		go.layer = layer;
		if (recursive)
		{
			IEnumerator enumerator = go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					Util.SetLayerRaw(transform.gameObject, layer, recursive);
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

	// Token: 0x060024F8 RID: 9464 RVA: 0x0010E3F8 File Offset: 0x0010C7F8
	public static void SetLayer(this GameObject go, Layer layer, bool recursive = false)
	{
		go.layer = (int)layer;
		if (recursive)
		{
			IEnumerator enumerator = go.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.SetLayer(layer, true);
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

	// Token: 0x060024F9 RID: 9465 RVA: 0x0010E470 File Offset: 0x0010C870
	public static float FastAngle(Vector3 v1, Vector3 v2)
	{
		return Mathf.Acos(Mathf.Clamp(Vector3.Dot(v1, v2), -1f, 1f)) * 57.29578f;
	}

	// Token: 0x060024FA RID: 9466 RVA: 0x0010E493 File Offset: 0x0010C893
	public static bool FloatsClose(float f1, float f2)
	{
		return Mathf.Abs(f1 - f2) <= 0.001f;
	}

	// Token: 0x060024FB RID: 9467 RVA: 0x0010E4A7 File Offset: 0x0010C8A7
	public static bool FloatsClose(float f1, float f2, float d)
	{
		return Mathf.Abs(f1 - f2) <= d;
	}

	// Token: 0x060024FC RID: 9468 RVA: 0x0010E4B8 File Offset: 0x0010C8B8
	public static bool ColorsClose(Color pixelColor, Color backgroundColor, float d = 0.01f)
	{
		return Util.FloatsClose(pixelColor.r, backgroundColor.r, d) && Util.FloatsClose(pixelColor.g, backgroundColor.g, d) && Util.FloatsClose(pixelColor.b, backgroundColor.b, d);
	}

	// Token: 0x060024FD RID: 9469 RVA: 0x0010E510 File Offset: 0x0010C910
	public static void MakeColorTransparent(Color[] pixels, Color backgroundColor)
	{
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
			if (Util.ColorsClose(color, backgroundColor, 0.01f))
			{
				color.a = 0f;
				pixels[i] = color;
			}
		}
	}

	// Token: 0x060024FE RID: 9470 RVA: 0x0010E568 File Offset: 0x0010C968
	public static void RemoveBackground(string path, Color backgroundColor)
	{
		byte[] data = File.ReadAllBytes(path);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		texture2D.LoadImage(data);
		Color[] pixels = texture2D.GetPixels();
		Util.MakeColorTransparent(pixels, backgroundColor);
		texture2D.SetPixels(pixels);
		File.WriteAllBytes(path, texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
	}

	// Token: 0x060024FF RID: 9471 RVA: 0x0010E5B8 File Offset: 0x0010C9B8
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

	// Token: 0x06002500 RID: 9472 RVA: 0x0010E7CF File Offset: 0x0010CBCF
	public static Vector3 CalculateTileOffset(Tile t)
	{
		return t.tileObject.CalculateTileOffset();
	}

	// Token: 0x06002501 RID: 9473 RVA: 0x0010E7DC File Offset: 0x0010CBDC
	public static List<List<Tile>> CopyTiles(List<List<Tile>> tiles)
	{
		List<List<Tile>> list = new List<List<Tile>>();
		foreach (List<Tile> list2 in tiles)
		{
			List<Tile> list3 = new List<Tile>();
			foreach (Tile tile in list2)
			{
				list3.Add(tile.Clone());
			}
			list.Add(list3);
		}
		return list;
	}

	// Token: 0x06002502 RID: 9474 RVA: 0x0010E894 File Offset: 0x0010CC94
	public static Vector3 ComputeMean(this List<Vector3> points)
	{
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < points.Count; i++)
		{
			vector += points[i];
		}
		return (points.Count != 0) ? (vector / (float)points.Count) : vector;
	}

	// Token: 0x06002503 RID: 9475 RVA: 0x0010E8EC File Offset: 0x0010CCEC
	public static string GetLongTagName(string tag)
	{
		string result = tag;
		int num;
		if (int.TryParse(tag, out num) && num >= 0 && num < Tile.tagNames.Length)
		{
			result = Tile.tagNames[num];
		}
		return result;
	}

	// Token: 0x06002504 RID: 9476 RVA: 0x0010E928 File Offset: 0x0010CD28
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

	// Token: 0x06002505 RID: 9477 RVA: 0x0010E990 File Offset: 0x0010CD90
	public static bool CheckNumFishy(this Vector3 v, string prefix = "")
	{
		for (int i = 0; i < 3; i++)
		{
			float f = v[i];
			if (float.IsNaN(f))
			{
				BWLog.Info(string.Concat(new object[]
				{
					prefix,
					"NaN[",
					i,
					"]"
				}));
				return true;
			}
			if (float.IsInfinity(f))
			{
				BWLog.Info(string.Concat(new object[]
				{
					prefix,
					"Inf[",
					i,
					"]"
				}));
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002506 RID: 9478 RVA: 0x0010EA30 File Offset: 0x0010CE30
	public static string GetBlockIconPath(string blockName, Dictionary<string, string> cache = null)
	{
		string result = null;
		string text = blockName + " HD.png";
		if (cache != null && cache.TryGetValue(text, out result))
		{
			return result;
		}
		string path = Tile.iconBasePath + "/Icons";
		string[] directories = Directory.GetDirectories(path);
		foreach (string path2 in directories)
		{
			string[] files = Directory.GetFiles(path2);
			foreach (string text2 in files)
			{
				string fileName = Path.GetFileName(text2);
				if (fileName == text)
				{
					result = text2;
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
		return result;
	}

	// Token: 0x06002507 RID: 9479 RVA: 0x0010EB00 File Offset: 0x0010CF00
	public static void AddGroupedTilesToBlockList(List<Block> blockList)
	{
		HashSet<Block> hashSet = new HashSet<Block>();
		foreach (Block block in blockList.FindAll((Block b) => b is BlockGrouped))
		{
			BlockGroup group = ((BlockGrouped)block).group;
			if (group != null)
			{
				hashSet.UnionWith(((BlockGrouped)block).group.GetBlocks());
			}
		}
		hashSet.ExceptWith(blockList);
		blockList.AddRange(hashSet);
	}

	// Token: 0x06002508 RID: 9480 RVA: 0x0010EBB0 File Offset: 0x0010CFB0
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
			foreach (string item in directories)
			{
				list.Add(item);
			}
		}
	}

	// Token: 0x06002509 RID: 9481 RVA: 0x0010EC90 File Offset: 0x0010D090
	public static string GetSignalName(int i)
	{
		return Convert.ToChar(i + 65).ToString();
	}

	// Token: 0x0600250A RID: 9482 RVA: 0x0010ECB4 File Offset: 0x0010D0B4
	private static string GetTagName(Tile t)
	{
		return Util.GetTagName(t.gaf);
	}

	// Token: 0x0600250B RID: 9483 RVA: 0x0010ECC4 File Offset: 0x0010D0C4
	private static string GetTagName(GAF gaf)
	{
		string text = (string)gaf.Args[0];
		int num;
		if (int.TryParse(text, out num) && num >= 0 && num < 100)
		{
			return string.Empty + Convert.ToChar(num + 65);
		}
		return text;
	}

	// Token: 0x0600250C RID: 9484 RVA: 0x0010ED18 File Offset: 0x0010D118
	public static bool RectTransformContains(Transform t, Vector3 screenPoint)
	{
		Vector2 screenPoint2 = NormalizedScreen.scale * screenPoint;
		Camera guiCamera = Blocksworld.guiCamera;
		return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)t, screenPoint2, guiCamera);
	}

	// Token: 0x0600250D RID: 9485 RVA: 0x0010ED4C File Offset: 0x0010D14C
	public static Vector3 CenterOfRectTransform(Transform t)
	{
		Vector3[] array = new Vector3[4];
		((RectTransform)t).GetWorldCorners(array);
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < 4; i++)
		{
			vector += array[i] * 0.25f;
		}
		return vector;
	}

	// Token: 0x0600250E RID: 9486 RVA: 0x0010EDA4 File Offset: 0x0010D1A4
	public static bool HasNoAngle(float rot)
	{
		rot = Mathf.Abs(rot);
		return (rot > -1f && rot < 1f) || (rot > 89f && rot < 91f) || (rot > 179f && rot < 181f) || (rot > 269f && rot < 271f);
	}

	// Token: 0x0600250F RID: 9487 RVA: 0x0010EE14 File Offset: 0x0010D214
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

	// Token: 0x06002510 RID: 9488 RVA: 0x0010EEBE File Offset: 0x0010D2BE
	public static bool IncludeNonProductionReadyBlockItems()
	{
		return BWEnvConfig.Flags.ContainsKey("INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS") && BWEnvConfig.Flags["INCLUDE_NON_PRODUCTION_READY_BLOCK_ITEMS"];
	}

	// Token: 0x06002511 RID: 9489 RVA: 0x0010EEE8 File Offset: 0x0010D2E8
	public static string ObfuscateSourceForUser(string source, int userID)
	{
		StringBuilder stringBuilder = new StringBuilder(source.Length);
		stringBuilder.Append("bw");
		return Util.MangleForUser(stringBuilder, source, userID);
	}

	// Token: 0x06002512 RID: 9490 RVA: 0x0010EF18 File Offset: 0x0010D318
	public static string UnobfuscateSourceForUser(string source, int userID)
	{
		if (source[0] == 'b' && source[1] == 'w')
		{
			StringBuilder sb = new StringBuilder(source.Length);
			return Util.MangleForUser(sb, source.Substring(2), userID);
		}
		return source;
	}

	// Token: 0x06002513 RID: 9491 RVA: 0x0010EF60 File Offset: 0x0010D360
	public static string MangleForUser(StringBuilder sb, string source, int userID)
	{
		string text = "bw-source +cipher" + userID;
		for (int i = 0; i < source.Length; i++)
		{
			sb.Append((char) (source[i] ^ text[i % text.Length]));
		}
		return sb.ToString();
	}

	// Token: 0x06002514 RID: 9492 RVA: 0x0010EFB9 File Offset: 0x0010D3B9
	public static string FixNonAscii(string inputStr)
	{
		if (string.IsNullOrEmpty(inputStr))
		{
			return string.Empty;
		}
		return Regex.Replace(inputStr, "[^\\u0020-\\u007E]+", "?");
	}

	// Token: 0x06002515 RID: 9493 RVA: 0x0010EFDC File Offset: 0x0010D3DC
	public static bool IsBlocksworldOfficialUser(int user_id)
	{
        if (BWEnvConfig.API_BASE_URL == "https://blocksworld-api.lindenlab.com")
        {
            return user_id == 24;
        }
        else
        {
            return false;
        }
	}

	// Token: 0x06002516 RID: 9494 RVA: 0x0010EFE3 File Offset: 0x0010D3E3
	public static bool IsIOSExclusiveUserStatus(int user_status)
	{
		return Util.IsIOSUserStatus(user_status) && !Util.IsSteamUserStatus(user_status);
	}

	// Token: 0x06002517 RID: 9495 RVA: 0x0010EFFC File Offset: 0x0010D3FC
	public static bool IsIOSUserStatus(int user_status)
	{
		return user_status <= 1 || (user_status & 768) != 0;
	}

	// Token: 0x06002518 RID: 9496 RVA: 0x0010F015 File Offset: 0x0010D415
	public static bool IsPremiumUserStatus(int user_status)
	{
		return user_status >= 0 && (user_status & 1) != 0;
	}

	// Token: 0x06002519 RID: 9497 RVA: 0x0010F02A File Offset: 0x0010D42A
	public static bool IsSteamUserStatus(int user_status)
	{
		return user_status > 0 && (user_status & 2) != 0;
	}

	// Token: 0x0600251A RID: 9498 RVA: 0x0010F03F File Offset: 0x0010D43F
	public static int PremiumMembershipTier(int user_status)
	{
		if (user_status > 0)
		{
			if ((user_status & 128) != 0)
			{
				return 3;
			}
			if ((user_status & 64) != 0)
			{
				return 2;
			}
			if ((user_status & 32) != 0)
			{
				return 1;
			}
		}
		return 0;
	}

	// Token: 0x04001FA5 RID: 8101
	public const float epsilon = 0.001f;

	// Token: 0x04001FA6 RID: 8102
	public static Vector3 nullVector3 = new Vector3(-31415.9277f, -31415.9277f, -31415.9277f);

	// Token: 0x04001FA7 RID: 8103
	public const int CAMERA_CHECK_LAYER_MASK = 261119;

	// Token: 0x04001FA8 RID: 8104
	private static List<Vector3> lineStart = new List<Vector3>();

	// Token: 0x04001FA9 RID: 8105
	private static List<Vector3> lineEnd = new List<Vector3>();

	// Token: 0x04001FAA RID: 8106
	private static List<Color> lineColor = new List<Color>();

	// Token: 0x04001FAB RID: 8107
	private static Vector3 drawPos = Vector3.zero;

	// Token: 0x04001FAC RID: 8108
	private static Quaternion drawRot = Quaternion.identity;

	// Token: 0x04001FAD RID: 8109
	private static bool drawScreenCoords = false;

	// Token: 0x0200032E RID: 814
	private class TypeCounter
	{
		// Token: 0x04001FB0 RID: 8112
		public string name;

		// Token: 0x04001FB1 RID: 8113
		public int count;
	}

	// Token: 0x0200032F RID: 815
	private enum UserStatus
	{
		// Token: 0x04001FB3 RID: 8115
		STATUS_NONE,
		// Token: 0x04001FB4 RID: 8116
		STATUS_PREMIUM,
		// Token: 0x04001FB5 RID: 8117
		STATUS_STEAM,
		// Token: 0x04001FB6 RID: 8118
		STATUS_EARLY_ACCESS = 4,
		// Token: 0x04001FB7 RID: 8119
		STATUS_MODERATOR = 8,
		// Token: 0x04001FB8 RID: 8120
		STATUS_LINDEN = 16,
		// Token: 0x04001FB9 RID: 8121
		STATUS_TIER1_MEMBER = 32,
		// Token: 0x04001FBA RID: 8122
		STATUS_TIER2_MEMBER = 64,
		// Token: 0x04001FBB RID: 8123
		STATUS_TIER3_MEMBER = 128,
		// Token: 0x04001FBC RID: 8124
		STATUS_IOS_IPAD = 256,
		// Token: 0x04001FBD RID: 8125
		STATUS_IOS_IPHONE = 512
	}
}
