using System;
using UnityEngine;

// Token: 0x0200024C RID: 588
public class VolcanoSettings : MonoBehaviour
{
	// Token: 0x0400170D RID: 5901
	public Vector3 relativeOffset = new Vector3(-0.05f, 0.3f, -0.12f);

	// Token: 0x0400170E RID: 5902
	public float smokePerFrame = 0.07f;

	// Token: 0x0400170F RID: 5903
	public float smokeLifetimeRandomFrom = 0.4f;

	// Token: 0x04001710 RID: 5904
	public float smokeLifetimeRandomTo = 0.5f;

	// Token: 0x04001711 RID: 5905
	public float smokeAngularVelocityRandomFrom = -200f;

	// Token: 0x04001712 RID: 5906
	public float smokeAngularVelocityRandomTo = 200f;

	// Token: 0x04001713 RID: 5907
	public float smokeRotationRandomFrom;

	// Token: 0x04001714 RID: 5908
	public float smokeRotationRandomTo = 360f;

	// Token: 0x04001715 RID: 5909
	public float smokeSizeRandomFrom = 0.4f;

	// Token: 0x04001716 RID: 5910
	public float smokeSizeRandomTo = 0.5f;

	// Token: 0x04001717 RID: 5911
	public float smokeSizeBias = 0.2f;

	// Token: 0x04001718 RID: 5912
	public float firePerFrame = 0.012f;

	// Token: 0x04001719 RID: 5913
	public float fireSizeRandomFrom = 0.04f;

	// Token: 0x0400171A RID: 5914
	public float fireSizeRandomTo = 0.07f;

	// Token: 0x0400171B RID: 5915
	public float fireLifetimeRandomFrom = 0.5f;

	// Token: 0x0400171C RID: 5916
	public float fireLifetimeRandomTo = 0.75f;
}
