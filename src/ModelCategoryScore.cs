using System;

[Serializable]
public class ModelCategoryScore
{
	public string modelCategory;

	public virtual float GetCategoryScore(ModelCategorizerContext context)
	{
		return 0f;
	}
}
