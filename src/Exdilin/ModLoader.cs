using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Blocks;
using Exdilin.Lua;
using ModExdilin;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Exdilin;

public static class ModLoader
{
	public delegate void OnModLoading(string id, string stage);

	public static List<Mod> mods = new List<Mod>();

	public static List<string> pendingErrorMsg = new List<string>();

	private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

	public static bool IsOverlayLoaded;

	public static event EventHandler OverlaysAvailable;

	public static IEnumerator ShowErrors()
	{
		while (!BWStandalone.Instance.menuLoaded)
		{
			yield return new WaitForSeconds(0.5f);
		}
		IsOverlayLoaded = true;
		foreach (string item in pendingErrorMsg)
		{
			BWStandalone.Overlays.ShowMessage(item);
			yield return new WaitForSeconds(0.5f);
			while (BWStandalone.Overlays.IsShowingPopup())
			{
				yield return new WaitForSeconds(0.5f);
			}
		}
		ModLoader.OverlaysAvailable?.Invoke(null, new EventArgs());
	}

	private static void InitLuaOptions()
	{
		Script.DefaultOptions.DebugPrint = delegate(string msg)
		{
			BWLog.Info("[Lua Mod] " + msg);
		};
		UserData.RegisterType<ExdilinAPI>();
		UserData.RegisterType<LuaBlockItem>();
		UserData.RegisterType<LuaBlock>();
		UserData.RegisterType<BlockItem>();
		UserData.RegisterType<PredicateEntry>();
		UserData.RegisterType<Vector3>();
		UserData.RegisterType<ScriptRowExecutionInfo>();
	}

	public static IEnumerator LoadModsCoroutine(OnModLoading modLoading)
	{
		string text = Path.Combine(BWFilesystem.CurrentUserDataFolder, "mods");
		string CurrentUserLuaModsFolder = Path.Combine(BWFilesystem.CurrentUserDataFolder, "lua_mods");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Directory.Exists(CurrentUserLuaModsFolder))
		{
			Directory.CreateDirectory(CurrentUserLuaModsFolder);
		}
		Mod item = new ExdilinMod();
		mods.Add(item);
		BWLog.Info(text);
		string[] directories = Directory.GetDirectories(text);
		string[] array = directories;
		foreach (string file in array)
		{
			Assembly asm = null;
			try
			{
				asm = Assembly.LoadFrom(file + "\\Assembly.dll");
			}
			catch (Exception ex)
			{
				BWLog.Error(ex.Message);
				BWLog.Error(ex.StackTrace);
			}
			yield return null;
			Type[] types = asm.GetTypes();
			Type[] array2 = types;
			foreach (Type type in array2)
			{
				if (type.IsSubclassOf(typeof(Mod)))
				{
					Mod mod = Activator.CreateInstance(type) as Mod;
					mod.Directory = file;
					mods.Add(mod);
					assemblies[mod.Id] = asm;
				}
			}
		}
		InitLuaOptions();
		string[] directories2 = Directory.GetDirectories(CurrentUserLuaModsFolder);
		array = directories2;
		foreach (string text2 in array)
		{
			Debug.Log("Loading Lua mod " + text2);
			Script script = new Script(CoreModules.Preset_HardSandbox | CoreModules.Metatables | CoreModules.ErrorHandling | CoreModules.Coroutine | CoreModules.OS_Time);
			bool flag = false;
			string fileName = Path.GetFileName(text2);
			modLoading(fileName, "load");
			try
			{
				script.DoString(File.ReadAllText(text2 + "/init.lua", Encoding.UTF8));
			}
			catch (ScriptRuntimeException ex2)
			{
				pendingErrorMsg.Add("Runtime error in Lua mod " + fileName + ": " + ex2.DecoratedMessage);
				BWLog.Error(ex2.Message + "\n" + ex2.StackTrace);
				flag = true;
			}
			catch (SyntaxErrorException ex3)
			{
				pendingErrorMsg.Add("Syntax error in Lua mod " + fileName + ": " + ex3.DecoratedMessage);
				BWLog.Error(ex3.Message + "\n" + ex3.StackTrace);
				flag = true;
			}
			if (!flag)
			{
				LuaMod luaMod = new LuaMod(script, text2);
				luaMod.Directory = text2;
				mods.Add(luaMod);
			}
			yield return null;
		}
		string text3 = "[";
		foreach (Mod mod3 in mods)
		{
			if (mod3.IsImportant)
			{
				if (text3.Length > 1)
				{
					text3 += "|";
				}
				text3 = text3 + mod3.Name + "/" + mod3.Version;
			}
		}
		text3 += "]";
		if (text3 != "[]")
		{
			BWEnvConfig.BLOCKSWORLD_VERSION += text3;
		}
		foreach (Mod mod4 in mods)
		{
			BWLog.Info("Pre-initializing mod " + mod4.Id + " " + mod4.Version);
			modLoading(mod4.Id, "preinit");
			Mod.ExecutionMod = mod4;
			foreach (Dependency dependency in mod4.Dependencies)
			{
				bool flag2 = false;
				bool flag3 = false;
				foreach (Mod mod5 in mods)
				{
					if (dependency.Id == mod5.Id)
					{
						flag2 = true;
						if (mod5.Version < dependency.MinimumVersion || mod5.Version > dependency.MaximumVersion)
						{
							flag3 = true;
						}
					}
				}
				if (!flag2)
				{
					pendingErrorMsg.Add("Missing dependency " + dependency.Id + " for mod " + mod4.Name + ".");
				}
				if (flag3)
				{
					BWLog.Info("out of date!");
					pendingErrorMsg.Add("Out of date dependency " + dependency.Id + " for mod " + mod4.Name + ". " + dependency.MinimumVersion?.ToString() + " or greater required.");
				}
			}
			mod4.PreInit();
			Mod.ExecutionMod = null;
		}
		foreach (Mod mod6 in mods)
		{
			Mod mod2 = (Mod.ExecutionMod = mod6);
			if (mod2.Id != "exdilin" && !(mod2 is LuaMod))
			{
				BWLog.Info("Applying patches of " + mod2.Id + " " + mod2.Version);
				mod2.ApplyPatches(assemblies[mod2.Id]);
			}
			Mod.ExecutionMod = null;
		}
		foreach (string key in BlockItemsRegistry.GetBlockEntries().Keys)
		{
			BlockEntry blockEntry = BlockItemsRegistry.GetBlockEntries()[key];
			if (blockEntry.blockType != null)
			{
				Block.blockNameTypeMap[key] = blockEntry.blockType;
			}
		}
		AssetsManager.Plug();
	}
}
