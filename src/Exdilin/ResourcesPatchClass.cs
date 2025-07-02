using System;
using Harmony;
using UnityEngine;

namespace Exdilin;

[HarmonyPatch(typeof(Resources))]
[HarmonyPatch("Load", new Type[] { typeof(string) })]
internal class ResourcesPatchClass
{
	private static bool Prefix(string path, ref UnityEngine.Object __result)
	{
		if (AssetsManager.HasObject(path))
		{
			__result = AssetsManager.GetObject(path);
			return false;
		}
		return true;
	}
}
