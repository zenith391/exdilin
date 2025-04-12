using System;

// Token: 0x020001CC RID: 460
[Serializable]
public class GeneralModelCategoryScores
{
	// Token: 0x04001349 RID: 4937
	public string modelCategory;

	// Token: 0x0400134A RID: 4938
	public GeneralModelCategoryScore[] scores;

	// Token: 0x0400134B RID: 4939
	[NonSerialized]
	public bool foldout;
}
