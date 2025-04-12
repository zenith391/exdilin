using System;
using System.Collections.Generic;

// Token: 0x020001CF RID: 463
public class ModelCategorizerContext
{
	// Token: 0x06001848 RID: 6216 RVA: 0x000AB89C File Offset: 0x000A9C9C
	public ModelCategorizerContext(List<ModelFeatureType> features)
	{
		this.ints.Clear();
		this.floats.Clear();
		this.bools.Clear();
		foreach (ModelFeatureType modelFeatureType in features)
		{
			modelFeatureType.SetContextValues(this);
		}
	}

	// Token: 0x06001849 RID: 6217 RVA: 0x000AB93C File Offset: 0x000A9D3C
	public int GetShapeCategoryCount(string cat)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetShapeCategoryCountFeatureName(cat));
	}

	// Token: 0x0600184A RID: 6218 RVA: 0x000AB94A File Offset: 0x000A9D4A
	public int GetBlockCount(string blockName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetBlockCountFeatureName(blockName));
	}

	// Token: 0x0600184B RID: 6219 RVA: 0x000AB958 File Offset: 0x000A9D58
	public int GetBlockTagCount(string tagName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetBlockTagCountFeatureName(tagName));
	}

	// Token: 0x0600184C RID: 6220 RVA: 0x000AB966 File Offset: 0x000A9D66
	public int GetPermanentTextureCount(string textureName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetTextureCountFeatureName(textureName));
	}

	// Token: 0x0600184D RID: 6221 RVA: 0x000AB974 File Offset: 0x000A9D74
	public int GetPermanentTextureTagCount(string tagName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetTextureTagCountFeatureName(tagName));
	}

	// Token: 0x0600184E RID: 6222 RVA: 0x000AB982 File Offset: 0x000A9D82
	public int GetPredicateCount(string predName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetPredicateCountFeatureName(predName));
	}

	// Token: 0x0600184F RID: 6223 RVA: 0x000AB990 File Offset: 0x000A9D90
	public int GetPredicateTagCount(string tagName)
	{
		return this.GetInt(MultiCounterModelFeatureType.GetPredicateTagCountFeatureName(tagName));
	}

	// Token: 0x06001850 RID: 6224 RVA: 0x000AB99E File Offset: 0x000A9D9E
	public bool GetBool(string name)
	{
		return this.bools.Contains(name);
	}

	// Token: 0x06001851 RID: 6225 RVA: 0x000AB9AC File Offset: 0x000A9DAC
	public bool HasInt(string name)
	{
		return this.ints.ContainsKey(name);
	}

	// Token: 0x06001852 RID: 6226 RVA: 0x000AB9BA File Offset: 0x000A9DBA
	public bool HasFloat(string name)
	{
		return this.floats.ContainsKey(name);
	}

	// Token: 0x06001853 RID: 6227 RVA: 0x000AB9C8 File Offset: 0x000A9DC8
	public int GetInt(string name)
	{
		int result;
		this.TryGetInt(name, out result);
		return result;
	}

	// Token: 0x06001854 RID: 6228 RVA: 0x000AB9E0 File Offset: 0x000A9DE0
	public float GetFloat(string name)
	{
		float result;
		this.TryGetFloat(name, out result);
		return result;
	}

	// Token: 0x06001855 RID: 6229 RVA: 0x000AB9F8 File Offset: 0x000A9DF8
	public bool TryGetInt(string name, out int result)
	{
		return this.ints.TryGetValue(name, out result);
	}

	// Token: 0x06001856 RID: 6230 RVA: 0x000ABA07 File Offset: 0x000A9E07
	public bool TryGetFloat(string name, out float result)
	{
		return this.floats.TryGetValue(name, out result);
	}

	// Token: 0x06001857 RID: 6231 RVA: 0x000ABA16 File Offset: 0x000A9E16
	public void SetFloat(string name, float value)
	{
		this.floats[name] = value;
	}

	// Token: 0x06001858 RID: 6232 RVA: 0x000ABA25 File Offset: 0x000A9E25
	public void SetInt(string name, int value)
	{
		this.ints[name] = value;
	}

	// Token: 0x06001859 RID: 6233 RVA: 0x000ABA34 File Offset: 0x000A9E34
	public void SetBool(string name, bool value)
	{
		if (value)
		{
			this.bools.Add(name);
		}
		else
		{
			this.bools.Remove(name);
		}
	}

	// Token: 0x0400134F RID: 4943
	private Dictionary<string, int> ints = new Dictionary<string, int>();

	// Token: 0x04001350 RID: 4944
	private Dictionary<string, float> floats = new Dictionary<string, float>();

	// Token: 0x04001351 RID: 4945
	private HashSet<string> bools = new HashSet<string>();
}
