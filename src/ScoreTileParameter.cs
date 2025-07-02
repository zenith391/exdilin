using UnityEngine;

public class ScoreTileParameter : IntTileParameter
{
	private int[] steps;

	public ScoreTileParameter(int parameterIndex = 0)
		: base(-10000000, 10000000, 1, 50f, parameterIndex, null, onlyShowPositive: false, string.Empty, string.Empty)
	{
	}

	protected override HudMeshStyle GetHudMeshStyle()
	{
		int num = Mathf.Abs(base.currentValue);
		if (num > 9999)
		{
			style = HudMeshOnGUI.dataSource.intParamStyle;
		}
		else
		{
			style = HudMeshOnGUI.dataSource.timeParamStyle;
		}
		return style;
	}

	private int[] GetSteps()
	{
		if (steps == null)
		{
			int num = 10;
			steps = new int[num];
			int num2 = 1;
			for (int i = 0; i < steps.Length; i++)
			{
				steps[i] = num2;
				num2 *= 10;
			}
		}
		return steps;
	}

	protected override bool UpdateValue()
	{
		bool result = false;
		float f = screenPos.x - startPositionScreen.x;
		float num = Mathf.Sign(f);
		float num2 = Mathf.Abs(f);
		int[] array = GetSteps();
		float num3 = 0f;
		float num4 = 80f;
		int num5 = ((num2 <= 0f) ? 1 : array[array.Length - 1]);
		for (int i = 0; i < array.Length; i++)
		{
			float num6 = (float)i * num4;
			if (num2 <= num6)
			{
				break;
			}
			num3 = Mathf.Clamp(num2 - num6, 0f, num4);
			num5 = array[i];
		}
		int num7 = base.currentValue;
		int num8 = Mathf.RoundToInt(Mathf.Abs(num3 / num4) * 10f);
		int value = startValue + Mathf.RoundToInt(num * (float)(((num5 != 1) ? num5 : 0) + Mathf.FloorToInt(num8 * num5)));
		value = Mathf.Clamp(value, minValue, maxValue);
		if (value != num7)
		{
			if (startValue != 0 && Mathf.Sign(value) != Mathf.Sign(startValue))
			{
				value = 0;
			}
			base.currentValue = value;
			result = true;
		}
		return result;
	}

	public override void OnHudMesh()
	{
		if (tile != null && base.useDoubleWidth && tile.IsShowing())
		{
			DisplayDescriptor();
			HudMeshStyle hudMeshStyle = style;
			HudMeshStyle hudMeshStyle2 = GetHudMeshStyle();
			if (hudMeshStyle != hudMeshStyle2)
			{
				label = null;
			}
			HudMeshOnGUI.Label(ref label, GetRightSideRect(), ValueAsString(), hudMeshStyle2);
		}
	}
}
