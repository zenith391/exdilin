public class GeneralModelCategoryScoreTypeVariableUsage
{
	public bool usesBlockName;

	public bool usesBlockCount;

	public bool usesBlockTag;

	public bool usesBlockTagCount;

	public bool usesTextureName;

	public bool usesTextureCount;

	public bool usesTextureTag;

	public bool usesTextureTagCount;

	public bool usesPredicateName;

	public bool usesPredicateCount;

	public bool usesPredicateTag;

	public bool usesPredicateTagCount;

	public bool usesShapeCategoryCount;

	public bool usesShapeCategoryName;

	public bool usesTimesScore;

	public bool usesCustomCount;

	public GeneralModelCategoryScoreTypeVariableUsage(GeneralModelCategoryScoreType type)
	{
		SetFromScoreType(type);
	}

	public void SetFromScoreType(GeneralModelCategoryScoreType type)
	{
		switch (type)
		{
		case GeneralModelCategoryScoreType.BlockCountGreaterThan:
			usesBlockName = true;
			usesBlockCount = true;
			break;
		case GeneralModelCategoryScoreType.BlockTagCountGreaterThan:
			usesBlockTag = true;
			usesBlockTagCount = true;
			break;
		case GeneralModelCategoryScoreType.BlockCountTimes:
			usesBlockName = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.BlockTagCountTimes:
			usesBlockTag = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.TextureCountGreaterThan:
			usesTextureName = true;
			usesTextureCount = true;
			break;
		case GeneralModelCategoryScoreType.TextureTagCountGreaterThan:
			usesTextureTag = true;
			usesTextureTagCount = true;
			break;
		case GeneralModelCategoryScoreType.TextureCountTimes:
			usesTextureName = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.TextureTagCountTimes:
			usesTextureTag = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.PredicateCountGreaterThan:
			usesPredicateName = true;
			usesPredicateCount = true;
			break;
		case GeneralModelCategoryScoreType.PredicateTagCountGreaterThan:
			usesPredicateTag = true;
			usesPredicateTagCount = true;
			break;
		case GeneralModelCategoryScoreType.PredicateCountTimes:
			usesPredicateName = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.PredicateTagCountTimes:
			usesPredicateTag = true;
			usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.ShapeCategoryCountGreaterThan:
			usesShapeCategoryName = true;
			usesShapeCategoryCount = true;
			break;
		case GeneralModelCategoryScoreType.MaxWheelsAlongSameLineGreaterThan:
			usesCustomCount = true;
			break;
		}
	}
}
