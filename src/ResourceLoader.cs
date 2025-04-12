using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000275 RID: 629
public static class ResourceLoader
{
	// Token: 0x06001D36 RID: 7478 RVA: 0x000CE278 File Offset: 0x000CC678
	public static void LoadPaintsFromResources(string baseDir)
	{
		foreach (Material material in Resources.LoadAll(baseDir, typeof(Material)))
		{
			string text = material.name;
			if (text.StartsWith("Paint "))
			{
				text = text.Substring("Paint ".Length);
			}
			Materials.SetShinyness(material);
			Materials.materials[text] = material;
			Blocksworld.publicProvidedGafs.Add(new GAF("Block.PaintTo", new object[]
			{
				text
			}));
		}
	}

	// Token: 0x06001D37 RID: 7479 RVA: 0x000CE30C File Offset: 0x000CC70C
	public static void LoadTexturesFromResources(string baseDir)
	{
		IEnumerator enumerator = Enum.GetValues(typeof(ShaderType)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				ShaderType shaderType = (ShaderType)obj;
				string name = Enum.GetName(typeof(ShaderType), shaderType);
				IEnumerator enumerator2 = Enum.GetValues(typeof(Mapping)).GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						object obj2 = enumerator2.Current;
						Mapping mapping = (Mapping)obj2;
						string name2 = Enum.GetName(typeof(Mapping), mapping);
						string path = string.Concat(new string[]
						{
							baseDir,
							"/",
							name,
							"/",
							name2
						});
						foreach (Texture2D texture in Resources.LoadAll(path, typeof(Texture2D)))
						{
							ResourceLoader.AddTexture(texture, mapping, shaderType);
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator2 as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = (enumerator as IDisposable)) != null)
			{
				disposable2.Dispose();
			}
		}
	}

	// Token: 0x06001D38 RID: 7480 RVA: 0x000CE484 File Offset: 0x000CC884
	public static string[] ParseTextAssetLines(string filename)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(filename);
		//Debug.Log(filename + ": " + textAsset.text);
		if (textAsset != null)
		{
			return textAsset.text.Split(new char[]
			{
				'\n'
			});
		}
		return null;
	}

	// Token: 0x06001D39 RID: 7481 RVA: 0x000CE4C4 File Offset: 0x000CC8C4
	public static void LoadSFXNames()
	{
		string[] array = ResourceLoader.ParseTextAssetLines("SFXList");
		if (array != null)
		{
			foreach (string text in array)
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

	// Token: 0x06001D3A RID: 7482 RVA: 0x000CE524 File Offset: 0x000CC924
	public static void LoadBlocksFromResources(string baseDir) {
		string[] array = ResourceLoader.ParseTextAssetLines("BlockList");
		List<string> list = new List<string>();
		if (array != null) {
			foreach (string text in array) {
				string text2 = text.Trim();
				if (text2.Length > 0) {
					list.Add(text2);
				}
			}
		} else {
			BWLog.Info("Could not find BlockList, loading all blocks instead...");
			foreach (GameObject gameObject in Resources.LoadAll(baseDir, typeof(GameObject))) {
				if (gameObject.name.StartsWith("Prefab ")) {
					string item = gameObject.name.Substring("Prefab ".Length);
					list.Add(item);
				}
			}
		}
		foreach (string text3 in list) {
			Blocksworld.existingBlockNames.Add(text3);
			Blocksworld.publicProvidedGafs.Add(new GAF("Block.Create", new object[]
			{
				text3
			}));
		}
	}

	// Token: 0x06001D3B RID: 7483 RVA: 0x000CE678 File Offset: 0x000CCA78
	public static void UpdateTextureInfos()
	{
		if (Materials.textureInfos == null)
		{
			Materials.textureInfos = new Dictionary<string, TextureInfo>();
			string[] array = ResourceLoader.ParseTextAssetLines("TextureList");
			foreach (string text in array)
			{
				if (text.Length != 0)
				{
					string[] array3 = text.Split(new char[]
					{
						','
					});
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
		}
	}

	// Token: 0x06001D3C RID: 7484 RVA: 0x000CE748 File Offset: 0x000CCB48
	public static void LoadTexture(string name, string baseDir = "Textures")
	{
		if (Materials.textureInfos.ContainsKey(name))
		{
			TextureInfo textureInfo = Materials.textureInfos[name];
			string name2 = Enum.GetName(typeof(ShaderType), textureInfo.shader);
			string name3 = Enum.GetName(typeof(Mapping), textureInfo.mapping);
			string path = string.Concat(new string[]
			{
				baseDir,
				"/",
				name2,
				"/",
				name3,
				"/",
				name
			});
			Texture2D texture2D = (Texture2D) Exdilin.AssetsManager.GetResource(path, typeof(Texture2D));
			if (texture2D != null)
			{
				float num = Materials.MipMapBias(name);
				if (num != 0f)
				{
					texture2D.mipMapBias = num;
				}
				ResourceLoader.AddTexture(texture2D, textureInfo.mapping, textureInfo.shader);
			}
		}
		else
		{
			OnScreenLog.AddLogItem("Could not find '" + name + "' in texture list. Use 'Blocksworld -> Generate Block and Texture Lists'", 5f, true);
			IEnumerator enumerator = Enum.GetValues(typeof(ShaderType)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ShaderType shaderType = (ShaderType)obj;
					string name4 = Enum.GetName(typeof(ShaderType), shaderType);
					IEnumerator enumerator2 = Enum.GetValues(typeof(Mapping)).GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Mapping mapping = (Mapping)obj2;
							string name5 = Enum.GetName(typeof(Mapping), mapping);
							string path2 = string.Concat(new string[]
							{
								baseDir,
								"/",
								name4,
								"/",
								name5,
								"/",
								name
							});
							Texture2D texture2D2 = (Texture2D)Resources.Load(path2, typeof(Texture2D));
							if (texture2D2 != null)
							{
								ResourceLoader.AddTexture(texture2D2, mapping, shaderType);
								return;
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator2 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = (enumerator as IDisposable)) != null)
				{
					disposable2.Dispose();
				}
			}
		}
	}

	// Token: 0x06001D3D RID: 7485 RVA: 0x000CE9C4 File Offset: 0x000CCDC4
	private static void AddTexture(Texture2D texture, Mapping mapping, ShaderType shader)
	{
		string name = texture.name;
		Materials.textures[name] = texture;
		Materials.SetMapping(name, mapping);
		Materials.shaders[name] = shader;
		ResourceLoader.loadedTextures.Add(name);
		if (shader == ShaderType.SkyCube || shader == ShaderType.Metal || shader == ShaderType.NormalGold)
		{
			string str = name;
			if (shader == ShaderType.NormalGold)
			{
				str = "Metal";
			}
			Cubemap cubemap = (Cubemap)Resources.Load("Cubemaps/" + str, typeof(Cubemap));
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

	// Token: 0x06001D3E RID: 7486 RVA: 0x000CEAA8 File Offset: 0x000CCEA8
	public static void UnloadUnusedBlockPrefabs(HashSet<string> toKeep)
	{
		HashSet<string> hashSet = new HashSet<string>(toKeep);
		List<string> list = new List<string>(Blocksworld.goPrefabs.Keys);
		foreach (string text in list)
		{
			if (toKeep.Contains(text))
			{
				GameObject gameObject = Blocksworld.goPrefabs[text];
				IEnumerator enumerator2 = gameObject.transform.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						object obj = enumerator2.Current;
						Transform transform = (Transform)obj;
						hashSet.Add(transform.gameObject.name);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator2 as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
		}
		foreach (string text2 in list)
		{
			if (!toKeep.Contains(text2))
			{
				Blocksworld.UnloadBlock(text2, hashSet);
			}
		}
	}

	// Token: 0x06001D3F RID: 7487 RVA: 0x000CEBE8 File Offset: 0x000CCFE8
	public static void UnloadUnusedTextures(HashSet<string> toKeep)
	{
		List<string> list = new List<string>();
		foreach (GAF gaf in Blocksworld.publicProvidedGafs)
		{
			if (gaf.Predicate == Block.predicatePaintTo)
			{
				list.Add((string)gaf.Args[0]);
			}
		}
		HashSet<Cubemap> hashSet = new HashSet<Cubemap>();
		foreach (string key in toKeep)
		{
			if (Materials.cubemaps.ContainsKey(key))
			{
				hashSet.Add(Materials.cubemaps[key]);
			}
		}
		foreach (string text in ResourceLoader.loadedTextures)
		{
			if (!toKeep.Contains(text))
			{
				if (Materials.textures.ContainsKey(text))
				{
					Texture assetToUnload = Materials.textures[text];
					Cubemap cubemap = null;
					if (Materials.cubemaps.ContainsKey(text))
					{
						cubemap = Materials.cubemaps[text];
					}
					Cubemap cubemap2 = null;
					string key2 = text + " Overlay";
					if (Materials.cubemaps.ContainsKey(key2))
					{
						cubemap2 = Materials.cubemaps[key2];
					}
					ShaderType shaderType = Materials.shaders[text];
					Materials.textures.Remove(text);
					Materials.RemoveMapping(text);
					Materials.shaders.Remove(text);
					Resources.UnloadAsset(assetToUnload);
					if (cubemap != null && !hashSet.Contains(cubemap))
					{
						Resources.UnloadAsset(cubemap);
					}
					if (cubemap2 != null && !hashSet.Contains(cubemap2))
					{
						Resources.UnloadAsset(cubemap2);
					}
					ResourceLoader.RemoveMaterials(list, text, shaderType);
					if (shaderType == ShaderType.NormalTerrain)
					{
						ResourceLoader.RemoveMaterials(list, text, ShaderType.Normal);
					}
				}
			}
		}
		ResourceLoader.loadedTextures.Clear();
		foreach (string item in toKeep)
		{
			ResourceLoader.loadedTextures.Add(item);
		}
	}

	// Token: 0x06001D40 RID: 7488 RVA: 0x000CEEBC File Offset: 0x000CD2BC
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

	// Token: 0x040017E1 RID: 6113
	public static HashSet<string> loadedTextures = new HashSet<string>();
}
