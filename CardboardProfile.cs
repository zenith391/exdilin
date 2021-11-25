using System;
using UnityEngine;

// Token: 0x0200001C RID: 28
[Serializable]
public class CardboardProfile
{
	// Token: 0x06000110 RID: 272 RVA: 0x0000720C File Offset: 0x0000560C
	public CardboardProfile Clone()
	{
		return new CardboardProfile
		{
			screen = this.screen,
			device = this.device
		};
	}

	// Token: 0x17000032 RID: 50
	// (get) Token: 0x06000111 RID: 273 RVA: 0x00007238 File Offset: 0x00005638
	public float VerticalLensOffset
	{
		get
		{
			return (this.device.lenses.offset - this.screen.border - this.screen.height / 2f) * (float)this.device.lenses.alignment;
		}
	}

	// Token: 0x06000112 RID: 274 RVA: 0x00007288 File Offset: 0x00005688
	public static CardboardProfile GetKnownProfile(CardboardProfile.ScreenSizes screenSize, CardboardProfile.DeviceTypes deviceType)
	{
		CardboardProfile.Screen screen;
		switch (screenSize)
		{
		case CardboardProfile.ScreenSizes.Nexus6:
			screen = CardboardProfile.Nexus6;
			break;
		case CardboardProfile.ScreenSizes.GalaxyS6:
			screen = CardboardProfile.GalaxyS6;
			break;
		case CardboardProfile.ScreenSizes.GalaxyNote4:
			screen = CardboardProfile.GalaxyNote4;
			break;
		case CardboardProfile.ScreenSizes.LGG3:
			screen = CardboardProfile.LGG3;
			break;
		case CardboardProfile.ScreenSizes.iPhone4:
			screen = CardboardProfile.iPhone4;
			break;
		case CardboardProfile.ScreenSizes.iPhone5:
			screen = CardboardProfile.iPhone5;
			break;
		case CardboardProfile.ScreenSizes.iPhone6:
			screen = CardboardProfile.iPhone6;
			break;
		case CardboardProfile.ScreenSizes.iPhone6p:
			screen = CardboardProfile.iPhone6p;
			break;
		default:
			screen = CardboardProfile.Nexus5;
			break;
		}
		CardboardProfile.Device device;
		if (deviceType != CardboardProfile.DeviceTypes.CardboardMay2015)
		{
			if (deviceType != CardboardProfile.DeviceTypes.GoggleTechC1Glass)
			{
				device = CardboardProfile.CardboardJun2014;
			}
			else
			{
				device = CardboardProfile.GoggleTechC1Glass;
			}
		}
		else
		{
			device = CardboardProfile.CardboardMay2015;
		}
		return new CardboardProfile
		{
			screen = screen,
			device = device
		};
	}

	// Token: 0x06000113 RID: 275 RVA: 0x00007370 File Offset: 0x00005770
	public void GetLeftEyeVisibleTanAngles(float[] result)
	{
		float val = Mathf.Tan(-this.device.maxFOV.outer * 0.0174532924f);
		float val2 = Mathf.Tan(this.device.maxFOV.upper * 0.0174532924f);
		float val3 = Mathf.Tan(this.device.maxFOV.inner * 0.0174532924f);
		float val4 = Mathf.Tan(-this.device.maxFOV.lower * 0.0174532924f);
		float num = this.screen.width / 4f;
		float num2 = this.screen.height / 2f;
		float num3 = this.device.lenses.separation / 2f - num;
		float num4 = -this.VerticalLensOffset;
		float screenDistance = this.device.lenses.screenDistance;
		float val5 = this.device.distortion.distort((num3 - num) / screenDistance);
		float val6 = this.device.distortion.distort((num4 + num2) / screenDistance);
		float val7 = this.device.distortion.distort((num3 + num) / screenDistance);
		float val8 = this.device.distortion.distort((num4 - num2) / screenDistance);
		result[0] = Math.Max(val, val5);
		result[1] = Math.Min(val2, val6);
		result[2] = Math.Min(val3, val7);
		result[3] = Math.Max(val4, val8);
	}

