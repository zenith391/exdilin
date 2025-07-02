using UnityEngine;

public class ColorTileParameter : NumericHandleTileParameter
{
	private int step = 1;

	private int minValue;

	private int maxValue;

	protected int startValue;

	private string oldName;

	private Shader vertexColorShader;

	public string storeName = "Light Blue";

	public int currentValue
	{
		get
		{
			return presenterSign * (int)base.objectValue;
		}
		set
		{
			base.objectValue = presenterSign * value;
			UpdateTileNameAndColor();
		}
	}

	public ColorTileParameter(int minValue, int maxValue, int step, float sensitivity = 25f, int parameterIndex = 0, int subParameterCount = 2)
		: base(sensitivity, parameterIndex, 1, onlyShowPositive: true, string.Empty, string.Empty)
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
	}

	protected override bool UpdateValue()
	{
		bool result = false;
		int num = Mathf.RoundToInt(Mathf.Clamp((float)startValue + (screenPos.x - startPositionScreen.x) * sensitivity * (float)step, minValue, maxValue));
		if (Mathf.Abs(num - currentValue) >= step)
		{
			currentValue = Mathf.RoundToInt((float)currentValue + (float)step * Mathf.Sign(num - currentValue));
			result = true;
		}
		UpdateTileNameAndColor();
		return result;
	}

	protected override void InitializeStartValue()
	{
		startValue = currentValue;
		UpdateTileNameAndColor();
	}

	protected override void SetValueAndStep(Tile tile)
	{
		object obj = tile.gaf.Args[base.parameterIndex];
		currentValue = (int)obj;
		UpdateTileNameAndColor();
	}

	private void UpdateTileNameAndColor()
	{
		if (tile.gaf.Args.Length <= base.parameterIndex)
		{
			BWLog.Info("UpdateTileNameAndColor not enough args");
			return;
		}
		int num = (int)tile.gaf.Args[base.parameterIndex];
		string colorName = base.settings.stringSliderColor[num].colorName;
		if (!(colorName != oldName))
		{
			return;
		}
		oldName = colorName;
		storeName = base.settings.stringSliderColor[num].userFacingColorName;
		object[] args = tile.gaf.Args;
		if (args.Length > base.parameterIndex + 1)
		{
			args[base.parameterIndex + 1] = colorName;
			tile.gaf = new GAF(tile.gaf.Predicate, args);
			Color[] value = new Color[2]
			{
				Color.gray,
				Color.gray
			};
			Blocksworld.colorDefinitions.TryGetValue(colorName, out value);
			if (value != null)
			{
				tile.SetTileBackgroundColor(value[0]);
			}
		}
	}

	protected override bool ValueAtMaxOrMore()
	{
		return currentValue >= maxValue;
	}

	protected override bool ValueAtMinOrLess()
	{
		return currentValue <= minValue;
	}

	public override void ApplyTileParameterUI(Tile tile)
	{
		base.ApplyTileParameterUI(tile);
		UpdateTileNameAndColor();
	}

	protected override HudMeshStyle GetHudMeshStyle()
	{
		if (style == null)
		{
			style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return style;
	}

	public override string ValueAsString()
	{
		return storeName;
	}
}
