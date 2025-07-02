using System;
using UnityEngine;

public class SimplexNoise
{
	private static int[][] grad3;

	private static int[][] grad4;

	private static int[] p;

	private static int[] perm;

	private static int[][] simplex;

	static SimplexNoise()
	{
		int[][] array = new int[12][];
		int num = 0;
		array[num] = new int[3] { 1, 1, 0 };
		int num2 = 1;
		array[num2] = new int[3] { -1, 1, 0 };
		int num3 = 2;
		array[num3] = new int[3] { 1, -1, 0 };
		int num4 = 3;
		array[num4] = new int[3] { -1, -1, 0 };
		array[4] = new int[3] { 1, 0, 1 };
		array[5] = new int[3] { -1, 0, 1 };
		array[6] = new int[3] { 1, 0, -1 };
		array[7] = new int[3] { -1, 0, -1 };
		array[8] = new int[3] { 0, 1, 1 };
		array[9] = new int[3] { 0, -1, 1 };
		array[10] = new int[3] { 0, 1, -1 };
		array[11] = new int[3] { 0, -1, -1 };
		grad3 = array;
		grad4 = new int[32][]
		{
			new int[4] { 0, 1, 1, 1 },
			new int[4] { 0, 1, 1, -1 },
			new int[4] { 0, 1, -1, 1 },
			new int[4] { 0, 1, -1, -1 },
			new int[4] { 0, -1, 1, 1 },
			new int[4] { 0, -1, 1, -1 },
			new int[4] { 0, -1, -1, 1 },
			new int[4] { 0, -1, -1, -1 },
			new int[4] { 1, 0, 1, 1 },
			new int[4] { 1, 0, 1, -1 },
			new int[4] { 1, 0, -1, 1 },
			new int[4] { 1, 0, -1, -1 },
			new int[4] { -1, 0, 1, 1 },
			new int[4] { -1, 0, 1, -1 },
			new int[4] { -1, 0, -1, 1 },
			new int[4] { -1, 0, -1, -1 },
			new int[4] { 1, 1, 0, 1 },
			new int[4] { 1, 1, 0, -1 },
			new int[4] { 1, -1, 0, 1 },
			new int[4] { 1, -1, 0, -1 },
			new int[4] { -1, 1, 0, 1 },
			new int[4] { -1, 1, 0, -1 },
			new int[4] { -1, -1, 0, 1 },
			new int[4] { -1, -1, 0, -1 },
			new int[4] { 1, 1, 1, 0 },
			new int[4] { 1, 1, -1, 0 },
			new int[4] { 1, -1, 1, 0 },
			new int[4] { 1, -1, -1, 0 },
			new int[4] { -1, 1, 1, 0 },
			new int[4] { -1, 1, -1, 0 },
			new int[4] { -1, -1, 1, 0 },
			new int[4] { -1, -1, -1, 0 }
		};
		p = new int[256]
		{
			151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
			96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
			69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
			6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
			94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
			33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
			171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
			27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
			60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
			245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
			1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
			18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
			164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
			226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
			255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
			58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
			119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
			101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
			19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
			112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
			238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
			145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
			181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
			50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
			222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
			78, 66, 215, 61, 156, 180
		};
		perm = new int[512];
		simplex = new int[64][]
		{
			new int[4] { 0, 1, 2, 3 },
			new int[4] { 0, 1, 3, 2 },
			new int[4],
			new int[4] { 0, 2, 3, 1 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 1, 2, 3, 0 },
			new int[4] { 0, 2, 1, 3 },
			new int[4],
			new int[4] { 0, 3, 1, 2 },
			new int[4] { 0, 3, 2, 1 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 1, 3, 2, 0 },
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 1, 2, 0, 3 },
			new int[4],
			new int[4] { 1, 3, 0, 2 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 2, 3, 0, 1 },
			new int[4] { 2, 3, 1, 0 },
			new int[4] { 1, 0, 2, 3 },
			new int[4] { 1, 0, 3, 2 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 2, 0, 3, 1 },
			new int[4],
			new int[4] { 2, 1, 3, 0 },
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 2, 0, 1, 3 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 3, 0, 1, 2 },
			new int[4] { 3, 0, 2, 1 },
			new int[4],
			new int[4] { 3, 1, 2, 0 },
			new int[4] { 2, 1, 0, 3 },
			new int[4],
			new int[4],
			new int[4],
			new int[4] { 3, 1, 0, 2 },
			new int[4],
			new int[4] { 3, 2, 0, 1 },
			new int[4] { 3, 2, 1, 0 }
		};
		for (int i = 0; i < 512; i++)
		{
			perm[i] = p[i & 0xFF];
		}
	}

	private static int fastfloor(float x)
	{
		if (!(x <= 0f))
		{
			return (int)x;
		}
		return (int)x - 1;
	}

	private static float dot(int[] g, float x, float y)
	{
		return (float)g[0] * x + (float)g[1] * y;
	}

	private static float dot(int[] g, float x, float y, float z)
	{
		return (float)g[0] * x + (float)g[1] * y + (float)g[2] * z;
	}

	private static float dot(int[] g, float x, float y, float z, float w)
	{
		return (float)g[0] * x + (float)g[1] * y + (float)g[2] * z + (float)g[3] * w;
	}

	public static float Noise(float xin, float yin)
	{
		float num = 0.5f * (Mathf.Sqrt(3f) - 1f);
		float num2 = (xin + yin) * num;
		int num3 = fastfloor(xin + num2);
		int num4 = fastfloor(yin + num2);
		float num5 = (3f - Mathf.Sqrt(3f)) / 6f;
		float num6 = (float)(num3 + num4) * num5;
		float num7 = (float)num3 - num6;
		float num8 = (float)num4 - num6;
		float num9 = xin - num7;
		float num10 = yin - num8;
		int num11;
		int num12;
		if (num9 > num10)
		{
			num11 = 1;
			num12 = 0;
		}
		else
		{
			num11 = 0;
			num12 = 1;
		}
		float num13 = num9 - (float)num11 + num5;
		float num14 = num10 - (float)num12 + num5;
		float num15 = num9 - 1f + 2f * num5;
		float num16 = num10 - 1f + 2f * num5;
		int num17 = num3 & 0xFF;
		int num18 = num4 & 0xFF;
		int num19 = perm[num17 + perm[num18]] % 12;
		int num20 = perm[num17 + num11 + perm[num18 + num12]] % 12;
		int num21 = perm[num17 + 1 + perm[num18 + 1]] % 12;
		float num22 = 0.5f - num9 * num9 - num10 * num10;
		float num23;
		if (num22 < 0f)
		{
			num23 = 0f;
		}
		else
		{
			num22 *= num22;
			num23 = num22 * num22 * dot(grad3[num19], num9, num10);
		}
		float num24 = 0.5f - num13 * num13 - num14 * num14;
		float num25;
		if (num24 < 0f)
		{
			num25 = 0f;
		}
		else
		{
			num24 *= num24;
			num25 = num24 * num24 * dot(grad3[num20], num13, num14);
		}
		float num26 = 0.5f - num15 * num15 - num16 * num16;
		float num27;
		if (num26 < 0f)
		{
			num27 = 0f;
		}
		else
		{
			num26 *= num26;
			num27 = num26 * num26 * dot(grad3[num21], num15, num16);
		}
		return 70f * (num23 + num25 + num27);
	}

	public static float Noise(float xin, float yin, float zin)
	{
		float num = (xin + yin + zin) * (1f / 3f);
		int num2 = fastfloor(xin + num);
		int num3 = fastfloor(yin + num);
		int num4 = fastfloor(zin + num);
		float num5 = (float)(num2 + num3 + num4) * (1f / 6f);
		float num6 = (float)num2 - num5;
		float num7 = (float)num3 - num5;
		float num8 = (float)num4 - num5;
		float num9 = xin - num6;
		float num10 = yin - num7;
		float num11 = zin - num8;
		int num12;
		int num13;
		int num14;
		int num15;
		int num16;
		int num17;
		if (num9 >= num10)
		{
			if (num10 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			else if (num9 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
		}
		else if (num10 < num11)
		{
			num12 = 0;
			num13 = 0;
			num14 = 1;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else if (num9 < num11)
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 1;
			num16 = 1;
			num17 = 0;
		}
		float num18 = num9 - (float)num12 + 1f / 6f;
		float num19 = num10 - (float)num13 + 1f / 6f;
		float num20 = num11 - (float)num14 + 1f / 6f;
		float num21 = num9 - (float)num15 + 1f / 3f;
		float num22 = num10 - (float)num16 + 1f / 3f;
		float num23 = num11 - (float)num17 + 1f / 3f;
		float num24 = num9 - 1f + 0.5f;
		float num25 = num10 - 1f + 0.5f;
		float num26 = num11 - 1f + 0.5f;
		int num27 = num2 & 0xFF;
		int num28 = num3 & 0xFF;
		int num29 = num4 & 0xFF;
		int num30 = perm[num27 + perm[num28 + perm[num29]]] % 12;
		int num31 = perm[num27 + num12 + perm[num28 + num13 + perm[num29 + num14]]] % 12;
		int num32 = perm[num27 + num15 + perm[num28 + num16 + perm[num29 + num17]]] % 12;
		int num33 = perm[num27 + 1 + perm[num28 + 1 + perm[num29 + 1]]] % 12;
		float num34 = 0.6f - num9 * num9 - num10 * num10 - num11 * num11;
		float num35;
		if (num34 < 0f)
		{
			num35 = 0f;
		}
		else
		{
			num34 *= num34;
			num35 = num34 * num34 * dot(grad3[num30], num9, num10, num11);
		}
		float num36 = 0.6f - num18 * num18 - num19 * num19 - num20 * num20;
		float num37;
		if (num36 < 0f)
		{
			num37 = 0f;
		}
		else
		{
			num36 *= num36;
			num37 = num36 * num36 * dot(grad3[num31], num18, num19, num20);
		}
		float num38 = 0.6f - num21 * num21 - num22 * num22 - num23 * num23;
		float num39;
		if (num38 < 0f)
		{
			num39 = 0f;
		}
		else
		{
			num38 *= num38;
			num39 = num38 * num38 * dot(grad3[num32], num21, num22, num23);
		}
		float num40 = 0.6f - num24 * num24 - num25 * num25 - num26 * num26;
		float num41;
		if (num40 < 0f)
		{
			num41 = 0f;
		}
		else
		{
			num40 *= num40;
			num41 = num40 * num40 * dot(grad3[num33], num24, num25, num26);
		}
		return 32f * (num35 + num37 + num39 + num41);
	}

	public static float Noise(float x, float y, float z, float w)
	{
		float num = (Mathf.Sqrt(5f) - 1f) / 4f;
		float num2 = (5f - Mathf.Sqrt(5f)) / 20f;
		float num3 = (x + y + z + w) * num;
		int num4 = fastfloor(x + num3);
		int num5 = fastfloor(y + num3);
		int num6 = fastfloor(z + num3);
		int num7 = fastfloor(w + num3);
		float num8 = (float)(num4 + num5 + num6 + num7) * num2;
		float num9 = (float)num4 - num8;
		float num10 = (float)num5 - num8;
		float num11 = (float)num6 - num8;
		float num12 = (float)num7 - num8;
		float num13 = x - num9;
		float num14 = y - num10;
		float num15 = z - num11;
		float num16 = w - num12;
		int num17 = ((!(num13 <= num14)) ? 32 : 0);
		int num18 = ((!(num13 <= num15)) ? 16 : 0);
		int num19 = ((!(num14 <= num15)) ? 8 : 0);
		int num20 = ((!(num13 <= num16)) ? 4 : 0);
		int num21 = ((!(num14 <= num16)) ? 2 : 0);
		int num22 = ((!(num15 <= num16)) ? 1 : 0);
		int num23 = num17 + num18 + num19 + num20 + num21 + num22;
		int num24 = ((simplex[num23][0] >= 3) ? 1 : 0);
		int num25 = ((simplex[num23][1] >= 3) ? 1 : 0);
		int num26 = ((simplex[num23][2] >= 3) ? 1 : 0);
		int num27 = ((simplex[num23][3] >= 3) ? 1 : 0);
		int num28 = ((simplex[num23][0] >= 2) ? 1 : 0);
		int num29 = ((simplex[num23][1] >= 2) ? 1 : 0);
		int num30 = ((simplex[num23][2] >= 2) ? 1 : 0);
		int num31 = ((simplex[num23][3] >= 2) ? 1 : 0);
		int num32 = ((simplex[num23][0] >= 1) ? 1 : 0);
		int num33 = ((simplex[num23][1] >= 1) ? 1 : 0);
		int num34 = ((simplex[num23][2] >= 1) ? 1 : 0);
		int num35 = ((simplex[num23][3] >= 1) ? 1 : 0);
		float num36 = num13 - (float)num24 + num2;
		float num37 = num14 - (float)num25 + num2;
		float num38 = num15 - (float)num26 + num2;
		float num39 = num16 - (float)num27 + num2;
		float num40 = num13 - (float)num28 + 2f * num2;
		float num41 = num14 - (float)num29 + 2f * num2;
		float num42 = num15 - (float)num30 + 2f * num2;
		float num43 = num16 - (float)num31 + 2f * num2;
		float num44 = num13 - (float)num32 + 3f * num2;
		float num45 = num14 - (float)num33 + 3f * num2;
		float num46 = num15 - (float)num34 + 3f * num2;
		float num47 = num16 - (float)num35 + 3f * num2;
		float num48 = num13 - 1f + 4f * num2;
		float num49 = num14 - 1f + 4f * num2;
		float num50 = num15 - 1f + 4f * num2;
		float num51 = num16 - 1f + 4f * num2;
		int num52 = num4 & 0xFF;
		int num53 = num5 & 0xFF;
		int num54 = num6 & 0xFF;
		int num55 = num7 & 0xFF;
		int num56 = perm[num52 + perm[num53 + perm[num54 + perm[num55]]]] % 32;
		int num57 = perm[num52 + num24 + perm[num53 + num25 + perm[num54 + num26 + perm[num55 + num27]]]] % 32;
		int num58 = perm[num52 + num28 + perm[num53 + num29 + perm[num54 + num30 + perm[num55 + num31]]]] % 32;
		int num59 = perm[num52 + num32 + perm[num53 + num33 + perm[num54 + num34 + perm[num55 + num35]]]] % 32;
		int num60 = perm[num52 + 1 + perm[num53 + 1 + perm[num54 + 1 + perm[num55 + 1]]]] % 32;
		float num61 = 0.6f - num13 * num13 - num14 * num14 - num15 * num15 - num16 * num16;
		float num62;
		if (num61 < 0f)
		{
			num62 = 0f;
		}
		else
		{
			num61 *= num61;
			num62 = num61 * num61 * dot(grad4[num56], num13, num14, num15, num16);
		}
		float num63 = 0.6f - num36 * num36 - num37 * num37 - num38 * num38 - num39 * num39;
		float num64;
		if (num63 < 0f)
		{
			num64 = 0f;
		}
		else
		{
			num63 *= num63;
			num64 = num63 * num63 * dot(grad4[num57], num36, num37, num38, num39);
		}
		float num65 = 0.6f - num40 * num40 - num41 * num41 - num42 * num42 - num43 * num43;
		float num66;
		if (num65 < 0f)
		{
			num66 = 0f;
		}
		else
		{
			num65 *= num65;
			num66 = num65 * num65 * dot(grad4[num58], num40, num41, num42, num43);
		}
		float num67 = 0.6f - num44 * num44 - num45 * num45 - num46 * num46 - num47 * num47;
		float num68;
		if (num67 < 0f)
		{
			num68 = 0f;
		}
		else
		{
			num67 *= num67;
			num68 = num67 * num67 * dot(grad4[num59], num44, num45, num46, num47);
		}
		float num69 = 0.6f - num48 * num48 - num49 * num49 - num50 * num50 - num51 * num51;
		float num70;
		if (num69 < 0f)
		{
			num70 = 0f;
		}
		else
		{
			num69 *= num69;
			num70 = num69 * num69 * dot(grad4[num60], num48, num49, num50, num51);
		}
		return 27f * (num62 + num64 + num66 + num68 + num70);
	}

	public static float BlurredNoise(float stepSize, float x, float y)
	{
		int num = 0;
		float num2 = 0f;
		for (float num3 = x - stepSize; num3 <= x + stepSize; num3 += stepSize)
		{
			for (float num4 = y - stepSize; num4 <= y + stepSize; num4 += stepSize)
			{
				num2 += Noise(num3, num4);
				num++;
			}
		}
		return num2 / (float)num;
	}

	public static float BlurredNoise(float stepSize, float x, float y, float z, float w)
	{
		int num = 0;
		float num2 = 0f;
		for (float num3 = x - stepSize; num3 <= x + stepSize; num3 += stepSize)
		{
			for (float num4 = y - stepSize; num4 <= y + stepSize; num4 += stepSize)
			{
				for (float num5 = z - stepSize; num5 <= z + stepSize; num5 += stepSize)
				{
					for (float num6 = w - stepSize; num6 <= w + stepSize; num6 += stepSize)
					{
						num2 += Noise(num3, num4, num5, num6);
						num++;
					}
				}
			}
		}
		return num2 / (float)num;
	}

	public static float SeamlessNoise(float x, float y, float dx, float dy, float xyOffset)
	{
		float x2 = xyOffset + Mathf.Cos(x * 2f * (float)Math.PI) * dx / ((float)Math.PI * 2f);
		float y2 = xyOffset + Mathf.Cos(y * 2f * (float)Math.PI) * dy / ((float)Math.PI * 2f);
		float z = xyOffset + Mathf.Sin(x * 2f * (float)Math.PI) * dx / ((float)Math.PI * 2f);
		float w = xyOffset + Mathf.Sin(y * 2f * (float)Math.PI) * dy / ((float)Math.PI * 2f);
		return Noise(x2, y2, z, w);
	}
}