	// Token: 0x06000114 RID: 276 RVA: 0x000074E0 File Offset: 0x000058E0
	public void GetLeftEyeNoLensTanAngles(float[] result)
	{
		float val = this.device.distortion.distortInv(Mathf.Tan(-this.device.maxFOV.outer * 0.0174532924f));
		float val2 = this.device.distortion.distortInv(Mathf.Tan(this.device.maxFOV.upper * 0.0174532924f));
		float val3 = this.device.distortion.distortInv(Mathf.Tan(this.device.maxFOV.inner * 0.0174532924f));
		float val4 = this.device.distortion.distortInv(Mathf.Tan(-this.device.maxFOV.lower * 0.0174532924f));
		float num = this.screen.width / 4f;
		float num2 = this.screen.height / 2f;
		float num3 = this.device.lenses.separation / 2f - num;
		float num4 = -this.VerticalLensOffset;
		float screenDistance = this.device.lenses.screenDistance;
		float val5 = (num3 - num) / screenDistance;
		float val6 = (num4 + num2) / screenDistance;
		float val7 = (num3 + num) / screenDistance;
		float val8 = (num4 - num2) / screenDistance;
		result[0] = Math.Max(val, val5);
		result[1] = Math.Min(val2, val6);
		result[2] = Math.Min(val3, val7);
		result[3] = Math.Max(val4, val8);
	}

