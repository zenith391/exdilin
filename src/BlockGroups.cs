using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class BlockGroups
{
	protected const string TYPE_KEY = "type";

	protected const string MODEL_KEY = "model";

	private static HashSet<BlockGroup> groups = new HashSet<BlockGroup>();

	private static Dictionary<string, HashSet<BlockGroup>> typeBlockGroups = new Dictionary<string, HashSet<BlockGroup>>();

	private static Dictionary<string, BlockGroupTemplate> blockGroupTemplates = new Dictionary<string, BlockGroupTemplate>();

	public static void Clear()
	{
		groups.Clear();
		typeBlockGroups.Clear();
		BlockGroup.usedGroupIds.Clear();
	}

	public static BlockGroupTemplate GetBlockGroupTemplate(string name)
	{
		return blockGroupTemplates[name];
	}

	public static void Init()
	{
		Object[] array = Resources.LoadAll("Block Groups");
		Object[] array2 = array;
		foreach (Object obj in array2)
		{
			TextAsset textAsset = obj as TextAsset;
			if (textAsset != null)
			{
				ParseBlockGroupTemplate(textAsset.name, textAsset.text);
			}
		}
	}

	public static void SetGroupId(Tile tile, int id)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			gaf.Args[0] = id;
		}
		else
		{
			BWLog.Error("Tile was not a group tile: " + gaf);
		}
	}

	public static void SetGroupIsMain(Tile tile, bool isMain)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			if (gaf.Args.Length < 3)
			{
				tile.gaf = new GAF(Block.predicateGroup, gaf.Args[0], gaf.Args[1], isMain ? 1 : 0);
			}
			else
			{
				gaf.Args[2] = (isMain ? 1 : 0);
			}
		}
		else
		{
			BWLog.Error("Tile was not a group tile: " + gaf);
		}
	}

	public static int GroupId(Tile tile)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			return Util.GetIntArg(gaf.Args, 0, -1);
		}
		BWLog.Error("Tile was not a group tile: " + gaf);
		return -1;
	}

	public static string GroupType(Tile tile)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			return Util.GetStringArg(gaf.Args, 1, string.Empty);
		}
		BWLog.Error("Tile was not a group tile: " + gaf);
		return string.Empty;
	}

	public static bool IsMainGroupBlock(Tile tile)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			return Util.GetIntBooleanArg(gaf.Args, 2, defaultValue: false);
		}
		BWLog.Error("Tile was not a group tile: " + gaf);
		return false;
	}

	public static Tile FindGroupTile(List<List<Tile>> tiles, string groupType)
	{
		foreach (Tile item in tiles[0])
		{
			GAF gaf = item.gaf;
			if (gaf.Predicate == Block.predicateGroup && groupType == GroupType(item))
			{
				return item;
			}
		}
		return null;
	}

	public static Tile FindGroupTile(List<List<Tile>> tiles, int groupId, string groupType)
	{
		foreach (Tile item in tiles[0])
		{
			GAF gaf = item.gaf;
			if (gaf.Predicate == Block.predicateGroup && groupId == GroupId(item) && groupType == GroupType(item))
			{
				return item;
			}
		}
		return null;
	}

	public static void SetGroup(List<List<Tile>> tiles, int groupId, string groupType, bool isMain)
	{
		Tile tile = FindGroupTile(tiles, groupType);
		if (tile == null)
		{
			tiles[0].Add(new Tile(Block.predicateGroup, groupId, groupType, isMain ? 1 : 0));
			return;
		}
		if (IsMainGroupBlock(tile) != isMain)
		{
			BWLog.Info("Inconsistent main group " + groupId + " " + groupType + " " + isMain + " " + IsMainGroupBlock(tile));
		}
		SetGroupId(tile, groupId);
		SetGroupIsMain(tile, isMain);
	}

	private static void GatherGroups(Block block, Dictionary<int, List<Block>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (Tile item in block.tiles[0])
		{
			GAF gaf = item.gaf;
			if (gaf.Predicate == Block.predicateGroup)
			{
				int num = GroupId(item);
				string text = GroupType(item);
				bool flag = IsMainGroupBlock(item);
				if (!groups.TryGetValue(num, out var value))
				{
					value = (groups[num] = new List<Block>());
				}
				if (flag)
				{
					value.Insert(0, block);
				}
				else
				{
					value.Add(block);
				}
				if (groupTypes.TryGetValue(num, out var value2) && value2 != text)
				{
					BWLog.Error("Inconsistent group type " + num + " " + text + " != " + value2);
				}
				groupTypes[num] = text;
			}
		}
	}

	private static void ParseBlockGroupTemplate(string name, string text)
	{
		JObject jObject = JSONDecoder.Decode(text);
		Dictionary<string, JObject> objectValue = jObject.ObjectValue;
		BlockGroupTemplate blockGroupTemplate = new BlockGroupTemplate();
		blockGroupTemplates[name] = blockGroupTemplate;
		if (objectValue.ContainsKey("model"))
		{
			blockGroupTemplate.blockInfos = ModelUtils.ParseModelJSON(objectValue["model"]);
		}
		if (objectValue.ContainsKey("type"))
		{
			blockGroupTemplate.type = objectValue["type"].StringValue;
		}
	}

	public static void AddGroup(List<Block> blocks, string type)
	{
		BlockGroup blockGroup = BlockGroup.Create(blocks, type);
		if (blockGroup != null)
		{
			AddGroup(blockGroup);
		}
	}

	public static void AddGroup(BlockGroup group)
	{
		string groupType = group.GetGroupType();
		if (!typeBlockGroups.TryGetValue(groupType, out var value))
		{
			value = new HashSet<BlockGroup>();
			typeBlockGroups[groupType] = value;
		}
		value.Add(group);
		groups.Add(group);
		group.Initialize();
	}

	public static void RemoveGroup(BlockGroup group)
	{
		string groupType = group.GetGroupType();
		if (typeBlockGroups.TryGetValue(groupType, out var value))
		{
			value.Remove(group);
			if (value.Count == 0)
			{
				typeBlockGroups.Remove(groupType);
			}
		}
		groups.Remove(group);
	}

	public static void GatherBlockGroupTiles(List<List<Tile>> info, Dictionary<int, List<List<List<Tile>>>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (Tile item in info[0])
		{
			GAF gaf = item.gaf;
			if (gaf.Predicate == Block.predicateGroup)
			{
				int num = GroupId(item);
				string text = GroupType(item);
				bool flag = IsMainGroupBlock(item);
				if (!groups.TryGetValue(num, out var value))
				{
					value = (groups[num] = new List<List<List<Tile>>>());
				}
				if (flag)
				{
					value.Insert(0, info);
				}
				else
				{
					value.Add(info);
				}
				if (groupTypes.TryGetValue(num, out var value2) && value2 != text)
				{
					BWLog.Error("Inconsistent group type " + num + " " + text + " != " + value2);
				}
				groupTypes[num] = text;
			}
		}
	}

	public static void GatherBlockGroupTiles(List<List<List<Tile>>> blockInfos, Dictionary<int, List<List<List<Tile>>>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (List<List<Tile>> blockInfo in blockInfos)
		{
			GatherBlockGroupTiles(blockInfo, groups, groupTypes);
		}
	}

	public static void GetHomogenousGroupBlockCounts(List<List<List<Tile>>> blockInfos, Dictionary<string, int> counts)
	{
		Dictionary<int, List<List<List<Tile>>>> dictionary = new Dictionary<int, List<List<List<Tile>>>>();
		Dictionary<int, string> groupTypes = new Dictionary<int, string>();
		GatherBlockGroupTiles(blockInfos, dictionary, groupTypes);
		foreach (KeyValuePair<int, List<List<List<Tile>>>> item in dictionary)
		{
			Tile tile = item.Value[0][0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
			string text = (string)tile.gaf.Args[0];
			string key = text + " x" + item.Value.Count;
			int value = (counts[key] = ((!counts.TryGetValue(key, out value)) ? 1 : (value + 1)));
		}
	}

	public static void GatherBlockGroups(List<Block> blocks)
	{
		Dictionary<int, List<Block>> dictionary = new Dictionary<int, List<Block>>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		foreach (Block block in blocks)
		{
			GatherGroups(block, dictionary, dictionary2);
		}
		foreach (int item in new List<int>(dictionary.Keys))
		{
			if (BlockGroup.usedGroupIds.Contains(item))
			{
				List<Block> value = dictionary[item];
				string value2 = dictionary2[item];
				int nextGroupId = BlockGroup.GetNextGroupId();
				dictionary[nextGroupId] = value;
				dictionary2[nextGroupId] = value2;
				dictionary.Remove(item);
				dictionary2.Remove(item);
			}
		}
		Read(dictionary, dictionary2);
	}

	public static void Read(Dictionary<int, List<Block>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (KeyValuePair<int, List<Block>> group in groups)
		{
			BlockGroup blockGroup = BlockGroup.Read(group.Value, groupTypes[group.Key]);
			AddGroup(blockGroup);
		}
	}

	public static void InitializeGroups()
	{
		foreach (BlockGroup group in groups)
		{
			group.Initialize();
		}
	}

	public static bool IsBlockGroupCreateGAF(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			string key = (string)gaf.Args[0];
			return blockGroupTemplates.ContainsKey(key);
		}
		return false;
	}
}
