using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200014F RID: 335
public class FloatTileParameter : NumericHandleTileParameter
{
	// Token: 0x060014BF RID: 5311 RVA: 0x00091EEE File Offset: 0x000902EE
	public FloatTileParameter(float minValue, float maxValue, float step, float sensitivity = 25f, int parameterIndex = 0, bool onlyShowPositive = false, string prefixValueString = "", string postfixValueString = "") : base(sensitivity, parameterIndex, 1, onlyShowPositive, prefixValueString, postfixValueString)
	{
		this.baseStep = step;
		this.step = step;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}

	// Token: 0x1700005C RID: 92
	// (get) Token: 0x060014C0 RID: 5312 RVA: 0x00091F28 File Offset: 0x00090328
	// (set) Token: 0x060014C1 RID: 5313 RVA: 0x00091F3D File Offset: 0x0009033D
	public float currentValue
	{
		get
		{
			return (float)this.presenterSign * (float)base.objectValue;
		}
		set
		{
			base.objectValue = (float)this.presenterSign * value;
		}
	}

	// Token: 0x060014C2 RID: 5314 RVA: 0x00091F54 File Offset: 0x00090354
	protected override float GetParameterScreenOffsetError(Tile thisTile, Tile goalTile)
	{
		float num = (float)goalTile.gaf.Args[base.parameterIndex] - (float)thisTile.gaf.Args[base.parameterIndex];
		if (base.settings.floatSliderMode == FloatTileParameterMode.RelativeBrackets)
		{
			return (float)this.presenterSign * this.GetRelativeBracketScreenOffset(num) / this.sensitivity;
		}
		return (float)this.presenterSign * (num / (this.step * this.sensitivity));
	}

	// Token: 0x060014C3 RID: 5315 RVA: 0x00091FD0 File Offset: 0x000903D0
	private FloatSliderRelativeBracket GetRelativeBracket(float offset)
	{
		FloatSliderRelativeBracket floatSliderRelativeBracket = null;
		FloatSliderRelativeBracket floatSliderRelativeBracket2 = null;
		float num = float.MaxValue;
		foreach (FloatSliderRelativeBracket floatSliderRelativeBracket3 in base.settings.floatSliderRelativeBrackets)
		{
			if (offset >= floatSliderRelativeBracket3.minDistance && offset <= floatSliderRelativeBracket3.maxDistance)
			{
				floatSliderRelativeBracket = floatSliderRelativeBracket3;
				break;
			}
			float num2 = Mathf.Min(Mathf.Abs(offset - floatSliderRelativeBracket3.minDistance), Mathf.Abs(offset - floatSliderRelativeBracket3.maxDistance));
			if (num2 < num)
			{
				floatSliderRelativeBracket2 = floatSliderRelativeBracket3;
				num = num2;
			}
		}
		return (floatSliderRelativeBracket != null) ? floatSliderRelativeBracket : floatSliderRelativeBracket2;
	}

