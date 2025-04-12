using System;

// Token: 0x0200023D RID: 573
public class SunIntensityVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AF7 RID: 6903 RVA: 0x000C5AA8 File Offset: 0x000C3EA8
	public override void Update()
	{
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		float sunIntensity = 0.01f * (float)parameter.objectValue;
		Blocksworld.worldSky.SetSunIntensity(sunIntensity);
	}

	// Token: 0x06001AF8 RID: 6904 RVA: 0x000C5AE2 File Offset: 0x000C3EE2
	public override void Destroy()
	{
		Blocksworld.worldSky.SetSunIntensity(1f);
		base.Destroy();
	}
}
