using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace Exdilin;

public static class AssetsManager
{
	private static Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();

	private static Dictionary<string, string> resourcesOwner = new Dictionary<string, string>();

	private static List<string> loadingResources = new List<string>();

	public static UnityEngine.Object GetObject(string fullId)
	{
		return resources[fullId];
	}

	public static UnityEngine.Object GetResource(string path, Type type)
	{
		string fullId = path.Replace('\\', '/');
		if (HasObject(fullId))
		{
			return GetObject(fullId);
		}
		return Resources.Load(path, type);
	}

	public static bool HasObject(string fullId)
	{
		UnityEngine.Object value = null;
		return resources.TryGetValue(fullId, out value);
	}

	public static void Plug()
	{
		HarmonyInstance harmonyInstance = HarmonyInstance.Create("io.zenith391.exdilin.AssetsManager");
		harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
	}

	public static bool HasObject(Mod mod, string id)
	{
		return HasObject(mod.Id + "/" + id);
	}

	public static Texture2D GetTexture(string fullId)
	{
		UnityEngine.Object value = null;
		resources.TryGetValue(fullId, out value);
		if (value != null && value is Texture2D)
		{
			return value as Texture2D;
		}
		return null;
	}

	public static AudioClip GetAudioClip(string fullId)
	{
		UnityEngine.Object value = null;
		resources.TryGetValue(fullId, out value);
		if (value != null && value is AudioClip)
		{
			return value as AudioClip;
		}
		return null;
	}

	public static Mesh GetMesh(string fullId)
	{
		UnityEngine.Object value = null;
		resources.TryGetValue(fullId, out value);
		if (value != null && value is Mesh)
		{
			return value as Mesh;
		}
		return null;
	}

	public static Texture2D GetTexture(Mod mod, string id)
	{
		return GetTexture(mod.Id + "/" + id);
	}

	public static Mesh GetMesh(Mod mod, string id)
	{
		return GetMesh(mod.Id + "/" + id);
	}

	public static Mod GetOwner(string id)
	{
		string value = null;
		resourcesOwner.TryGetValue(id, out value);
		if (value != null)
		{
			return Mod.GetById(resourcesOwner[id]);
		}
		BWLog.Info("no mod id for " + id);
		return null;
	}

	public static IEnumerator LoadAudioFromURL(Mod mod, string id, string url, bool absolute = false)
	{
		BWLog.Info("Exdilin) Loading audio " + id + " from " + url);
		WWW www = new WWW(url);
		yield return www;
		AudioClip audioClip = www.audioClip;
		string text = (absolute ? id : (mod.Id + "/" + id));
		string name = text.Substring(text.LastIndexOf('/') + 1);
		audioClip.name = name;
		resources.Add(text, audioClip);
		resourcesOwner.Add(text, mod.Id);
		if (loadingResources.Contains(text))
		{
			loadingResources.Remove(text);
		}
	}

	public static IEnumerator LoadTextureFromURL(Mod mod, string id, string url, bool absolute = false)
	{
		BWLog.Info("Exdilin) Loading texture " + id + " from " + url);
		WWW www = new WWW(url);
		yield return www;
		Texture2D texture = www.texture;
		string text = (absolute ? id : (mod.Id + "/" + id));
		string name = text.Substring(text.LastIndexOf('/') + 1);
		texture.name = name;
		resources.Add(text, texture);
		resourcesOwner.Add(text, mod.Id);
		if (loadingResources.Contains(text))
		{
			loadingResources.Remove(text);
		}
	}

	public static IEnumerator LoadAudio(Mod mod, string id, string path, bool absolute = false)
	{
		return LoadAudioFromURL(mod, id, "file:///" + mod.Directory + "/" + path, absolute);
	}

	public static IEnumerator LoadTexture(Mod mod, string id, string path, bool absolute = false)
	{
		return LoadTextureFromURL(mod, id, "file:///" + mod.Directory + "/" + path, absolute);
	}

	public static Texture2D UseTemporaryTexture(Mod mod, string id, string url)
	{
		string text = mod.Id + "/" + id;
		if (resources.ContainsKey(text))
		{
			return resources[text] as Texture2D;
		}
		if (!loadingResources.Contains(text))
		{
			loadingResources.Add(text);
			Blocksworld.bw.StartCoroutine(LoadTextureFromURL(mod, id, url));
			return resources[text] as Texture2D;
		}
		return null;
	}

	public static UnityEngine.Object Unload(Mod mod, string id)
	{
		string key = mod.Id + "/" + id;
		if (resources.TryGetValue(key, out var value))
		{
			resources.Remove(key);
			resourcesOwner.Remove(key);
			return value;
		}
		return null;
	}

	public static void PutAsset(string id, UnityEngine.Object obj)
	{
		resources[id] = obj;
	}

	public static IEnumerator LoadMesh(Mod mod, string id, string path, bool absolute = false)
	{
		LoadMeshSync(mod, id, path, absolute);
		yield break;
	}

	public static void LoadMeshSync(Mod mod, string id, string path, bool absolute = false)
	{
		BWLog.Info("Exdilin) Loading mesh " + id + " from " + path);
		ObjImporter objImporter = new ObjImporter();
		string key = (absolute ? id : (mod.Id + "/" + id));
		resources.Add(key, objImporter.ImportFile(mod.Directory + "/" + path));
		resourcesOwner.Add(key, mod.Id);
	}

	public static IEnumerator LoadTexture(string id, string path)
	{
		return LoadTexture(Mod.ExecutionMod, id, path);
	}
}
