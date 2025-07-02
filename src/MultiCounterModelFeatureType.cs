using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blocks;
using SimpleJSON;

public class MultiCounterModelFeatureType : ModelFeatureType
{
	public int blockCount;

	public Dictionary<Predicate, int> predicateCounts = new Dictionary<Predicate, int>();

	public Dictionary<Predicate, int> predicateCountsBeforeThen = new Dictionary<Predicate, int>();

	public Dictionary<Predicate, int> predicateCountsAfterThen = new Dictionary<Predicate, int>();

	public Dictionary<string, int> predicateTagCounts = new Dictionary<string, int>();

	public Dictionary<string, int> predicateTagCountsBeforeThen = new Dictionary<string, int>();

	public Dictionary<string, int> predicateTagCountsAfterThen = new Dictionary<string, int>();

	public Dictionary<string, int> blockTypeCounts = new Dictionary<string, int>();

	public Dictionary<string, int> blockTagCounts = new Dictionary<string, int>();

	public Dictionary<string, int> blockShapeCategoryCounts = new Dictionary<string, int>();

	public Dictionary<string, int> permanentTextureCounts = new Dictionary<string, int>();

	public Dictionary<string, int> scriptedTextureCountsBeforeThen = new Dictionary<string, int>();

	public Dictionary<string, int> scriptedTextureCountsAfterThen = new Dictionary<string, int>();

	public Dictionary<string, int> permanentTextureTagCounts = new Dictionary<string, int>();

	public Dictionary<string, int> scriptedTextureTagCountsBeforeThen = new Dictionary<string, int>();

	public Dictionary<string, int> scriptedTextureTagCountsAfterThen = new Dictionary<string, int>();

	private static HashSet<Predicate> ignorePredicates;

	public const string BLOCK = "Block";

	public const string BLOCK_TAG = "BlockTag";

	public const string SHAPE_CATEGORY = "ShapeCategory";

	public const string PREDICATE = "Predicate";

	public const string PREDICATE_TAG = "PredicateTag";

	public const string TEXTURE = "Texture";

	public const string TEXTURE_TAG = "TextureTag";

	public const string PERMANENT_TEXTURE = "PermanentTexture";

	public const string PERMANENT_TEXTURE_TAG = "PermanentTextureTag";

	public const string BEFORE_THEN = "BeforeThen";

	public const string AFTER_THEN = "AfterThen";

	public const string COUNT = "Count";

	[CompilerGenerated]
	private static Func<Predicate, string> f__mg_cache0;

