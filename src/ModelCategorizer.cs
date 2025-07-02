using System.Collections.Generic;
using System.Text;

public class ModelCategorizer
{
	public static List<ModelCategoryScore> categoryScores;

	private static Dictionary<string, float> categoryScoresDict = new Dictionary<string, float>();

	public static string GetModelCategory(List<ModelFeatureType> features, StringBuilder feedbackBuilder = null)
	{
		if (categoryScores == null)
		{
			categoryScores = new List<ModelCategoryScore>();
			GeneralModelCategoryScores[] array = EntityTagsRegistry.allTags.categoryScores;
			foreach (GeneralModelCategoryScores generalModelCategoryScores in array)
			{
				GeneralModelCategoryScore[] scores = generalModelCategoryScores.scores;
				foreach (GeneralModelCategoryScore generalModelCategoryScore in scores)
				{
					generalModelCategoryScore.modelCategory = generalModelCategoryScores.modelCategory;
					categoryScores.Add(generalModelCategoryScore);
				}
			}
		}
		ModelCategorizerContext context = new ModelCategorizerContext(features);
		categoryScoresDict.Clear();
		foreach (ModelCategoryScore categoryScore2 in categoryScores)
		{
			if (!categoryScoresDict.TryGetValue(categoryScore2.modelCategory, out var value))
			{
				value = 0f;
				categoryScoresDict[categoryScore2.modelCategory] = value;
			}
			float categoryScore = categoryScore2.GetCategoryScore(context);
			categoryScoresDict[categoryScore2.modelCategory] = value + categoryScore;
			if (feedbackBuilder != null && categoryScore != 0f)
			{
				feedbackBuilder.AppendLine("Adding score " + categoryScore + " for category " + categoryScore2.modelCategory + " using " + categoryScore2.ToString() + " total " + (value + categoryScore));
			}
		}
		float num = float.MinValue;
		string result = string.Empty;
		foreach (KeyValuePair<string, float> item in categoryScoresDict)
		{
			if (item.Value > num)
			{
				num = item.Value;
				result = item.Key;
			}
		}
		return result;
	}
}
