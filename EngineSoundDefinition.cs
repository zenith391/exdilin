using System;

// Token: 0x020001FF RID: 511
[Serializable]
public class EngineSoundDefinition
{
	// Token: 0x040015A6 RID: 5542
	public string name = "Default";

	// Token: 0x040015A7 RID: 5543
	public string loopSFXName = "Wheel Engine Loop Default";

	// Token: 0x040015A8 RID: 5544
	public float baseVolume = 0.6f;

	// Token: 0x040015A9 RID: 5545
	public float basePitch = 0.7f;

	// Token: 0x040015AA RID: 5546
	public float RPMIncreaseSpeed = 0.5f;

	// Token: 0x040015AB RID: 5547
	public float RPMDecaySpeed = 0.9f;

	// Token: 0x040015AC RID: 5548
	public float RPMVolumeMod = 0.2f;

	// Token: 0x040015AD RID: 5549
	public float RPMPitchMod = 0.01f;

	// Token: 0x040015AE RID: 5550
	public float maximumRPM = 200f;

	// Token: 0x040015AF RID: 5551
	public float wheelSpinThreshold = 0.1f;

	// Token: 0x040015B0 RID: 5552
	public float wheelSpinMax = 20f;

	// Token: 0x040015B1 RID: 5553
	public float wheelSpinVolumeMod;

	// Token: 0x040015B2 RID: 5554
	public float wheelSpinPitchMod;
}
