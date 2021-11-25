using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020001B9 RID: 441
public class Materials
{
	// Token: 0x060017C5 RID: 6085 RVA: 0x000A6F73 File Offset: 0x000A5373
	public static void RemoveMapping(string texture)
	{
		Materials.mappings.Remove(texture);
	}

	// Token: 0x060017C6 RID: 6086 RVA: 0x000A6F81 File Offset: 0x000A5381
	public static bool HasMapping(string texture)
	{
		return Materials.mappings.ContainsKey(texture);
	}

	// Token: 0x060017C7 RID: 6087 RVA: 0x000A6F8E File Offset: 0x000A538E
	public static void SetMapping(string texture, Mapping mapping)
	{
		Materials.mappings[texture] = mapping;
	}

	// Token: 0x060017C8 RID: 6088 RVA: 0x000A6F9C File Offset: 0x000A539C
	public static Mapping GetMapping(string texture)
	{
		if (Materials.mappings.ContainsKey(texture))
		{
			return Materials.mappings[texture];
		}
		ResourceLoader.LoadTexture(texture, "Textures");
		if (Materials.mappings.ContainsKey(texture))
		{
			return Materials.mappings[texture];
		}
		BWLog.Info("Could not get mapping for '" + texture + "'");
		return Mapping.OneSideTo1x1;
	}

	// Token: 0x060017C9 RID: 6089 RVA: 0x000A7002 File Offset: 0x000A5402
	public static void Init()
	{
		ResourceLoader.LoadPaintsFromResources("Paints");
	}

	// Token: 0x060017CA RID: 6090 RVA: 0x000A7010 File Offset: 0x000A5410
	private static Vector2[] UVList(double[] numbers)
	{
		Vector2[] array = new Vector2[numbers.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2((float)numbers[i * 2], (float)numbers[i * 2 + 1]);
		}
		return array;
	}

	// Token: 0x060017CB RID: 6091 RVA: 0x000A705C File Offset: 0x000A545C
	private static void LoadPaint(string resource)
	{
		Material material = Resources.Load("Paints/Paint " + resource) as Material;
		Materials.SetShinyness(material);
		Materials.materials[resource] = material;
		Blocksworld.publicProvidedGafs.Add(new GAF("Block.PaintTo", new object[]
		{
			resource
		}));
	}

	// Token: 0x060017CC RID: 6092 RVA: 0x000A70B0 File Offset: 0x000A54B0
	private static void LoadTexture(string resource, Mapping mapping, ShaderType shader)
	{
		Materials.textures[resource] = (Resources.Load("Textures/" + resource) as Texture);
		Materials.mappings[resource] = mapping;
		Materials.shaders[resource] = shader;
		Blocksworld.publicProvidedGafs.Add(new GAF("Block.TextureTo", new object[]
		{
			resource,
			Vector3.zero
		}));
	}

	// Token: 0x060017CD RID: 6093 RVA: 0x000A7124 File Offset: 0x000A5524
	public static Side FindSide(Vector3 normal)
	{
		Vector3 vector = Util.Round(normal);
		if (vector.z > 0f)
		{
			return Side.Front;
		}
		if (vector.z < 0f)
		{
			return Side.Back;
		}
		if (vector.x > 0f)
		{
			return Side.Right;
		}
		if (vector.x < 0f)
		{
			return Side.Left;
		}
		if (vector.y > 0f)
		{
			return Side.Top;
		}
		return Side.Bottom;
	}

	// Token: 0x060017CE RID: 6094 RVA: 0x000A7198 File Offset: 0x000A5598
	public static Vector3 SideToNormal(Side side)
	{
		switch (side)
		{
		case Side.Front:
			return Vector3.forward;
		case Side.Back:
			return Vector3.back;
		case Side.Right:
			return Vector3.right;
		case Side.Left:
			return Vector3.left;
		case Side.Top:
			return Vector3.up;
		case Side.Bottom:
			return Vector3.down;
		default:
			return Vector3.forward;
		}
	}

