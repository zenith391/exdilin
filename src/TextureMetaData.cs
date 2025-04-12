using System;
using UnityEngine;

// Token: 0x0200022E RID: 558
[Serializable]
public class TextureMetaData
{
	// Token: 0x0400164E RID: 5710
	public string name;

	// Token: 0x0400164F RID: 5711
	public Vector3 preferredSize;

	// Token: 0x04001650 RID: 5712
	public bool fourSidesIgnoreRightLeft = true;

	// Token: 0x04001651 RID: 5713
	public bool twoSidesMirror = true;

	// Token: 0x04001652 RID: 5714
	public float mipMapBias;

	// Token: 0x04001653 RID: 5715
	public TextureApplicationChangeRule[] applicationRules;
}
