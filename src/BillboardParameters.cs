using System;
using UnityEngine;

// Token: 0x020001EB RID: 491
public class BillboardParameters : MonoBehaviour
{
	// Token: 0x040013E5 RID: 5093
	public bool snapHorizon;

	// Token: 0x040013E6 RID: 5094
	public bool mirrorInWater = true;

	// Token: 0x040013E7 RID: 5095
	public bool ignoreFog;

	// Token: 0x040013E8 RID: 5096
	public float realDistance = 300f;

	// Token: 0x040013E9 RID: 5097
	public float apparentDistance = 300f;

	// Token: 0x040013EA RID: 5098
	public bool parallax;

	// Token: 0x040013EB RID: 5099
	public Vector3 parallaxMin = Vector3.zero;

	// Token: 0x040013EC RID: 5100
	public Vector3 parallaxMax = Vector3.zero;

	// Token: 0x040013ED RID: 5101
	public bool showLensflare;

	// Token: 0x040013EE RID: 5102
	public float lensFlareBrightness = 0.5f;

	// Token: 0x040013EF RID: 5103
	public Color lensFlareColor = Color.white;

	// Token: 0x040013F0 RID: 5104
	public bool blendLensFlareWithLight = true;
}