	private static HashSet<Predicate> GetIgnorePredicates()
	{
		if (ignorePredicates == null)
		{
			ignorePredicates = new HashSet<Predicate>
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
		return ignorePredicates;
	}

	private static string PredicateToString(Predicate pred)
	{
		return pred.Name;
	}

	public static string GetBlockCountFeatureName(string blockName)
	{
		return GetCountKey(blockName, "Block");
	}

	public static string GetBlockTagCountFeatureName(string blockTag)
	{
		return GetCountKey(blockTag, "BlockTag");
	}

	public static string GetTextureCountFeatureName(string textureName)
	{
		return GetCountKey(textureName, "PermanentTexture");
	}

	public static string GetTextureTagCountFeatureName(string textureTag)
	{
		return GetCountKey(textureTag, "PermanentTextureTag");
	}

	public static string GetPredicateCountFeatureName(string predName)
	{
		return GetCountKey(predName, "Predicate");
	}

	public static string GetPredicateTagCountFeatureName(string predTag)
	{
		return GetCountKey(predTag, "PredicateTag");
	}

	public static string GetShapeCategoryCountFeatureName(string categoryName)
	{
		return GetCountKey(categoryName, "ShapeCategory");
	}

	public override void SetContextValues(ModelCategorizerContext ctx)
	{
		DoWithAllValues(delegate(string name, int value)
		{
			ctx.SetInt(name, value);
		});
	}

	public override void ToJSON(JSONStreamEncoder e)
	{
		DoWithAllValues(delegate(string name, int value)
		{
			e.WriteKey(name);
			e.WriteNumber(value);
		});
	}

	private void DoWithAllValues(Action<string, int> action)
	{
		DoWithAllValues(blockTypeCounts, "Block", action);
		DoWithAllValues(blockTagCounts, "BlockTag", action);
		DoWithAllValues(blockShapeCategoryCounts, "ShapeCategory", action);
		Dictionary<Predicate, int> dict = predicateCounts;
		Dictionary<Predicate, int> dictBeforeThen = predicateCountsBeforeThen;
		Dictionary<Predicate, int> dictAfterThen = predicateCountsAfterThen;
		string prefix = "Predicate";
		DoWithAllValues(dict, dictBeforeThen, dictAfterThen, prefix, action, PredicateToString);
		DoWithAllValues(predicateTagCounts, predicateTagCountsBeforeThen, predicateTagCountsAfterThen, "PredicateTag", action);
		DoWithAllValues(null, scriptedTextureCountsBeforeThen, scriptedTextureCountsAfterThen, "Texture", action);
		DoWithAllValues(null, scriptedTextureTagCountsBeforeThen, scriptedTextureTagCountsAfterThen, "TextureTag", action);
		DoWithAllValues(permanentTextureCounts, "PermanentTexture", action);
		DoWithAllValues(permanentTextureTagCounts, "PermanentTextureTag", action);
	}

	private void DoWithAllValues<K>(Dictionary<K, int> dict, Dictionary<K, int> dictBeforeThen, Dictionary<K, int> dictAfterThen, string prefix, Action<string, int> action, Func<K, string> keyToString = null)
	{
		if (dict != null)
		{
			DoWithAllValues(dict, prefix, action, keyToString);
		}
		if (dictBeforeThen != null)
		{
			DoWithAllValues(dictBeforeThen, prefix + "BeforeThen", action, keyToString);
		}
		if (dictAfterThen != null)
		{
			DoWithAllValues(dictAfterThen, prefix + "AfterThen", action, keyToString);
		}
	}

	private static string GetCountKey<K>(K key, string postfix, Func<K, string> keyToString = null)
	{
		string text = ((keyToString == null) ? key.ToString() : keyToString(key));
		return text + postfix + "Count";
	}

	private void DoWithAllValues<K>(Dictionary<K, int> dict, string postfix, Action<string, int> action, Func<K, string> keyToString = null)
	{
		foreach (KeyValuePair<K, int> item in dict)
		{
			action(GetCountKey(item.Key, postfix, keyToString), item.Value);
		}
	}

	public override void Reset()
	{
		blockCount = 0;
		predicateCounts.Clear();
		predicateCountsBeforeThen.Clear();
		predicateCountsAfterThen.Clear();
		predicateTagCounts.Clear();
		predicateTagCountsBeforeThen.Clear();
		predicateTagCountsAfterThen.Clear();
		blockTypeCounts.Clear();
		blockTagCounts.Clear();
		blockShapeCategoryCounts.Clear();
		permanentTextureCounts.Clear();
		scriptedTextureCountsBeforeThen.Clear();
		scriptedTextureCountsAfterThen.Clear();
		permanentTextureTagCounts.Clear();
		scriptedTextureTagCountsBeforeThen.Clear();
		scriptedTextureTagCountsAfterThen.Clear();
	}

	private void Increment<T>(Dictionary<T, int> dict, T key)
	{
		if (!dict.TryGetValue(key, out var value))
		{
			dict[key] = 1;
		}
		else
		{
			dict[key] = value + 1;
		}
	}

	private void IncrementBeforeAfter<T>(T key, Dictionary<T, int> anyDict, Dictionary<T, int> beforeDict, Dictionary<T, int> afterDict, bool beforeThen, HashSet<string> tags = null, Dictionary<string, int> anyTags = null, Dictionary<string, int> beforeTags = null, Dictionary<string, int> afterTags = null)
	{
		if (anyDict != null)
		{
			Increment(anyDict, key);
		}
		if (beforeThen)
		{
			Increment(beforeDict, key);
		}
		else
		{
			Increment(afterDict, key);
		}
		if (tags == null)
		{
			return;
		}
		foreach (string tag in tags)
		{
			IncrementBeforeAfter(tag, anyTags, beforeTags, afterTags, beforeThen);
		}
	}

	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		Predicate predicate = tile.gaf.Predicate;
		HashSet<string> tags = EntityTagsRegistry.PredicateTags(predicate.Name);
		if (!GetIgnorePredicates().Contains(predicate))
		{
			IncrementBeforeAfter(predicate, predicateCounts, predicateCountsBeforeThen, predicateCountsAfterThen, beforeThen, tags, predicateTagCounts, predicateTagCountsAfterThen, predicateTagCountsBeforeThen);
		}
		if (predicate == Block.predicateTextureTo)
		{
			string key = (string)tile.gaf.Args[0];
			HashSet<string> tags2 = EntityTagsRegistry.TextureTags(key);
			bool flag = rowIndex == 0;
			IncrementBeforeAfter(key, (!flag) ? null : permanentTextureCounts, scriptedTextureCountsBeforeThen, scriptedTextureCountsAfterThen, beforeThen, tags2, (!flag) ? null : permanentTextureTagCounts, scriptedTextureTagCountsAfterThen, scriptedTextureTagCountsBeforeThen);
		}
		else
		{
			if (predicate != Block.predicateCreate)
			{
				return;
			}
			blockCount++;
			string text = (string)tile.gaf.Args[0];
			HashSet<string> hashSet = EntityTagsRegistry.BlockTags(text);
			Increment(blockTypeCounts, text);
			foreach (string item in hashSet)
			{
				Increment(blockTagCounts, item);
			}
			HashSet<string> shapeCategories = Scarcity.GetShapeCategories(text);
			foreach (string item2 in shapeCategories)
			{
				if (item2 == null)
				{
					BWLog.Error("Category was null!");
				}
				else
				{
					Increment(blockShapeCategoryCounts, item2);
				}
			}
		}
	}
}