	// Token: 0x060014C4 RID: 5316 RVA: 0x00092070 File Offset: 0x00090470
	protected float GetRelativeBracketScreenOffset(float valueOffset)
	{
		float num = Mathf.Abs(valueOffset);
		FloatSliderRelativeBracket floatSliderRelativeBracket = null;
		FloatSliderRelativeBracket floatSliderRelativeBracket2 = null;
		float num2 = float.MaxValue;
		foreach (FloatSliderRelativeBracket floatSliderRelativeBracket3 in base.settings.floatSliderRelativeBrackets)
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
		FloatSliderRelativeBracket floatSliderRelativeBracket4 = (floatSliderRelativeBracket != null) ? floatSliderRelativeBracket : floatSliderRelativeBracket2;
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

	// Token: 0x060014C5 RID: 5317 RVA: 0x00092188 File Offset: 0x00090588
	protected override bool UpdateValue()
	{
		bool result = false;
		float num = (this.screenPos.x - this.startPositionScreen.x) * this.sensitivity;
		FloatTileParameterMode floatSliderMode = base.settings.floatSliderMode;
		if (floatSliderMode == FloatTileParameterMode.Plain || floatSliderMode == FloatTileParameterMode.AbsoluteBrackets)
		{
			float num2 = Mathf.Clamp(this.startValue + num * this.step, this.minValue, this.maxValue);
			float num3 = this.step * 0.4f;
			if (Mathf.Abs(num2 - this.currentValue) >= num3)
			{
				float currentValue = this.currentValue;
				this.currentValue += this.step * (float)Mathf.RoundToInt((num2 - this.currentValue) / this.step);
				this.currentValue = this.step * Mathf.Round(num2 / this.step);
				result = (Mathf.Abs(currentValue - this.currentValue) > num3);
			}
		}
		else if (floatSliderMode == FloatTileParameterMode.RelativeBrackets)
		{
			float num4 = Mathf.Sign(num);
			float num5 = Mathf.Abs(num);
			FloatSliderRelativeBracket relativeBracket = this.GetRelativeBracket(num5);
			if (relativeBracket != null)
			{
				float num6 = relativeBracket.maxValue - relativeBracket.minValue;
				float num7 = relativeBracket.maxDistance - relativeBracket.minDistance;
				float num8 = (num5 - relativeBracket.minDistance) / num7;
				int num9 = Mathf.RoundToInt(num6 / relativeBracket.step);
				int num10 = Mathf.RoundToInt(num8 * (float)num9);
				float num11 = num4 * (relativeBracket.step * (float)num10 + relativeBracket.minValue);
				float num12 = Mathf.Clamp(this.startValue + num11, this.minValue, this.maxValue);
				if (Mathf.Abs(num12 - this.currentValue) >= relativeBracket.step)
				{
					this.currentValue = num12;
					result = true;
				}
			}
			else
			{
				BWLog.Info("No bracket found for offset " + num);
			}
		}
		return result;
	}

	// Token: 0x060014C6 RID: 5318 RVA: 0x0009235F File Offset: 0x0009075F
	protected override void InitializeStartValue()
	{
		this.startValue = this.currentValue;
	}

	// Token: 0x060014C7 RID: 5319 RVA: 0x00092370 File Offset: 0x00090770
	protected override void SetValueAndStep(Tile tile)
	{
		float num = (float)tile.gaf.Args[base.parameterIndex];
		if (this.onlyShowPositive)
		{
			this.presenterSign = Mathf.RoundToInt(Mathf.Sign(num));
		}
		this.currentValue = (float)this.presenterSign * num;
	}

	// Token: 0x060014C8 RID: 5320 RVA: 0x000923C0 File Offset: 0x000907C0
	protected override void SetStep()
	{
		this.step = this.AbsoluteBracketedStep(this.currentValue);
	}

	// Token: 0x060014C9 RID: 5321 RVA: 0x000923D4 File Offset: 0x000907D4
	protected override bool ValueAtMaxOrMore()
	{
		return this.currentValue >= this.maxValue;
	}

	// Token: 0x060014CA RID: 5322 RVA: 0x000923E7 File Offset: 0x000907E7
	protected override bool ValueAtMinOrLess()
	{
		return this.currentValue <= this.minValue;
	}

	// Token: 0x060014CB RID: 5323 RVA: 0x000923FC File Offset: 0x000907FC
	private float AbsoluteBracketedStep(float v)
	{
		if (this.stepAbsoluteBrackets.Count == 0)
		{
			return this.baseStep;
		}
		for (int i = 0; i < this.stepAbsoluteBrackets.Count; i++)
		{
			if (v >= this.stepAbsoluteBrackets[i].min && v <= this.stepAbsoluteBrackets[i].max)
			{
				return this.stepAbsoluteBrackets[i].step;
			}
		}
		return this.baseStep;
	}

	// Token: 0x060014CC RID: 5324 RVA: 0x0009248C File Offset: 0x0009088C
	public void SetStepBracket(float min, float max, float stepInBracket)
	{
		this.stepAbsoluteBrackets.Add(new FloatTileParameter.floatAbsoluteBracket
		{
			min = min,
			max = max,
			step = stepInBracket
		});
	}

	// Token: 0x060014CD RID: 5325 RVA: 0x000924C8 File Offset: 0x000908C8
	public override string ValueAsString()
	{
		float f = this.step - Mathf.Floor(this.step);
		bool flag = Mathf.Abs(f) > 0.09f;
		bool flag2 = !flag && Mathf.Abs(f) > 0.009f;
		bool flag3 = flag || flag2;
		float num = this.currentValue * base.settings.floatPresentMultiplier;
		string str;
		if (this.step >= 1f && !flag3)
		{
			str = num.ToString("f0");
		}
		else if (this.step < 0.1f || flag2)
		{
			str = num.ToString("f2");
		}
		else
		{
			str = num.ToString("f1");
		}
		return this.prefixValueString + str + this.postfixValueString;
	}

	// Token: 0x04001054 RID: 4180
	private float baseStep;

	// Token: 0x04001055 RID: 4181
	private float step;

	// Token: 0x04001056 RID: 4182
	private float minValue;

	// Token: 0x04001057 RID: 4183
	private float maxValue;

	// Token: 0x04001058 RID: 4184
	protected float startValue;

	// Token: 0x04001059 RID: 4185
	private List<FloatTileParameter.floatAbsoluteBracket> stepAbsoluteBrackets = new List<FloatTileParameter.floatAbsoluteBracket>();

	// Token: 0x02000150 RID: 336
	internal struct floatAbsoluteBracket
	{
		// Token: 0x0400105A RID: 4186
		internal float min;

		// Token: 0x0400105B RID: 4187
		internal float max;

		// Token: 0x0400105C RID: 4188
		internal float step;
	}
}
