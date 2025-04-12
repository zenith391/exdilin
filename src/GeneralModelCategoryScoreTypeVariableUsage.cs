using System;

// Token: 0x020001CB RID: 459
public class GeneralModelCategoryScoreTypeVariableUsage
{
	// Token: 0x0600183D RID: 6205 RVA: 0x000AB4FF File Offset: 0x000A98FF
	public GeneralModelCategoryScoreTypeVariableUsage(GeneralModelCategoryScoreType type)
	{
		this.SetFromScoreType(type);
	}

	// Token: 0x0600183E RID: 6206 RVA: 0x000AB510 File Offset: 0x000A9910
	public void SetFromScoreType(GeneralModelCategoryScoreType type)
	{
		switch (type)
		{
		case GeneralModelCategoryScoreType.BlockCountGreaterThan:
			this.usesBlockName = true;
			this.usesBlockCount = true;
			break;
		case GeneralModelCategoryScoreType.BlockTagCountGreaterThan:
			this.usesBlockTag = true;
			this.usesBlockTagCount = true;
			break;
		case GeneralModelCategoryScoreType.BlockCountTimes:
			this.usesBlockName = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.BlockTagCountTimes:
			this.usesBlockTag = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.TextureCountGreaterThan:
			this.usesTextureName = true;
			this.usesTextureCount = true;
			break;
		case GeneralModelCategoryScoreType.TextureTagCountGreaterThan:
			this.usesTextureTag = true;
			this.usesTextureTagCount = true;
			break;
		case GeneralModelCategoryScoreType.TextureCountTimes:
			this.usesTextureName = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.TextureTagCountTimes:
			this.usesTextureTag = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.PredicateCountGreaterThan:
			this.usesPredicateName = true;
			this.usesPredicateCount = true;
			break;
		case GeneralModelCategoryScoreType.PredicateTagCountGreaterThan:
			this.usesPredicateTag = true;
			this.usesPredicateTagCount = true;
			break;
		case GeneralModelCategoryScoreType.PredicateCountTimes:
			this.usesPredicateName = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.PredicateTagCountTimes:
			this.usesPredicateTag = true;
			this.usesTimesScore = true;
			break;
		case GeneralModelCategoryScoreType.ShapeCategoryCountGreaterThan:
			this.usesShapeCategoryName = true;
			this.usesShapeCategoryCount = true;
			break;
		case GeneralModelCategoryScoreType.MaxWheelsAlongSameLineGreaterThan:
			this.usesCustomCount = true;
			break;
		}
	}

	// Token: 0x04001339 RID: 4921
	public bool usesBlockName;

	// Token: 0x0400133A RID: 4922
	public bool usesBlockCount;

	// Token: 0x0400133B RID: 4923
	public bool usesBlockTag;

	// Token: 0x0400133C RID: 4924
	public bool usesBlockTagCount;

	// Token: 0x0400133D RID: 4925
	public bool usesTextureName;

	// Token: 0x0400133E RID: 4926
	public bool usesTextureCount;

	// Token: 0x0400133F RID: 4927
	public bool usesTextureTag;

	// Token: 0x04001340 RID: 4928
	public bool usesTextureTagCount;

	// Token: 0x04001341 RID: 4929
	public bool usesPredicateName;

	// Token: 0x04001342 RID: 4930
	public bool usesPredicateCount;

	// Token: 0x04001343 RID: 4931
	public bool usesPredicateTag;

	// Token: 0x04001344 RID: 4932
	public bool usesPredicateTagCount;

	// Token: 0x04001345 RID: 4933
	public bool usesShapeCategoryCount;

	// Token: 0x04001346 RID: 4934
	public bool usesShapeCategoryName;

	// Token: 0x04001347 RID: 4935
	public bool usesTimesScore;

	// Token: 0x04001348 RID: 4936
	public bool usesCustomCount;
}
