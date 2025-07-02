using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Exdilin.Lua;

public class ExdilinAPI
{
	private static int nextId = 7000;

	private LuaMod mod;

	public ExdilinAPI(LuaMod mod)
	{
		this.mod = mod;
	}

	public static int allocate_block_item_id()
	{
		return nextId++;
	}

	public LuaBlockItem new_block_item(Table t)
	{
		LuaBlockItem luaBlockItem = new LuaBlockItem();
		int id = (int)(t.Get("numericalId").CastToNumber() ?? ((double)allocate_block_item_id()));
		string text = t.Get("name").CastToString() ?? "";
		string gafPredicateName = t.Get("predicateName").CastToString();
		Table table = t.Get("predicateArgs").Table;
		List<string> list = new List<string>();
		foreach (DynValue value in table.Values)
		{
			list.Add(value.String);
		}
		string iconName = t.Get("iconName").CastToString();
		string iconBackgroundColor = t.Get("iconBackgroundColor").CastToString() ?? "White";
		RarityLevelEnum rarity = (RarityLevelEnum)Enum.Parse(RarityLevelEnum.common.GetType(), t.Get("rarityLevel").CastToString() ?? "common", ignoreCase: true);
		if (string.IsNullOrEmpty(text))
		{
			throw new ScriptRuntimeException("invalid or non present name");
		}
		object[] gafDefaultArgs = list.ToArray();
		BlockItem item = new BlockItem(id, text, "", text, gafPredicateName, gafDefaultArgs, iconName, iconBackgroundColor, rarity);
		luaBlockItem.item = item;
		luaBlockItem.buildPaneTab = t.Get("buildPaneTab").CastToString() ?? "Blocks";
		luaBlockItem.argumentPatterns = list.ToArray();
		DynValue dynValue = t.Get("infinite");
		luaBlockItem.infinite = dynValue.IsNil() || dynValue.CastToBool();
		luaBlockItem.count = (int)(t.Get("count").CastToNumber() ?? 1.0);
		return luaBlockItem;
	}

	public PredicateEntry register_predicate(Table t)
	{
		string text = t.Get("Id").CastToString();
		if (string.IsNullOrEmpty(text))
		{
			throw new ScriptRuntimeException("missing or empty id");
		}
		PredicateEntry predicateEntry = new PredicateEntry(text);
		DynValue sensor = t.Get("sensor");
		if (sensor.IsNotNil())
		{
			predicateEntry.SetSensorDelegate(delegate(ScriptRowExecutionInfo executionInfo, object[] args)
			{
				DynValue dynValue = mod.SafeCall(sensor, executionInfo, args);
				return (TileResultCode)Enum.Parse(typeof(TileResultCode), (dynValue.CastToString() ?? "True").ToLower());
			});
		}
		DynValue action = t.Get("action");
		if (action.IsNotNil())
		{
			predicateEntry.SetActionDelegate(delegate(ScriptRowExecutionInfo executionInfo, object[] args)
			{
				DynValue dynValue = mod.SafeCall(action, executionInfo, args);
				return (TileResultCode)Enum.Parse(typeof(TileResultCode), (dynValue.CastToString() ?? "True").ToLower());
			});
		}
		return predicateEntry;
	}

	public LuaBlockItem texture_item(string name, int subTab = 3, bool absolute = false)
	{
		LuaBlockItem luaBlockItem = new LuaBlockItem();
		int id = allocate_block_item_id();
		if (string.IsNullOrEmpty(name))
		{
			throw new ScriptRuntimeException("name is null or empty");
		}
		string text = (absolute ? name : (name + " " + mod.Id));
		BlockItem item = new BlockItem(id, name, "", name, "Block.TextureTo", new object[2]
		{
			text,
			default(Vector3)
		}, name, "White", RarityLevelEnum.common);
		luaBlockItem.item = item;
		luaBlockItem.buildPaneTab = "Textures";
		luaBlockItem.buildPaneSubTab = subTab;
		luaBlockItem.argumentPatterns = new string[2] { text, "*" };
		return luaBlockItem;
	}

