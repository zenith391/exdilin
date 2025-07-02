using System;

public class DelegateCategoryScore : ModelCategoryScore
{
	public Func<ModelCategorizerContext, float> ScoreFunction;

	public override float GetCategoryScore(ModelCategorizerContext context)
	{
		return ScoreFunction(context);
	}
}
