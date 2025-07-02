using Blocks;

public class FogParametersVisualizer : ParameterValueVisualizer
{
	private string _defaultFogColor = string.Empty;

	private float _defaultFogStart = -1f;

	private float _defaultFogEnd = -1f;

	public override void Update()
	{
		if (string.IsNullOrEmpty(_defaultFogColor))
		{
			_defaultFogColor = WorldEnvironmentManager.GetFogPaint();
		}
		if (_defaultFogStart < 0f)
		{
			_defaultFogStart = Blocksworld.fogStart;
			_defaultFogEnd = Blocksworld.fogEnd;
		}
		Tile selectedTile = Blocksworld.bw.tileParameterEditor.selectedTile;
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		if (selectedTile.gaf.Predicate.Name == "Master.FogColorTo")
		{
			Blocksworld.worldSky.SetFogColor((string)selectedTile.gaf.Args[1]);
			return;
		}
		float num = Blocksworld.fogStart;
		float fogEnd = Blocksworld.fogEnd;
		if (selectedTile.gaf.Predicate.Name == "Master.FogDensityTo")
		{
			float num2 = (float)parameter.objectValue;
			WorldEnvironmentManager.OverrideFogDensityTemporarily(num2);
			fogEnd = WorldEnvironmentManager.ComputeFogEnd(num, num2);
		}
		else
		{
			float num3 = (float)parameter.objectValue;
			fogEnd += num3 - num;
			num = num3;
			WorldEnvironmentManager.OverrideFogStartTemporarily(num);
		}
		Blocksworld.bw.SetFog(num, fogEnd);
	}

	public override void Destroy()
	{
		if (!string.IsNullOrEmpty(_defaultFogColor))
		{
			Blocksworld.worldSky.SetFogColor(_defaultFogColor);
		}
		if (_defaultFogStart >= 0f && _defaultFogEnd >= 0f)
		{
			Blocksworld.bw.SetFog(_defaultFogStart, _defaultFogEnd);
		}
		base.Destroy();
	}
}
