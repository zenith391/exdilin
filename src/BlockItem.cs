using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using Exdilin;

// Token: 0x02000049 RID: 73
public class BlockItem
{
	// Token: 0x06000254 RID: 596 RVA: 0x0000D66C File Offset: 0x0000BA6C
	public BlockItem(int id, string intenalIdentifier, string label, string title, string gafPredicateName, object[] gafDefaultArgs, string iconName, string iconBackgroundColor, RarityLevelEnum rarity)
	{
		this.Id = id;
		this.InternalIdentifier = intenalIdentifier;
		this.Label = label;
		this.Title = title;
		this.GafPredicateName = gafPredicateName;
		this.GafDefaultArgs = gafDefaultArgs;
		this.Rarity = rarity;
		this.IconName = iconName;
		this.IconBackgroundColor = iconBackgroundColor;
	}

	// Token: 0x06000255 RID: 597 RVA: 0x0000D6C4 File Offset: 0x0000BAC4
	public static void LoadBlockItemsFromResources()
	{
		string text = Resources.Load<TextAsset>("block_items_json").text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
		GafToBlockItem.Init();
		BlockItem.AllBlockItems = new List<BlockItem>();
		BlockItem._indexById = new Dictionary<int, BlockItem>();
		BlockItem._indexByInternalIdentifier = new Dictionary<string, BlockItem>();
		BlockItem._indexByGafPredicateName = new Dictionary<string, List<BlockItem>>();
		foreach (JObject jobject in arrayValue)
		{
			BlockItem blockItem = BlockItem.BlockItemFromJSON(jobject);
			if (blockItem != null)
			{
				BlockItem.AllBlockItems.Add(blockItem);
				BlockItem._indexById[blockItem.Id] = blockItem;
				BlockItem._indexByInternalIdentifier[blockItem.InternalIdentifier] = blockItem;
				if (!BlockItem._indexByGafPredicateName.ContainsKey(blockItem.GafPredicateName))
				{
					BlockItem._indexByGafPredicateName[blockItem.GafPredicateName] = new List<BlockItem>();
				}
				BlockItem._indexByGafPredicateName[blockItem.GafPredicateName].Add(blockItem);
				List<JObject> arrayValue2 = jobject["argument_patterns"].ArrayValue;
				string[] array = new string[arrayValue2.Count];
				for (int i = 0; i < arrayValue2.Count; i++)
				{
					array[i] = arrayValue2[i].StringValue;
				}
				GafToBlockItem.CreatePatternMatch(blockItem, array);
			}
		}
        foreach (BlockItemEntry blockEntry in BlockItemsRegistry.GetItemEntries())
        {
            BlockItem blockItem = blockEntry.item;
            if (blockItem != null)
            {
                BlockItem.AllBlockItems.Add(blockItem);
                BlockItem._indexById[blockItem.Id] = blockItem;
                BlockItem._indexByInternalIdentifier[blockItem.InternalIdentifier] = blockItem;
                if (!BlockItem._indexByGafPredicateName.ContainsKey(blockItem.GafPredicateName))
                {
                    BlockItem._indexByGafPredicateName[blockItem.GafPredicateName] = new List<BlockItem>();
                }
                BlockItem._indexByGafPredicateName[blockItem.GafPredicateName].Add(blockItem);
                GafToBlockItem.CreatePatternMatch(blockItem, blockEntry.argumentPatterns);
            }
        }
    }

	// Token: 0x06000256 RID: 598 RVA: 0x0000D84C File Offset: 0x0000BC4C
	public static bool Exists(int id)
	{
		return BlockItem._indexById.ContainsKey(id);
	}

	// Token: 0x06000257 RID: 599 RVA: 0x0000D859 File Offset: 0x0000BC59
	public static BlockItem FindByID(int id)
	{
		return BlockItem._indexById[id];
	}

	// Token: 0x06000258 RID: 600 RVA: 0x0000D866 File Offset: 0x0000BC66
	public static bool Exists(string internalIdentifier)
	{
		return BlockItem._indexByInternalIdentifier.ContainsKey(internalIdentifier);
	}

	// Token: 0x06000259 RID: 601 RVA: 0x0000D873 File Offset: 0x0000BC73
	public static BlockItem FindByInternalIdentifier(string internalIdentifier)
	{
		return BlockItem._indexByInternalIdentifier[internalIdentifier];
	}

	// Token: 0x0600025A RID: 602 RVA: 0x0000D880 File Offset: 0x0000BC80
	public static List<BlockItem> FindByGafPredicateName(string gafPredicateName)
	{
		if (BlockItem._indexByGafPredicateName.ContainsKey(gafPredicateName))
		{
			return BlockItem._indexByGafPredicateName[gafPredicateName];
		}
		return new List<BlockItem>(0);
	}

	// Token: 0x0600025B RID: 603 RVA: 0x0000D8A4 File Offset: 0x0000BCA4
	public static BlockItem FindByGafPredicateNameAndArguments(string gafPredicateName, object[] gafArguments)
	{
		List<BlockItem> list;
		if (!BlockItem._indexByGafPredicateName.TryGetValue(gafPredicateName, out list))
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			BlockItem blockItem = list[i];
			bool flag = false;
			for (int j = 0; j < Mathf.Min(gafArguments.Length, blockItem.GafDefaultArgs.Length); j++)
			{
				object obj = gafArguments[j];
				object obj2 = blockItem.GafDefaultArgs[j];
				if (obj != null && obj2 != null)
				{
					if (obj.GetType() != obj2.GetType())
					{
						flag = true;
						break;
					}
					if (obj is float && obj2 is float && (float)obj != (float)obj2)
					{
						flag = true;
						break;
					}
					if (obj is int && obj2 is int && (int)obj != (int)obj2)
					{
						flag = true;
						break;
					}
					if (obj is string && obj2 is string && (string)obj != (string)obj2)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return blockItem;
			}
		}
		return null;
	}

