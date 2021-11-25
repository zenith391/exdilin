using System;
using UnityEngine;

// Token: 0x0200020F RID: 527
public class LaserMetaData : MonoBehaviour
{
	// Token: 0x040015D3 RID: 5587
	public Vector3 exitOffset;

	// Token: 0x040015D4 RID: 5588
	public Vector3 projectileExitOffset;

	// Token: 0x040015D5 RID: 5589
	public string loopSfx = "Laser Beam Loop";

	// Token: 0x040015D6 RID: 5590
	public string pulseSfx = "Laser Pulse";

	// Token: 0x040015D7 RID: 5591
	public string projectileSfx = "Gun Fire";

	// Token: 0x040015D8 RID: 5592
	public float beamSizeMultiplier = 1f;

	// Token: 0x040015D9 RID: 5593
	public float pulseSizeMultiplier = 1f;

	// Token: 0x040015DA RID: 5594
	public float projectileSizeMultiplier = 1f;

	// Token: 0x040015DB RID: 5595
	public float pulseFrequencyMultiplier = 1f;

	// Token: 0x040015DC RID: 5596
	public float pulseLengthMultiplier = 1f;

	// Token: 0x040015DD RID: 5597
	public bool fixLaserColor;

	// Token: 0x040015DE RID: 5598
	public Color laserColor;

	// Token: 0x040015DF RID: 5599
	public float projectileRecoilForce = 1f;

	// Token: 0x040015E0 RID: 5600
	public float projectileHitForce = 2f;

	// Token: 0x040015E1 RID: 5601
	public float projectileMaxRecoilPerMass = 0.3f;
}
