using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000157 RID: 343
public class PiecewiseLinearIntFloatConverter : BidiIntFloatConverter
{
	// Token: 0x060014EB RID: 5355 RVA: 0x00092F88 File Offset: 0x00091388
	public PiecewiseLinearIntFloatConverter(int[] intValues, float[] floatValues, bool onlyShowPositive)
	{
		for (int j = 0; j < Mathf.Min(intValues.Length, floatValues.Length) - 1; j++)
		{
			this.ranges.Add(new PiecewiseLinearIntFloatConverter.IntFloatRange
			{
				intValueMin = intValues[j],
				intValueMax = intValues[j + 1],
				floatValueMin = floatValues[j],
				floatValueMax = floatValues[j + 1]
			});
		}
		this.ranges.Sort((PiecewiseLinearIntFloatConverter.IntFloatRange r1, PiecewiseLinearIntFloatConverter.IntFloatRange r2) => r1.intValueMin.CompareTo(r2.intValueMin));
		this.intToFloat = delegate(int i)
		{
			PiecewiseLinearIntFloatConverter.IntFloatRange intFloatRange = null;
			int num = int.MaxValue;
			foreach (PiecewiseLinearIntFloatConverter.IntFloatRange intFloatRange2 in this.ranges)
			{
				if (intFloatRange2.ContainsInt(i))
				{
					return intFloatRange2.IntToFloat(i);
				}
				int num2 = intFloatRange2.DistanceToInt(i);
				if (num2 < num)
				{
					num = num2;
					intFloatRange = intFloatRange2;
				}
			}
			if (intFloatRange != null)
			{
				return intFloatRange.IntToFloat(i);
			}
			return 0f;
		};
		this.floatToInt = delegate(float f)
		{
			PiecewiseLinearIntFloatConverter.IntFloatRange intFloatRange = null;
			float num = float.MaxValue;
			foreach (PiecewiseLinearIntFloatConverter.IntFloatRange intFloatRange2 in this.ranges)
			{
				if (intFloatRange2.ContainsFloat(f))
				{
					return intFloatRange2.FloatToInt(f);
				}
				float num2 = intFloatRange2.DistanceToFloat(f);
				if (num2 < num)
				{
					num = num2;
					intFloatRange = intFloatRange2;
				}
			}
			if (intFloatRange != null)
			{
				return intFloatRange.FloatToInt(f);
			}
			return 0;
		};
	}

	// Token: 0x04001070 RID: 4208
	private List<PiecewiseLinearIntFloatConverter.IntFloatRange> ranges = new List<PiecewiseLinearIntFloatConverter.IntFloatRange>();

	// Token: 0x02000158 RID: 344
	private class IntFloatRange
	{
		// Token: 0x060014F0 RID: 5360 RVA: 0x000931AC File Offset: 0x000915AC
		public bool ContainsInt(int i)
		{
			return i <= this.intValueMax && i >= this.intValueMin;
		}

		// Token: 0x060014F1 RID: 5361 RVA: 0x000931C9 File Offset: 0x000915C9
		public bool ContainsFloat(float f)
		{
			return f <= this.floatValueMax && f >= this.floatValueMin;
		}

		// Token: 0x060014F2 RID: 5362 RVA: 0x000931E6 File Offset: 0x000915E6
		public int DistanceToInt(int i)
		{
			return Mathf.Min(Mathf.Abs(this.intValueMin - i), Mathf.Abs(this.intValueMax - i));
		}

		// Token: 0x060014F3 RID: 5363 RVA: 0x00093207 File Offset: 0x00091607
		public float DistanceToFloat(float f)
		{
			return Mathf.Min(Mathf.Abs(this.floatValueMin - f), Mathf.Abs(this.floatValueMax - f));
		}

		// Token: 0x060014F4 RID: 5364 RVA: 0x00093228 File Offset: 0x00091628
		public float IntToFloat(int i)
		{
			int num = this.intValueMax - this.intValueMin;
			float num2 = this.floatValueMax - this.floatValueMin;
			float num3 = 1f;
			if (num != 0)
			{
				num3 = num2 / (float)num;
			}
			float num4 = this.floatValueMin - (float)this.intValueMin * num3;
			return (float)i * num3 + num4;
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x0009327C File Offset: 0x0009167C
		public int FloatToInt(float f)
		{
			int num = this.intValueMax - this.intValueMin;
			float num2 = this.floatValueMax - this.floatValueMin;
			float num3 = 1f;
			if (num2 != 0f)
			{
				num3 = (float)num / num2;
			}
			float num4 = (float)this.intValueMin - this.floatValueMin * num3;
			return Mathf.RoundToInt(f * num3 + num4);
		}

		// Token: 0x04001072 RID: 4210
		public int intValueMin;

		// Token: 0x04001073 RID: 4211
		public int intValueMax;

		// Token: 0x04001074 RID: 4212
		public float floatValueMin;

		// Token: 0x04001075 RID: 4213
		public float floatValueMax;
	}
}
