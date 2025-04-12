using System;
using System.Collections.Generic;
using System.Text;

// Token: 0x020001CE RID: 462
public class ModelCategorizer
{
	// Token: 0x06001846 RID: 6214 RVA: 0x000AB67C File Offset: 0x000A9A7C
	public static string GetModelCategory(List<ModelFeatureType> features, StringBuilder feedbackBuilder = null)
	{
		if (ModelCategorizer.categoryScores == null)
		{
			ModelCategorizer.categoryScores = new List<ModelCategoryScore>();
			foreach (GeneralModelCategoryScores generalModelCategoryScores in EntityTagsRegistry.allTags.categoryScores)
			{
				foreach (GeneralModelCategoryScore generalModelCategoryScore in generalModelCategoryScores.scores)
				{
					generalModelCategoryScore.modelCategory = generalModelCategoryScores.modelCategory;
					ModelCategorizer.categoryScores.Add(generalModelCategoryScore);
				}
			}
		}
		ModelCategorizerContext context = new ModelCategorizerContext(features);
		ModelCategorizer.categoryScoresDict.Clear();
		foreach (ModelCategoryScore modelCategoryScore in ModelCategorizer.categoryScores)
		{
			float num;
			if (!ModelCategorizer.categoryScoresDict.TryGetValue(modelCategoryScore.modelCategory, out num))
			{
				num = 0f;
				ModelCategorizer.categoryScoresDict[modelCategoryScore.modelCategory] = num;
			}
			float categoryScore = modelCategoryScore.GetCategoryScore(context);
			ModelCategorizer.categoryScoresDict[modelCategoryScore.modelCategory] = num + categoryScore;
			if (feedbackBuilder != null && categoryScore != 0f)
			{
				feedbackBuilder.AppendLine(string.Concat(new object[]
				{
					"Adding score ",
					categoryScore,
					" for category ",
					modelCategoryScore.modelCategory,
					" using ",
					modelCategoryScore.ToString(),
					" total ",
					num + categoryScore
				}));
			}
		}
		float num2 = float.MinValue;
		string result = string.Empty;
		foreach (KeyValuePair<string, float> keyValuePair in ModelCategorizer.categoryScoresDict)
		{
			if (keyValuePair.Value > num2)
			{
				num2 = keyValuePair.Value;
				result = keyValuePair.Key;
			}
		}
		return result;
	}

	// Token: 0x0400134D RID: 4941
	public static List<ModelCategoryScore> categoryScores;

	// Token: 0x0400134E RID: 4942
	private static Dictionary<string, float> categoryScoresDict = new Dictionary<string, float>();
}
