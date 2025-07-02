using System;
using System.Collections.Generic;
using Blocks;
using Exdilin;
using UnityEngine;

public static class ResourceLoader
{
	public static HashSet<string> loadedTextures = new HashSet<string>();

	public static void LoadPaintsFromResources(string baseDir)
	{
		UnityEngine.Object[] array = Resources.LoadAll(baseDir, typeof(Material));
		for (int i = 0; i < array.Length; i++)
		{
			Material material = (Material)array[i];
			string text = material.name;
			if (text.StartsWith("Paint "))
			{
				text = text.Substring("Paint ".Length);
			}
			Materials.SetShinyness(material);
			Materials.materials[text] = material;
			Blocksworld.publicProvidedGafs.Add(new GAF("Block.PaintTo", text));
		}
	}

	public static void LoadTexturesFromResources(string baseDir)
	{
		foreach (object value in Enum.GetValues(typeof(ShaderType)))
		{
			ShaderType shaderType = (ShaderType)value;
			string name = Enum.GetName(typeof(ShaderType), shaderType);
			foreach (object value2 in Enum.GetValues(typeof(Mapping)))
			{
				Mapping mapping = (Mapping)value2;
				string name2 = Enum.GetName(typeof(Mapping), mapping);
				string path = baseDir + "/" + name + "/" + name2;
				UnityEngine.Object[] array = Resources.LoadAll(path, typeof(Texture2D));
				for (int i = 0; i < array.Length; i++)
				{
					Texture2D texture = (Texture2D)array[i];
					AddTexture(texture, mapping, shaderType);
				}
			}
		}
	}

