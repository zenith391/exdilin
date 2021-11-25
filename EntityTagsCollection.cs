using System;
using UnityEngine;

// Token: 0x02000201 RID: 513
[Serializable]
public class EntityTagsCollection : MonoBehaviour
{
	// Token: 0x040015B5 RID: 5557
	public string[] allBlockTags;

	// Token: 0x040015B6 RID: 5558
	public string[] allPredicateTags;

	// Token: 0x040015B7 RID: 5559
	public string[] allTextureTags;

	// Token: 0x040015B8 RID: 5560
	public BlockTags[] blockTags;

	// Token: 0x040015B9 RID: 5561
	public PredicateTags[] predicateTags;

	// Token: 0x040015BA RID: 5562
	public TextureTags[] textureTags;

	// Token: 0x040015BB RID: 5563
	public GeneralModelCategoryScores[] categoryScores;
}
