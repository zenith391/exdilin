using System;

[Serializable]
public class GeneralModelCategoryScores
{
	public string modelCategory;

	public GeneralModelCategoryScore[] scores;

	[NonSerialized]
	public bool foldout;
}