	public static string[] ParseTextAssetLines(string filename)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(filename);
		if (textAsset != null)
		{
			return textAsset.text.Split('\n');
		}
		return null;
	}

	public static void LoadSFXNames()
	{
		string[] array = ParseTextAssetLines("SFXList");
		if (array != null)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					Sound.existingSfxs.Add(text);
				}
			}
		}
		else
		{
			BWLog.Info("Could not find SFXList");
		}
	}

	public static void LoadBlocksFromResources(string baseDir)
	{
		string[] array = ParseTextAssetLines("BlockList");
		List<string> list = new List<string>();
		if (array != null)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (text2.Length > 0)
				{
					list.Add(text2);
				}
			}
		}
		else
		{
			BWLog.Info("Could not find BlockList, loading all blocks instead...");
			UnityEngine.Object[] array3 = Resources.LoadAll(baseDir, typeof(GameObject));
			for (int j = 0; j < array3.Length; j++)
			{
				GameObject gameObject = (GameObject)array3[j];
				if (gameObject.name.StartsWith("Prefab "))
				{
					string item = gameObject.name.Substring("Prefab ".Length);
					list.Add(item);
				}
			}
		}
		foreach (string item2 in list)
		{
			Blocksworld.existingBlockNames.Add(item2);
			Blocksworld.publicProvidedGafs.Add(new GAF("Block.Create", item2));
		}
	}

	public static void UpdateTextureInfos()
	{
		if (Materials.textureInfos != null)
		{
			return;
		}
		Materials.textureInfos = new Dictionary<string, TextureInfo>();
		string[] array = ParseTextAssetLines("TextureList");
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Length == 0)
			{
				continue;
			}
			string[] array3 = text.Split(',');
			if (array3.Length == 3)
			{
				string text2 = array3[0];
				if (text2.Length > 0)
				{
					TextureInfo value = new TextureInfo(text2, (ShaderType)Enum.Parse(typeof(ShaderType), array3[1]), (Mapping)Enum.Parse(typeof(Mapping), array3[2]));
					Materials.textureInfos[text2] = value;
				}
			}
		}
	}

	public static void LoadTexture(string name, string baseDir = "Textures")
	{
		if (Materials.textureInfos.ContainsKey(name))
		{
			TextureInfo textureInfo = Materials.textureInfos[name];
			string name2 = Enum.GetName(typeof(ShaderType), textureInfo.shader);
			string name3 = Enum.GetName(typeof(Mapping), textureInfo.mapping);
			string path = baseDir + "/" + name2 + "/" + name3 + "/" + name;
			Texture2D texture2D = (Texture2D)AssetsManager.GetResource(path, typeof(Texture2D));
			if (texture2D != null)
			{
				float num = Materials.MipMapBias(name);
				if (num != 0f)
				{
					texture2D.mipMapBias = num;
				}
				AddTexture(texture2D, textureInfo.mapping, textureInfo.shader);
			}
			return;
		}
		OnScreenLog.AddLogItem("Could not find '" + name + "' in texture list. Use 'Blocksworld -> Generate Block and Texture Lists'", 5f, log: true);
		foreach (object value in Enum.GetValues(typeof(ShaderType)))
		{
			ShaderType shaderType = (ShaderType)value;
			string name4 = Enum.GetName(typeof(ShaderType), shaderType);
			foreach (object value2 in Enum.GetValues(typeof(Mapping)))
			{
				Mapping mapping = (Mapping)value2;
				string name5 = Enum.GetName(typeof(Mapping), mapping);
				string path2 = baseDir + "/" + name4 + "/" + name5 + "/" + name;
				Texture2D texture2D2 = (Texture2D)Resources.Load(path2, typeof(Texture2D));
				if (texture2D2 != null)
				{
					AddTexture(texture2D2, mapping, shaderType);
					return;
				}
			}
		}
	}

	private static void AddTexture(Texture2D texture, Mapping mapping, ShaderType shader)
	{
		string name = texture.name;
		Materials.textures[name] = texture;
		Materials.SetMapping(name, mapping);
		Materials.shaders[name] = shader;
		loadedTextures.Add(name);
		if (shader == ShaderType.SkyCube || shader == ShaderType.Metal || shader == ShaderType.NormalGold)
		{
			string text = name;
			if (shader == ShaderType.NormalGold)
			{
				text = "Metal";
			}
			Cubemap cubemap = (Cubemap)Resources.Load("Cubemaps/" + text, typeof(Cubemap));
			if (cubemap != null)
			{
				Materials.cubemaps[name] = cubemap;
			}
			Cubemap cubemap2 = (Cubemap)Resources.Load("Cubemaps/" + name + " Overlay", typeof(Cubemap));
			if (cubemap2 != null)
			{
				Materials.cubemaps[name + " Overlay"] = cubemap2;
			}
		}
	}

	public static void UnloadUnusedBlockPrefabs(HashSet<string> toKeep)
	{
		HashSet<string> hashSet = new HashSet<string>(toKeep);
		List<string> list = new List<string>(Blocksworld.goPrefabs.Keys);
		foreach (string item in list)
		{
			if (!toKeep.Contains(item))
			{
				continue;
			}
			GameObject gameObject = Blocksworld.goPrefabs[item];
			foreach (object item2 in gameObject.transform)
			{
				Transform transform = (Transform)item2;
				hashSet.Add(transform.gameObject.name);
			}
		}
		foreach (string item3 in list)
		{
			if (!toKeep.Contains(item3))
			{
				Blocksworld.UnloadBlock(item3, hashSet);
			}
		}
	}

	public static void UnloadUnusedTextures(HashSet<string> toKeep)
	{
		List<string> list = new List<string>();
		foreach (GAF publicProvidedGaf in Blocksworld.publicProvidedGafs)
		{
			if (publicProvidedGaf.Predicate == Block.predicatePaintTo)
			{
				list.Add((string)publicProvidedGaf.Args[0]);
			}
		}
		HashSet<Cubemap> hashSet = new HashSet<Cubemap>();
		foreach (string item in toKeep)
		{
			if (Materials.cubemaps.ContainsKey(item))
			{
				hashSet.Add(Materials.cubemaps[item]);
			}
		}
		foreach (string loadedTexture in loadedTextures)
		{
			if (!toKeep.Contains(loadedTexture) && Materials.textures.ContainsKey(loadedTexture))
			{
				Texture assetToUnload = Materials.textures[loadedTexture];
				Cubemap cubemap = null;
				if (Materials.cubemaps.ContainsKey(loadedTexture))
				{
					cubemap = Materials.cubemaps[loadedTexture];
				}
				Cubemap cubemap2 = null;
				string key = loadedTexture + " Overlay";
				if (Materials.cubemaps.ContainsKey(key))
				{
					cubemap2 = Materials.cubemaps[key];
				}
				ShaderType shaderType = Materials.shaders[loadedTexture];
				Materials.textures.Remove(loadedTexture);
				Materials.RemoveMapping(loadedTexture);
				Materials.shaders.Remove(loadedTexture);
				Resources.UnloadAsset(assetToUnload);
				if (cubemap != null && !hashSet.Contains(cubemap))
				{
					Resources.UnloadAsset(cubemap);
				}
				if (cubemap2 != null && !hashSet.Contains(cubemap2))
				{
					Resources.UnloadAsset(cubemap2);
				}
				RemoveMaterials(list, loadedTexture, shaderType);
				if (shaderType == ShaderType.NormalTerrain)
				{
					RemoveMaterials(list, loadedTexture, ShaderType.Normal);
				}
			}
		}
		loadedTextures.Clear();
		foreach (string item2 in toKeep)
		{
			loadedTextures.Add(item2);
		}
	}

	private static void RemoveMaterials(List<string> paints, string name, ShaderType shader)
	{
		foreach (string paint in paints)
		{
			string materialName = Materials.GetMaterialName(paint, name, shader);
			Material material = null;
			if (material == null && Materials.materials.ContainsKey(materialName))
			{
				material = Materials.materials[materialName];
			}
			if (material == null && Materials.materialCache.ContainsKey(materialName))
			{
				material = Materials.materialCache[materialName];
			}
			if (material != null)
			{
				Materials.materialCache.Remove(materialName);
				Materials.materials.Remove(materialName);
				Materials.materialCachePaint.Remove(material);
				Materials.materialCacheTexture.Remove(material);
				UnityEngine.Object.Destroy(material);
			}
		}
	}
}
