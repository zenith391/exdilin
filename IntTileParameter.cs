using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000151 RID: 337
public class IntTileParameter : NumericHandleTileParameter
{
	// Token: 0x060014CE RID: 5326 RVA: 0x000925A0 File Offset: 0x000909A0
	public IntTileParameter(int minValue, int maxValue, int step, float sensitivity = 25f, int parameterIndex = 0, BidiIntFloatConverter converter = null, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "") : base(sensitivity, parameterIndex, 1, onlyShowPositive, prefixValueString, postfixValueString)
	{
		this.baseStep = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.converter = converter;
	}

	// Token: 0x060014CF RID: 5327 RVA: 0x000925F4 File Offset: 0x000909F4
	public IntTileParameter(int minValue, int maxValue, int step, float sensitivity, int parameterIndex, BidiIntStringConverter stringConverter) : base(sensitivity, parameterIndex, 1, false, string.Empty, string.Empty)
	{
		this.baseStep = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.stringConverter = stringConverter;
	}

	// Token: 0x1700005D RID: 93
	// (get) Token: 0x060014D0 RID: 5328 RVA: 0x00092650 File Offset: 0x00090A50
	// (set) Token: 0x060014D1 RID: 5329 RVA: 0x000926B4 File Offset: 0x00090AB4
	public int currentValue
	{
		get
		{
			if (this.converter != null)
			{
				return this.presenterSign * this.converter.floatToInt((float)base.objectValue);
			}
			if (this.stringConverter != null)
			{
				return (int)base.objectValue;
			}
			return this.presenterSign * (int)base.objectValue;
		}
		set
		{
			if (this.converter != null)
			{
				base.objectValue = this.converter.intToFloat(this.presenterSign * value);
			}
			else if (this.stringConverter != null)
			{
				base.objectValue = value;
			}
			else
			{
				base.objectValue = this.presenterSign * value;
			}
		}
	}

	// Token: 0x060014D2 RID: 5330 RVA: 0x00092724 File Offset: 0x00090B24
	private void DebugConverter()
	{
		if (this.converter is PiecewiseLinearIntFloatConverter)
		{
			PiecewiseLinearIntFloatConverter piecewiseLinearIntFloatConverter = (PiecewiseLinearIntFloatConverter)this.converter;
			BWLog.Info("Piecewise converter found for " + this.tile.gaf.Predicate.Name);
			BWLog.Info("Int to float: ");
			float num = float.MaxValue;
			float num2 = float.MinValue;
			for (int i = this.minValue; i <= this.maxValue; i++)
			{
				float num3 = piecewiseLinearIntFloatConverter.intToFloat(i);
				num = Mathf.Min(num, num3);
				num2 = Mathf.Max(num2, num3);
				BWLog.Info(string.Concat(new object[]
				{
					"  ",
					i,
					" : ",
					num3
				}));
			}
			BWLog.Info("Float to int: ");
			for (float num4 = num; num4 <= num2; num4 += 0.1f)
			{
				BWLog.Info(string.Concat(new object[]
				{
					"  ",
					num4,
					" : ",
					piecewiseLinearIntFloatConverter.floatToInt(num4)
				}));
			}
		}
	}

	// Token: 0x060014D3 RID: 5331 RVA: 0x0009285C File Offset: 0x00090C5C
	protected float ConvertToFloat(object o)
	{
		if (o is int)
		{
			return (float)((int)o);
		}
		if (o is float)
		{
			return (float)o;
		}
		BWLog.Info("Can not convert " + o + " to float");
		return (float)this.minValue;
	}

