using System;

[Serializable]
public enum GeneralModelCategoryScoreType
{
	BlockCountGreaterThan,
	BlockTagCountGreaterThan,
	BlockCountTimes,
	BlockTagCountTimes,
	TextureCountGreaterThan,
	TextureTagCountGreaterThan,
	TextureCountTimes,
	TextureTagCountTimes,
	PredicateCountGreaterThan,
	PredicateTagCountGreaterThan,
	PredicateCountTimes,
	PredicateTagCountTimes,
	ShapeCategoryCountGreaterThan,
	MaxWheelsAlongSameLineGreaterThan,
	NonConditionalFreeze
}
