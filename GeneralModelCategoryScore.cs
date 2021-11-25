using System;
using System.Text;

// Token: 0x020001C9 RID: 457
[Serializable]
public class GeneralModelCategoryScore : ModelCategoryScore
{
	// Token: 0x06001839 RID: 6201 RVA: 0x000AB03A File Offset: 0x000A943A
	private float GetTimesScore(int count)
	{
		return this.timesScore * (float)count;
	}

	// Token: 0x0600183A RID: 6202 RVA: 0x000AB045 File Offset: 0x000A9445
	private float GetScore(bool triggered)
	{
		if (triggered)
		{
			return this.customScore;
		}
		return 0f;
	}

	// Token: 0x0600183B RID: 6203 RVA: 0x000AB05C File Offset: 0x000A945C
	public override string ToString()
	{
		GeneralModelCategoryScoreTypeVariableUsage generalModelCategoryScoreTypeVariableUsage = new GeneralModelCategoryScoreTypeVariableUsage(this.type);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append(this.type.ToString());
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockName)
		{
			stringBuilder.Append("'" + this.blockName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockTag)
		{
			stringBuilder.Append("'" + this.blockTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockCount)
		{
			stringBuilder.Append(this.blockCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockTagCount)
		{
			stringBuilder.Append(this.blockTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureName)
		{
			stringBuilder.Append("'" + this.textureName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureTag)
		{
			stringBuilder.Append("'" + this.textureTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureCount)
		{
			stringBuilder.Append(this.textureCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureTagCount)
		{
			stringBuilder.Append(this.textureTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateName)
		{
			stringBuilder.Append("'" + this.predicateName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateTag)
		{
			stringBuilder.Append("'" + this.predicateTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateCount)
		{
			stringBuilder.Append(this.predicateCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateTagCount)
		{
			stringBuilder.Append(this.predicateTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesShapeCategoryName)
		{
			stringBuilder.Append("'" + this.shapeCategoryName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesShapeCategoryCount)
		{
			stringBuilder.Append(this.shapeCategoryCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesCustomCount)
		{
			stringBuilder.Append(this.customCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTimesScore)
		{
			stringBuilder.Append(this.timesScore + " ");
		}
		else
		{
			stringBuilder.Append(this.customScore + " ");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	// Token: 0x0600183C RID: 6204 RVA: 0x000AB344 File Offset: 0x000A9744
	public override float GetCategoryScore(ModelCategorizerContext ctx)
	{
		switch (this.type)
		{
		case GeneralModelCategoryScoreType.BlockCountGreaterThan:
			return this.GetScore(ctx.GetBlockCount(this.blockName) > this.blockCount);
		case GeneralModelCategoryScoreType.BlockTagCountGreaterThan:
			return this.GetScore(ctx.GetBlockTagCount(this.blockTag) > this.blockTagCount);
		case GeneralModelCategoryScoreType.BlockCountTimes:
			return this.GetTimesScore(ctx.GetBlockCount(this.blockName));
		case GeneralModelCategoryScoreType.BlockTagCountTimes:
			return this.GetTimesScore(ctx.GetBlockTagCount(this.blockTag));
		case GeneralModelCategoryScoreType.TextureCountGreaterThan:
			return this.GetScore(ctx.GetPermanentTextureCount(this.textureName) > this.textureCount);
		case GeneralModelCategoryScoreType.TextureTagCountGreaterThan:
			return this.GetScore(ctx.GetPermanentTextureTagCount(this.textureTag) > this.textureTagCount);
		case GeneralModelCategoryScoreType.TextureCountTimes:
			return this.GetTimesScore(ctx.GetPermanentTextureCount(this.textureName));
		case GeneralModelCategoryScoreType.TextureTagCountTimes:
			return this.GetTimesScore(ctx.GetPermanentTextureTagCount(this.textureTag));
		case GeneralModelCategoryScoreType.PredicateCountGreaterThan:
			return this.GetScore(ctx.GetPredicateCount(this.textureName) > this.textureCount);
		case GeneralModelCategoryScoreType.PredicateTagCountGreaterThan:
			return this.GetScore(ctx.GetPredicateTagCount(this.textureTag) > this.textureTagCount);
		case GeneralModelCategoryScoreType.PredicateCountTimes:
			return this.GetTimesScore(ctx.GetPredicateCount(this.textureName));
		case GeneralModelCategoryScoreType.PredicateTagCountTimes:
			return this.GetTimesScore(ctx.GetPredicateTagCount(this.textureTag));
		case GeneralModelCategoryScoreType.ShapeCategoryCountGreaterThan:
			return this.GetScore(ctx.GetShapeCategoryCount(this.shapeCategoryName) > this.shapeCategoryCount);
		case GeneralModelCategoryScoreType.MaxWheelsAlongSameLineGreaterThan:
			return this.GetScore(ctx.GetInt("MaxWheelsAlongSameLine") > this.customCount);
		case GeneralModelCategoryScoreType.NonConditionalFreeze:
			return this.GetScore(ctx.GetBool("NonConditionalFreeze"));
		default:
			return 0f;
		}
	}

	// Token: 0x04001314 RID: 4884
	public GeneralModelCategoryScoreType type;

	// Token: 0x04001315 RID: 4885
	public string blockName;

	// Token: 0x04001316 RID: 4886
	public string blockTag;

	// Token: 0x04001317 RID: 4887
	public int blockCount;

	// Token: 0x04001318 RID: 4888
	public int blockTagCount;

	// Token: 0x04001319 RID: 4889
	public string predicateName;

	// Token: 0x0400131A RID: 4890
	public string predicateTag;

	// Token: 0x0400131B RID: 4891
	public int predicateCount;

	// Token: 0x0400131C RID: 4892
	public int predicateTagCount;

	// Token: 0x0400131D RID: 4893
	public string textureName;

	// Token: 0x0400131E RID: 4894
	public string textureTag;

	// Token: 0x0400131F RID: 4895
	public int textureCount;

	// Token: 0x04001320 RID: 4896
	public int textureTagCount;

	// Token: 0x04001321 RID: 4897
	public string shapeCategoryName;

	// Token: 0x04001322 RID: 4898
	public int shapeCategoryCount;

	// Token: 0x04001323 RID: 4899
	public int rangeCountFrom;

	// Token: 0x04001324 RID: 4900
	public int rangeCountTo;

	// Token: 0x04001325 RID: 4901
	public int customCount;

	// Token: 0x04001326 RID: 4902
	public float customScore;

	// Token: 0x04001327 RID: 4903
	public float timesScore;

	// Token: 0x04001328 RID: 4904
	public float fractionScore;
}
