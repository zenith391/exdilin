using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Harmony;
using System;

namespace Exdilin
{

	[HarmonyPatch(typeof(Resources))]
	[HarmonyPatch("Load", typeof(string))]
	class ResourcesPatchClass {
		static bool Prefix(string path, ref UnityEngine.Object __result) {
			//Debug.Log("Resources.Load(\"" + path + "\") = " + AssetsManager.HasObject(path));
			if (AssetsManager.HasObject(path)) {
				__result = AssetsManager.GetObject(path);
				return false;
			}
			return true;
		}
	}

	public static class AssetsManager
    {
        private static Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
		private static Dictionary<string, string> resourcesOwner = new Dictionary<string, string>();
		private static List<string> loadingResources = new List<string>();

		public static UnityEngine.Object GetObject(string fullId) {
			return resources[fullId];
		}

		public static UnityEngine.Object GetResource(string path, Type type) {
			string sanitized = path.Replace('\\', '/');
			if (AssetsManager.HasObject(sanitized))
				return AssetsManager.GetObject(sanitized);
			return Resources.Load(path, type);
		}

		public static bool HasObject(string fullId)
        {
            UnityEngine.Object obj = null;
            return resources.TryGetValue(fullId, out obj);
        }

		/// <summary>
		/// Plug the required system to <code>UnityEngine.Resources</code>
		/// </summary>
		/// <remarks>This should only be called by Exdilin itself and mods should not call it.</remarks>
		public static void Plug() {
			HarmonyInstance harmony = HarmonyInstance.Create("io.zenith391.exdilin.AssetsManager");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static bool HasObject(Mod mod, string id)
        {
            return HasObject(mod.Id + "/" + id);
        }

        public static Texture2D GetTexture(string fullId)
        {
            UnityEngine.Object obj = null;
            resources.TryGetValue(fullId, out obj);
            if (obj != null && obj is Texture2D) {
                return obj as Texture2D;
            } else {
                return null;
            }
        }

		public static AudioClip GetAudioClip(string fullId) {
			UnityEngine.Object obj = null;
			resources.TryGetValue(fullId, out obj);
			if (obj != null && obj is AudioClip) {
				return obj as AudioClip;
			} else {
				return null;
			}
		}

		public static Mesh GetMesh(string fullId)
        {
            UnityEngine.Object obj = null;
            resources.TryGetValue(fullId, out obj);
            if (obj != null && obj is Mesh) {
                return obj as Mesh;
            } else {
                return null;
            }
        }

        public static Texture2D GetTexture(Mod mod, string id)
        {
            return GetTexture(mod.Id + "/" + id);
        }

        public static Mesh GetMesh(Mod mod, string id)
        {
            return GetMesh(mod.Id + "/" + id);
        }

		public static Mod GetOwner(string id) {
			string modId = null;
			resourcesOwner.TryGetValue(id, out modId);
			if (modId != null) {
				return Mod.GetById(resourcesOwner[id]);
			} else {
				BWLog.Info("no mod id for " + id);
				return null;
			}
		}

		public static IEnumerator LoadAudioFromURL(Mod mod, string id, string url, bool absolute = false) {
			BWLog.Info("Exdilin) Loading audio " + id + " from " + url);
			WWW www = new WWW(url);
			yield return www;
			AudioClip clip = www.audioClip;
			string fullId = absolute ? id : mod.Id + "/" + id;
			string name = fullId.Substring(fullId.LastIndexOf('/') + 1);
			clip.name = name;
			resources.Add(fullId, clip);
			resourcesOwner.Add(fullId, mod.Id);
			if (loadingResources.Contains(fullId)) {
				loadingResources.Remove(fullId);
			}
			yield break;
		}

		public static IEnumerator LoadTextureFromURL(Mod mod, string id, string url, bool absolute = false)
        {
            BWLog.Info("Exdilin) Loading texture " + id + " from " + url);
            WWW www = new WWW(url);
            yield return www;
            Texture2D tex = www.texture;
			string fullId = absolute ? id : mod.Id + "/" + id;
			string name = fullId.Substring(fullId.LastIndexOf('/')+1);
			tex.name = name;
            resources.Add(fullId, tex);
			resourcesOwner.Add(fullId, mod.Id);
			if (loadingResources.Contains(fullId)) {
				loadingResources.Remove(fullId);
			}
			yield break;
        }

		public static IEnumerator LoadAudio(Mod mod, string id, string path, bool absolute = false) {
			return LoadAudioFromURL(mod, id, "file:///" + mod.Directory + "/" + path, absolute);
		}

		public static IEnumerator LoadTexture(Mod mod, string id, string path, bool absolute = false) {
			return LoadTextureFromURL(mod, id, "file:///" + mod.Directory + "/" + path, absolute);
		}

		public static Texture2D UseTemporaryTexture(Mod mod, string id, string url) {
			string fullId = mod.Id + "/" + id;
			if (resources.ContainsKey(fullId)) {
				return resources[fullId] as Texture2D;
			} else if (!loadingResources.Contains(fullId)) {
				loadingResources.Add(fullId);
				Blocksworld.bw.StartCoroutine(LoadTextureFromURL(mod, id, url));
				return resources[fullId] as Texture2D;
			} else {
				return null;
			}
		}

		public static UnityEngine.Object Unload(Mod mod, string id) {
			string fullId = mod.Id + "/" + id;
			if (resources.TryGetValue(fullId, out UnityEngine.Object obj)) {
				resources.Remove(fullId);
				resourcesOwner.Remove(fullId);
				return obj;
			} else {
				return null;
			}
		}

		public static void PutAsset(string id, UnityEngine.Object obj) {
			resources[id] = obj;
		}

		/// <summary>
		/// Althought it looks like it is asynchronous and breaks while loading, this is actually a synchronous
		/// operation wrapped in an IEnumerator, so LoadMeshSync is recommended instead.
		/// </summary>
		/// <returns>The mesh.</returns>
		/// <param name="mod">Mod.</param>
		/// <param name="id">Identifier.</param>
		/// <param name="path">Path.</param>
		/// <param name="absolute">If set to <c>true</c> absolute.</param>
		public static IEnumerator LoadMesh(Mod mod, string id, string path, bool absolute = false)
        {
			LoadMeshSync(mod, id, path, absolute);
            yield break;
        }

		public static void LoadMeshSync(Mod mod, string id, string path, bool absolute = false) {
			BWLog.Info("Exdilin) Loading mesh " + id + " from " + path);
			ObjImporter importer = new ObjImporter();
			string fullId = absolute ? id : mod.Id + "/" + id;
			resources.Add(fullId, importer.ImportFile(mod.Directory + "/" + path));
			resourcesOwner.Add(fullId, mod.Id);
		}

		public static IEnumerator LoadTexture(string id, string path)
        {
            return LoadTexture(Mod.ExecutionMod, id, path);
        }
    }
}