	// Token: 0x06000115 RID: 277 RVA: 0x00007650 File Offset: 0x00005A50
	public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum)
	{
		float screenDistance = this.device.lenses.screenDistance;
		float num = (this.screen.width - this.device.lenses.separation) / 2f;
		float num2 = this.VerticalLensOffset + this.screen.height / 2f;
		float num3 = (undistortedFrustum[0] * screenDistance + num) / this.screen.width;
		float num4 = (undistortedFrustum[1] * screenDistance + num2) / this.screen.height;
		float num5 = (undistortedFrustum[2] * screenDistance + num) / this.screen.width;
		float num6 = (undistortedFrustum[3] * screenDistance + num2) / this.screen.height;
		return new Rect(num3, num6, num5 - num3, num4 - num6);
	}

	// Token: 0x06000116 RID: 278 RVA: 0x00007710 File Offset: 0x00005B10
	public static float GetMaxRadius(float[] tanAngleRect)
	{
		float num = Mathf.Max(Mathf.Abs(tanAngleRect[0]), Mathf.Abs(tanAngleRect[2]));
		float num2 = Mathf.Max(Mathf.Abs(tanAngleRect[1]), Mathf.Abs(tanAngleRect[3]));
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	// Token: 0x06000117 RID: 279 RVA: 0x00007758 File Offset: 0x00005B58
	private static double[] solveLeastSquares(double[,] matA, double[] vecY)
	{
		int length = matA.GetLength(0);
		int length2 = matA.GetLength(1);
		if (length != vecY.Length)
		{
			Debug.LogError("Matrix / vector dimension mismatch");
			return null;
		}
		if (length2 != 2)
		{
			Debug.LogError("Only 2 coefficients supported.");
			return null;
		}
		double[,] array = new double[length2, length2];
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				double num = 0.0;
				for (int k = 0; k < length; k++)
				{
					num += matA[k, j] * matA[k, i];
				}
				array[j, i] = num;
			}
		}
		double[,] array2 = new double[length2, length2];
		double num2 = array[0, 0] * array[1, 1] - array[0, 1] * array[1, 0];
		array2[0, 0] = array[1, 1] / num2;
		array2[1, 1] = array[0, 0] / num2;
		array2[0, 1] = -array[1, 0] / num2;
		array2[1, 0] = -array[0, 1] / num2;
		double[] array3 = new double[length2];
		for (int l = 0; l < length2; l++)
		{
			double num3 = 0.0;
			for (int m = 0; m < length; m++)
			{
				num3 += matA[m, l] * vecY[m];
			}
			array3[l] = num3;
		}
		double[] array4 = new double[length2];
		for (int n = 0; n < length2; n++)
		{
			double num4 = 0.0;
			for (int num5 = 0; num5 < length2; num5++)
			{
				num4 += array2[num5, n] * array3[num5];
			}
			array4[n] = num4;
		}
		return array4;
	}

	// Token: 0x06000118 RID: 280 RVA: 0x0000794C File Offset: 0x00005D4C
	public static CardboardProfile.Distortion ApproximateInverse(float k1, float k2, float maxRadius = 1f, int numSamples = 10)
	{
		return CardboardProfile.ApproximateInverse(new CardboardProfile.Distortion
		{
			k1 = k1,
			k2 = k2
		}, maxRadius, numSamples);
	}

	// Token: 0x06000119 RID: 281 RVA: 0x0000797C File Offset: 0x00005D7C
	public static CardboardProfile.Distortion ApproximateInverse(CardboardProfile.Distortion distort, float maxRadius = 1f, int numSamples = 10)
	{
		double[,] array = new double[numSamples, 2];
		double[] array2 = new double[numSamples];
		for (int i = 0; i < numSamples; i++)
		{
			float num = maxRadius * (float)(i + 1) / (float)numSamples;
			double num2 = (double)distort.distort(num);
			double num3 = num2;
			for (int j = 0; j < 2; j++)
			{
				num3 *= num2 * num2;
				array[i, j] = num3;
			}
			array2[i] = (double)num - num2;
		}
		double[] array3 = CardboardProfile.solveLeastSquares(array, array2);
		return new CardboardProfile.Distortion
		{
			k1 = (float)array3[0],
			k2 = (float)array3[1]
		};
	}

	// Token: 0x0600011A RID: 282 RVA: 0x00007A24 File Offset: 0x00005E24
	// Note: this type is marked as 'beforefieldinit'.
	static CardboardProfile()
	{
		CardboardProfile.Device cardboardJun = default(CardboardProfile.Device);
		cardboardJun.lenses.separation = 0.06f;
		cardboardJun.lenses.offset = 0.035f;
		cardboardJun.lenses.screenDistance = 0.042f;
		cardboardJun.lenses.alignment = 1;
		cardboardJun.maxFOV.outer = 40f;
		cardboardJun.maxFOV.inner = 40f;
		cardboardJun.maxFOV.upper = 40f;
		cardboardJun.maxFOV.lower = 40f;
		cardboardJun.distortion.k1 = 0.441f;
		cardboardJun.distortion.k2 = 0.156f;
		cardboardJun.inverse = CardboardProfile.ApproximateInverse(0.441f, 0.156f, 1f, 10);
		CardboardProfile.CardboardJun2014 = cardboardJun;
		CardboardProfile.Device cardboardMay = default(CardboardProfile.Device);
		cardboardMay.lenses.separation = 0.064f;
		cardboardMay.lenses.offset = 0.035f;
		cardboardMay.lenses.screenDistance = 0.039f;
		cardboardMay.lenses.alignment = 1;
		cardboardMay.maxFOV.outer = 60f;
		cardboardMay.maxFOV.inner = 60f;
		cardboardMay.maxFOV.upper = 60f;
		cardboardMay.maxFOV.lower = 60f;
		cardboardMay.distortion.k1 = 0.34f;
		cardboardMay.distortion.k2 = 0.55f;
		cardboardMay.inverse = CardboardProfile.ApproximateInverse(0.34f, 0.55f, 1f, 10);
		CardboardProfile.CardboardMay2015 = cardboardMay;
		CardboardProfile.Device goggleTechC1Glass = default(CardboardProfile.Device);
		goggleTechC1Glass.lenses.separation = 0.065f;
		goggleTechC1Glass.lenses.offset = 0.036f;
		goggleTechC1Glass.lenses.screenDistance = 0.058f;
		goggleTechC1Glass.lenses.alignment = 1;
		goggleTechC1Glass.maxFOV.outer = 50f;
		goggleTechC1Glass.maxFOV.inner = 50f;
		goggleTechC1Glass.maxFOV.upper = 50f;
		goggleTechC1Glass.maxFOV.lower = 50f;
		goggleTechC1Glass.distortion.k1 = 0.3f;
		goggleTechC1Glass.distortion.k2 = 0f;
		goggleTechC1Glass.inverse = CardboardProfile.ApproximateInverse(0.3f, 0f, 1f, 10);
		CardboardProfile.GoggleTechC1Glass = goggleTechC1Glass;
		CardboardProfile.Default = new CardboardProfile
		{
			screen = CardboardProfile.Nexus5,
			device = CardboardProfile.CardboardJun2014
		};
	}

	// Token: 0x0400013D RID: 317
	public CardboardProfile.Screen screen;

	// Token: 0x0400013E RID: 318
	public CardboardProfile.Device device;

	// Token: 0x0400013F RID: 319
	public static readonly CardboardProfile.Screen Nexus5 = new CardboardProfile.Screen
	{
		width = 0.11f,
		height = 0.062f,
		border = 0.004f
	};

	// Token: 0x04000140 RID: 320
	public static readonly CardboardProfile.Screen Nexus6 = new CardboardProfile.Screen
	{
		width = 0.133f,
		height = 0.074f,
		border = 0.004f
	};

	// Token: 0x04000141 RID: 321
	public static readonly CardboardProfile.Screen GalaxyS6 = new CardboardProfile.Screen
	{
		width = 0.114f,
		height = 0.0635f,
		border = 0.0035f
	};

	// Token: 0x04000142 RID: 322
	public static readonly CardboardProfile.Screen GalaxyNote4 = new CardboardProfile.Screen
	{
		width = 0.125f,
		height = 0.0705f,
		border = 0.0045f
	};

	// Token: 0x04000143 RID: 323
	public static readonly CardboardProfile.Screen LGG3 = new CardboardProfile.Screen
	{
		width = 0.121f,
		height = 0.068f,
		border = 0.003f
	};

	// Token: 0x04000144 RID: 324
	public static readonly CardboardProfile.Screen iPhone4 = new CardboardProfile.Screen
	{
		width = 0.075f,
		height = 0.05f,
		border = 0.0045f
	};

	// Token: 0x04000145 RID: 325
	public static readonly CardboardProfile.Screen iPhone5 = new CardboardProfile.Screen
	{
		width = 0.089f,
		height = 0.05f,
		border = 0.0045f
	};

	// Token: 0x04000146 RID: 326
	public static readonly CardboardProfile.Screen iPhone6 = new CardboardProfile.Screen
	{
		width = 0.104f,
		height = 0.058f,
		border = 0.005f
	};

	// Token: 0x04000147 RID: 327
	public static readonly CardboardProfile.Screen iPhone6p = new CardboardProfile.Screen
	{
		width = 0.112f,
		height = 0.068f,
		border = 0.005f
	};

	// Token: 0x04000148 RID: 328
	public static readonly CardboardProfile.Device CardboardJun2014;

	// Token: 0x04000149 RID: 329
	public static readonly CardboardProfile.Device CardboardMay2015;

	// Token: 0x0400014A RID: 330
	public static readonly CardboardProfile.Device GoggleTechC1Glass;

	// Token: 0x0400014B RID: 331
	public static readonly CardboardProfile Default;

	// Token: 0x0200001D RID: 29
	[Serializable]
	public struct Screen
	{
		// Token: 0x0400014C RID: 332
		public float width;

		// Token: 0x0400014D RID: 333
		public float height;

		// Token: 0x0400014E RID: 334
		public float border;
	}

	// Token: 0x0200001E RID: 30
	[Serializable]
	public struct Lenses
	{
		// Token: 0x0400014F RID: 335
		public float separation;

		// Token: 0x04000150 RID: 336
		public float offset;

		// Token: 0x04000151 RID: 337
		public float screenDistance;

		// Token: 0x04000152 RID: 338
		public int alignment;

		// Token: 0x04000153 RID: 339
		public const int AlignTop = -1;

		// Token: 0x04000154 RID: 340
		public const int AlignCenter = 0;

		// Token: 0x04000155 RID: 341
		public const int AlignBottom = 1;
	}

	// Token: 0x0200001F RID: 31
	[Serializable]
	public struct MaxFOV
	{
		// Token: 0x04000156 RID: 342
		public float outer;

		// Token: 0x04000157 RID: 343
		public float inner;

		// Token: 0x04000158 RID: 344
		public float upper;

		// Token: 0x04000159 RID: 345
		public float lower;
	}

	// Token: 0x02000020 RID: 32
	[Serializable]
	public struct Distortion
	{
		// Token: 0x0600011B RID: 283 RVA: 0x00007E94 File Offset: 0x00006294
		public float distort(float r)
		{
			float num = r * r;
			return ((this.k2 * num + this.k1) * num + 1f) * r;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00007EC0 File Offset: 0x000062C0
		public float distortInv(float radius)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = radius - this.distort(num);
			while (Mathf.Abs(num2 - num) > 0.0001f)
			{
				float num4 = radius - this.distort(num2);
				float num5 = num2 - num4 * ((num2 - num) / (num4 - num3));
				num = num2;
				num2 = num5;
				num3 = num4;
			}
			return num2;
		}

		// Token: 0x0400015A RID: 346
		public float k1;

		// Token: 0x0400015B RID: 347
		public float k2;
	}

	// Token: 0x02000021 RID: 33
	[Serializable]
	public struct Device
	{
		// Token: 0x0400015C RID: 348
		public CardboardProfile.Lenses lenses;

		// Token: 0x0400015D RID: 349
		public CardboardProfile.MaxFOV maxFOV;

		// Token: 0x0400015E RID: 350
		public CardboardProfile.Distortion distortion;

		// Token: 0x0400015F RID: 351
		public CardboardProfile.Distortion inverse;
	}

	// Token: 0x02000022 RID: 34
	public enum ScreenSizes
	{
		// Token: 0x04000161 RID: 353
		Nexus5,
		// Token: 0x04000162 RID: 354
		Nexus6,
		// Token: 0x04000163 RID: 355
		GalaxyS6,
		// Token: 0x04000164 RID: 356
		GalaxyNote4,
		// Token: 0x04000165 RID: 357
		LGG3,
		// Token: 0x04000166 RID: 358
		iPhone4,
		// Token: 0x04000167 RID: 359
		iPhone5,
		// Token: 0x04000168 RID: 360
		iPhone6,
		// Token: 0x04000169 RID: 361
		iPhone6p
	}

	// Token: 0x02000023 RID: 35
	public enum DeviceTypes
	{
		// Token: 0x0400016B RID: 363
		CardboardJun2014,
		// Token: 0x0400016C RID: 364
		CardboardMay2015,
		// Token: 0x0400016D RID: 365
		GoggleTechC1Glass
	}
}
