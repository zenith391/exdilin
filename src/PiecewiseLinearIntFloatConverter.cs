using System.Collections.Generic;
using UnityEngine;

public class PiecewiseLinearIntFloatConverter : BidiIntFloatConverter
{
	private class IntFloatRange
	{
		public int intValueMin;

		public int intValueMax;

		public float floatValueMin;

		public float floatValueMax;

		public bool ContainsInt(int i)
		{
			if (i <= intValueMax)
			{
				return i >= intValueMin;
			}
			return false;
		}

		public bool ContainsFloat(float f)
		{
			if (f <= floatValueMax)
			{
				return f >= floatValueMin;
			}
			return false;
		}

		public int DistanceToInt(int i)
		{
			return Mathf.Min(Mathf.Abs(intValueMin - i), Mathf.Abs(intValueMax - i));
		}

		public float DistanceToFloat(float f)
		{
			return Mathf.Min(Mathf.Abs(floatValueMin - f), Mathf.Abs(floatValueMax - f));
		}

		public float IntToFloat(int i)
		{
			int num = intValueMax - intValueMin;
			float num2 = floatValueMax - floatValueMin;
			float num3 = 1f;
			if (num != 0)
			{
				num3 = num2 / (float)num;
			}
			float num4 = floatValueMin - (float)intValueMin * num3;
			return (float)i * num3 + num4;
		}

		public int FloatToInt(float f)
		{
			int num = intValueMax - intValueMin;
			float num2 = floatValueMax - floatValueMin;
			float num3 = 1f;
			if (num2 != 0f)
			{
				num3 = (float)num / num2;
			}
			float num4 = (float)intValueMin - floatValueMin * num3;
			return Mathf.RoundToInt(f * num3 + num4);
		}
	}

	private List<IntFloatRange> ranges = new List<IntFloatRange>();

	public PiecewiseLinearIntFloatConverter(int[] intValues, float[] floatValues, bool onlyShowPositive)
	{
		for (int i = 0; i < Mathf.Min(intValues.Length, floatValues.Length) - 1; i++)
		{
			ranges.Add(new IntFloatRange
			{
				intValueMin = intValues[i],
				intValueMax = intValues[i + 1],
				floatValueMin = floatValues[i],
				floatValueMax = floatValues[i + 1]
			});
		}
		ranges.Sort((IntFloatRange r1, IntFloatRange r2) => r1.intValueMin.CompareTo(r2.intValueMin));
		intToFloat = delegate(int i2)
		{
			IntFloatRange intFloatRange = null;
			int num = int.MaxValue;
			foreach (IntFloatRange range in ranges)
			{
				if (range.ContainsInt(i2))
				{
					return range.IntToFloat(i2);
				}
				int num2 = range.DistanceToInt(i2);
				if (num2 < num)
				{
					num = num2;
					intFloatRange = range;
				}
			}
			return intFloatRange?.IntToFloat(i2) ?? 0f;
		};
		floatToInt = delegate(float f)
		{
			IntFloatRange intFloatRange = null;
			float num = float.MaxValue;
			foreach (IntFloatRange range2 in ranges)
			{
				if (range2.ContainsFloat(f))
				{
					return range2.FloatToInt(f);
				}
				float num2 = range2.DistanceToFloat(f);
				if (num2 < num)
				{
					num = num2;
					intFloatRange = range2;
				}
			}
			return intFloatRange?.FloatToInt(f) ?? 0;
		};
	}
}
