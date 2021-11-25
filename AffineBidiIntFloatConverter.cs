using System;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x02000155 RID: 341
public class AffineBidiIntFloatConverter : BidiIntFloatConverter
{
	// Token: 0x060014E2 RID: 5346 RVA: 0x00092D34 File Offset: 0x00091134
	public AffineBidiIntFloatConverter()
	{
		if (AffineBidiIntFloatConverter.f__mg_cache0 == null)
		{
			AffineBidiIntFloatConverter.f__mg_cache0 = new Func<float, int>(Mathf.RoundToInt);
		}
		this.finalFloatToIntConverter = AffineBidiIntFloatConverter.f__mg_cache0;
		//base..ctor();
		this.intToFloat = ((int i) => this.bias + this.multiplier * (float)i);
		this.floatToInt = ((float f) => this.finalFloatToIntConverter((f - this.bias) / this.multiplier));
	}

	// Token: 0x1700005E RID: 94
	// (get) Token: 0x060014E3 RID: 5347 RVA: 0x00092D8E File Offset: 0x0009118E
	// (set) Token: 0x060014E4 RID: 5348 RVA: 0x00092D96 File Offset: 0x00091196
	public float bias { get; set; }

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x060014E5 RID: 5349 RVA: 0x00092D9F File Offset: 0x0009119F
	// (set) Token: 0x060014E6 RID: 5350 RVA: 0x00092DA7 File Offset: 0x000911A7
	public float multiplier { get; set; }

	// Token: 0x060014E7 RID: 5351 RVA: 0x00092DB0 File Offset: 0x000911B0
	public static AffineBidiIntFloatConverter FromRange(float fromValue, float toValue, int intMin, int intMax)
	{
		int num = intMax - intMin;
		float num2 = toValue - fromValue;
		float num3 = 1f;
		if (num != 0)
		{
			num3 = num2 / (float)num;
		}
		float bias = fromValue - (float)intMin * num3;
		return new AffineBidiIntFloatConverter
		{
			bias = bias,
			multiplier = num3
		};
	}

	// Token: 0x0400106D RID: 4205
	private Func<float, int> finalFloatToIntConverter;

	// Token: 0x0400106E RID: 4206
	[CompilerGenerated]
	private static Func<float, int> f__mg_cache0;
}
