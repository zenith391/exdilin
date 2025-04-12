using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Exdilin.Lua {

	public class LuaBlockItem : BlockItemEntry {
		public void register() {
			BlockItemsRegistry.AddBlockItem(this);
		}
	}

	public class LuaBlock : BlockEntry {

		public void register() {
			BlockItemsRegistry.AddBlock(this);
		}

		public LuaBlockItem default_block_item(string iconName, string rarity = "common") {
			LuaBlockItem blockItem = new LuaBlockItem();
			RarityLevelEnum rarityEnum = (RarityLevelEnum) Enum.Parse(typeof(RarityLevelEnum), rarity, true);
			BlockItem item = new BlockItem(ExdilinAPI.allocate_block_item_id(), id, "", id, "Block.Create",
				new object[] { id }, iconName, "White", rarityEnum);
			blockItem.buildPaneTab = "Blocks";
			blockItem.item = item;
			blockItem.argumentPatterns = new string[] { id };
			return blockItem;
		}
	}

	public class ExdilinAPI {

		static int nextId = 7000;
		private LuaMod mod;

		public ExdilinAPI(LuaMod mod) {
			this.mod = mod;
		}

		public static int allocate_block_item_id() {
			return nextId++;
		}

		public LuaBlockItem new_block_item(Table t) {
			LuaBlockItem blockItem = new LuaBlockItem();
			int numId = (int) (t.Get("numericalId").CastToNumber() ?? allocate_block_item_id());
			string name = t.Get("name").CastToString() ?? "";
			string predicateName = t.Get("predicateName").CastToString();
			Table predicateArgsTable = t.Get("predicateArgs").Table;
			List<string> predicateArgs = new List<string>();
			foreach (DynValue dynValue in predicateArgsTable.Values) {
				predicateArgs.Add(dynValue.String);
			}
			string iconName = t.Get("iconName").CastToString();
			string backgroundColor = t.Get("iconBackgroundColor").CastToString() ?? "White";
			RarityLevelEnum rarity = (RarityLevelEnum) Enum.Parse(RarityLevelEnum.common.GetType(), t.Get("rarityLevel").CastToString() ?? "common", true);
			if (string.IsNullOrEmpty(name)) {
				throw new ScriptRuntimeException("invalid or non present name");
			}
			BlockItem item = new BlockItem(numId, name, "", name, predicateName, predicateArgs.ToArray(), iconName, backgroundColor, rarity);
			blockItem.item = item;
			blockItem.buildPaneTab = t.Get("buildPaneTab").CastToString() ?? "Blocks";
			blockItem.argumentPatterns = predicateArgs.ToArray();

			DynValue infVal = t.Get("infinite");
			blockItem.infinite = infVal.IsNil() || infVal.CastToBool();
			blockItem.count = (int) (t.Get("count").CastToNumber() ?? 1);
			return blockItem;
		}

		public PredicateEntry register_predicate(Table t) {
			string id = t.Get("Id").CastToString();
			if (string.IsNullOrEmpty(id)) {
				throw new ScriptRuntimeException("missing or empty id");
			}
			PredicateEntry entry = new PredicateEntry(id);

			DynValue sensor = t.Get("sensor");
			if (sensor.IsNotNil()) {
				entry.SetSensorDelegate(delegate (ScriptRowExecutionInfo executionInfo, object[] args)
				{
					DynValue dyn = mod.SafeCall(sensor, executionInfo, args);
					return (TileResultCode) Enum.Parse(typeof(TileResultCode), (dyn.CastToString() ?? "True").ToLower());
				});
			}

			DynValue action = t.Get("action");
			if (action.IsNotNil()) {
				entry.SetActionDelegate(delegate (ScriptRowExecutionInfo executionInfo, object[] args)
				{
					DynValue dyn = mod.SafeCall(action, executionInfo, args);
					return (TileResultCode) Enum.Parse(typeof(TileResultCode), (dyn.CastToString() ?? "True").ToLower());
				});
			}

			return entry;
		}

		public LuaBlockItem texture_item(string name, int subTab = 3, bool absolute = false) {
			LuaBlockItem blockItem = new LuaBlockItem();
			int numId = allocate_block_item_id();
			if (string.IsNullOrEmpty(name)) {
				throw new ScriptRuntimeException("name is null or empty");
			}
			string path = absolute ? name : name + " " + mod.Id;
			BlockItem item = new BlockItem(numId, name, "", name, "Block.TextureTo", new object[] { path, new Vector3() }, name, "White", RarityLevelEnum.common);
			blockItem.item = item;
			blockItem.buildPaneTab = "Textures";
			blockItem.buildPaneSubTab = subTab;
			blockItem.argumentPatterns = new string[] { path, "*" };
			return blockItem;
		}

		public LuaBlockItem music_item(string name, int subTab = 1) {
			LuaBlockItem blockItem = new LuaBlockItem();
			int numId = allocate_block_item_id();
			if (string.IsNullOrEmpty(name)) {
				throw new ScriptRuntimeException("name is null or empty");
			}
			BlockItem item = new BlockItem(numId, name, name, name, "Jukebox.SetMusic", new object[] { name }, name, "White", RarityLevelEnum.common);
			blockItem.item = item;
			blockItem.buildPaneTab = "Actions & Scripting";
			blockItem.buildPaneSubTab = subTab;
			blockItem.argumentPatterns = new string[] { name };
			return blockItem;
		}

		public LuaBlockItem sfx_item(string name, int subTab = 1) {
			LuaBlockItem blockItem = new LuaBlockItem();
			int numId = allocate_block_item_id();
			if (string.IsNullOrEmpty(name)) {
				throw new ScriptRuntimeException("name is null or empty");
			}
			BlockItem item = new BlockItem(numId, name, name, name, "Block.PlaySoundDurational", new object[] { name, "Block" }, name, "Purple", RarityLevelEnum.common);
			blockItem.item = item;
			blockItem.buildPaneTab = "Sound Effects";
			blockItem.buildPaneSubTab = subTab;
			blockItem.argumentPatterns = new string[] { name, "*" };
			return blockItem;
		}

		public LuaBlock new_block(Table t) {
			LuaBlock block = new LuaBlock();
			if (t.Get("id").IsNil()) {
				throw new ScriptRuntimeException("id required");
			}
			block.id = t.Get("id").CastToString();
			block.modelName = t.Get("modelName").CastToString() ?? block.id;
			return block;
		}

		public void load_texture(string id, string path, bool absolute = true) {
			if (Blocksworld.bw == null) {
				throw new ScriptRuntimeException("load texture must be called after Blocksworld init");
			}
			Blocksworld.bw.StartCoroutine(AssetsManager.LoadTexture(mod, id, path, absolute));
		}

		public void load_audio(string id, string path, bool absolute = true) {
			if (Blocksworld.bw == null) {
				throw new ScriptRuntimeException("load audio must be called after Blocksworld init");
			}
			Blocksworld.bw.StartCoroutine(AssetsManager.LoadAudio(mod, id, path, absolute));
		}

		public void load_block_model(string name, string path, bool absolute = true, bool convex = true) {
			ObjImporter importer = new ObjImporter();
			Mesh mesh = importer.ImportFile(mod.Directory + "/" + path);

			GameObject prefab = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Prefab Cube") as GameObject);
			prefab.GetComponent<MeshFilter>().mesh = mesh;
			UnityEngine.Object.Destroy(prefab.GetComponent<BoxCollider>());
			MeshCollider collider = prefab.AddComponent<MeshCollider>();
			collider.convex = convex;
			collider.sharedMesh = mesh;
			prefab.name = "Prefab " + name;
			GameObject shape = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Shape Cube") as GameObject);
			GameObject glue = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Glue Cube") as GameObject);
			prefab.SetActive(false);
			shape.SetActive(false);
			glue.SetActive(false);
			AssetsManager.PutAsset("Blocks/Prefab " + name, prefab);
			AssetsManager.PutAsset("Blocks/Shape " + name, shape);
			AssetsManager.PutAsset("Blocks/Glue " + name, glue);
		}

		/// <summary>
		/// Registers the texture to be used by blocks.
		/// </summary>
		/// <param name="name">Name</param>
		public void register_texture(string name, string shaderTypeName = "Normal", string mappingName = "OneSideTo1x1", bool absolute = false) {
			// TODO add entry to ResourcesLoader's textures with mapping and shader
			if (!absolute) {
				name = name + " " + mod.Id;
			}
			ShaderType shaderType = (ShaderType) Enum.Parse(typeof(ShaderType), shaderTypeName, true);
			Mapping mapping = (Mapping) Enum.Parse(typeof(Mapping), mappingName, true);
			TextureInfo textureInfo = new TextureInfo(name, shaderType, mapping);
			Materials.textureInfos[name] = textureInfo;
		}

		public void register_and_load_texture(string name, string path, string shaderTypeName = "Normal", string mappingName = "OneSideTo1x1", bool addIcon = true, bool absolute = false) {
			string namePath = absolute ? name : name + " " + mod.Id;
			string texPath = "Textures/" + shaderTypeName + "/" + mappingName + "/" + namePath;
			load_texture(texPath, path, true);
			if (addIcon) {
				load_texture("Icons/HD/" + name + "_HD", path, true);
			}
			register_texture(namePath, shaderTypeName, mappingName, true);
		}

	}
}