	// Token: 0x060017CF RID: 6095 RVA: 0x000A71F4 File Offset: 0x000A55F4
	public static Vector3 SideToForward(Side side)
	{
		switch (side)
		{
		case Side.Front:
			return Vector3.down;
		case Side.Back:
			return Vector3.up;
		case Side.Right:
			return Vector3.forward;
		case Side.Left:
			return Vector3.forward;
		case Side.Top:
			return Vector3.forward;
		case Side.Bottom:
			return Vector3.forward;
		default:
			return Vector3.up;
		}
	}

	// Token: 0x060017D0 RID: 6096 RVA: 0x000A7250 File Offset: 0x000A5650
	public static Vector2[] CopyUVs(Vector2[] src)
	{
		Vector2[] array = new Vector2[src.Length];
		for (int i = 0; i < src.Length; i++)
		{
			array[i] = new Vector2(src[i].x, src[i].y);
		}
		return array;
	}

	// Token: 0x060017D1 RID: 6097 RVA: 0x000A72A4 File Offset: 0x000A56A4
	public static void SetMaterial(GameObject go, Mesh mesh, string type, string paint, string texture, Vector3 normal, Vector3 scale, string oldTexture)
	{
		bool flag = go.IsLayer(Layer.Terrain) || go.layer == 12;
		ShaderType shaderType = ShaderType.Normal;
		if (!Materials.shaders.TryGetValue(texture, out shaderType))
		{
			ResourceLoader.LoadTexture(texture, "Textures");
			if (!Materials.shaders.TryGetValue(texture, out shaderType))
			{
				BWLog.Info(string.Concat(new string[]
				{
					"Could not find shader for texture ",
					texture,
					" type: ",
					type,
					" "
				}));
			}
		}
		bool flag2 = !flag && shaderType == ShaderType.NormalTerrain;
		if (flag2)
		{
			shaderType = ShaderType.Normal;
		}
		Material material = Materials.GetMaterial(paint, texture, shaderType);
		if (material != null)
		{
			go.GetComponent<Renderer>().sharedMaterial = material;
		}
		if (flag || !Materials.TextureRequiresUVs(texture))
		{
			return;
		}
		if (!Materials.mappings.ContainsKey(texture))
		{
			BWLog.Warning("Missing texture '" + texture + "'");
			return;
		}
		if (!Materials.uvWraps.ContainsKey(type))
		{
			Materials.uvWraps[type] = Materials.CopyUVs(mesh.uv);
			Materials.uvDecals[type] = Materials.CopyUVs(mesh.uv2);
		}
		Vector2[] array = null;
		Mapping mapping = Materials.mappings[texture];
		if (flag2)
		{
			mapping = Mapping.OneSideTo1x1;
		}
		bool flag3 = Materials.FourSidesIgnoreRightLeft(texture);
		Vector3 wrapScale = Block.GetWrapScale(texture, scale);
		Mapping mapping2 = Mapping.None;
		bool flag4 = string.IsNullOrEmpty(oldTexture);
		if (!flag4)
		{
			mapping2 = Materials.GetMapping(oldTexture);
		}
		if ((!(wrapScale != scale) || !(texture != oldTexture)) && !flag4 && mapping2 == mapping && (mapping2 != mapping || Materials.TextureRequiresUVs(oldTexture) || !(texture != oldTexture)))
		{
			return;
		}
		scale = wrapScale;
		switch (mapping)
		{
		case Mapping.AllSidesTo4x1:
			array = Materials.CopyUVs(Materials.uvWraps[type]);
			for (int i = 0; i < array.Length; i++)
			{
				Vector2[] array2 = array;
				int num = i;
				array2[num].y = array2[num].y * 0.5f;
				Vector2[] array3 = array;
				int num2 = i;
				array3[num2].y = array3[num2].y + 0.5f;
				if (array[i].x >= 0.6666667f)
				{
					if (array[i].x > 0.8333333f)
					{
						array[i].x = array[i].x - 0.333333343f;
						array[i].y = 0.5118577f;
					}
					else
					{
						array[i].x = array[i].x - 0.6666667f;
						array[i].y = 0.98828125f;
					}
				}
				else if (array[i].x >= 0.333333343f)
				{
					Vector2[] array4 = array;
					int num3 = i;
					array4[num3].y = array4[num3].y - 0.5f;
				}
				Vector2[] array5 = array;
				int num4 = i;
				array5[num4].x = array5[num4].x * 3f;
			}
			break;
		case Mapping.AllSidesTo1x1:
		case Mapping.TwoSidesWrapTo1x1:
			array = Materials.CopyUVs(Materials.uvWraps[type]);
			if (type == "Wheel" || type == "Bulky Wheel" || type == "Raycast Wheel")
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].x >= 0.333333343f && array[j].x < 0.6666667f)
					{
						array[j].x = 0f;
						array[j].y = 0f;
					}
					else
					{
						Vector2[] array6 = array;
						int num5 = j;
						array6[num5].x = array6[num5].x * 6f;
						Vector2[] array7 = array;
						int num6 = j;
						array7[num6].y = array7[num6].y * 3f;
					}
				}
			}
			else
			{
				Vector3 one = Vector3.one;
				float num7 = 18f;
				one.x = 1f / (scale.x / Mathf.Ceil(scale.x / num7));
				one.y = 1f / (scale.y / Mathf.Ceil(scale.y / num7));
				one.z = 1f / (scale.z / Mathf.Ceil(scale.z / num7));
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].x < 0.333333343f)
					{
						Vector2[] array8 = array;
						int num8 = k;
						array8[num8].x = array8[num8].x * (scale.x * 6f * one.x);
						Vector2[] array9 = array;
						int num9 = k;
						array9[num9].y = array9[num9].y * (scale.y * one.y);
					}
					else if (array[k].x >= 0.6666667f)
					{
						Vector2[] array10 = array;
						int num10 = k;
						array10[num10].x = array10[num10].x * (scale.x * 6f * one.x);
						Vector2[] array11 = array;
						int num11 = k;
						array11[num11].y = array11[num11].y * (scale.z * one.z);
					}
					else
					{
						Vector2[] array12 = array;
						int num12 = k;
						array12[num12].x = array12[num12].x * (scale.z * 6f * one.z);
						Vector2[] array13 = array;
						int num13 = k;
						array13[num13].y = array13[num13].y * (scale.y * one.y);
					}
				}
			}
			break;
		case Mapping.OneSideTo1x1:
		case Mapping.TwoSidesTo1x1:
		case Mapping.OneSideWrapTo1x1:
		{
			array = Materials.CopyUVs(Materials.uvDecals[type]);
			float num14 = 0f;
			float num15 = 1f;
			int uRepeats = 1;
			int vRepeats = 1;
			switch (Materials.FindSide(normal))
			{
			case Side.Front:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.y);
				break;
			case Side.Back:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.y);
				num14 = 1f;
				num15 = 0f;
				break;
			case Side.Right:
				uRepeats = Mathf.RoundToInt(scale.z);
				vRepeats = Mathf.RoundToInt(scale.y);
				num14 = 2f;
				num15 = 3f;
				break;
			case Side.Left:
				uRepeats = Mathf.RoundToInt(scale.z);
				vRepeats = Mathf.RoundToInt(scale.y);
				num14 = 3f;
				num15 = 2f;
				break;
			case Side.Top:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.z);
				num14 = 4f;
				num15 = 5f;
				break;
			case Side.Bottom:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.z);
				num14 = 5f;
				num15 = 4f;
				break;
			}
			num14 /= 6f;
			float x = num14 + 0.166666672f;
			num15 /= 6f;
			float x2 = num15 + 0.166666672f;
			Materials.SetUVs(array, mapping, num14, x, num15, x2, uRepeats, vRepeats, texture);
			break;
		}
		case Mapping.FourSidesTo1x1:
			array = Materials.CopyUVs(Materials.uvWraps[type]);
			for (int l = 0; l < array.Length; l++)
			{
				float x3 = array[l].x;
				bool flag5 = (flag3 && x3 >= 0.333333343f && x3 < 0.6666667f) || (!flag3 && x3 >= 0.6666667f);
				if (flag5)
				{
					array[l].x = 0f;
					array[l].y = 0f;
				}
				else
				{
					Vector2[] array14 = array;
					int num16 = l;
					array14[num16].x = array14[num16].x * (6f * wrapScale.x);
					Vector2[] array15 = array;
					int num17 = l;
					array15[num17].y = array15[num17].y * wrapScale.y;
				}
			}
			break;
		case Mapping.None:
			array = Materials.CopyUVs(Materials.uvWraps[type]);
			break;
		}
		if (array != null)
		{
			mesh.uv = array;
		}
	}

	// Token: 0x060017D2 RID: 6098 RVA: 0x000A7B6C File Offset: 0x000A5F6C
	private static void SetUVs(Vector2[] uvs, Mapping mapping, float x1, float x2, float x3, float x4, int uRepeats, int vRepeats, string texture)
	{
		bool flag = mapping == Mapping.TwoSidesTo1x1;
		bool flag2 = flag && Materials.TwoSidesMirror(texture);
		for (int i = 0; i < uvs.Length; i++)
		{
			if (uvs[i].x >= x1 && uvs[i].x < x2)
			{
				if (mapping != Mapping.OneSideWrapTo1x1)
				{
					uRepeats = 1;
					vRepeats = 1;
				}
				int num = i;
				uvs[num].x = uvs[num].x * (float)(6 * uRepeats);
				if (vRepeats > 1 && mapping != Mapping.OneSideWrapTo1x1)
				{
					int num2 = i;
					uvs[num2].y = uvs[num2].y * (float)vRepeats;
				}
			}
			else if (flag && uvs[i].x >= x3 && uvs[i].x < x4)
			{
				if (mapping != Mapping.TwoSidesWrapTo1x1)
				{
					uRepeats = 1;
					vRepeats = 1;
				}
				float num3 = uvs[i].x * 6f * (float)uRepeats;
				uvs[i].x = ((!flag2) ? num3 : (1f - num3));
				if (vRepeats > 1)
				{
					int num4 = i;
					uvs[num4].y = uvs[num4].y * (float)vRepeats;
				}
			}
			else
			{
				uvs[i] = Vector2.zero;
			}
		}
	}

	// Token: 0x060017D3 RID: 6099 RVA: 0x000A7CBC File Offset: 0x000A60BC
	public static void PlanarProjection(Mesh mesh, float rotation)
	{
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		float num = 1000f;
		float num2 = -1000f;
		float num3 = 1000f;
		float num4 = -1000f;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[i].x < num)
			{
				num = vertices[i].x;
			}
			if (vertices[i].x > num2)
			{
				num2 = vertices[i].x;
			}
			if (vertices[i].y < num3)
			{
				num3 = vertices[i].y;
			}
			if (vertices[i].y > num4)
			{
				num4 = vertices[i].y;
			}
		}
		float num5 = num2 - num;
		float num6 = num4 - num3;
		for (int j = 0; j < vertices.Length; j++)
		{
			Vector3 lhs = Util.ProjectOntoPlane(vertices[j], Vector3.forward);
			float num7 = Vector3.Dot(lhs, Vector3.right);
			float num8 = Vector3.Dot(lhs, Vector3.up);
			float x = (num7 - num) / num5;
			float y = (num8 - num3) / num6;
			uv[j] = new Vector2(x, y);
		}
		for (int k = 0; k < vertices.Length; k++)
		{
            Vector2 half = 0.5f * Vector2.one;
            uv[k] = (Quaternion.Euler(0f, 0f, -rotation) * (uv[k] - 0.5f * Vector2.one)) + new Vector3(half.x, half.y);
		}
		mesh.uv = uv;
	}

	// Token: 0x060017D4 RID: 6100 RVA: 0x000A7E94 File Offset: 0x000A6294
	public static void PlanarProjection(Mesh mesh, Vector3 normal, float rotation)
	{
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Quaternion rotation2 = Quaternion.FromToRotation(normal, Vector3.forward);
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = rotation2 * vertices[i];
		}
		float num = 1000f;
		float num2 = -1000f;
		float num3 = 1000f;
		float num4 = -1000f;
		for (int j = 0; j < vertices.Length; j++)
		{
			if (vertices[j].x < num)
			{
				num = vertices[j].x;
			}
			if (vertices[j].x > num2)
			{
				num2 = vertices[j].x;
			}
			if (vertices[j].y < num3)
			{
				num3 = vertices[j].y;
			}
			if (vertices[j].y > num4)
			{
				num4 = vertices[j].y;
			}
		}
		float num5 = num2 - num;
		float num6 = num4 - num3;
		for (int k = 0; k < vertices.Length; k++)
		{
			Vector3 lhs = Util.ProjectOntoPlane(vertices[k], Vector3.forward);
			float num7 = Vector3.Dot(lhs, Vector3.right);
			float num8 = Vector3.Dot(lhs, Vector3.up);
			float x = 1f - (num7 - num) / num5;
			float y = (num8 - num3) / num6;
			uv[k] = new Vector2(x, y);
		}
		for (int l = 0; l < vertices.Length; l++)
		{
            Vector2 half = 0.5f * Vector2.one;
			uv[l] = Quaternion.Euler(0f, 0f, -rotation) * (uv[l] - 0.5f * Vector2.one) + new Vector3(half.x, half.y);
		}
		mesh.uv = uv;
	}

	// Token: 0x060017D5 RID: 6101 RVA: 0x000A80BC File Offset: 0x000A64BC
	public static void CubicProjection(Block block)
	{
		Mesh mesh = block.go.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		int[] triangles = mesh.triangles;
		Vector3[] normals = mesh.normals;
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			int num = triangles[i * 3];
			int num2 = triangles[i * 3 + 1];
			int num3 = triangles[i * 3 + 2];
			Vector3 vector = (normals[num] + normals[num2] + normals[num3]) / 3f;
			Vector3 vector2 = Vector3.up;
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				vector2 = Vector3.right;
			}
			if (Mathf.Abs(vector.z) > Mathf.Abs(vector.y) && Mathf.Abs(vector.z) > Mathf.Abs(vector.x))
			{
				vector2 = Vector3.forward;
			}
			for (int j = 0; j < 3; j++)
			{
				int num4 = triangles[i * 3 + j];
				Vector3 a = Util.ProjectOntoPlane(vertices[num4], vector2);
				float num5 = 18f;
				Vector3 b = new Vector3(Mathf.Repeat(block.go.transform.position.x - num5 / 2f + 0.5f, num5), Mathf.Repeat(block.go.transform.position.y, num5), Mathf.Repeat(block.go.transform.position.z - num5 / 2f + 0.5f, num5));
				a = (a + b) / num5;
				if (vector2 == Vector3.up)
				{
					uv[num4] = new Vector2(a.x, a.z);
				}
				else if (vector2 == Vector3.right)
				{
					uv[num4] = new Vector2(a.y, a.z);
				}
				else
				{
					uv[num4] = new Vector2(a.x, a.y);
				}
			}
		}
		mesh.uv = uv;
		mesh.uv2 = uv;
	}

	// Token: 0x060017D6 RID: 6102 RVA: 0x000A8348 File Offset: 0x000A6748
	public static string GetMaterialName(string paint, string texture, ShaderType shader)
	{
		int num = (int)shader;
		return paint + texture + num.ToString();
	}

	// Token: 0x060017D7 RID: 6103 RVA: 0x000A836C File Offset: 0x000A676C
	public static Material GetMaterial(string paint, string texture, ShaderType shader)
	{
		string materialName = Materials.GetMaterialName(paint, texture, shader);
		Material material;
		if (Materials.materialCache.ContainsKey(materialName))
		{
			material = Materials.materialCache[materialName];
		}
		else if (Materials.materials.ContainsKey(paint) && (Materials.textures.ContainsKey(texture) || texture == "Glass" || texture == "Invisible"))
		{
			material = new Material(Materials.materials[paint]);
			if (shader != ShaderType.Normal)
			{
				Shader shader2 = Shader.Find("Blocksworld/" + shader);
				if (shader2 == null)
				{
					BWLog.Info("Could not find shader " + shader);
				}
				material.shader = shader2;
			}
			material.mainTexture = Materials.textures[texture];
			if (Materials.cubemaps.ContainsKey(texture) && material.HasProperty("_CubeMapTex"))
			{
				material.SetTexture("_CubeMapTex", Materials.cubemaps[texture]);
			}
			if (Materials.cubemaps.ContainsKey(texture + " Overlay") && material.HasProperty("_CubeMapTexOverlay"))
			{
				material.SetTexture("_CubeMapTexOverlay", Materials.cubemaps[texture + " Overlay"]);
			}
			material.SetColor("_FogColor", Blocksworld.fogColor);
			material.SetFloat("_FogStart", Blocksworld.fogStart * Blocksworld.fogMultiplier);
			material.SetFloat("_FogEnd", Blocksworld.fogEnd * Blocksworld.fogMultiplier);
			switch (shader)
			{
			case ShaderType.Metal:
				material.SetFloat("_Metallic", 1f);
				break;
			default:
				switch (shader)
				{
				case ShaderType.GlassWater:
				{
					Color color = material.GetColor("_Color");
					material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.55f));
					material.SetColor("_SpecColor", Color.white);
					break;
				}
				case ShaderType.Invisible:
				{
					Color color = material.GetColor("_Color");
					material.SetColor("_Color", new Color(color.r, color.g, color.b, 1f));
					break;
				}
				case ShaderType.Volume:
				{
					Color color = material.GetColor("_Color");
					material.SetColor("_Color", new Color(color.r, color.g, color.b, 1f));
					break;
				}
				}
				break;
			case ShaderType.Glass:
			{
				Color color = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.55f));
				material.SetColor("_SpecColor", Color.white);
				break;
			}
			case ShaderType.Water:
			{
				Color color = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.55f));
				material.SetColor("_SpecColor", Color.white);
				break;
			}
			}
			if (material.HasProperty("_PerturbTex"))
			{
				material.SetTexture("_PerturbTex", Materials.GetPerturbTexture(texture));
			}
			Materials.materialCache[materialName] = material;
			Materials.materialCachePaint[material] = paint;
			Materials.materialCacheTexture[material] = texture;
		}
		else
		{
			BWLog.Warning(string.Concat(new object[]
			{
				"Missing paint '",
				paint,
				"' + ",
				Materials.materials.ContainsKey(paint),
				" or texture '",
				texture,
				"' ",
				Materials.textures.ContainsKey(texture)
			}));
			ResourceLoader.LoadTexture("Plain");
			material = Materials.GetMaterial("Grey", "Plain", ShaderType.Normal);
		}
		return material;
	}

	// Token: 0x060017D8 RID: 6104 RVA: 0x000A8764 File Offset: 0x000A6B64
	public static void UpdateCachedColors()
	{
		foreach (KeyValuePair<string, Material> keyValuePair in Materials.materialCache)
		{
			Material value = keyValuePair.Value;
			if (Materials.materialCachePaint.ContainsKey(value))
			{
				string str = Materials.materialCachePaint[value];
				Material material = Resources.Load("Paints/Paint " + str) as Material;
				value.SetColor("_Color", material.GetColor("_Color"));
				value.SetColor("_Emission", material.GetColor("_Emission"));
				value.SetColor("_SpecColor", material.GetColor("_SpecColor"));
				value.SetFloat("_Shininess", material.GetFloat("_Shininess"));
				value.SetFloat("_Gloss", material.GetFloat("_Gloss"));
				value.SetFloat("_TextureEmissive", material.GetFloat("_TextureEmissive"));
				UnityEngine.Object.Destroy(material);
			}
		}
	}

	// Token: 0x060017D9 RID: 6105 RVA: 0x000A8884 File Offset: 0x000A6C84
	public static void SetShinyness(Material matPaint)
	{
		matPaint.SetFloat("_Shininess", 0.25f);
		matPaint.SetFloat("_Gloss", 4f);
		matPaint.SetFloat("_TextureEmissive", 0.558209f);
	}

	// Token: 0x060017DA RID: 6106 RVA: 0x000A88B8 File Offset: 0x000A6CB8
	public static void DetectTerrainMaterials(GameObject go)
	{
		foreach (Transform transform in go.GetComponentsInChildren<Transform>())
		{
			if (transform.GetComponent<Renderer>() != null)
			{
				Material[] sharedMaterials = transform.GetComponent<Renderer>().sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					if (!Materials.materialCachePaint.ContainsKey(sharedMaterials[j]))
					{
						string paint = sharedMaterials[j].name.Substring("Paint ".Length);
						Material material = Materials.GetMaterial(paint, "Plain", ShaderType.Normal);
						sharedMaterials[j] = material;
					}
				}
				transform.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
			}
		}
	}

	// Token: 0x060017DB RID: 6107 RVA: 0x000A8967 File Offset: 0x000A6D67
	public static bool TextureIsTransparent(string texture)
	{
		return Materials.transparentTextures.Contains(texture);
	}

	// Token: 0x060017DC RID: 6108 RVA: 0x000A8974 File Offset: 0x000A6D74
	public static bool IsMaterialShaderTexture(string texture)
	{
		return Materials.noUvTextures.Contains(texture);
	}

	// Token: 0x060017DD RID: 6109 RVA: 0x000A8981 File Offset: 0x000A6D81
	public static bool TextureRequiresUVs(string texture)
	{
		return !Materials.noUvTextures.Contains(texture);
	}

	// Token: 0x060017DE RID: 6110 RVA: 0x000A8994 File Offset: 0x000A6D94
	public static bool FourSidesIgnoreRightLeft(string texture)
	{
		bool flag;
		return !Materials.fourSidesIgnoreRightLeft.TryGetValue(texture, out flag) || flag;
	}

	// Token: 0x060017DF RID: 6111 RVA: 0x000A89B8 File Offset: 0x000A6DB8
	public static bool TwoSidesMirror(string texture)
	{
		bool flag;
		return !Materials.twoSidesMirror.TryGetValue(texture, out flag) || flag;
	}

	// Token: 0x060017E0 RID: 6112 RVA: 0x000A89DC File Offset: 0x000A6DDC
	public static float MipMapBias(string texture)
	{
		float result = 0f;
		if (Materials.mipMapBias.TryGetValue(texture, out result))
		{
			return result;
		}
		return result;
	}

	// Token: 0x060017E1 RID: 6113 RVA: 0x000A8A04 File Offset: 0x000A6E04
	public static bool IsNormalTerrainTexture(string texture)
	{
		bool result = false;
		if (Materials.textureInfos.ContainsKey(texture))
		{
			result = (Materials.textureInfos[texture].shader == ShaderType.NormalTerrain);
		}
		return result;
	}

	// Token: 0x060017E2 RID: 6114 RVA: 0x000A8A3C File Offset: 0x000A6E3C
	public static bool IsTerrainTexture(string texture)
	{
		bool result = false;
		if (Materials.textureInfos.ContainsKey(texture))
		{
			result = (Materials.textureInfos[texture].shader == ShaderType.Terrain);
		}
		return result;
	}

	// Token: 0x060017E3 RID: 6115 RVA: 0x000A8A70 File Offset: 0x000A6E70
	public static bool IsStreamingLiquid(string texture)
	{
		bool result = false;
		if (Materials.textureInfos.ContainsKey(texture))
		{
			result = (Materials.textureInfos[texture].shader == ShaderType.StreamingLiquid);
		}
		return result;
	}

	// Token: 0x060017E4 RID: 6116 RVA: 0x000A8AA8 File Offset: 0x000A6EA8
	private static Texture2D GetPerturbTexture(string texture)
	{
		texture = "Just one texture for now";
		Texture2D texture2D;
		if (Materials.perturbTextures.TryGetValue(texture, out texture2D))
		{
			return texture2D;
		}
		texture2D = (Texture2D)Resources.Load("Env Effect/Perturb Texture", typeof(Texture2D));
		Materials.perturbTextures[texture] = texture2D;
		return texture2D;
	}

	// Token: 0x040012D0 RID: 4816
	public static Dictionary<string, Material> materials = new Dictionary<string, Material>();

	// Token: 0x040012D1 RID: 4817
	public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

	// Token: 0x040012D2 RID: 4818
	public static Dictionary<string, Cubemap> cubemaps = new Dictionary<string, Cubemap>();

	// Token: 0x040012D3 RID: 4819
	private static Dictionary<string, Mapping> mappings = new Dictionary<string, Mapping>();

	// Token: 0x040012D4 RID: 4820
	public static Dictionary<string, ShaderType> shaders = new Dictionary<string, ShaderType>();

	// Token: 0x040012D5 RID: 4821
	public static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

	// Token: 0x040012D6 RID: 4822
	public static Dictionary<Material, string> materialCachePaint = new Dictionary<Material, string>();

	// Token: 0x040012D7 RID: 4823
	public static Dictionary<Material, string> materialCacheTexture = new Dictionary<Material, string>();

	// Token: 0x040012D8 RID: 4824
	public static Dictionary<string, Vector2[]> uvs = new Dictionary<string, Vector2[]>();

	// Token: 0x040012D9 RID: 4825
	public static Dictionary<string, Vector2[]> uvWraps = new Dictionary<string, Vector2[]>();

	// Token: 0x040012DA RID: 4826
	public static Dictionary<string, Vector2[]> uvDecals = new Dictionary<string, Vector2[]>();

	// Token: 0x040012DB RID: 4827
	public static Dictionary<string, TextureInfo> textureInfos;

	// Token: 0x040012DC RID: 4828
	public static Dictionary<string, Vector3> wrapTexturePrefSizes = new Dictionary<string, Vector3>();

	// Token: 0x040012DD RID: 4829
	public static Dictionary<string, bool> fourSidesIgnoreRightLeft = new Dictionary<string, bool>();

	// Token: 0x040012DE RID: 4830
	public static Dictionary<string, bool> twoSidesMirror = new Dictionary<string, bool>();

	// Token: 0x040012DF RID: 4831
	public static Dictionary<string, float> mipMapBias = new Dictionary<string, float>();

	// Token: 0x040012E0 RID: 4832
	public static Dictionary<string, List<TextureApplicationChangeRule>> textureApplicationRules = new Dictionary<string, List<TextureApplicationChangeRule>>();

	// Token: 0x040012E1 RID: 4833
	public const string TERRAIN_TEXTURE_POSTFIX = "_Terrain";

	// Token: 0x040012E2 RID: 4834
	public const string SKY_TEXTURE_POSTFIX = "_Sky";

	// Token: 0x040012E3 RID: 4835
	public const string WATER_TEXTURE_POSTFIX = "_Water";

	// Token: 0x040012E4 RID: 4836
	public const string BLOCK_TEXTURE_POSTFIX = "_Block";

	// Token: 0x040012E5 RID: 4837
	private static Dictionary<string, Texture2D> perturbTextures = new Dictionary<string, Texture2D>();

	// Token: 0x040012E6 RID: 4838
	private static HashSet<string> transparentTextures = new HashSet<string>
	{
		"Glass",
		"Water",
		"Transparent",
		"Texture Soccer Net",
		"Texture Water Stream",
		"Water_Block",
		"Invisible",
		"Texture Grid Holographic",
		"Texture Reticle Square",
		"Texture Reticle Circle",
		"Texture Reticle Arrow",
		"Cloaked"
	};

	// Token: 0x040012E7 RID: 4839
	private static HashSet<string> noUvTextures = new HashSet<string>
	{
		"Plain",
		"Transparent",
		"Metal",
		"Pulsate Glow",
		"Glass",
		"Invisible",
		"Cloaked"
	};
}
