using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Token: 0x020001C6 RID: 454
public class EntityTagsRegistry
{
	// Token: 0x06001822 RID: 6178 RVA: 0x000AAD30 File Offset: 0x000A9130
	private static void ReadTags<T>(Dictionary<string, HashSet<string>> dict, T[] arr, IEnumerable<string> all) where T : EntityTags
	{
		foreach (T t in arr)
		{
			EntityTagsNameSelectMode selectMode = t.selectMode;
			if (selectMode != EntityTagsNameSelectMode.Exact)
			{
				if (selectMode == EntityTagsNameSelectMode.Regexp)
				{
					HashSet<string> value = new HashSet<string>(t.tags);
					Regex regex = new Regex(t.nameRegexp);
					foreach (string text in all)
					{
						if (regex.IsMatch(text))
						{
							dict[text] = value;
						}
					}
				}
			}
			else
			{
				dict[t.name] = new HashSet<string>(t.tags);
			}
		}
	}

	// Token: 0x06001823 RID: 6179 RVA: 0x000AAE28 File Offset: 0x000A9228
	public static HashSet<string> EntityTags(Dictionary<string, HashSet<string>> dict, string entityName)
	{
		HashSet<string> result;
		if (dict.TryGetValue(entityName, out result))
		{
			return result;
		}
		return EntityTagsRegistry.emptySet;
	}

	// Token: 0x06001824 RID: 6180 RVA: 0x000AAE4C File Offset: 0x000A924C
	public static bool EntityHasTag(Dictionary<string, HashSet<string>> dict, string entityName, string tagName)
	{
		HashSet<string> hashSet;
		return dict.TryGetValue(entityName, out hashSet) && hashSet.Contains(tagName);
	}

	// Token: 0x06001825 RID: 6181 RVA: 0x000AAE70 File Offset: 0x000A9270
	public static bool EntityHasAnyTag(Dictionary<string, HashSet<string>> dict, string entityName, IEnumerable<string> tagNames)
	{
		HashSet<string> hashSet;
		if (dict.TryGetValue(entityName, out hashSet))
		{
			foreach (string item in tagNames)
			{
				if (hashSet.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	// Token: 0x06001826 RID: 6182 RVA: 0x000AAEE4 File Offset: 0x000A92E4
	public static bool BlockHasTag(string blockType, string tag)
	{
		return EntityTagsRegistry.EntityHasTag(EntityTagsRegistry.blockNameTags, blockType, tag);
	}

	// Token: 0x06001827 RID: 6183 RVA: 0x000AAEF2 File Offset: 0x000A92F2
	public static bool PredicateHasTag(string predName, string tag)
	{
		return EntityTagsRegistry.EntityHasTag(EntityTagsRegistry.predicateTags, predName, tag);
	}

	// Token: 0x06001828 RID: 6184 RVA: 0x000AAF00 File Offset: 0x000A9300
	public static bool TextureHasTag(string texName, string tag)
	{
		return EntityTagsRegistry.EntityHasTag(EntityTagsRegistry.textureTags, texName, tag);
	}

	// Token: 0x06001829 RID: 6185 RVA: 0x000AAF0E File Offset: 0x000A930E
	public static bool BlockHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityTagsRegistry.EntityHasAnyTag(EntityTagsRegistry.blockNameTags, name, tags);
	}

	// Token: 0x0600182A RID: 6186 RVA: 0x000AAF1C File Offset: 0x000A931C
	public static bool PredicateHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityTagsRegistry.EntityHasAnyTag(EntityTagsRegistry.predicateTags, name, tags);
	}

	// Token: 0x0600182B RID: 6187 RVA: 0x000AAF2A File Offset: 0x000A932A
	public static bool TextureHasAnyTag(string name, IEnumerable<string> tags)
	{
		return EntityTagsRegistry.EntityHasAnyTag(EntityTagsRegistry.textureTags, name, tags);
	}

	// Token: 0x0600182C RID: 6188 RVA: 0x000AAF38 File Offset: 0x000A9338
	public static HashSet<string> BlockTags(string name)
	{
		return EntityTagsRegistry.EntityTags(EntityTagsRegistry.blockNameTags, name);
	}

	// Token: 0x0600182D RID: 6189 RVA: 0x000AAF45 File Offset: 0x000A9345
	public static HashSet<string> PredicateTags(string name)
	{
		return EntityTagsRegistry.EntityTags(EntityTagsRegistry.predicateTags, name);
	}

	// Token: 0x0600182E RID: 6190 RVA: 0x000AAF52 File Offset: 0x000A9352
	public static HashSet<string> TextureTags(string name)
	{
		return EntityTagsRegistry.EntityTags(EntityTagsRegistry.textureTags, name);
	}

	// Token: 0x0600182F RID: 6191 RVA: 0x000AAF60 File Offset: 0x000A9360
	public static void Read()
	{
		EntityTagsRegistry.allTags = (Resources.Load("EntityTags") as EntityTagsCollection);
		EntityTagsRegistry.ReadTags<BlockTags>(EntityTagsRegistry.blockNameTags, EntityTagsRegistry.allTags.blockTags, Blocksworld.existingBlockNames);
		EntityTagsRegistry.ReadTags<PredicateTags>(EntityTagsRegistry.predicateTags, EntityTagsRegistry.allTags.predicateTags, PredicateRegistry.GetAllPredicateNames());
		EntityTagsRegistry.ReadTags<TextureTags>(EntityTagsRegistry.textureTags, EntityTagsRegistry.allTags.textureTags, Materials.textureInfos.Keys);
	}

	// Token: 0x0400130E RID: 4878
	public static EntityTagsCollection allTags;

	// Token: 0x0400130F RID: 4879
	public static Dictionary<string, HashSet<string>> blockNameTags = new Dictionary<string, HashSet<string>>();

	// Token: 0x04001310 RID: 4880
	public static Dictionary<string, HashSet<string>> textureTags = new Dictionary<string, HashSet<string>>();

	// Token: 0x04001311 RID: 4881
	public static Dictionary<string, HashSet<string>> predicateTags = new Dictionary<string, HashSet<string>>();

	// Token: 0x04001312 RID: 4882
	private static HashSet<string> emptySet = new HashSet<string>();
}
