using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blocks;
using SimpleJSON;

// Token: 0x020001D3 RID: 467
public class MultiCounterModelFeatureType : ModelFeatureType
{
	// Token: 0x06001869 RID: 6249 RVA: 0x000ABD38 File Offset: 0x000AA138
	private static HashSet<Predicate> GetIgnorePredicates()
	{
		if (MultiCounterModelFeatureType.ignorePredicates == null)
		{
			MultiCounterModelFeatureType.ignorePredicates = new HashSet<Predicate>
			{
				Block.predicateStop,
				Block.predicateCreate,
				Block.predicateMoveTo,
				Block.predicateRotateTo,
				Block.predicateScaleTo,
				Block.predicatePaintTo,
				Block.predicateTextureTo,
				Block.predicateThen
			};
		}
		return MultiCounterModelFeatureType.ignorePredicates;
	}

	// Token: 0x0600186A RID: 6250 RVA: 0x000ABDC0 File Offset: 0x000AA1C0
	private static string PredicateToString(Predicate pred)
	{
		return pred.Name;
	}

	// Token: 0x0600186B RID: 6251 RVA: 0x000ABDC8 File Offset: 0x000AA1C8
	public static string GetBlockCountFeatureName(string blockName)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(blockName, "Block", null);
	}

	// Token: 0x0600186C RID: 6252 RVA: 0x000ABDD6 File Offset: 0x000AA1D6
	public static string GetBlockTagCountFeatureName(string blockTag)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(blockTag, "BlockTag", null);
	}

	// Token: 0x0600186D RID: 6253 RVA: 0x000ABDE4 File Offset: 0x000AA1E4
	public static string GetTextureCountFeatureName(string textureName)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(textureName, "PermanentTexture", null);
	}

	// Token: 0x0600186E RID: 6254 RVA: 0x000ABDF2 File Offset: 0x000AA1F2
	public static string GetTextureTagCountFeatureName(string textureTag)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(textureTag, "PermanentTextureTag", null);
	}

	// Token: 0x0600186F RID: 6255 RVA: 0x000ABE00 File Offset: 0x000AA200
	public static string GetPredicateCountFeatureName(string predName)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(predName, "Predicate", null);
	}

	// Token: 0x06001870 RID: 6256 RVA: 0x000ABE0E File Offset: 0x000AA20E
	public static string GetPredicateTagCountFeatureName(string predTag)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(predTag, "PredicateTag", null);
	}

	// Token: 0x06001871 RID: 6257 RVA: 0x000ABE1C File Offset: 0x000AA21C
	public static string GetShapeCategoryCountFeatureName(string categoryName)
	{
		return MultiCounterModelFeatureType.GetCountKey<string>(categoryName, "ShapeCategory", null);
	}

	// Token: 0x06001872 RID: 6258 RVA: 0x000ABE2C File Offset: 0x000AA22C
	public override void SetContextValues(ModelCategorizerContext ctx)
	{
		this.DoWithAllValues(delegate(string name, int value)
		{
			ctx.SetInt(name, value);
		});
	}

	// Token: 0x06001873 RID: 6259 RVA: 0x000ABE58 File Offset: 0x000AA258
	public override void ToJSON(JSONStreamEncoder e)
	{
		this.DoWithAllValues(delegate(string name, int value)
		{
			e.WriteKey(name);
			e.WriteNumber((long)value);
		});
	}

	// Token: 0x06001874 RID: 6260 RVA: 0x000ABE84 File Offset: 0x000AA284
	private void DoWithAllValues(Action<string, int> action)
	{
		this.DoWithAllValues<string>(this.blockTypeCounts, "Block", action, null);
		this.DoWithAllValues<string>(this.blockTagCounts, "BlockTag", action, null);
		this.DoWithAllValues<string>(this.blockShapeCategoryCounts, "ShapeCategory", action, null);
		Dictionary<Predicate, int> dict = this.predicateCounts;
		Dictionary<Predicate, int> dictBeforeThen = this.predicateCountsBeforeThen;
		Dictionary<Predicate, int> dictAfterThen = this.predicateCountsAfterThen;
		string prefix = "Predicate";
		if (MultiCounterModelFeatureType.f__mg_cache0 == null)
		{
			MultiCounterModelFeatureType.f__mg_cache0 = new Func<Predicate, string>(MultiCounterModelFeatureType.PredicateToString);
		}
		this.DoWithAllValues<Predicate>(dict, dictBeforeThen, dictAfterThen, prefix, action, MultiCounterModelFeatureType.f__mg_cache0);
		this.DoWithAllValues<string>(this.predicateTagCounts, this.predicateTagCountsBeforeThen, this.predicateTagCountsAfterThen, "PredicateTag", action, null);
		this.DoWithAllValues<string>(null, this.scriptedTextureCountsBeforeThen, this.scriptedTextureCountsAfterThen, "Texture", action, null);
		this.DoWithAllValues<string>(null, this.scriptedTextureTagCountsBeforeThen, this.scriptedTextureTagCountsAfterThen, "TextureTag", action, null);
		this.DoWithAllValues<string>(this.permanentTextureCounts, "PermanentTexture", action, null);
		this.DoWithAllValues<string>(this.permanentTextureTagCounts, "PermanentTextureTag", action, null);
	}

	// Token: 0x06001875 RID: 6261 RVA: 0x000ABF80 File Offset: 0x000AA380
	private void DoWithAllValues<K>(Dictionary<K, int> dict, Dictionary<K, int> dictBeforeThen, Dictionary<K, int> dictAfterThen, string prefix, Action<string, int> action, Func<K, string> keyToString = null)
	{
		if (dict != null)
		{
			this.DoWithAllValues<K>(dict, prefix, action, keyToString);
		}
		if (dictBeforeThen != null)
		{
			this.DoWithAllValues<K>(dictBeforeThen, prefix + "BeforeThen", action, keyToString);
		}
		if (dictAfterThen != null)
		{
			this.DoWithAllValues<K>(dictAfterThen, prefix + "AfterThen", action, keyToString);
		}
	}

	// Token: 0x06001876 RID: 6262 RVA: 0x000ABFDC File Offset: 0x000AA3DC
	private static string GetCountKey<K>(K key, string postfix, Func<K, string> keyToString = null)
	{
		string str;
		if (keyToString != null)
		{
			str = keyToString(key);
		}
		else
		{
			str = key.ToString();
		}
		return str + postfix + "Count";
	}

	// Token: 0x06001877 RID: 6263 RVA: 0x000AC018 File Offset: 0x000AA418
	private void DoWithAllValues<K>(Dictionary<K, int> dict, string postfix, Action<string, int> action, Func<K, string> keyToString = null)
	{
		foreach (KeyValuePair<K, int> keyValuePair in dict)
		{
			action(MultiCounterModelFeatureType.GetCountKey<K>(keyValuePair.Key, postfix, keyToString), keyValuePair.Value);
		}
	}

	// Token: 0x06001878 RID: 6264 RVA: 0x000AC084 File Offset: 0x000AA484
	public override void Reset()
	{
		this.blockCount = 0;
		this.predicateCounts.Clear();
		this.predicateCountsBeforeThen.Clear();
		this.predicateCountsAfterThen.Clear();
		this.predicateTagCounts.Clear();
		this.predicateTagCountsBeforeThen.Clear();
		this.predicateTagCountsAfterThen.Clear();
		this.blockTypeCounts.Clear();
		this.blockTagCounts.Clear();
		this.blockShapeCategoryCounts.Clear();
		this.permanentTextureCounts.Clear();
		this.scriptedTextureCountsBeforeThen.Clear();
		this.scriptedTextureCountsAfterThen.Clear();
		this.permanentTextureTagCounts.Clear();
		this.scriptedTextureTagCountsBeforeThen.Clear();
		this.scriptedTextureTagCountsAfterThen.Clear();
	}

	// Token: 0x06001879 RID: 6265 RVA: 0x000AC140 File Offset: 0x000AA540
	private void Increment<T>(Dictionary<T, int> dict, T key)
	{
		int num;
		if (!dict.TryGetValue(key, out num))
		{
			dict[key] = 1;
		}
		else
		{
			dict[key] = num + 1;
		}
	}

	// Token: 0x0600187A RID: 6266 RVA: 0x000AC174 File Offset: 0x000AA574
	private void IncrementBeforeAfter<T>(T key, Dictionary<T, int> anyDict, Dictionary<T, int> beforeDict, Dictionary<T, int> afterDict, bool beforeThen, HashSet<string> tags = null, Dictionary<string, int> anyTags = null, Dictionary<string, int> beforeTags = null, Dictionary<string, int> afterTags = null)
	{
		if (anyDict != null)
		{
			this.Increment<T>(anyDict, key);
		}
		if (beforeThen)
		{
			this.Increment<T>(beforeDict, key);
		}
		else
		{
			this.Increment<T>(afterDict, key);
		}
		if (tags != null)
		{
			foreach (string key2 in tags)
			{
				this.IncrementBeforeAfter<string>(key2, anyTags, beforeTags, afterTags, beforeThen, null, null, null, null);
			}
		}
	}

	// Token: 0x0600187B RID: 6267 RVA: 0x000AC20C File Offset: 0x000AA60C
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		Predicate predicate = tile.gaf.Predicate;
		HashSet<string> tags = EntityTagsRegistry.PredicateTags(predicate.Name);
		if (!MultiCounterModelFeatureType.GetIgnorePredicates().Contains(predicate))
		{
			this.IncrementBeforeAfter<Predicate>(predicate, this.predicateCounts, this.predicateCountsBeforeThen, this.predicateCountsAfterThen, beforeThen, tags, this.predicateTagCounts, this.predicateTagCountsAfterThen, this.predicateTagCountsBeforeThen);
		}
		if (predicate == Block.predicateTextureTo)
		{
			string text = (string)tile.gaf.Args[0];
			HashSet<string> tags2 = EntityTagsRegistry.TextureTags(text);
			bool flag = rowIndex == 0;
			this.IncrementBeforeAfter<string>(text, (!flag) ? null : this.permanentTextureCounts, this.scriptedTextureCountsBeforeThen, this.scriptedTextureCountsAfterThen, beforeThen, tags2, (!flag) ? null : this.permanentTextureTagCounts, this.scriptedTextureTagCountsAfterThen, this.scriptedTextureTagCountsBeforeThen);
		}
		else if (predicate == Block.predicateCreate)
		{
			this.blockCount++;
			string text2 = (string)tile.gaf.Args[0];
			HashSet<string> hashSet = EntityTagsRegistry.BlockTags(text2);
			this.Increment<string>(this.blockTypeCounts, text2);
			foreach (string key in hashSet)
			{
				this.Increment<string>(this.blockTagCounts, key);
			}
			HashSet<string> shapeCategories = Scarcity.GetShapeCategories(text2);
			foreach (string text3 in shapeCategories)
			{
				if (text3 == null)
				{
					BWLog.Error("Category was null!");
				}
				else
				{
					this.Increment<string>(this.blockShapeCategoryCounts, text3);
				}
			}
		}
	}

	// Token: 0x04001358 RID: 4952
	public int blockCount;

	// Token: 0x04001359 RID: 4953
	public Dictionary<Predicate, int> predicateCounts = new Dictionary<Predicate, int>();

	// Token: 0x0400135A RID: 4954
	public Dictionary<Predicate, int> predicateCountsBeforeThen = new Dictionary<Predicate, int>();

	// Token: 0x0400135B RID: 4955
	public Dictionary<Predicate, int> predicateCountsAfterThen = new Dictionary<Predicate, int>();

	// Token: 0x0400135C RID: 4956
	public Dictionary<string, int> predicateTagCounts = new Dictionary<string, int>();

	// Token: 0x0400135D RID: 4957
	public Dictionary<string, int> predicateTagCountsBeforeThen = new Dictionary<string, int>();

	// Token: 0x0400135E RID: 4958
	public Dictionary<string, int> predicateTagCountsAfterThen = new Dictionary<string, int>();

	// Token: 0x0400135F RID: 4959
	public Dictionary<string, int> blockTypeCounts = new Dictionary<string, int>();

	// Token: 0x04001360 RID: 4960
	public Dictionary<string, int> blockTagCounts = new Dictionary<string, int>();

	// Token: 0x04001361 RID: 4961
	public Dictionary<string, int> blockShapeCategoryCounts = new Dictionary<string, int>();

	// Token: 0x04001362 RID: 4962
	public Dictionary<string, int> permanentTextureCounts = new Dictionary<string, int>();

	// Token: 0x04001363 RID: 4963
	public Dictionary<string, int> scriptedTextureCountsBeforeThen = new Dictionary<string, int>();

	// Token: 0x04001364 RID: 4964
	public Dictionary<string, int> scriptedTextureCountsAfterThen = new Dictionary<string, int>();

	// Token: 0x04001365 RID: 4965
	public Dictionary<string, int> permanentTextureTagCounts = new Dictionary<string, int>();

	// Token: 0x04001366 RID: 4966
	public Dictionary<string, int> scriptedTextureTagCountsBeforeThen = new Dictionary<string, int>();

	// Token: 0x04001367 RID: 4967
	public Dictionary<string, int> scriptedTextureTagCountsAfterThen = new Dictionary<string, int>();

	// Token: 0x04001368 RID: 4968
	private static HashSet<Predicate> ignorePredicates;

	// Token: 0x04001369 RID: 4969
	public const string BLOCK = "Block";

	// Token: 0x0400136A RID: 4970
	public const string BLOCK_TAG = "BlockTag";

	// Token: 0x0400136B RID: 4971
	public const string SHAPE_CATEGORY = "ShapeCategory";

	// Token: 0x0400136C RID: 4972
	public const string PREDICATE = "Predicate";

	// Token: 0x0400136D RID: 4973
	public const string PREDICATE_TAG = "PredicateTag";

	// Token: 0x0400136E RID: 4974
	public const string TEXTURE = "Texture";

	// Token: 0x0400136F RID: 4975
	public const string TEXTURE_TAG = "TextureTag";

	// Token: 0x04001370 RID: 4976
	public const string PERMANENT_TEXTURE = "PermanentTexture";

	// Token: 0x04001371 RID: 4977
	public const string PERMANENT_TEXTURE_TAG = "PermanentTextureTag";

	// Token: 0x04001372 RID: 4978
	public const string BEFORE_THEN = "BeforeThen";

	// Token: 0x04001373 RID: 4979
	public const string AFTER_THEN = "AfterThen";

	// Token: 0x04001374 RID: 4980
	public const string COUNT = "Count";

	// Token: 0x04001375 RID: 4981
	[CompilerGenerated]
	private static Func<Predicate, string> f__mg_cache0;
}
