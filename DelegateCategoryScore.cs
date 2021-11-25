using System;

// Token: 0x020001C5 RID: 453
public class DelegateCategoryScore : ModelCategoryScore
{
	// Token: 0x06001820 RID: 6176 RVA: 0x000AAD18 File Offset: 0x000A9118
	public override float GetCategoryScore(ModelCategorizerContext context)
	{
		return this.ScoreFunction(context);
	}

	// Token: 0x0400130D RID: 4877
	public Func<ModelCategorizerContext, float> ScoreFunction;
}
