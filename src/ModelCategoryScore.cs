using System;

// Token: 0x020001D0 RID: 464
[Serializable]
public class ModelCategoryScore
{
	// Token: 0x0600185B RID: 6235 RVA: 0x000AAD09 File Offset: 0x000A9109
	public virtual float GetCategoryScore(ModelCategorizerContext context)
	{
		return 0f;
	}

	// Token: 0x04001352 RID: 4946
	public string modelCategory;
}
