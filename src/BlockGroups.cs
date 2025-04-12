using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000042 RID: 66
public class BlockGroups
{
	// Token: 0x0600022A RID: 554 RVA: 0x0000C83E File Offset: 0x0000AC3E
	public static void Clear()
	{
		BlockGroups.groups.Clear();
		BlockGroups.typeBlockGroups.Clear();
		BlockGroup.usedGroupIds.Clear();
	}

	// Token: 0x0600022B RID: 555 RVA: 0x0000C85E File Offset: 0x0000AC5E
	public static BlockGroupTemplate GetBlockGroupTemplate(string name)
	{
		return BlockGroups.blockGroupTemplates[name];
	}

	// Token: 0x0600022C RID: 556 RVA: 0x0000C86C File Offset: 0x0000AC6C
	public static void Init()
	{
		UnityEngine.Object[] array = Resources.LoadAll("Block Groups");
		foreach (UnityEngine.Object @object in array)
		{
			TextAsset textAsset = @object as TextAsset;
			if (textAsset != null)
			{
				BlockGroups.ParseBlockGroupTemplate(textAsset.name, textAsset.text);
			}
		}
	}

	// Token: 0x0600022D RID: 557 RVA: 0x0000C8C8 File Offset: 0x0000ACC8
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

	// Token: 0x0600022E RID: 558 RVA: 0x0000C910 File Offset: 0x0000AD10
	public static void SetGroupIsMain(Tile tile, bool isMain)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			if (gaf.Args.Length < 3)
			{
				tile.gaf = new GAF(Block.predicateGroup, new object[]
				{
					gaf.Args[0],
					gaf.Args[1],
					(!isMain) ? 0 : 1
				});
			}
			else
			{
				gaf.Args[2] = ((!isMain) ? 0 : 1);
			}
		}
		else
		{
			BWLog.Error("Tile was not a group tile: " + gaf);
		}
	}

	// Token: 0x0600022F RID: 559 RVA: 0x0000C9B8 File Offset: 0x0000ADB8
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

	// Token: 0x06000230 RID: 560 RVA: 0x0000C9FC File Offset: 0x0000ADFC
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

	// Token: 0x06000231 RID: 561 RVA: 0x0000CA48 File Offset: 0x0000AE48
	public static bool IsMainGroupBlock(Tile tile)
	{
		GAF gaf = tile.gaf;
		if (gaf.Predicate == Block.predicateGroup)
		{
			return Util.GetIntBooleanArg(gaf.Args, 2, false);
		}
		BWLog.Error("Tile was not a group tile: " + gaf);
		return false;
	}

	// Token: 0x06000232 RID: 562 RVA: 0x0000CA8C File Offset: 0x0000AE8C
	public static Tile FindGroupTile(List<List<Tile>> tiles, string groupType)
	{
		foreach (Tile tile in tiles[0])
		{
			GAF gaf = tile.gaf;
			if (gaf.Predicate == Block.predicateGroup && groupType == BlockGroups.GroupType(tile))
			{
				return tile;
			}
		}
		return null;
	}

	// Token: 0x06000233 RID: 563 RVA: 0x0000CB14 File Offset: 0x0000AF14
	public static Tile FindGroupTile(List<List<Tile>> tiles, int groupId, string groupType)
	{
		foreach (Tile tile in tiles[0])
		{
			GAF gaf = tile.gaf;
			if (gaf.Predicate == Block.predicateGroup && groupId == BlockGroups.GroupId(tile) && groupType == BlockGroups.GroupType(tile))
			{
				return tile;
			}
		}
		return null;
	}

	// Token: 0x06000234 RID: 564 RVA: 0x0000CBA8 File Offset: 0x0000AFA8
	public static void SetGroup(List<List<Tile>> tiles, int groupId, string groupType, bool isMain)
	{
		Tile tile = BlockGroups.FindGroupTile(tiles, groupType);
		if (tile == null)
		{
			tiles[0].Add(new Tile(Block.predicateGroup, new object[]
			{
				groupId,
				groupType,
				(!isMain) ? 0 : 1
			}));
		}
		else
		{
			if (BlockGroups.IsMainGroupBlock(tile) != isMain)
			{
				BWLog.Info(string.Concat(new object[]
				{
					"Inconsistent main group ",
					groupId,
					" ",
					groupType,
					" ",
					isMain,
					" ",
					BlockGroups.IsMainGroupBlock(tile)
				}));
			}
			BlockGroups.SetGroupId(tile, groupId);
			BlockGroups.SetGroupIsMain(tile, isMain);
		}
	}

	// Token: 0x06000235 RID: 565 RVA: 0x0000CC74 File Offset: 0x0000B074
	private static void GatherGroups(Block block, Dictionary<int, List<Block>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (Tile tile in block.tiles[0])
		{
			GAF gaf = tile.gaf;
			if (gaf.Predicate == Block.predicateGroup)
			{
				int num = BlockGroups.GroupId(tile);
				string text = BlockGroups.GroupType(tile);
				bool flag = BlockGroups.IsMainGroupBlock(tile);
				List<Block> list;
				if (!groups.TryGetValue(num, out list))
				{
					list = new List<Block>();
					groups[num] = list;
				}
				if (flag)
				{
					list.Insert(0, block);
				}
				else
				{
					list.Add(block);
				}
				string text2;
				if (groupTypes.TryGetValue(num, out text2) && text2 != text)
				{
					BWLog.Error(string.Concat(new object[]
					{
						"Inconsistent group type ",
						num,
						" ",
						text,
						" != ",
						text2
					}));
				}
				groupTypes[num] = text;
			}
		}
	}

	// Token: 0x06000236 RID: 566 RVA: 0x0000CD98 File Offset: 0x0000B198
	private static void ParseBlockGroupTemplate(string name, string text)
	{
		JObject jobject = JSONDecoder.Decode(text);
		Dictionary<string, JObject> objectValue = jobject.ObjectValue;
		BlockGroupTemplate blockGroupTemplate = new BlockGroupTemplate();
		BlockGroups.blockGroupTemplates[name] = blockGroupTemplate;
		if (objectValue.ContainsKey("model"))
		{
			blockGroupTemplate.blockInfos = ModelUtils.ParseModelJSON(objectValue["model"]);
		}
		if (objectValue.ContainsKey("type"))
		{
			blockGroupTemplate.type = objectValue["type"].StringValue;
		}
	}

	// Token: 0x06000237 RID: 567 RVA: 0x0000CE14 File Offset: 0x0000B214
	public static void AddGroup(List<Block> blocks, string type)
	{
		BlockGroup blockGroup = BlockGroup.Create(blocks, type);
		if (blockGroup != null)
		{
			BlockGroups.AddGroup(blockGroup);
		}
	}

	// Token: 0x06000238 RID: 568 RVA: 0x0000CE38 File Offset: 0x0000B238
	public static void AddGroup(BlockGroup group)
	{
		string groupType = group.GetGroupType();
		HashSet<BlockGroup> hashSet;
		if (!BlockGroups.typeBlockGroups.TryGetValue(groupType, out hashSet))
		{
			hashSet = new HashSet<BlockGroup>();
			BlockGroups.typeBlockGroups[groupType] = hashSet;
		}
		hashSet.Add(group);
		BlockGroups.groups.Add(group);
		group.Initialize();
	}

	// Token: 0x06000239 RID: 569 RVA: 0x0000CE8C File Offset: 0x0000B28C
	public static void RemoveGroup(BlockGroup group)
	{
		string groupType = group.GetGroupType();
		HashSet<BlockGroup> hashSet;
		if (BlockGroups.typeBlockGroups.TryGetValue(groupType, out hashSet))
		{
			hashSet.Remove(group);
			if (hashSet.Count == 0)
			{
				BlockGroups.typeBlockGroups.Remove(groupType);
			}
		}
		BlockGroups.groups.Remove(group);
	}

	// Token: 0x0600023A RID: 570 RVA: 0x0000CEE0 File Offset: 0x0000B2E0
	public static void GatherBlockGroupTiles(List<List<Tile>> info, Dictionary<int, List<List<List<Tile>>>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (Tile tile in info[0])
		{
			GAF gaf = tile.gaf;
			if (gaf.Predicate == Block.predicateGroup)
			{
				int num = BlockGroups.GroupId(tile);
				string text = BlockGroups.GroupType(tile);
				bool flag = BlockGroups.IsMainGroupBlock(tile);
				List<List<List<Tile>>> list;
				if (!groups.TryGetValue(num, out list))
				{
					list = new List<List<List<Tile>>>();
					groups[num] = list;
				}
				if (flag)
				{
					list.Insert(0, info);
				}
				else
				{
					list.Add(info);
				}
				string text2;
				if (groupTypes.TryGetValue(num, out text2) && text2 != text)
				{
					BWLog.Error(string.Concat(new object[]
					{
						"Inconsistent group type ",
						num,
						" ",
						text,
						" != ",
						text2
					}));
				}
				groupTypes[num] = text;
			}
		}
	}

	// Token: 0x0600023B RID: 571 RVA: 0x0000D000 File Offset: 0x0000B400
	public static void GatherBlockGroupTiles(List<List<List<Tile>>> blockInfos, Dictionary<int, List<List<List<Tile>>>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (List<List<Tile>> info in blockInfos)
		{
			BlockGroups.GatherBlockGroupTiles(info, groups, groupTypes);
		}
	}

	// Token: 0x0600023C RID: 572 RVA: 0x0000D058 File Offset: 0x0000B458
	public static void GetHomogenousGroupBlockCounts(List<List<List<Tile>>> blockInfos, Dictionary<string, int> counts)
	{
		Dictionary<int, List<List<List<Tile>>>> dictionary = new Dictionary<int, List<List<List<Tile>>>>();
		Dictionary<int, string> groupTypes = new Dictionary<int, string>();
		BlockGroups.GatherBlockGroupTiles(blockInfos, dictionary, groupTypes);
		foreach (KeyValuePair<int, List<List<List<Tile>>>> keyValuePair in dictionary)
		{
			Tile tile = keyValuePair.Value[0][0].Find((Tile t) => t.gaf.Predicate == Block.predicateCreate);
			string arg = (string)tile.gaf.Args[0];
			string key = arg + " x" + keyValuePair.Value.Count;
			int num;
			if (!counts.TryGetValue(key, out num))
			{
				num = 1;
			}
			else
			{
				num++;
			}
			counts[key] = num;
		}
	}

	// Token: 0x0600023D RID: 573 RVA: 0x0000D150 File Offset: 0x0000B550
	public static void GatherBlockGroups(List<Block> blocks)
	{
		Dictionary<int, List<Block>> dictionary = new Dictionary<int, List<Block>>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		foreach (Block block in blocks)
		{
			BlockGroups.GatherGroups(block, dictionary, dictionary2);
		}
		foreach (int num in new List<int>(dictionary.Keys))
		{
			if (BlockGroup.usedGroupIds.Contains(num))
			{
				List<Block> value = dictionary[num];
				string value2 = dictionary2[num];
				int nextGroupId = BlockGroup.GetNextGroupId();
				dictionary[nextGroupId] = value;
				dictionary2[nextGroupId] = value2;
				dictionary.Remove(num);
				dictionary2.Remove(num);
			}
		}
		BlockGroups.Read(dictionary, dictionary2);
	}

	// Token: 0x0600023E RID: 574 RVA: 0x0000D258 File Offset: 0x0000B658
	public static void Read(Dictionary<int, List<Block>> groups, Dictionary<int, string> groupTypes)
	{
		foreach (KeyValuePair<int, List<Block>> keyValuePair in groups)
		{
			BlockGroup group = BlockGroup.Read(keyValuePair.Value, groupTypes[keyValuePair.Key]);
			BlockGroups.AddGroup(group);
		}
	}

	// Token: 0x0600023F RID: 575 RVA: 0x0000D2C8 File Offset: 0x0000B6C8
	public static void InitializeGroups()
	{
		foreach (BlockGroup blockGroup in BlockGroups.groups)
		{
			blockGroup.Initialize();
		}
	}

	// Token: 0x06000240 RID: 576 RVA: 0x0000D324 File Offset: 0x0000B724
	public static bool IsBlockGroupCreateGAF(GAF gaf)
	{
		if (gaf.Predicate == Block.predicateCreate)
		{
			string key = (string)gaf.Args[0];
			return BlockGroups.blockGroupTemplates.ContainsKey(key);
		}
		return false;
	}

	// Token: 0x04000209 RID: 521
	protected const string TYPE_KEY = "type";

	// Token: 0x0400020A RID: 522
	protected const string MODEL_KEY = "model";

	// Token: 0x0400020B RID: 523
	private static HashSet<BlockGroup> groups = new HashSet<BlockGroup>();

	// Token: 0x0400020C RID: 524
	private static Dictionary<string, HashSet<BlockGroup>> typeBlockGroups = new Dictionary<string, HashSet<BlockGroup>>();

	// Token: 0x0400020D RID: 525
	private static Dictionary<string, BlockGroupTemplate> blockGroupTemplates = new Dictionary<string, BlockGroupTemplate>();
}
