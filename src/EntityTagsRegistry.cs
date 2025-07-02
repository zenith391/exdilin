using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class EntityTagsRegistry
{
	public static EntityTagsCollection allTags;

	public static Dictionary<string, HashSet<string>> blockNameTags = new Dictionary<string, HashSet<string>>();

	public static Dictionary<string, HashSet<string>> textureTags = new Dictionary<string, HashSet<string>>();

	public static Dictionary<string, HashSet<string>> predicateTags = new Dictionary<string, HashSet<string>>();

	private static HashSet<string> emptySet = new HashSet<string>();

	private static void ReadTags<T>(Dictionary<string, HashSet<string>> dict, T[] arr, IEnumerable<string> all) where T : EntityTags
	{
		foreach (T val in arr)
		{
			switch (val.selectMode)
			{
			case EntityTagsNameSelectMode.Regexp:
			{
				HashSet<string> value = new HashSet<string>(val.tags);
				Regex regex = new Regex(val.nameRegexp);
				foreach (string item in all)
				{
					if (regex.IsMatch(item))
					{
						dict[item] = value;
					}
				}
				break;
			}
			case EntityTagsNameSelectMode.Exact:
				dict[val.name] = new HashSet<string>(val.tags);
				break;
			}
		}
	}

	public static HashSet<string> EntityTags(Dictionary<string, HashSet<string>> dict, string entityName)
	{
		if (dict.TryGetValue(entityName, out var value))
		{
			return value;
		}
		return emptySet;
	}

	public static bool EntityHasTag(Dictionary<string, HashSet<string>> dict, string entityName, string tagName)
	{
		if (dict.TryGetValue(entityName, out var value))
		{
			return value.Contains(tagName);
		}
		return false;
	}

	public static bool EntityHasAnyTag(Dictionary<string, HashSet<string>> dict, string entityName, IEnumerable<string> tagNames)
	{
		if (dict.TryGetValue(entityName, out var value))
		{
			foreach (string tagName in tagNames)
			{
				if (value.Contains(tagName))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static bool BlockHasTag(string blockType, string tag)
	{
		return EntityHasTag(blockNameTags, blockType, tag);
	}

	public static bool PredicateHasTag(string predName, string tag)
	{
		return EntityHasTag(predicateTags, predName, tag);
	}

	public static bool TextureHasTag(string texName, string tag)
	{
		return EntityHasTag(textureTags, texName, tag);
	}

	public static bool BlockHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityHasAnyTag(blockNameTags, name, tags);
	}

	public static bool PredicateHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityHasAnyTag(predicateTags, name, tags);
	}

	public static bool TextureHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityHasAnyTag(textureTags, name, tags);
	}

	public static HashSet<string> BlockTags(string name)
	{
		return EntityTags(blockNameTags, name);
	}

	public static HashSet<string> PredicateTags(string name)
	{
		return EntityTags(predicateTags, name);
	}

	public static HashSet<string> TextureTags(string name)
	{
		return EntityTags(textureTags, name);
	}

	public static void Read()
	{
		allTags = Resources.Load("EntityTags") as EntityTagsCollection;
		ReadTags(blockNameTags, allTags.blockTags, Blocksworld.existingBlockNames);
		ReadTags(predicateTags, allTags.predicateTags, PredicateRegistry.GetAllPredicateNames());
		ReadTags(textureTags, allTags.textureTags, Materials.textureInfos.Keys);
	}
}
