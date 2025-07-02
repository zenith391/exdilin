using System;
using System.Collections.Generic;
using Exdilin;
using SimpleJSON;
using UnityEngine;

public class BlockItem
{
	public readonly int Id;

	public readonly string InternalIdentifier;

	public readonly string GafPredicateName;

	public readonly object[] GafDefaultArgs;

	public readonly string IconName;

	public readonly string IconBackgroundColor;

	public readonly string Label;

	public readonly string Title;

	public readonly RarityLevelEnum Rarity;

	public static List<BlockItem> AllBlockItems;

	private static Dictionary<int, BlockItem> _indexById;

	private static Dictionary<string, BlockItem> _indexByInternalIdentifier;

	private static Dictionary<string, List<BlockItem>> _indexByGafPredicateName;

	public bool IsRare => Rarity != RarityLevelEnum.common;

	public bool HasRarityBorder
	{
		get
		{
			if (Rarity != RarityLevelEnum.common)
			{
				return Rarity != RarityLevelEnum.uncommon;
			}
			return false;
		}
	}

	public BlockItem(int id, string intenalIdentifier, string label, string title, string gafPredicateName, object[] gafDefaultArgs, string iconName, string iconBackgroundColor, RarityLevelEnum rarity)
	{
		Id = id;
		InternalIdentifier = intenalIdentifier;
		Label = label;
		Title = title;
		GafPredicateName = gafPredicateName;
		GafDefaultArgs = gafDefaultArgs;
		Rarity = rarity;
		IconName = iconName;
		IconBackgroundColor = iconBackgroundColor;
	}

	public static void LoadBlockItemsFromResources()
	{
		string text = Resources.Load<TextAsset>("block_items_json").text;
		List<JObject> arrayValue = JSONDecoder.Decode(text).ArrayValue;
		GafToBlockItem.Init();
		AllBlockItems = new List<BlockItem>();
		_indexById = new Dictionary<int, BlockItem>();
		_indexByInternalIdentifier = new Dictionary<string, BlockItem>();
		_indexByGafPredicateName = new Dictionary<string, List<BlockItem>>();
		foreach (JObject item2 in arrayValue)
		{
			BlockItem blockItem = BlockItemFromJSON(item2);
			if (blockItem != null)
			{
				AllBlockItems.Add(blockItem);
				_indexById[blockItem.Id] = blockItem;
				_indexByInternalIdentifier[blockItem.InternalIdentifier] = blockItem;
				if (!_indexByGafPredicateName.ContainsKey(blockItem.GafPredicateName))
				{
					_indexByGafPredicateName[blockItem.GafPredicateName] = new List<BlockItem>();
				}
				_indexByGafPredicateName[blockItem.GafPredicateName].Add(blockItem);
				List<JObject> arrayValue2 = item2["argument_patterns"].ArrayValue;
				string[] array = new string[arrayValue2.Count];
				for (int i = 0; i < arrayValue2.Count; i++)
				{
					array[i] = arrayValue2[i].StringValue;
				}
				GafToBlockItem.CreatePatternMatch(blockItem, array);
			}
		}
		BlockItemEntry[] itemEntries = BlockItemsRegistry.GetItemEntries();
		foreach (BlockItemEntry blockItemEntry in itemEntries)
		{
			BlockItem item = blockItemEntry.item;
			if (item != null)
			{
				AllBlockItems.Add(item);
				_indexById[item.Id] = item;
				_indexByInternalIdentifier[item.InternalIdentifier] = item;
				if (!_indexByGafPredicateName.ContainsKey(item.GafPredicateName))
				{
					_indexByGafPredicateName[item.GafPredicateName] = new List<BlockItem>();
				}
				_indexByGafPredicateName[item.GafPredicateName].Add(item);
				GafToBlockItem.CreatePatternMatch(item, blockItemEntry.argumentPatterns);
			}
		}
	}

	public static bool Exists(int id)
	{
		return _indexById.ContainsKey(id);
	}

	public static BlockItem FindByID(int id)
	{
		return _indexById[id];
	}

	public static bool Exists(string internalIdentifier)
	{
		return _indexByInternalIdentifier.ContainsKey(internalIdentifier);
	}

	public static BlockItem FindByInternalIdentifier(string internalIdentifier)
	{
		return _indexByInternalIdentifier[internalIdentifier];
	}

	public static List<BlockItem> FindByGafPredicateName(string gafPredicateName)
	{
		if (_indexByGafPredicateName.ContainsKey(gafPredicateName))
		{
			return _indexByGafPredicateName[gafPredicateName];
		}
		return new List<BlockItem>(0);
	}

	public static BlockItem FindByGafPredicateNameAndArguments(string gafPredicateName, object[] gafArguments)
	{
		if (!_indexByGafPredicateName.TryGetValue(gafPredicateName, out var value))
		{
			return null;
		}
		for (int i = 0; i < value.Count; i++)
		{
			BlockItem blockItem = value[i];
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

	private static BlockItem BlockItemFromJSON(JObject jObj)
	{
		string stringValue = jObj["predicate"].StringValue;
		stringValue = SymbolCompat.RenamePredicate(stringValue);
		Predicate predicate = PredicateRegistry.ByName(stringValue);
		if (predicate == null)
		{
			BWLog.Error("Unknown predicate " + stringValue);
			return null;
		}
		if (!(bool)jObj["production_ready"] && !Util.IncludeNonProductionReadyBlockItems())
		{
			return null;
		}
		int intValue = jObj["id"].IntValue;
		string stringValue2 = jObj["internal_identifier"].StringValue;
		string stringValue3 = jObj["label"].StringValue;
		string stringValue4 = jObj["title"].StringValue;
		object[] gafDefaultArgs = GAFArgumentsFromJSON(jObj["default_args"], predicate);
		string key = "icon_name";
		string iconName = ((!jObj.ContainsKey(key)) ? string.Empty : jObj[key].StringValue);
		string iconBackgroundColor = ((!jObj.ContainsKey("icon_background_color")) ? string.Empty : jObj["icon_background_color"].StringValue);
		RarityLevelEnum enumValue = new RarityLevel(jObj["rarity"].StringValue).EnumValue;
		return new BlockItem(intValue, stringValue2, stringValue3, stringValue4, stringValue, gafDefaultArgs, iconName, iconBackgroundColor, enumValue);
	}

	private static object[] GAFArgumentsFromJSON(JObject jObj, Predicate predicate)
	{
		List<JObject> arrayValue = jObj.ArrayValue;
		Type[] argTypes = predicate.ArgTypes;
		int num = Mathf.Min(arrayValue.Count, argTypes.Length);
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ArgFromJSON(argTypes[i], jObj[i]);
		}
		return array;
	}

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
		BWLog.Error(type?.ToString() + "Don't know how to materialize this type");
		return null;
	}

	public bool IsUnsellable()
	{
		if (Rarity != RarityLevelEnum.ip)
		{
			return Rarity == RarityLevelEnum.unique;
		}
		return true;
	}

	public new string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < GafDefaultArgs.Length; i++)
		{
			text += GafDefaultArgs[i].ToString();
			string text2 = text;
			text = string.Concat(text2, "(", GafDefaultArgs[i].GetType(), ")");
			if (i < GafDefaultArgs.Length - 1)
			{
				text += ", ";
			}
		}
		return $"{InternalIdentifier} ({Id}), Predicate: {GafPredicateName}, Args: {text}";
	}
}
