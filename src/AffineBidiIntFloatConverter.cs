using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AffineBidiIntFloatConverter : BidiIntFloatConverter
{
	private Func<float, int> finalFloatToIntConverter;

	[CompilerGenerated]
	private static Func<float, int> f__mg_cache0;

	public float bias { get; set; }

	public float multiplier { get; set; }

	public AffineBidiIntFloatConverter()
	{
		finalFloatToIntConverter = Mathf.RoundToInt;
		intToFloat = (int i) => bias + multiplier * (float)i;
		floatToInt = (float f) => finalFloatToIntConverter((f - bias) / multiplier);
	}

	public static AffineBidiIntFloatConverter FromRange(float fromValue, float toValue, int intMin, int intMax)
	{
		int num = intMax - intMin;
		float num2 = toValue - fromValue;
		float num3 = 1f;
		if (num != 0)
		{
			num3 = num2 / (float)num;
		}
		float num4 = fromValue - (float)intMin * num3;
		return new AffineBidiIntFloatConverter
		{
			bias = num4,
			multiplier = num3
		};
	}
}
