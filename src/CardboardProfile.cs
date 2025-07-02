using System;
using UnityEngine;

[Serializable]
public class CardboardProfile
{
	[Serializable]
	public struct Screen
	{
		public float width;

		public float height;

		public float border;
	}

	[Serializable]
	public struct Lenses
	{
		public float separation;

		public float offset;

		public float screenDistance;

		public int alignment;

		public const int AlignTop = -1;

		public const int AlignCenter = 0;

		public const int AlignBottom = 1;
	}

	[Serializable]
	public struct MaxFOV
	{
		public float outer;

		public float inner;

		public float upper;

		public float lower;
	}

	[Serializable]
	public struct Distortion
	{
		public float k1;

		public float k2;

		public float distort(float r)
		{
			float num = r * r;
			return ((k2 * num + k1) * num + 1f) * r;
		}

		public float distortInv(float radius)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = radius - distort(num);
			while (Mathf.Abs(num2 - num) > 0.0001f)
			{
				float num4 = radius - distort(num2);
				float num5 = num2 - num4 * ((num2 - num) / (num4 - num3));
				num = num2;
				num2 = num5;
				num3 = num4;
			}
			return num2;
		}
	}

	[Serializable]
	public struct Device
	{
		public Lenses lenses;

		public MaxFOV maxFOV;

		public Distortion distortion;

		public Distortion inverse;
	}

	public enum ScreenSizes
	{
		Nexus5,
		Nexus6,
		GalaxyS6,
		GalaxyNote4,
		LGG3,
		iPhone4,
		iPhone5,
		iPhone6,
		iPhone6p
	}

	public enum DeviceTypes
	{
		CardboardJun2014,
		CardboardMay2015,
		GoggleTechC1Glass
	}

	public Screen screen;

	public Device device;

	public static readonly Screen Nexus5;

	public static readonly Screen Nexus6;

	public static readonly Screen GalaxyS6;

	public static readonly Screen GalaxyNote4;

	public static readonly Screen LGG3;

	public static readonly Screen iPhone4;

	public static readonly Screen iPhone5;

	public static readonly Screen iPhone6;

	public static readonly Screen iPhone6p;

	public static readonly Device CardboardJun2014;

	public static readonly Device CardboardMay2015;

	public static readonly Device GoggleTechC1Glass;

	public static readonly CardboardProfile Default;

	public float VerticalLensOffset => (device.lenses.offset - screen.border - screen.height / 2f) * (float)device.lenses.alignment;

	public CardboardProfile Clone()
	{
		return new CardboardProfile
		{
			screen = screen,
			device = device
		};
	}

	public static CardboardProfile GetKnownProfile(ScreenSizes screenSize, DeviceTypes deviceType)
	{
		Screen screen = screenSize switch
		{
			ScreenSizes.Nexus6 => Nexus6, 
			ScreenSizes.GalaxyS6 => GalaxyS6, 
			ScreenSizes.GalaxyNote4 => GalaxyNote4, 
			ScreenSizes.LGG3 => LGG3, 
			ScreenSizes.iPhone4 => iPhone4, 
			ScreenSizes.iPhone5 => iPhone5, 
			ScreenSizes.iPhone6 => iPhone6, 
			ScreenSizes.iPhone6p => iPhone6p, 
			_ => Nexus5, 
		};
		Device device = deviceType switch
		{
			DeviceTypes.GoggleTechC1Glass => GoggleTechC1Glass, 
			DeviceTypes.CardboardMay2015 => CardboardMay2015, 
			_ => CardboardJun2014, 
		};
		return new CardboardProfile
		{
			screen = screen,
			device = device
		};
	}

	public void GetLeftEyeVisibleTanAngles(float[] result)
	{
		float val = Mathf.Tan((0f - device.maxFOV.outer) * ((float)Math.PI / 180f));
		float val2 = Mathf.Tan(device.maxFOV.upper * ((float)Math.PI / 180f));
		float val3 = Mathf.Tan(device.maxFOV.inner * ((float)Math.PI / 180f));
		float val4 = Mathf.Tan((0f - device.maxFOV.lower) * ((float)Math.PI / 180f));
		float num = screen.width / 4f;
		float num2 = screen.height / 2f;
		float num3 = device.lenses.separation / 2f - num;
		float num4 = 0f - VerticalLensOffset;
		float screenDistance = device.lenses.screenDistance;
		float val5 = device.distortion.distort((num3 - num) / screenDistance);
		float val6 = device.distortion.distort((num4 + num2) / screenDistance);
		float val7 = device.distortion.distort((num3 + num) / screenDistance);
		float val8 = device.distortion.distort((num4 - num2) / screenDistance);
		result[0] = Math.Max(val, val5);
		result[1] = Math.Min(val2, val6);
		result[2] = Math.Min(val3, val7);
		result[3] = Math.Max(val4, val8);
	}

	public void GetLeftEyeNoLensTanAngles(float[] result)
	{
		float val = device.distortion.distortInv(Mathf.Tan((0f - device.maxFOV.outer) * ((float)Math.PI / 180f)));
		float val2 = device.distortion.distortInv(Mathf.Tan(device.maxFOV.upper * ((float)Math.PI / 180f)));
		float val3 = device.distortion.distortInv(Mathf.Tan(device.maxFOV.inner * ((float)Math.PI / 180f)));
		float val4 = device.distortion.distortInv(Mathf.Tan((0f - device.maxFOV.lower) * ((float)Math.PI / 180f)));
		float num = screen.width / 4f;
		float num2 = screen.height / 2f;
		float num3 = device.lenses.separation / 2f - num;
		float num4 = 0f - VerticalLensOffset;
		float screenDistance = device.lenses.screenDistance;
		float val5 = (num3 - num) / screenDistance;
		float val6 = (num4 + num2) / screenDistance;
		float val7 = (num3 + num) / screenDistance;
		float val8 = (num4 - num2) / screenDistance;
		result[0] = Math.Max(val, val5);
		result[1] = Math.Min(val2, val6);
		result[2] = Math.Min(val3, val7);
		result[3] = Math.Max(val4, val8);
	}

	public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum)
	{
		float screenDistance = device.lenses.screenDistance;
		float num = (screen.width - device.lenses.separation) / 2f;
		float num2 = VerticalLensOffset + screen.height / 2f;
		float num3 = (undistortedFrustum[0] * screenDistance + num) / screen.width;
		float num4 = (undistortedFrustum[1] * screenDistance + num2) / screen.height;
		float num5 = (undistortedFrustum[2] * screenDistance + num) / screen.width;
		float num6 = (undistortedFrustum[3] * screenDistance + num2) / screen.height;
		return new Rect(num3, num6, num5 - num3, num4 - num6);
	}

	public static float GetMaxRadius(float[] tanAngleRect)
	{
		float num = Mathf.Max(Mathf.Abs(tanAngleRect[0]), Mathf.Abs(tanAngleRect[2]));
		float num2 = Mathf.Max(Mathf.Abs(tanAngleRect[1]), Mathf.Abs(tanAngleRect[3]));
		return Mathf.Sqrt(num * num + num2 * num2);
	}

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
		array2[0, 1] = (0.0 - array[1, 0]) / num2;
		array2[1, 0] = (0.0 - array[0, 1]) / num2;
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

	public static Distortion ApproximateInverse(float k1, float k2, float maxRadius = 1f, int numSamples = 10)
	{
		return ApproximateInverse(new Distortion
		{
			k1 = k1,
			k2 = k2
		}, maxRadius, numSamples);
	}

	public static Distortion ApproximateInverse(Distortion distort, float maxRadius = 1f, int numSamples = 10)
	{
		double[,] array = new double[numSamples, 2];
		double[] array2 = new double[numSamples];
		for (int i = 0; i < numSamples; i++)
		{
			float num = maxRadius * (float)(i + 1) / (float)numSamples;
			double num2 = distort.distort(num);
			double num3 = num2;
			for (int j = 0; j < 2; j++)
			{
				num3 = (array[i, j] = num3 * (num2 * num2));
			}
			array2[i] = (double)num - num2;
		}
		double[] array3 = solveLeastSquares(array, array2);
		return new Distortion
		{
			k1 = (float)array3[0],
			k2 = (float)array3[1]
		};
	}

	static CardboardProfile()
	{
		Nexus5 = new Screen
		{
			width = 0.11f,
			height = 0.062f,
			border = 0.004f
		};
		Nexus6 = new Screen
		{
			width = 0.133f,
			height = 0.074f,
			border = 0.004f
		};
		GalaxyS6 = new Screen
		{
			width = 0.114f,
			height = 0.0635f,
			border = 0.0035f
		};
		GalaxyNote4 = new Screen
		{
			width = 0.125f,
			height = 0.0705f,
			border = 0.0045f
		};
		LGG3 = new Screen
		{
			width = 0.121f,
			height = 0.068f,
			border = 0.003f
		};
		iPhone4 = new Screen
		{
			width = 0.075f,
			height = 0.05f,
			border = 0.0045f
		};
		iPhone5 = new Screen
		{
			width = 0.089f,
			height = 0.05f,
			border = 0.0045f
		};
		iPhone6 = new Screen
		{
			width = 0.104f,
			height = 0.058f,
			border = 0.005f
		};
		iPhone6p = new Screen
		{
			width = 0.112f,
			height = 0.068f,
			border = 0.005f
		};
		CardboardJun2014 = new Device
		{
			lenses = 
			{
				separation = 0.06f,
				offset = 0.035f,
				screenDistance = 0.042f,
				alignment = 1
			},
			maxFOV = 
			{
				outer = 40f,
				inner = 40f,
				upper = 40f,
				lower = 40f
			},
			distortion = 
			{
				k1 = 0.441f,
				k2 = 0.156f
			},
			inverse = ApproximateInverse(0.441f, 0.156f)
		};
		CardboardMay2015 = new Device
		{
			lenses = 
			{
				separation = 0.064f,
				offset = 0.035f,
				screenDistance = 0.039f,
				alignment = 1
			},
			maxFOV = 
			{
				outer = 60f,
				inner = 60f,
				upper = 60f,
				lower = 60f
			},
			distortion = 
			{
				k1 = 0.34f,
				k2 = 0.55f
			},
			inverse = ApproximateInverse(0.34f, 0.55f)
		};
		GoggleTechC1Glass = new Device
		{
			lenses = 
			{
				separation = 0.065f,
				offset = 0.036f,
				screenDistance = 0.058f,
				alignment = 1
			},
			maxFOV = 
			{
				outer = 50f,
				inner = 50f,
				upper = 50f,
				lower = 50f
			},
			distortion = 
			{
				k1 = 0.3f,
				k2 = 0f
			},
			inverse = ApproximateInverse(0.3f, 0f)
		};
		Default = new CardboardProfile
		{
			screen = Nexus5,
			device = CardboardJun2014
		};
	}
}
