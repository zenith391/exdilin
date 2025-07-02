using System.Collections.Generic;
using UnityEngine;

public class IntTileParameter : NumericHandleTileParameter
{
	internal struct intBracket
	{
		internal int min;

		internal int max;

		internal int step;
	}

	protected int baseStep = 1;

	protected int step = 1;

	protected int minValue;

	protected int maxValue;

	protected int startValue;

	public BidiIntFloatConverter converter;

	public BidiIntStringConverter stringConverter;

	private List<intBracket> stepBrackets = new List<intBracket>();

	public int currentValue
	{
		get
		{
			if (converter != null)
			{
				return presenterSign * converter.floatToInt((float)base.objectValue);
			}
			if (stringConverter != null)
			{
				return (int)base.objectValue;
			}
			return presenterSign * (int)base.objectValue;
		}
		set
		{
			if (converter != null)
			{
				base.objectValue = converter.intToFloat(presenterSign * value);
			}
			else if (stringConverter != null)
			{
				base.objectValue = value;
			}
			else
			{
				base.objectValue = presenterSign * value;
			}
		}
	}

	public IntTileParameter(int minValue, int maxValue, int step, float sensitivity = 25f, int parameterIndex = 0, BidiIntFloatConverter converter = null, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "")
		: base(sensitivity, parameterIndex, 1, onlyShowPositive, prefixValueString, postfixValueString)
	{
		baseStep = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.converter = converter;
	}

	public IntTileParameter(int minValue, int maxValue, int step, float sensitivity, int parameterIndex, BidiIntStringConverter stringConverter)
		: base(sensitivity, parameterIndex, 1, onlyShowPositive: false, string.Empty, string.Empty)
	{
		baseStep = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.stringConverter = stringConverter;
	}

	private void DebugConverter()
	{
		if (converter is PiecewiseLinearIntFloatConverter)
		{
			PiecewiseLinearIntFloatConverter piecewiseLinearIntFloatConverter = (PiecewiseLinearIntFloatConverter)converter;
			BWLog.Info("Piecewise converter found for " + tile.gaf.Predicate.Name);
			BWLog.Info("Int to float: ");
			float num = float.MaxValue;
			float num2 = float.MinValue;
			for (int i = minValue; i <= maxValue; i++)
			{
				float num3 = piecewiseLinearIntFloatConverter.intToFloat(i);
				num = Mathf.Min(num, num3);
				num2 = Mathf.Max(num2, num3);
				BWLog.Info("  " + i + " : " + num3);
			}
			BWLog.Info("Float to int: ");
			for (float num4 = num; num4 <= num2; num4 += 0.1f)
			{
				BWLog.Info("  " + num4 + " : " + piecewiseLinearIntFloatConverter.floatToInt(num4));
			}
		}
	}

	protected float ConvertToFloat(object o)
	{
		if (o is int)
		{
			return (int)o;
		}
		if (o is float)
		{
			return (float)o;
		}
		BWLog.Info("Can not convert " + o?.ToString() + " to float");
		return minValue;
	}

	protected override float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		float num = ConvertToFloat(thisTile.gaf.Args[base.parameterIndex]);
		float num2 = ConvertToFloat(goalTile.gaf.Args[base.parameterIndex]);
		if (converter != null)
		{
			int num3 = converter.floatToInt(num);
			int num4 = converter.floatToInt(num2);
			return (float)presenterSign * ((float)(num4 - num3) / ((float)step * sensitivity));
		}
		if (stringConverter != null)
		{
			return 0f;
		}
		float num5 = num2 - num;
		return (float)presenterSign * (num5 / ((float)step * sensitivity));
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
		return result;
	}

	protected override void InitializeStartValue()
	{
		startValue = currentValue;
	}

	protected override void SetValueAndStep(Tile tile)
	{
		object obj = tile.gaf.Args[base.parameterIndex];
		if (obj is int)
		{
			currentValue = (int)obj;
		}
		else if (obj is float num)
		{
			if (onlyShowPositive)
			{
				presenterSign = Mathf.RoundToInt(Mathf.Sign(num));
			}
			if (converter != null)
			{
				currentValue = presenterSign * converter.floatToInt(num);
			}
			else
			{
				currentValue = (int)num;
			}
		}
		else if (obj is string)
		{
			if (stringConverter != null)
			{
				currentValue = stringConverter.IntValue((string)obj);
			}
			else
			{
				currentValue = 0;
			}
		}
	}

	protected override void SetStep()
	{
		step = BracketedStep(currentValue);
	}

	protected override bool ValueAtMaxOrMore()
	{
		return currentValue >= maxValue;
	}

	protected override bool ValueAtMinOrLess()
	{
		return currentValue <= minValue;
	}

	private int BracketedStep(int v)
	{
		if (stepBrackets.Count == 0)
		{
			return baseStep;
		}
		for (int i = 0; i < stepBrackets.Count; i++)
		{
			if (v >= stepBrackets[i].min && v <= stepBrackets[i].max)
			{
				return stepBrackets[i].step;
			}
		}
		return baseStep;
	}

	public void SetStepBracket(int min, int max, int stepInBracket)
	{
		stepBrackets.Add(new intBracket
		{
			min = min,
			max = max,
			step = stepInBracket
		});
	}

	public override string ValueAsString()
	{
		if (stringConverter != null)
		{
			return stringConverter.StringValue(currentValue);
		}
		return prefixValueString + currentValue + postfixValueString;
	}
}
