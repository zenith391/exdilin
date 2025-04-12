using System;
using Blocks;

// Token: 0x0200023E RID: 574
public class FogParametersVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AFA RID: 6906 RVA: 0x000C5B24 File Offset: 0x000C3F24
	public override void Update()
	{
		if (string.IsNullOrEmpty(this._defaultFogColor))
		{
			this._defaultFogColor = WorldEnvironmentManager.GetFogPaint();
		}
		if (this._defaultFogStart < 0f)
		{
			this._defaultFogStart = Blocksworld.fogStart;
			this._defaultFogEnd = Blocksworld.fogEnd;
		}
		Tile selectedTile = Blocksworld.bw.tileParameterEditor.selectedTile;
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		if (selectedTile.gaf.Predicate.Name == "Master.FogColorTo")
		{
			Blocksworld.worldSky.SetFogColor((string)selectedTile.gaf.Args[1]);
		}
		else
		{
			float num = Blocksworld.fogStart;
			float num2 = Blocksworld.fogEnd;
			if (selectedTile.gaf.Predicate.Name == "Master.FogDensityTo")
			{
				float num3 = (float)parameter.objectValue;
				WorldEnvironmentManager.OverrideFogDensityTemporarily(num3);
				num2 = WorldEnvironmentManager.ComputeFogEnd(num, num3);
			}
			else
			{
				float num4 = (float)parameter.objectValue;
				num2 += num4 - num;
				num = num4;
				WorldEnvironmentManager.OverrideFogStartTemporarily(num);
			}
			Blocksworld.bw.SetFog(num, num2);
		}
	}

	// Token: 0x06001AFB RID: 6907 RVA: 0x000C5C48 File Offset: 0x000C4048
	public override void Destroy()
	{
		if (!string.IsNullOrEmpty(this._defaultFogColor))
		{
			Blocksworld.worldSky.SetFogColor(this._defaultFogColor);
		}
		if (this._defaultFogStart >= 0f && this._defaultFogEnd >= 0f)
		{
			Blocksworld.bw.SetFog(this._defaultFogStart, this._defaultFogEnd);
		}
		base.Destroy();
	}

	// Token: 0x040016A1 RID: 5793
	private string _defaultFogColor = string.Empty;

	// Token: 0x040016A2 RID: 5794
	private float _defaultFogStart = -1f;

	// Token: 0x040016A3 RID: 5795
	private float _defaultFogEnd = -1f;
}
