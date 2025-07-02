using System;
using UnityEngine;

public static class HudMeshUtils
{
	public static Mesh GetMesh(GameObject go)
	{
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component == null)
		{
			return null;
		}
		return component.sharedMesh;
	}

	public static GameObject CreateQuad()
	{
		GameObject gameObject = new GameObject("Quad");
		gameObject.layer = LayerMask.NameToLayer("GUI");
		MeshUtils.AddBWDefaultMeshRenderer(gameObject);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		array = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f)
		};
		array2 = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		array3 = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.vertices = array;
		mesh.triangles = array3;
		mesh.uv = array2;
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		return gameObject;
	}

	public static GameObject CreateMeshObject(string name, Texture2D texture)
	{
		return CreateMeshObject(name, texture, Vector2.one);
	}

	public static GameObject CreateMeshObject(string name, Texture2D texture, Vector2 uvMax)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = LayerMask.NameToLayer("GUI");
		MeshRenderer meshRenderer = MeshUtils.AddBWDefaultMeshRenderer(gameObject);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		int num = 4;
		int num2 = 4;
		int num3 = 6;
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num2];
		int[] array3 = new int[num3];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector3.zero;
		}
		array3 = new int[6] { 0, 1, 2, 1, 3, 2 };
		array2 = new Vector2[4]
		{
			new Vector2(0f, uvMax.y),
			new Vector2(uvMax.x, uvMax.y),
			new Vector2(0f, 0f),
			new Vector2(uvMax.x, 0f)
		};
		mesh.vertices = array;
		mesh.triangles = array3;
		mesh.uv = array2;
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		meshRenderer.sharedMaterial = HudMeshOnGUI.templateMaterial;
		meshRenderer.material.SetTexture("_MainTex", texture);
		return gameObject;
	}

	public static GameObject CreateNineSidedMeshObject(string name, Texture2D texture, RectOffset border, Vector2 uvMax)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = LayerMask.NameToLayer("GUI");
		MeshRenderer meshRenderer = MeshUtils.AddBWDefaultMeshRenderer(gameObject);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		float num = texture.width;
		float num2 = texture.height;
		float x = (float)border.left / num;
		float x2 = uvMax.x - (float)border.right / num;
		float y = (float)border.bottom / num2;
		float y2 = uvMax.y - (float)border.top / num2;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[16];
		mesh.uv = new Vector2[16]
		{
			new Vector2(0f, uvMax.y),
			new Vector2(x, uvMax.y),
			new Vector2(x2, uvMax.y),
			new Vector2(uvMax.x, uvMax.y),
			new Vector2(0f, y2),
			new Vector2(x, y2),
			new Vector2(x2, y2),
			new Vector2(uvMax.x, y2),
			new Vector2(0f, y),
			new Vector2(x, y),
			new Vector2(x2, y),
			new Vector2(uvMax.x, y),
			new Vector2(0f, 0f),
			new Vector2(x, 0f),
			new Vector2(x2, 0f),
			new Vector2(uvMax.x, 0f)
		};
		mesh.triangles = new int[54]
		{
			0, 1, 4, 4, 1, 5, 1, 2, 5, 5,
			2, 6, 2, 3, 6, 6, 3, 7, 4, 5,
			8, 8, 5, 9, 5, 6, 9, 9, 6, 10,
			6, 7, 10, 10, 7, 11, 8, 9, 12, 12,
			9, 13, 9, 10, 13, 13, 10, 14, 10, 11,
			14, 14, 11, 15
		};
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		meshRenderer.sharedMaterial = HudMeshOnGUI.templateMaterial;
		meshRenderer.material.SetTexture("_MainTex", texture);
		return gameObject;
	}

	public static void UpdateVertPositionsNineSided(Mesh mesh, Rect r, RectOffset border, float depth, bool invertY = true)
	{
		r = new Rect(Mathf.Floor(r.x), Mathf.Floor(r.y), Mathf.Floor(r.width), Mathf.Floor(r.height));
		Vector3[] vertices = mesh.vertices;
		Vector3 vector = new Vector3(r.x, r.y, depth);
		float pixelScale = NormalizedScreen.pixelScale;
		float num = (float)border.left * pixelScale;
		float num2 = (float)border.right * pixelScale;
		float num3 = (float)border.top * pixelScale;
		float num4 = (float)border.bottom * pixelScale;
		if ((float)border.horizontal > r.width)
		{
			num *= r.width / (float)border.horizontal;
			num2 *= r.width / (float)border.horizontal;
		}
		if ((float)border.vertical > r.height)
		{
			num3 *= r.height / (float)border.vertical;
			num4 *= r.height / (float)border.vertical;
		}
		vertices[0] = vector;
		vertices[1] = vector + num * Vector3.right;
		vertices[2] = vector + (r.width - num2) * Vector3.right;
		vertices[3] = vector + r.width * Vector3.right;
		Vector3 vector2 = num3 * Vector3.up;
		vertices[4] = vertices[0] + vector2;
		vertices[5] = vertices[1] + vector2;
		vertices[6] = vertices[2] + vector2;
		vertices[7] = vertices[3] + vector2;
		vector2 = (r.height - num4) * Vector3.up;
		vertices[8] = vertices[0] + vector2;
		vertices[9] = vertices[1] + vector2;
		vertices[10] = vertices[2] + vector2;
		vertices[11] = vertices[3] + vector2;
		vector2 = r.height * Vector3.up;
		vertices[12] = vertices[0] + vector2;
		vertices[13] = vertices[1] + vector2;
		vertices[14] = vertices[2] + vector2;
		vertices[15] = vertices[3] + vector2;
		ToGUICameraSpace(vertices, invertY);
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
	}

	public static void UpdateVertPositions(Mesh mesh, Rect r, float depth, bool invertY = true)
	{
		r = new Rect(Mathf.Floor(r.x), Mathf.Floor(r.y), Mathf.Floor(r.width), Mathf.Floor(r.height));
		Vector3[] vertices = mesh.vertices;
		vertices[0] = new Vector3(r.x, r.y, depth);
		vertices[1] = new Vector3(r.xMax, r.y, depth);
		vertices[2] = new Vector3(r.x, r.yMax, depth);
		vertices[3] = new Vector3(r.xMax, r.yMax, depth);
		ToGUICameraSpace(vertices, invertY);
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
	}

	public static Vector3 ToGUICameraSpace(Vector3 point, bool invertY)
	{
		Vector3 result = Blocksworld.guiCamera.ScreenToWorldPoint(point);
		if (invertY)
		{
			return new Vector3(result.x, (float)NormalizedScreen.height - result.y, result.z);
		}
		return result;
	}

	public static Rect NormalizedRect(Rect r)
	{
		return new Rect(r.x * NormalizedScreen.scale, r.y * NormalizedScreen.scale, r.width * NormalizedScreen.scale, r.height * NormalizedScreen.scale);
	}

	private static void ToGUICameraSpace(Vector3[] points, bool invertY)
	{
		for (int i = 0; i < points.Length; i++)
		{
			points[i] = ToGUICameraSpace(points[i], invertY);
		}
	}

	public static float GetTextWidth(string text, HudMeshStyle style)
	{
		Font font = style.font;
		font.RequestCharactersInTexture(text);
		float num = 0f;
		bool flag = false;
		foreach (char c in text)
		{
			CharacterInfo info;
			if (!flag && c == '<')
			{
				flag = true;
			}
			else if (flag)
			{
				if (c == '>')
				{
					flag = false;
				}
			}
			else if (font.GetCharacterInfo(c, out info))
			{
				num += (float)info.advance;
			}
		}
		if (style.fontSize != 0)
		{
			num *= (float)style.fontSize / (float)font.fontSize;
		}
		return num;
	}

	public static Vector2 CalcSize(HudMeshStyle style, string text)
	{
		return new Vector2(GetTextWidth(text, style), style.font.lineHeight);
	}

	public static float CalcHeight(HudMeshStyle style, string text, float width)
	{
		Font font = style.font;
		font.RequestCharactersInTexture(text);
		float num = ((style.fontSize != 0) ? ((float)style.fontSize / (float)font.fontSize) : 1f);
		float num2 = width / num;
		float num3 = 0f;
		int num4 = 1;
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			CharacterInfo info;
			if (c.ToString() == Environment.NewLine)
			{
				num3 = 0f;
				if (i < text.Length - 1)
				{
					num4++;
				}
			}
			else if (!flag && c == '<')
			{
				flag = true;
			}
			else if (flag)
			{
				if (c == '>')
				{
					flag = false;
				}
			}
			else if (font.GetCharacterInfo(text[i], out info))
			{
				num3 += (float)info.advance;
				if (num3 > num2)
				{
					num3 = info.advance;
					num4++;
				}
			}
		}
		float num5 = (float)(num4 * font.lineHeight) * num;
		return num5 + (float)style.padding.vertical;
	}
}
