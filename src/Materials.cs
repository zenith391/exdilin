using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class Materials
{
	public static Dictionary<string, Material> materials = new Dictionary<string, Material>();

	public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

	public static Dictionary<string, Cubemap> cubemaps = new Dictionary<string, Cubemap>();

	private static Dictionary<string, Mapping> mappings = new Dictionary<string, Mapping>();

	public static Dictionary<string, ShaderType> shaders = new Dictionary<string, ShaderType>();

	public static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

	public static Dictionary<Material, string> materialCachePaint = new Dictionary<Material, string>();

	public static Dictionary<Material, string> materialCacheTexture = new Dictionary<Material, string>();

	public static Dictionary<string, Vector2[]> uvs = new Dictionary<string, Vector2[]>();

	public static Dictionary<string, Vector2[]> uvWraps = new Dictionary<string, Vector2[]>();

	public static Dictionary<string, Vector2[]> uvDecals = new Dictionary<string, Vector2[]>();

	public static Dictionary<string, TextureInfo> textureInfos;

	public static Dictionary<string, Vector3> wrapTexturePrefSizes = new Dictionary<string, Vector3>();

	public static Dictionary<string, bool> fourSidesIgnoreRightLeft = new Dictionary<string, bool>();

	public static Dictionary<string, bool> twoSidesMirror = new Dictionary<string, bool>();

	public static Dictionary<string, float> mipMapBias = new Dictionary<string, float>();

	public static Dictionary<string, List<TextureApplicationChangeRule>> textureApplicationRules = new Dictionary<string, List<TextureApplicationChangeRule>>();

	public const string TERRAIN_TEXTURE_POSTFIX = "_Terrain";

	public const string SKY_TEXTURE_POSTFIX = "_Sky";

	public const string WATER_TEXTURE_POSTFIX = "_Water";

	public const string BLOCK_TEXTURE_POSTFIX = "_Block";

	private static Dictionary<string, Texture2D> perturbTextures = new Dictionary<string, Texture2D>();

	private static HashSet<string> transparentTextures = new HashSet<string>
	{
		"Glass", "Water", "Transparent", "Texture Soccer Net", "Texture Water Stream", "Water_Block", "Invisible", "Texture Grid Holographic", "Texture Reticle Square", "Texture Reticle Circle",
		"Texture Reticle Arrow", "Cloaked"
	};

	private static HashSet<string> noUvTextures = new HashSet<string> { "Plain", "Transparent", "Metal", "Pulsate Glow", "Glass", "Invisible", "Cloaked" };

	public static void RemoveMapping(string texture)
	{
		mappings.Remove(texture);
	}

	public static bool HasMapping(string texture)
	{
		return mappings.ContainsKey(texture);
	}

	public static void SetMapping(string texture, Mapping mapping)
	{
		mappings[texture] = mapping;
	}

	public static Mapping GetMapping(string texture)
	{
		if (mappings.ContainsKey(texture))
		{
			return mappings[texture];
		}
		ResourceLoader.LoadTexture(texture);
		if (mappings.ContainsKey(texture))
		{
			return mappings[texture];
		}
		BWLog.Info("Could not get mapping for '" + texture + "'");
		return Mapping.OneSideTo1x1;
	}

	public static void Init()
	{
		ResourceLoader.LoadPaintsFromResources("Paints");
	}

	private static Vector2[] UVList(double[] numbers)
	{
		Vector2[] array = new Vector2[numbers.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2((float)numbers[i * 2], (float)numbers[i * 2 + 1]);
		}
		return array;
	}

	private static void LoadPaint(string resource)
	{
		Material material = Resources.Load("Paints/Paint " + resource) as Material;
		SetShinyness(material);
		materials[resource] = material;
		Blocksworld.publicProvidedGafs.Add(new GAF("Block.PaintTo", resource));
	}

	private static void LoadTexture(string resource, Mapping mapping, ShaderType shader)
	{
		textures[resource] = Resources.Load("Textures/" + resource) as Texture;
		mappings[resource] = mapping;
		shaders[resource] = shader;
		Blocksworld.publicProvidedGafs.Add(new GAF("Block.TextureTo", resource, Vector3.zero));
	}

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

	public static Vector3 SideToNormal(Side side)
	{
		return side switch
		{
			Side.Front => Vector3.forward, 
			Side.Back => Vector3.back, 
			Side.Right => Vector3.right, 
			Side.Left => Vector3.left, 
			Side.Top => Vector3.up, 
			Side.Bottom => Vector3.down, 
			_ => Vector3.forward, 
		};
	}

	public static Vector3 SideToForward(Side side)
	{
		return side switch
		{
			Side.Front => Vector3.down, 
			Side.Back => Vector3.up, 
			Side.Right => Vector3.forward, 
			Side.Left => Vector3.forward, 
			Side.Top => Vector3.forward, 
			Side.Bottom => Vector3.forward, 
			_ => Vector3.up, 
		};
	}

	public static Vector2[] CopyUVs(Vector2[] src)
	{
		Vector2[] array = new Vector2[src.Length];
		for (int i = 0; i < src.Length; i++)
		{
			array[i] = new Vector2(src[i].x, src[i].y);
		}
		return array;
	}

	public static void SetMaterial(GameObject go, Mesh mesh, string type, string paint, string texture, Vector3 normal, Vector3 scale, string oldTexture)
	{
		bool flag = go.IsLayer(Layer.Terrain) || go.layer == 12;
		ShaderType value = ShaderType.Normal;
		if (!shaders.TryGetValue(texture, out value))
		{
			ResourceLoader.LoadTexture(texture);
			if (!shaders.TryGetValue(texture, out value))
			{
				BWLog.Info("Could not find shader for texture " + texture + " type: " + type + " ");
			}
		}
		bool flag2 = !flag && value == ShaderType.NormalTerrain;
		if (flag2)
		{
			value = ShaderType.Normal;
		}
		Material material = GetMaterial(paint, texture, value);
		if (material != null)
		{
			go.GetComponent<Renderer>().sharedMaterial = material;
		}
		if (flag || !TextureRequiresUVs(texture))
		{
			return;
		}
		if (!mappings.ContainsKey(texture))
		{
			BWLog.Warning("Missing texture '" + texture + "'");
			return;
		}
		if (!uvWraps.ContainsKey(type))
		{
			uvWraps[type] = CopyUVs(mesh.uv);
			uvDecals[type] = CopyUVs(mesh.uv2);
		}
		Vector2[] array = null;
		Mapping mapping = mappings[texture];
		if (flag2)
		{
			mapping = Mapping.OneSideTo1x1;
		}
		bool flag3 = FourSidesIgnoreRightLeft(texture);
		Vector3 wrapScale = Block.GetWrapScale(texture, scale);
		Mapping mapping2 = Mapping.None;
		bool flag4 = string.IsNullOrEmpty(oldTexture);
		if (!flag4)
		{
			mapping2 = GetMapping(oldTexture);
		}
		if ((!(wrapScale != scale) || !(texture != oldTexture)) && !flag4 && mapping2 == mapping && (mapping2 != mapping || TextureRequiresUVs(oldTexture) || !(texture != oldTexture)))
		{
			return;
		}
		scale = wrapScale;
		switch (mapping)
		{
		case Mapping.AllSidesTo4x1:
		{
			array = CopyUVs(uvWraps[type]);
			for (int l = 0; l < array.Length; l++)
			{
				Vector2[] array12 = array;
				int num14 = l;
				array12[num14].y = array12[num14].y * 0.5f;
				Vector2[] array13 = array;
				int num15 = l;
				array13[num15].y = array13[num15].y + 0.5f;
				if (array[l].x >= 2f / 3f)
				{
					if (array[l].x > 5f / 6f)
					{
						array[l].x = array[l].x - 1f / 3f;
						array[l].y = 0.5118577f;
					}
					else
					{
						array[l].x = array[l].x - 2f / 3f;
						array[l].y = 0.98828125f;
					}
				}
				else if (array[l].x >= 1f / 3f)
				{
					Vector2[] array14 = array;
					int num16 = l;
					array14[num16].y = array14[num16].y - 0.5f;
				}
				Vector2[] array15 = array;
				int num17 = l;
				array15[num17].x = array15[num17].x * 3f;
			}
			break;
		}
		case Mapping.AllSidesTo1x1:
		case Mapping.TwoSidesWrapTo1x1:
			array = CopyUVs(uvWraps[type]);
			switch (type)
			{
			case "Wheel":
			case "Bulky Wheel":
			case "Raycast Wheel":
			{
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].x >= 1f / 3f && array[k].x < 2f / 3f)
					{
						array[k].x = 0f;
						array[k].y = 0f;
						continue;
					}
					Vector2[] array10 = array;
					int num10 = k;
					array10[num10].x = array10[num10].x * 6f;
					Vector2[] array11 = array;
					int num11 = k;
					array11[num11].y = array11[num11].y * 3f;
				}
				break;
			}
			default:
			{
				Vector3 one = Vector3.one;
				float num3 = 18f;
				one.x = 1f / (scale.x / Mathf.Ceil(scale.x / num3));
				one.y = 1f / (scale.y / Mathf.Ceil(scale.y / num3));
				one.z = 1f / (scale.z / Mathf.Ceil(scale.z / num3));
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].x < 1f / 3f)
					{
						Vector2[] array4 = array;
						int num4 = j;
						array4[num4].x = array4[num4].x * (scale.x * 6f * one.x);
						Vector2[] array5 = array;
						int num5 = j;
						array5[num5].y = array5[num5].y * (scale.y * one.y);
					}
					else if (array[j].x >= 2f / 3f)
					{
						Vector2[] array6 = array;
						int num6 = j;
						array6[num6].x = array6[num6].x * (scale.x * 6f * one.x);
						Vector2[] array7 = array;
						int num7 = j;
						array7[num7].y = array7[num7].y * (scale.z * one.z);
					}
					else
					{
						Vector2[] array8 = array;
						int num8 = j;
						array8[num8].x = array8[num8].x * (scale.z * 6f * one.z);
						Vector2[] array9 = array;
						int num9 = j;
						array9[num9].y = array9[num9].y * (scale.y * one.y);
					}
				}
				break;
			}
			}
			break;
		case Mapping.OneSideTo1x1:
		case Mapping.TwoSidesTo1x1:
		case Mapping.OneSideWrapTo1x1:
		{
			array = CopyUVs(uvDecals[type]);
			float num12 = 0f;
			float num13 = 1f;
			int uRepeats = 1;
			int vRepeats = 1;
			switch (FindSide(normal))
			{
			case Side.Front:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.y);
				break;
			case Side.Back:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.y);
				num12 = 1f;
				num13 = 0f;
				break;
			case Side.Right:
				uRepeats = Mathf.RoundToInt(scale.z);
				vRepeats = Mathf.RoundToInt(scale.y);
				num12 = 2f;
				num13 = 3f;
				break;
			case Side.Left:
				uRepeats = Mathf.RoundToInt(scale.z);
				vRepeats = Mathf.RoundToInt(scale.y);
				num12 = 3f;
				num13 = 2f;
				break;
			case Side.Top:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.z);
				num12 = 4f;
				num13 = 5f;
				break;
			case Side.Bottom:
				uRepeats = Mathf.RoundToInt(scale.x);
				vRepeats = Mathf.RoundToInt(scale.z);
				num12 = 5f;
				num13 = 4f;
				break;
			}
			num12 /= 6f;
			float x2 = num12 + 1f / 6f;
			num13 /= 6f;
			float x3 = num13 + 1f / 6f;
			SetUVs(array, mapping, num12, x2, num13, x3, uRepeats, vRepeats, texture);
			break;
		}
		case Mapping.FourSidesTo1x1:
		{
			array = CopyUVs(uvWraps[type]);
			for (int i = 0; i < array.Length; i++)
			{
				float x = array[i].x;
				if ((flag3 && x >= 1f / 3f && x < 2f / 3f) || (!flag3 && x >= 2f / 3f))
				{
					array[i].x = 0f;
					array[i].y = 0f;
					continue;
				}
				Vector2[] array2 = array;
				int num = i;
				array2[num].x = array2[num].x * (6f * wrapScale.x);
				Vector2[] array3 = array;
				int num2 = i;
				array3[num2].y = array3[num2].y * wrapScale.y;
			}
			break;
		}
		case Mapping.None:
			array = CopyUVs(uvWraps[type]);
			break;
		}
		if (array != null)
		{
			mesh.uv = array;
		}
	}

	private static void SetUVs(Vector2[] uvs, Mapping mapping, float x1, float x2, float x3, float x4, int uRepeats, int vRepeats, string texture)
	{
		bool flag = mapping == Mapping.TwoSidesTo1x1;
		bool flag2 = flag && TwoSidesMirror(texture);
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
			Vector2 vector = 0.5f * Vector2.one;
			uv[k] = Quaternion.Euler(0f, 0f, 0f - rotation) * (uv[k] - 0.5f * Vector2.one) + new Vector3(vector.x, vector.y);
		}
		mesh.uv = uv;
	}

	public static void PlanarProjection(Mesh mesh, Vector3 normal, float rotation)
	{
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Quaternion quaternion = Quaternion.FromToRotation(normal, Vector3.forward);
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = quaternion * vertices[i];
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
			Vector2 vector = 0.5f * Vector2.one;
			uv[l] = Quaternion.Euler(0f, 0f, 0f - rotation) * (uv[l] - 0.5f * Vector2.one) + new Vector3(vector.x, vector.y);
		}
		mesh.uv = uv;
	}

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
				Vector3 vector3 = Util.ProjectOntoPlane(vertices[num4], vector2);
				float num5 = 18f;
				Vector3 vector4 = new Vector3(Mathf.Repeat(block.go.transform.position.x - num5 / 2f + 0.5f, num5), Mathf.Repeat(block.go.transform.position.y, num5), Mathf.Repeat(block.go.transform.position.z - num5 / 2f + 0.5f, num5));
				vector3 = (vector3 + vector4) / num5;
				if (vector2 == Vector3.up)
				{
					uv[num4] = new Vector2(vector3.x, vector3.z);
				}
				else if (vector2 == Vector3.right)
				{
					uv[num4] = new Vector2(vector3.y, vector3.z);
				}
				else
				{
					uv[num4] = new Vector2(vector3.x, vector3.y);
				}
			}
		}
		mesh.uv = uv;
		mesh.uv2 = uv;
	}

	public static string GetMaterialName(string paint, string texture, ShaderType shader)
	{
		int num = (int)shader;
		return paint + texture + num;
	}

	public static Material GetMaterial(string paint, string texture, ShaderType shader)
	{
		string materialName = GetMaterialName(paint, texture, shader);
		Material material;
		if (materialCache.ContainsKey(materialName))
		{
			material = materialCache[materialName];
		}
		else if (materials.ContainsKey(paint) && (textures.ContainsKey(texture) || texture == "Glass" || texture == "Invisible"))
		{
			material = new Material(materials[paint]);
			if (shader != ShaderType.Normal)
			{
				Shader shader2 = Shader.Find("Blocksworld/" + shader);
				if (shader2 == null)
				{
					BWLog.Info("Could not find shader " + shader);
				}
				material.shader = shader2;
			}
			material.mainTexture = textures[texture];
			if (cubemaps.ContainsKey(texture) && material.HasProperty("_CubeMapTex"))
			{
				material.SetTexture("_CubeMapTex", cubemaps[texture]);
			}
			if (cubemaps.ContainsKey(texture + " Overlay") && material.HasProperty("_CubeMapTexOverlay"))
			{
				material.SetTexture("_CubeMapTexOverlay", cubemaps[texture + " Overlay"]);
			}
			material.SetColor("_FogColor", Blocksworld.fogColor);
			material.SetFloat("_FogStart", Blocksworld.fogStart * Blocksworld.fogMultiplier);
			material.SetFloat("_FogEnd", Blocksworld.fogEnd * Blocksworld.fogMultiplier);
			switch (shader)
			{
			case ShaderType.Metal:
				material.SetFloat("_Metallic", 1f);
				break;
			case ShaderType.GlassWater:
			{
				Color color5 = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color5.r, color5.g, color5.b, 0.55f));
				material.SetColor("_SpecColor", Color.white);
				break;
			}
			case ShaderType.Invisible:
			{
				Color color4 = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color4.r, color4.g, color4.b, 1f));
				break;
			}
			case ShaderType.Volume:
			{
				Color color3 = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color3.r, color3.g, color3.b, 1f));
				break;
			}
			case ShaderType.Glass:
			{
				Color color2 = material.GetColor("_Color");
				material.SetColor("_Color", new Color(color2.r, color2.g, color2.b, 0.55f));
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
				material.SetTexture("_PerturbTex", GetPerturbTexture(texture));
			}
			materialCache[materialName] = material;
			materialCachePaint[material] = paint;
			materialCacheTexture[material] = texture;
		}
		else
		{
			BWLog.Warning("Missing paint '" + paint + "' + " + materials.ContainsKey(paint) + " or texture '" + texture + "' " + textures.ContainsKey(texture));
			ResourceLoader.LoadTexture("Plain");
			material = GetMaterial("Grey", "Plain", ShaderType.Normal);
		}
		return material;
	}

	public static void UpdateCachedColors()
	{
		foreach (KeyValuePair<string, Material> item in materialCache)
		{
			Material value = item.Value;
			if (materialCachePaint.ContainsKey(value))
			{
				string text = materialCachePaint[value];
				Material material = Resources.Load("Paints/Paint " + text) as Material;
				value.SetColor("_Color", material.GetColor("_Color"));
				value.SetColor("_Emission", material.GetColor("_Emission"));
				value.SetColor("_SpecColor", material.GetColor("_SpecColor"));
				value.SetFloat("_Shininess", material.GetFloat("_Shininess"));
				value.SetFloat("_Gloss", material.GetFloat("_Gloss"));
				value.SetFloat("_TextureEmissive", material.GetFloat("_TextureEmissive"));
				Object.Destroy(material);
			}
		}
	}

	public static void SetShinyness(Material matPaint)
	{
		matPaint.SetFloat("_Shininess", 0.25f);
		matPaint.SetFloat("_Gloss", 4f);
		matPaint.SetFloat("_TextureEmissive", 0.558209f);
	}

	public static void DetectTerrainMaterials(GameObject go)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform.GetComponent<Renderer>() != null))
			{
				continue;
			}
			Material[] sharedMaterials = transform.GetComponent<Renderer>().sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (!materialCachePaint.ContainsKey(sharedMaterials[j]))
				{
					string paint = sharedMaterials[j].name.Substring("Paint ".Length);
					Material material = GetMaterial(paint, "Plain", ShaderType.Normal);
					sharedMaterials[j] = material;
				}
			}
			transform.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
		}
	}

	public static bool TextureIsTransparent(string texture)
	{
		return transparentTextures.Contains(texture);
	}

	public static bool IsMaterialShaderTexture(string texture)
	{
		return noUvTextures.Contains(texture);
	}

	public static bool TextureRequiresUVs(string texture)
	{
		return !noUvTextures.Contains(texture);
	}

	public static bool FourSidesIgnoreRightLeft(string texture)
	{
		bool value;
		return !fourSidesIgnoreRightLeft.TryGetValue(texture, out value) || value;
	}

	public static bool TwoSidesMirror(string texture)
	{
		bool value;
		return !twoSidesMirror.TryGetValue(texture, out value) || value;
	}

	public static float MipMapBias(string texture)
	{
		float value = 0f;
		mipMapBias.TryGetValue(texture, out value);
		return value;
	}

	public static bool IsNormalTerrainTexture(string texture)
	{
		bool result = false;
		if (textureInfos.ContainsKey(texture))
		{
			result = textureInfos[texture].shader == ShaderType.NormalTerrain;
		}
		return result;
	}

	public static bool IsTerrainTexture(string texture)
	{
		bool result = false;
		if (textureInfos.ContainsKey(texture))
		{
			result = textureInfos[texture].shader == ShaderType.Terrain;
		}
		return result;
	}

	public static bool IsStreamingLiquid(string texture)
	{
		bool result = false;
		if (textureInfos.ContainsKey(texture))
		{
			result = textureInfos[texture].shader == ShaderType.StreamingLiquid;
		}
		return result;
	}

	private static Texture2D GetPerturbTexture(string texture)
	{
		texture = "Just one texture for now";
		if (perturbTextures.TryGetValue(texture, out var value))
		{
			return value;
		}
		value = (Texture2D)Resources.Load("Env Effect/Perturb Texture", typeof(Texture2D));
		perturbTextures[texture] = value;
		return value;
	}
}
