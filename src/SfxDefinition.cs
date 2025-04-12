using System;

// Token: 0x02000225 RID: 549
[Serializable]
public class SfxDefinition
{
	// Token: 0x04001637 RID: 5687
	public string name;

	// Token: 0x04001638 RID: 5688
	public string label;

	// Token: 0x04001639 RID: 5689
	public float shortestPlayInterval;

	// Token: 0x0400163A RID: 5690
	public bool useLengthForPlayInterval;

	// Token: 0x0400163B RID: 5691
	public float durationalTime;

	// Token: 0x0400163C RID: 5692
	public bool isVox;

	// Token: 0x0400163D RID: 5693
	public bool duckMusicVolume;

	// Token: 0x0400163E RID: 5694
	public float durationalVolume = 1f;
}
