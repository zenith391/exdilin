using System;
using Blocks;
using UnityEngine;

// Token: 0x020001FA RID: 506
public class BodyPartInfo : MonoBehaviour
{
	// Token: 0x04001588 RID: 5512
	public BlocksterBody.Bone bone;

	// Token: 0x04001589 RID: 5513
	public Vector3 offsetFromBone;

	// Token: 0x0400158A RID: 5514
	public BodyPartInfo.ColorGroup colorGroup;

	// Token: 0x0400158B RID: 5515
	public string defaultPaint;

	// Token: 0x0400158C RID: 5516
	public bool canBeTextured;

	// Token: 0x0400158D RID: 5517
	public bool canBeMaterialTextured = true;

	// Token: 0x0400158E RID: 5518
	[HideInInspector]
	public string currentPaint;

	// Token: 0x0400158F RID: 5519
	[HideInInspector]
	public string currentTexture;

	// Token: 0x020001FB RID: 507
	public enum ColorGroup
	{
		// Token: 0x04001591 RID: 5521
		None,
		// Token: 0x04001592 RID: 5522
		SkinFace,
		// Token: 0x04001593 RID: 5523
		SkinRightArm,
		// Token: 0x04001594 RID: 5524
		SkinLeftArm,
		// Token: 0x04001595 RID: 5525
		SkinRightHand,
		// Token: 0x04001596 RID: 5526
		SkinLeftHand,
		// Token: 0x04001597 RID: 5527
		SkinRightLeg,
		// Token: 0x04001598 RID: 5528
		SkinLeftLeg,
		// Token: 0x04001599 RID: 5529
		Shirt,
		// Token: 0x0400159A RID: 5530
		Pant
	}
}
