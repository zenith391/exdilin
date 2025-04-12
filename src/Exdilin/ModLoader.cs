using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using MoonSharp.Interpreter;
using Exdilin.Lua;

namespace Exdilin
{
	public static class ModLoader {

		public static List<Mod> mods = new List<Mod>();
		public static List<string> pendingErrorMsg = new List<string>();
		private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		public static event EventHandler OverlaysAvailable;
		public static bool IsOverlayLoaded;

		public delegate void OnModLoading(string id, string stage);

		public static IEnumerator ShowErrors() {
			while (!BWStandalone.Instance.menuLoaded) {
				yield return new WaitForSeconds(0.5f);
			}
			IsOverlayLoaded = true;
			foreach (string msg in pendingErrorMsg) {
				BWStandalone.Overlays.ShowMessage(msg);
				yield return new WaitForSeconds(0.5f);
				while (BWStandalone.Overlays.IsShowingPopup()) {
					yield return new WaitForSeconds(0.5f);
				}
			}
			OverlaysAvailable?.Invoke(null, new EventArgs());
			yield break;
		}

		private static void InitLuaOptions() {
			Script.DefaultOptions.DebugPrint = delegate (string msg)
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
            string CurrentUserModsFolder = Path.Combine(BWFilesystem.CurrentUserDataFolder, "mods");
			string CurrentUserLuaModsFolder = Path.Combine(BWFilesystem.CurrentUserDataFolder, "lua_mods");
            if (!Directory.Exists(CurrentUserModsFolder))
            {
                Directory.CreateDirectory(CurrentUserModsFolder);
            }
			if (!Directory.Exists(CurrentUserLuaModsFolder)) {
				Directory.CreateDirectory(CurrentUserLuaModsFolder);
			}

			Mod m = new ModExdilin.ExdilinMod();
            mods.Add(m);
            BWLog.Info(CurrentUserModsFolder);
            string[] files = Directory.GetDirectories(CurrentUserModsFolder);
            foreach (string file in files)
            {
                Assembly asm = null;
                try
                {
                    asm = Assembly.LoadFrom(file + "\\Assembly.dll");
                }
                catch (Exception e)
                {
                    // TODO
                    BWLog.Error(e.Message);
                    BWLog.Error(e.StackTrace);
                }
                yield return null;

                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsSubclassOf(typeof(Mod)))
                    {
                        Mod mod = Activator.CreateInstance(t) as Mod;
                        mod.Directory = file;
                        mods.Add(mod);
                        assemblies[mod.Id] = asm;
                    }
                }
            }

			InitLuaOptions();
			string[] fullLuaMods = Directory.GetDirectories(CurrentUserLuaModsFolder); // lua mods that *can* contain assets
			foreach (string fullLuaMod in fullLuaMods) {
				Debug.Log("Loading Lua mod " + fullLuaMod);
				Script script = new Script(CoreModules.Basic | CoreModules.Bit32 | CoreModules.ErrorHandling | CoreModules.OS_Time
					| CoreModules.String | CoreModules.Table | CoreModules.TableIterators | CoreModules.Coroutine | CoreModules.GlobalConsts
					| CoreModules.Math | CoreModules.Metatables);
				bool failed = false;
				string id = Path.GetFileName(fullLuaMod);
				modLoading(id, "load");
				try {
					script.DoString(File.ReadAllText(fullLuaMod + "/init.lua", System.Text.Encoding.UTF8));
				} catch (ScriptRuntimeException ex) {
					pendingErrorMsg.Add("Runtime error in Lua mod " + id + ": " + ex.DecoratedMessage);
					BWLog.Error(ex.Message + "\n" + ex.StackTrace);
					failed = true;
				} catch (SyntaxErrorException ex) {
					pendingErrorMsg.Add("Syntax error in Lua mod " + id + ": " + ex.DecoratedMessage);
					BWLog.Error(ex.Message + "\n" + ex.StackTrace);
					failed = true;
				}
				if (!failed) {
					LuaMod mod = new LuaMod(script, fullLuaMod);
					mod.Directory = fullLuaMod;
					mods.Add(mod);
				}
				yield return null;
			}

			string versionConcat = "[";
            foreach (Mod mod in mods)
            {
                if (mod.IsImportant)
                {
                    if (versionConcat.Length > 1)
                    {
                        versionConcat += "|";
                    }
                    versionConcat += mod.Name + "/" + mod.Version;
                }
            }
            versionConcat += "]";

            if (versionConcat != "[]")
            {
                BWEnvConfig.BLOCKSWORLD_VERSION += versionConcat;
            }

            foreach (Mod mod in mods)
            {
                BWLog.Info("Pre-initializing mod " + mod.Id + " " + mod.Version);
				modLoading(mod.Id, "preinit");
				Mod.ExecutionMod = mod;
                foreach (Dependency dep in mod.Dependencies)
                {
                    bool present = false;
                    bool outOfDate = false;
                    foreach (Mod modDep in mods)
                    {
                        if (dep.Id == modDep.Id)
                        {
                            present = true;
                            if (modDep.Version < dep.MinimumVersion || modDep.Version > dep.MaximumVersion)
                            {
                                outOfDate = true;
                            }
						}
                    }
                    if (!present)
                    {
                        pendingErrorMsg.Add("Missing dependency " + dep.Id + " for mod " + mod.Name + ".");
                    }
                    if (outOfDate)
                    {
						BWLog.Info("out of date!");
						pendingErrorMsg.Add("Out of date dependency " + dep.Id + " for mod " + mod.Name + ". " + dep.MinimumVersion + " or greater required.");
                    }
                }
                mod.PreInit();
                Mod.ExecutionMod = null;
            }

            foreach (Mod mod in mods)
            {
                Mod.ExecutionMod = mod;
                if (mod.Id != "exdilin")
                {
					if (!(mod is LuaMod)) {
						BWLog.Info("Applying patches of " + mod.Id + " " + mod.Version);
						mod.ApplyPatches(assemblies[mod.Id]);
					}
                }
                Mod.ExecutionMod = null;
            }

            foreach (string key in BlockItemsRegistry.GetBlockEntries().Keys)
            {
                BlockEntry entry = BlockItemsRegistry.GetBlockEntries()[key];
                if (entry.blockType != null)
                {
                    Blocks.Block.blockNameTypeMap[key] = entry.blockType;
                }
            }

			AssetsManager.Plug();

            yield break;
        }
    }
}
