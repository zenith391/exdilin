using System.Collections.Generic;
using UnityEngine;

public class FloatTileParameter : NumericHandleTileParameter
{
	internal struct floatAbsoluteBracket
	{
		internal float min;

		internal float max;

		internal float step;
	}

	private float baseStep;

	private float step;

	private float minValue;

	private float maxValue;

	protected float startValue;

	private List<floatAbsoluteBracket> stepAbsoluteBrackets = new List<floatAbsoluteBracket>();

	public float currentValue
	{
		get
		{
			return (float)presenterSign * (float)base.objectValue;
		}
		set
		{
			base.objectValue = (float)presenterSign * value;
		}
	}

	public FloatTileParameter(float minValue, float maxValue, float step, float sensitivity = 25f, int parameterIndex = 0, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "")
		: base(sensitivity, parameterIndex, 1, onlyShowPositive, prefixValueString, postfixValueString)
	{
		baseStep = step;
		this.step = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}

	protected override float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		float num = (float)goalTile.gaf.Args[base.parameterIndex] - (float)thisTile.gaf.Args[base.parameterIndex];
		if (base.settings.floatSliderMode == FloatTileParameterMode.RelativeBrackets)
		{
			return (float)presenterSign * GetRelativeBracketScreenOffset(num) / sensitivity;
		}
		return (float)presenterSign * (num / (step * sensitivity));
	}

	private FloatSliderRelativeBracket GetRelativeBracket(float offset)
	{
		FloatSliderRelativeBracket floatSliderRelativeBracket = null;
		FloatSliderRelativeBracket result = null;
		float num = float.MaxValue;
		FloatSliderRelativeBracket[] floatSliderRelativeBrackets = base.settings.floatSliderRelativeBrackets;
		foreach (FloatSliderRelativeBracket floatSliderRelativeBracket2 in floatSliderRelativeBrackets)
		{
			if (offset >= floatSliderRelativeBracket2.minDistance && offset <= floatSliderRelativeBracket2.maxDistance)
			{
				floatSliderRelativeBracket = floatSliderRelativeBracket2;
				break;
			}
			float num2 = Mathf.Min(Mathf.Abs(offset - floatSliderRelativeBracket2.minDistance), Mathf.Abs(offset - floatSliderRelativeBracket2.maxDistance));
			if (num2 < num)
			{
				result = floatSliderRelativeBracket2;
				num = num2;
			}
		}
		if (floatSliderRelativeBracket == null)
		{
			return result;
		}
		return floatSliderRelativeBracket;
	}

	protected float GetRelativeBracketScreenOffset(float valueOffset)
	{
		float num = Mathf.Abs(valueOffset);
		FloatSliderRelativeBracket floatSliderRelativeBracket = null;
		FloatSliderRelativeBracket floatSliderRelativeBracket2 = null;
		float num2 = float.MaxValue;
		FloatSliderRelativeBracket[] floatSliderRelativeBrackets = base.settings.floatSliderRelativeBrackets;
		foreach (FloatSliderRelativeBracket floatSliderRelativeBracket3 in floatSliderRelativeBrackets)
		{
			if (num >= floatSliderRelativeBracket3.minValue && num <= floatSliderRelativeBracket3.maxValue)
			{
				floatSliderRelativeBracket = floatSliderRelativeBracket3;
				break;
			}
			float num3 = Mathf.Min(Mathf.Abs(num - floatSliderRelativeBracket3.minValue), Mathf.Abs(num - floatSliderRelativeBracket3.maxValue));
			if (num3 < num2)
			{
				floatSliderRelativeBracket2 = floatSliderRelativeBracket3;
				num2 = num3;
			}
		}
		FloatSliderRelativeBracket floatSliderRelativeBracket4 = ((floatSliderRelativeBracket != null) ? floatSliderRelativeBracket : floatSliderRelativeBracket2);
		if (floatSliderRelativeBracket4 != null)
		{
			float num4 = floatSliderRelativeBracket4.maxDistance - floatSliderRelativeBracket4.minDistance;
			float num5 = floatSliderRelativeBracket4.maxValue - floatSliderRelativeBracket4.minValue;
			float num6 = (num - floatSliderRelativeBracket4.minValue) / num5;
			return Mathf.Sign(valueOffset) * (num6 * num4 + floatSliderRelativeBracket4.minDistance);
		}
		BWLog.Info("Could not find bracket for value offset " + valueOffset);
		return 0f;
	}

	protected override bool UpdateValue()
	{
		bool result = false;
		float num = (screenPos.x - startPositionScreen.x) * sensitivity;
		switch (base.settings.floatSliderMode)
		{
		case FloatTileParameterMode.Plain:
		case FloatTileParameterMode.AbsoluteBrackets:
		{
			float num11 = Mathf.Clamp(startValue + num * step, minValue, maxValue);
			float num12 = step * 0.4f;
			if (Mathf.Abs(num11 - currentValue) >= num12)
			{
				float num13 = currentValue;
				currentValue += step * (float)Mathf.RoundToInt((num11 - currentValue) / step);
				currentValue = step * Mathf.Round(num11 / step);
				result = Mathf.Abs(num13 - currentValue) > num12;
			}
			break;
		}
		case FloatTileParameterMode.RelativeBrackets:
		{
			float num2 = Mathf.Sign(num);
			float num3 = Mathf.Abs(num);
			FloatSliderRelativeBracket relativeBracket = GetRelativeBracket(num3);
			if (relativeBracket != null)
			{
				float num4 = relativeBracket.maxValue - relativeBracket.minValue;
				float num5 = relativeBracket.maxDistance - relativeBracket.minDistance;
				float num6 = (num3 - relativeBracket.minDistance) / num5;
				int num7 = Mathf.RoundToInt(num4 / relativeBracket.step);
				int num8 = Mathf.RoundToInt(num6 * (float)num7);
				float num9 = num2 * (relativeBracket.step * (float)num8 + relativeBracket.minValue);
				float num10 = Mathf.Clamp(startValue + num9, minValue, maxValue);
				if (Mathf.Abs(num10 - currentValue) >= relativeBracket.step)
				{
					currentValue = num10;
					result = true;
				}
			}
			else
			{
				BWLog.Info("No bracket found for offset " + num);
			}
			break;
		}
		}
		return result;
	}

	protected override void InitializeStartValue()
	{
		startValue = currentValue;
	}

	protected override void SetValueAndStep(Tile tile)
	{
		float num = (float)tile.gaf.Args[base.parameterIndex];
		if (onlyShowPositive)
		{
			presenterSign = Mathf.RoundToInt(Mathf.Sign(num));
		}
		currentValue = (float)presenterSign * num;
	}

	protected override void SetStep()
	{
		step = AbsoluteBracketedStep(currentValue);
	}

	protected override bool ValueAtMaxOrMore()
	{
		return currentValue >= maxValue;
	}

	protected override bool ValueAtMinOrLess()
	{
		return currentValue <= minValue;
	}

	private float AbsoluteBracketedStep(float v)
	{
		if (stepAbsoluteBrackets.Count == 0)
		{
			return baseStep;
		}
		for (int i = 0; i < stepAbsoluteBrackets.Count; i++)
		{
			if (v >= stepAbsoluteBrackets[i].min && v <= stepAbsoluteBrackets[i].max)
			{
				return stepAbsoluteBrackets[i].step;
			}
		}
		return baseStep;
	}

	public void SetStepBracket(float min, float max, float stepInBracket)
	{
		stepAbsoluteBrackets.Add(new floatAbsoluteBracket
		{
			min = min,
			max = max,
			step = stepInBracket
		});
	}

	public override string ValueAsString()
	{
		float f = step - Mathf.Floor(step);
		bool flag = Mathf.Abs(f) > 0.09f;
		bool flag2 = !flag && Mathf.Abs(f) > 0.009f;
		bool flag3 = flag || flag2;
		float num = currentValue * base.settings.floatPresentMultiplier;
		return string.Concat(str1: (step >= 1f && !flag3) ? num.ToString("f0") : ((!(step < 0.1f || flag2)) ? num.ToString("f1") : num.ToString("f2")), str0: prefixValueString, str2: postfixValueString);
	}
}