	// Token: 0x0600025C RID: 604 RVA: 0x0000D9F0 File Offset: 0x0000BDF0
	private static BlockItem BlockItemFromJSON(JObject jObj)
	{
		string text = jObj["predicate"].StringValue;
		text = SymbolCompat.RenamePredicate(text);
		Predicate predicate = PredicateRegistry.ByName(text, true);
		if (predicate == null)
		{
			BWLog.Error("Unknown predicate " + text);
			return null;
		}
		if (!(bool)jObj["production_ready"] && !Util.IncludeNonProductionReadyBlockItems())
		{
			return null;
		}
		int intValue = jObj["id"].IntValue;
		string stringValue = jObj["internal_identifier"].StringValue;
		string stringValue2 = jObj["label"].StringValue;
		string stringValue3 = jObj["title"].StringValue;
		object[] gafDefaultArgs = BlockItem.GAFArgumentsFromJSON(jObj["default_args"], predicate);
		string key = "icon_name";
		string iconName = (!jObj.ContainsKey(key)) ? string.Empty : jObj[key].StringValue;
		string iconBackgroundColor = (!jObj.ContainsKey("icon_background_color")) ? string.Empty : jObj["icon_background_color"].StringValue;
		RarityLevelEnum enumValue = new RarityLevel(jObj["rarity"].StringValue).EnumValue;
		return new BlockItem(intValue, stringValue, stringValue2, stringValue3, text, gafDefaultArgs, iconName, iconBackgroundColor, enumValue);
	}

	// Token: 0x0600025D RID: 605 RVA: 0x0000DB40 File Offset: 0x0000BF40
	private static object[] GAFArgumentsFromJSON(JObject jObj, Predicate predicate)
	{
		List<JObject> arrayValue = jObj.ArrayValue;
		Type[] argTypes = predicate.ArgTypes;
		int num = Mathf.Min(arrayValue.Count, argTypes.Length);
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = BlockItem.ArgFromJSON(argTypes[i], jObj[i]);
		}
		return array;
	}

	// Token: 0x0600025E RID: 606 RVA: 0x0000DBA0 File Offset: 0x0000BFA0
	private static object ArgFromJSON(Type type, JObject obj)
	{
		if (type == typeof(float))
		{
			return (float)obj;
		}
		if (type == typeof(int))
		{
			return (int)obj;
		}
		if (type == typeof(bool))
		{
			return (bool)obj;
		}
		if (type == typeof(string))
		{
			return (string)obj;
		}
		if (type == typeof(Vector3))
		{
			return obj.Vector3Value();
		}
		if (type == typeof(Quaternion))
		{
			return obj.QuaternionValue();
		}
		BWLog.Error(type + "Don't know how to materialize this type");
		return null;
	}

	// Token: 0x0600025F RID: 607 RVA: 0x0000DC61 File Offset: 0x0000C061
	public bool IsUnsellable()
	{
		return this.Rarity == RarityLevelEnum.ip || this.Rarity == RarityLevelEnum.unique;
	}

	// Token: 0x1700003F RID: 63
	// (get) Token: 0x06000260 RID: 608 RVA: 0x0000DC7B File Offset: 0x0000C07B
	public bool IsRare
	{
		get
		{
			return this.Rarity != RarityLevelEnum.common;
		}
	}

	// Token: 0x17000040 RID: 64
	// (get) Token: 0x06000261 RID: 609 RVA: 0x0000DC89 File Offset: 0x0000C089
	public bool HasRarityBorder
	{
		get
		{
			return this.Rarity != RarityLevelEnum.common && this.Rarity != RarityLevelEnum.uncommon;
		}
	}

	// Token: 0x06000262 RID: 610 RVA: 0x0000DCA8 File Offset: 0x0000C0A8
	public new string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < this.GafDefaultArgs.Length; i++)
		{
			text += this.GafDefaultArgs[i].ToString();
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"(",
				this.GafDefaultArgs[i].GetType(),
				")"
			});
			if (i < this.GafDefaultArgs.Length - 1)
			{
				text += ", ";
			}
		}
		return string.Format("{0} ({1}), Predicate: {2}, Args: {3}", new object[]
		{
			this.InternalIdentifier,
			this.Id,
			this.GafPredicateName,
			text
		});
	}

	// Token: 0x0400021F RID: 543
	public readonly int Id;

	// Token: 0x04000220 RID: 544
	public readonly string InternalIdentifier;

	// Token: 0x04000221 RID: 545
	public readonly string GafPredicateName;

	// Token: 0x04000222 RID: 546
	public readonly object[] GafDefaultArgs;

	// Token: 0x04000223 RID: 547
	public readonly string IconName;

	// Token: 0x04000224 RID: 548
	public readonly string IconBackgroundColor;

	// Token: 0x04000225 RID: 549
	public readonly string Label;

	// Token: 0x04000226 RID: 550
	public readonly string Title;

	// Token: 0x04000227 RID: 551
	public readonly RarityLevelEnum Rarity;

	// Token: 0x04000228 RID: 552
	public static List<BlockItem> AllBlockItems;

	// Token: 0x04000229 RID: 553
	private static Dictionary<int, BlockItem> _indexById;

	// Token: 0x0400022A RID: 554
	private static Dictionary<string, BlockItem> _indexByInternalIdentifier;

	// Token: 0x0400022B RID: 555
	private static Dictionary<string, List<BlockItem>> _indexByGafPredicateName;
}
