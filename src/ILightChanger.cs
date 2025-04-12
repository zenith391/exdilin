using System;
using UnityEngine;

// Token: 0x0200019C RID: 412
public interface ILightChanger
{
	// Token: 0x060016F3 RID: 5875
	Color GetDynamicalLightTint();

	// Token: 0x060016F4 RID: 5876
	float GetFogMultiplier();

	// Token: 0x060016F5 RID: 5877
	Color GetFogColorOverride();

	// Token: 0x060016F6 RID: 5878
	float GetLightIntensityMultiplier();
}
