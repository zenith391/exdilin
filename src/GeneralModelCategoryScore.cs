using System;
using System.Text;

[Serializable]
public class GeneralModelCategoryScore : ModelCategoryScore
{
	public GeneralModelCategoryScoreType type;

	public string blockName;

	public string blockTag;

	public int blockCount;

	public int blockTagCount;

	public string predicateName;

	public string predicateTag;

	public int predicateCount;

	public int predicateTagCount;

	public string textureName;

	public string textureTag;

	public int textureCount;

	public int textureTagCount;

	public string shapeCategoryName;

	public int shapeCategoryCount;

	public int rangeCountFrom;

	public int rangeCountTo;

	public int customCount;

	public float customScore;

	public float timesScore;

	public float fractionScore;

	private float GetTimesScore(int count)
	{
		return timesScore * (float)count;
	}

	private float GetScore(bool triggered)
	{
		if (triggered)
		{
			return customScore;
		}
		return 0f;
	}

	public override string ToString()
	{
		GeneralModelCategoryScoreTypeVariableUsage generalModelCategoryScoreTypeVariableUsage = new GeneralModelCategoryScoreTypeVariableUsage(type);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append(type.ToString());
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockName)
		{
			stringBuilder.Append("'" + blockName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockTag)
		{
			stringBuilder.Append("'" + blockTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockCount)
		{
			stringBuilder.Append(blockCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesBlockTagCount)
		{
			stringBuilder.Append(blockTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureName)
		{
			stringBuilder.Append("'" + textureName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureTag)
		{
			stringBuilder.Append("'" + textureTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureCount)
		{
			stringBuilder.Append(textureCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTextureTagCount)
		{
			stringBuilder.Append(textureTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateName)
		{
			stringBuilder.Append("'" + predicateName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateTag)
		{
			stringBuilder.Append("'" + predicateTag + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateCount)
		{
			stringBuilder.Append(predicateCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesPredicateTagCount)
		{
			stringBuilder.Append(predicateTagCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesShapeCategoryName)
		{
			stringBuilder.Append("'" + shapeCategoryName + "' ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesShapeCategoryCount)
		{
			stringBuilder.Append(shapeCategoryCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesCustomCount)
		{
			stringBuilder.Append(customCount + " ");
		}
		if (generalModelCategoryScoreTypeVariableUsage.usesTimesScore)
		{
			stringBuilder.Append(timesScore + " ");
		}
		else
		{
			stringBuilder.Append(customScore + " ");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public override float GetCategoryScore(ModelCategorizerContext ctx)
	{
		return type switch
		{
			GeneralModelCategoryScoreType.BlockCountGreaterThan => GetScore(ctx.GetBlockCount(blockName) > blockCount), 
			GeneralModelCategoryScoreType.BlockTagCountGreaterThan => GetScore(ctx.GetBlockTagCount(blockTag) > blockTagCount), 
			GeneralModelCategoryScoreType.BlockCountTimes => GetTimesScore(ctx.GetBlockCount(blockName)), 
			GeneralModelCategoryScoreType.BlockTagCountTimes => GetTimesScore(ctx.GetBlockTagCount(blockTag)), 
			GeneralModelCategoryScoreType.TextureCountGreaterThan => GetScore(ctx.GetPermanentTextureCount(textureName) > textureCount), 
			GeneralModelCategoryScoreType.TextureTagCountGreaterThan => GetScore(ctx.GetPermanentTextureTagCount(textureTag) > textureTagCount), 
			GeneralModelCategoryScoreType.TextureCountTimes => GetTimesScore(ctx.GetPermanentTextureCount(textureName)), 
			GeneralModelCategoryScoreType.TextureTagCountTimes => GetTimesScore(ctx.GetPermanentTextureTagCount(textureTag)), 
			GeneralModelCategoryScoreType.PredicateCountGreaterThan => GetScore(ctx.GetPredicateCount(textureName) > textureCount), 
			GeneralModelCategoryScoreType.PredicateTagCountGreaterThan => GetScore(ctx.GetPredicateTagCount(textureTag) > textureTagCount), 
			GeneralModelCategoryScoreType.PredicateCountTimes => GetTimesScore(ctx.GetPredicateCount(textureName)), 
			GeneralModelCategoryScoreType.PredicateTagCountTimes => GetTimesScore(ctx.GetPredicateTagCount(textureTag)), 
			GeneralModelCategoryScoreType.ShapeCategoryCountGreaterThan => GetScore(ctx.GetShapeCategoryCount(shapeCategoryName) > shapeCategoryCount), 
			GeneralModelCategoryScoreType.MaxWheelsAlongSameLineGreaterThan => GetScore(ctx.GetInt("MaxWheelsAlongSameLine") > customCount), 
			GeneralModelCategoryScoreType.NonConditionalFreeze => GetScore(ctx.GetBool("NonConditionalFreeze")), 
			_ => 0f, 
		};
	}
}
