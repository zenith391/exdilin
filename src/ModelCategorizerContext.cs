using System.Collections.Generic;

public class ModelCategorizerContext
{
	private Dictionary<string, int> ints = new Dictionary<string, int>();

	private Dictionary<string, float> floats = new Dictionary<string, float>();

	private HashSet<string> bools = new HashSet<string>();

	public ModelCategorizerContext(List<ModelFeatureType> features)
	{
		ints.Clear();
		floats.Clear();
		bools.Clear();
		foreach (ModelFeatureType feature in features)
		{
			feature.SetContextValues(this);
		}
	}

	public int GetShapeCategoryCount(string cat)
	{
		return GetInt(MultiCounterModelFeatureType.GetShapeCategoryCountFeatureName(cat));
	}

	public int GetBlockCount(string blockName)
	{
		return GetInt(MultiCounterModelFeatureType.GetBlockCountFeatureName(blockName));
	}

	public int GetBlockTagCount(string tagName)
	{
		return GetInt(MultiCounterModelFeatureType.GetBlockTagCountFeatureName(tagName));
	}

	public int GetPermanentTextureCount(string textureName)
	{
		return GetInt(MultiCounterModelFeatureType.GetTextureCountFeatureName(textureName));
	}

	public int GetPermanentTextureTagCount(string tagName)
	{
		return GetInt(MultiCounterModelFeatureType.GetTextureTagCountFeatureName(tagName));
	}

	public int GetPredicateCount(string predName)
	{
		return GetInt(MultiCounterModelFeatureType.GetPredicateCountFeatureName(predName));
	}

	public int GetPredicateTagCount(string tagName)
	{
		return GetInt(MultiCounterModelFeatureType.GetPredicateTagCountFeatureName(tagName));
	}

	public bool GetBool(string name)
	{
		return bools.Contains(name);
	}

	public bool HasInt(string name)
	{
		return ints.ContainsKey(name);
	}

	public bool HasFloat(string name)
	{
		return floats.ContainsKey(name);
	}

	public int GetInt(string name)
	{
		TryGetInt(name, out var result);
		return result;
	}

	public float GetFloat(string name)
	{
		TryGetFloat(name, out var result);
		return result;
	}

	public bool TryGetInt(string name, out int result)
	{
		return ints.TryGetValue(name, out result);
	}

	public bool TryGetFloat(string name, out float result)
	{
		return floats.TryGetValue(name, out result);
	}

	public void SetFloat(string name, float value)
	{
		floats[name] = value;
	}

	public void SetInt(string name, int value)
	{
		ints[name] = value;
	}

	public void SetBool(string name, bool value)
	{
		if (value)
		{
			bools.Add(name);
		}
		else
		{
			bools.Remove(name);
		}
	}
}
