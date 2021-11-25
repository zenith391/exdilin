using System;
using UnityEngine;

// Token: 0x0200014D RID: 333
public class ColorTileParameter : NumericHandleTileParameter
{
	// Token: 0x0600149B RID: 5275 RVA: 0x00091C09 File Offset: 0x00090009
	public ColorTileParameter(int minValue, int maxValue, int step, float sensitivity = 25f, int parameterIndex = 0, int subParameterCount = 2) : base(sensitivity, parameterIndex, 1, true, string.Empty, string.Empty)
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
	}

	// Token: 0x17000056 RID: 86
	// (get) Token: 0x0600149C RID: 5276 RVA: 0x00091C41 File Offset: 0x00090041
	// (set) Token: 0x0600149D RID: 5277 RVA: 0x00091C55 File Offset: 0x00090055
	public int currentValue
	{
		get
		{
			return this.presenterSign * (int)base.objectValue;
		}
		set
		{
			base.objectValue = this.presenterSign * value;
			this.UpdateTileNameAndColor();
		}
	}

	// Token: 0x0600149E RID: 5278 RVA: 0x00091C70 File Offset: 0x00090070
	protected override bool UpdateValue()
	{
		bool result = false;
		int num = Mathf.RoundToInt(Mathf.Clamp((float)this.startValue + (this.screenPos.x - this.startPositionScreen.x) * this.sensitivity * (float)this.step, (float)this.minValue, (float)this.maxValue));
		if (Mathf.Abs(num - this.currentValue) >= this.step)
		{
			this.currentValue = Mathf.RoundToInt((float)this.currentValue + (float)this.step * Mathf.Sign((float)(num - this.currentValue)));
			result = true;
		}
		this.UpdateTileNameAndColor();
		return result;
	}

	// Token: 0x0600149F RID: 5279 RVA: 0x00091D10 File Offset: 0x00090110
	protected override void InitializeStartValue()
	{
		this.startValue = this.currentValue;
		this.UpdateTileNameAndColor();
	}

	// Token: 0x060014A0 RID: 5280 RVA: 0x00091D24 File Offset: 0x00090124
	protected override void SetValueAndStep(Tile tile)
	{
		object obj = tile.gaf.Args[base.parameterIndex];
		this.currentValue = (int)obj;
		this.UpdateTileNameAndColor();
	}

	// Token: 0x060014A1 RID: 5281 RVA: 0x00091D58 File Offset: 0x00090158
	private void UpdateTileNameAndColor()
	{
		if (this.tile.gaf.Args.Length <= base.parameterIndex)
		{
			BWLog.Info("UpdateTileNameAndColor not enough args");
			return;
		}
		int num = (int)this.tile.gaf.Args[base.parameterIndex];
		string colorName = base.settings.stringSliderColor[num].colorName;
		if (colorName != this.oldName)
		{
			this.oldName = colorName;
			this.storeName = base.settings.stringSliderColor[num].userFacingColorName;
			object[] args = this.tile.gaf.Args;
			if (args.Length > base.parameterIndex + 1)
			{
				args[base.parameterIndex + 1] = colorName;
				this.tile.gaf = new GAF(this.tile.gaf.Predicate, args);
				Color[] array = new Color[]
				{
					Color.gray,
					Color.gray
				};
				Blocksworld.colorDefinitions.TryGetValue(colorName, out array);
				if (array != null)
				{
					this.tile.SetTileBackgroundColor(array[0]);
				}
			}
		}
	}

	// Token: 0x060014A2 RID: 5282 RVA: 0x00091E8E File Offset: 0x0009028E
	protected override bool ValueAtMaxOrMore()
	{
		return this.currentValue >= this.maxValue;
	}

	// Token: 0x060014A3 RID: 5283 RVA: 0x00091EA1 File Offset: 0x000902A1
	protected override bool ValueAtMinOrLess()
	{
		return this.currentValue <= this.minValue;
	}

	// Token: 0x060014A4 RID: 5284 RVA: 0x00091EB4 File Offset: 0x000902B4
	public override void ApplyTileParameterUI(Tile tile)
	{
		base.ApplyTileParameterUI(tile);
		this.UpdateTileNameAndColor();
	}

	// Token: 0x060014A5 RID: 5285 RVA: 0x00091EC3 File Offset: 0x000902C3
	protected override HudMeshStyle GetHudMeshStyle()
	{
		if (this.style == null)
		{
			this.style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return this.style;
	}

	// Token: 0x060014A6 RID: 5286 RVA: 0x00091EE6 File Offset: 0x000902E6
	public override string ValueAsString()
	{
		return this.storeName;
	}

	// Token: 0x04001043 RID: 4163
	private int step = 1;

	// Token: 0x04001044 RID: 4164
	private int minValue;

	// Token: 0x04001045 RID: 4165
	private int maxValue;

	// Token: 0x04001046 RID: 4166
	protected int startValue;

	// Token: 0x04001047 RID: 4167
	private string oldName;

	// Token: 0x04001048 RID: 4168
	private Shader vertexColorShader;

	// Token: 0x04001049 RID: 4169
	public string storeName = "Light Blue";
}