	// Token: 0x060014D4 RID: 5332 RVA: 0x000928AC File Offset: 0x00090CAC
	protected override float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		float num = this.ConvertToFloat(thisTile.gaf.Args[base.parameterIndex]);
		float num2 = this.ConvertToFloat(goalTile.gaf.Args[base.parameterIndex]);
		if (this.converter != null)
		{
			int num3 = this.converter.floatToInt(num);
			int num4 = this.converter.floatToInt(num2);
			return (float)this.presenterSign * ((float)(num4 - num3) / ((float)this.step * this.sensitivity));
		}
		if (this.stringConverter != null)
		{
			return 0f;
		}
		float num5 = num2 - num;
		return (float)this.presenterSign * (num5 / ((float)this.step * this.sensitivity));
	}

	// Token: 0x060014D5 RID: 5333 RVA: 0x00092968 File Offset: 0x00090D68
	protected override bool UpdateValue()
	{
		bool result = false;
		int num = Mathf.RoundToInt(Mathf.Clamp((float)this.startValue + (this.screenPos.x - this.startPositionScreen.x) * this.sensitivity * (float)this.step, (float)this.minValue, (float)this.maxValue));
		if (Mathf.Abs(num - this.currentValue) >= this.step)
		{
			this.currentValue = Mathf.RoundToInt((float)this.currentValue + (float)this.step * Mathf.Sign((float)(num - this.currentValue)));
			result = true;
		}
		return result;
	}

	// Token: 0x060014D6 RID: 5334 RVA: 0x00092A02 File Offset: 0x00090E02
	protected override void InitializeStartValue()
	{
		this.startValue = this.currentValue;
	}

	// Token: 0x060014D7 RID: 5335 RVA: 0x00092A10 File Offset: 0x00090E10
	protected override void SetValueAndStep(Tile tile)
	{
		object obj = tile.gaf.Args[base.parameterIndex];
		if (obj is int)
		{
			this.currentValue = (int)obj;
		}
		else if (obj is float)
		{
			float num = (float)obj;
			if (this.onlyShowPositive)
			{
				this.presenterSign = Mathf.RoundToInt(Mathf.Sign(num));
			}
			if (this.converter != null)
			{
				this.currentValue = this.presenterSign * this.converter.floatToInt(num);
			}
			else
			{
				this.currentValue = (int)num;
			}
		}
		else if (obj is string)
		{
			if (this.stringConverter != null)
			{
				this.currentValue = this.stringConverter.IntValue((string)obj);
			}
			else
			{
				this.currentValue = 0;
			}
		}
	}

	// Token: 0x060014D8 RID: 5336 RVA: 0x00092AEE File Offset: 0x00090EEE
	protected override void SetStep()
	{
		this.step = this.BracketedStep(this.currentValue);
	}

	// Token: 0x060014D9 RID: 5337 RVA: 0x00092B02 File Offset: 0x00090F02
	protected override bool ValueAtMaxOrMore()
	{
		return this.currentValue >= this.maxValue;
	}

	// Token: 0x060014DA RID: 5338 RVA: 0x00092B15 File Offset: 0x00090F15
	protected override bool ValueAtMinOrLess()
	{
		return this.currentValue <= this.minValue;
	}

	// Token: 0x060014DB RID: 5339 RVA: 0x00092B28 File Offset: 0x00090F28
	private int BracketedStep(int v)
	{
		if (this.stepBrackets.Count == 0)
		{
			return this.baseStep;
		}
		for (int i = 0; i < this.stepBrackets.Count; i++)
		{
			if (v >= this.stepBrackets[i].min && v <= this.stepBrackets[i].max)
			{
				return this.stepBrackets[i].step;
			}
		}
		return this.baseStep;
	}

	// Token: 0x060014DC RID: 5340 RVA: 0x00092BB8 File Offset: 0x00090FB8
	public void SetStepBracket(int min, int max, int stepInBracket)
	{
		this.stepBrackets.Add(new IntTileParameter.intBracket
		{
			min = min,
			max = max,
			step = stepInBracket
		});
	}

	// Token: 0x060014DD RID: 5341 RVA: 0x00092BF4 File Offset: 0x00090FF4
	public override string ValueAsString()
	{
		if (this.stringConverter != null)
		{
			return this.stringConverter.StringValue(this.currentValue);
		}
		return this.prefixValueString + this.currentValue.ToString() + this.postfixValueString;
	}

	// Token: 0x0400105D RID: 4189
	protected int baseStep = 1;

	// Token: 0x0400105E RID: 4190
	protected int step = 1;

	// Token: 0x0400105F RID: 4191
	protected int minValue;

	// Token: 0x04001060 RID: 4192
	protected int maxValue;

	// Token: 0x04001061 RID: 4193
	protected int startValue;

	// Token: 0x04001062 RID: 4194
	public BidiIntFloatConverter converter;

	// Token: 0x04001063 RID: 4195
	public BidiIntStringConverter stringConverter;

	// Token: 0x04001064 RID: 4196
	private List<IntTileParameter.intBracket> stepBrackets = new List<IntTileParameter.intBracket>();

	// Token: 0x02000152 RID: 338
	internal struct intBracket
	{
		// Token: 0x04001065 RID: 4197
		internal int min;

		// Token: 0x04001066 RID: 4198
		internal int max;

		// Token: 0x04001067 RID: 4199
		internal int step;
	}
}