	public LuaBlockItem music_item(string name, int subTab = 1)
	{
		LuaBlockItem luaBlockItem = new LuaBlockItem();
		int id = allocate_block_item_id();
		if (string.IsNullOrEmpty(name))
		{
			throw new ScriptRuntimeException("name is null or empty");
		}
		BlockItem item = new BlockItem(id, name, name, name, "Jukebox.SetMusic", new object[1] { name }, name, "White", RarityLevelEnum.common);
		luaBlockItem.item = item;
		luaBlockItem.buildPaneTab = "Actions & Scripting";
		luaBlockItem.buildPaneSubTab = subTab;
		luaBlockItem.argumentPatterns = new string[1] { name };
		return luaBlockItem;
	}

	public LuaBlockItem sfx_item(string name, int subTab = 1)
	{
		LuaBlockItem luaBlockItem = new LuaBlockItem();
		int id = allocate_block_item_id();
		if (string.IsNullOrEmpty(name))
		{
			throw new ScriptRuntimeException("name is null or empty");
		}
		BlockItem item = new BlockItem(id, name, name, name, "Block.PlaySoundDurational", new object[2] { name, "Block" }, name, "Purple", RarityLevelEnum.common);
		luaBlockItem.item = item;
		luaBlockItem.buildPaneTab = "Sound Effects";
		luaBlockItem.buildPaneSubTab = subTab;
		luaBlockItem.argumentPatterns = new string[2] { name, "*" };
		return luaBlockItem;
	}

	public LuaBlock new_block(Table t)
	{
		LuaBlock luaBlock = new LuaBlock();
		if (t.Get("id").IsNil())
		{
			throw new ScriptRuntimeException("id required");
		}
		luaBlock.id = t.Get("id").CastToString();
		luaBlock.modelName = t.Get("modelName").CastToString() ?? luaBlock.id;
		return luaBlock;
	}

	public void load_texture(string id, string path, bool absolute = true)
	{
		if (Blocksworld.bw == null)
		{
			throw new ScriptRuntimeException("load texture must be called after Blocksworld init");
		}
		Blocksworld.bw.StartCoroutine(AssetsManager.LoadTexture(mod, id, path, absolute));
	}

	public void load_audio(string id, string path, bool absolute = true)
	{
		if (Blocksworld.bw == null)
		{
			throw new ScriptRuntimeException("load audio must be called after Blocksworld init");
		}
		Blocksworld.bw.StartCoroutine(AssetsManager.LoadAudio(mod, id, path, absolute));
	}

	public void load_block_model(string name, string path, bool absolute = true, bool convex = true)
	{
		ObjImporter objImporter = new ObjImporter();
		Mesh mesh = objImporter.ImportFile(mod.Directory + "/" + path);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Prefab Cube") as GameObject);
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.convex = convex;
		meshCollider.sharedMesh = mesh;
		gameObject.name = "Prefab " + name;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Shape Cube") as GameObject);
		GameObject gameObject3 = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Glue Cube") as GameObject);
		gameObject.SetActive(value: false);
		gameObject2.SetActive(value: false);
		gameObject3.SetActive(value: false);
		AssetsManager.PutAsset("Blocks/Prefab " + name, gameObject);
		AssetsManager.PutAsset("Blocks/Shape " + name, gameObject2);
		AssetsManager.PutAsset("Blocks/Glue " + name, gameObject3);
	}

	public void register_texture(string name, string shaderTypeName = "Normal", string mappingName = "OneSideTo1x1", bool absolute = false)
	{
		if (!absolute)
		{
			name = name + " " + mod.Id;
		}
		ShaderType shader = (ShaderType)Enum.Parse(typeof(ShaderType), shaderTypeName, ignoreCase: true);
		Mapping mapping = (Mapping)Enum.Parse(typeof(Mapping), mappingName, ignoreCase: true);
		TextureInfo value = new TextureInfo(name, shader, mapping);
		Materials.textureInfos[name] = value;
	}

	public void register_and_load_texture(string name, string path, string shaderTypeName = "Normal", string mappingName = "OneSideTo1x1", bool addIcon = true, bool absolute = false)
	{
		string text = (absolute ? name : (name + " " + mod.Id));
		string id = "Textures/" + shaderTypeName + "/" + mappingName + "/" + text;
		load_texture(id, path);
		if (addIcon)
		{
			load_texture("Icons/HD/" + name + "_HD", path);
		}
		register_texture(text, shaderTypeName, mappingName, absolute: true);
	}
}
