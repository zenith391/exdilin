public class SunIntensityVisualizer : ParameterValueVisualizer
{
	public override void Update()
	{
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		float sunIntensity = 0.01f * (float)parameter.objectValue;
		Blocksworld.worldSky.SetSunIntensity(sunIntensity);
	}

	public override void Destroy()
	{
		Blocksworld.worldSky.SetSunIntensity(1f);
		base.Destroy();
	}
}
