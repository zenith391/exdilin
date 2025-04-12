using System;
using UnityEngine;

// Token: 0x02000128 RID: 296
public class PasteModelAnimationCommand : ModelAnimationCommand
{
	// Token: 0x0600140B RID: 5131 RVA: 0x0008BFCF File Offset: 0x0008A3CF
	protected override Vector3 GetStartPos()
	{
		return base.GetUpperRightWorldPos();
	}

	// Token: 0x0600140C RID: 5132 RVA: 0x0008BFD7 File Offset: 0x0008A3D7
	protected override Vector3 GetTargetPos()
	{
		return base.GetBlockCenter();
	}

	// Token: 0x0600140D RID: 5133 RVA: 0x0008BFDF File Offset: 0x0008A3DF
	protected override float GetScaleFromFraction(float fraction)
	{
		return 2f - fraction;
	}
}
