using System;
using UnityEngine;

// Token: 0x0200010D RID: 269
public class CellNoise
{
	// Token: 0x06001316 RID: 4886 RVA: 0x00082588 File Offset: 0x00080988
	public static void CellNoiseFunc(Vector3 input, int seed, Func<Vector3, Vector3, float> distanceFunc, ref float[] distanceArray, int sizeX = 999999, int sizeY = 999999, int sizeZ = 999999)
	{
		for (int i = 0; i < distanceArray.Length; i++)
		{
			distanceArray[i] = 6666f;
		}
		int num = (int)Math.Floor((double)input.x);
		int num2 = (int)Math.Floor((double)input.y);
		int num3 = (int)Math.Floor((double)input.z);
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				for (int l = -1; l < 2; l++)
				{
					int num4 = num + j;
					int num5 = num2 + k;
					int num6 = num3 + l;
					uint num7 = CellNoise.lcgRandom(CellNoise.hash((uint)((num4 + 14321) % sizeX + seed), (uint)((num5 + 34324) % sizeY), (uint)((num6 + 113231) % sizeZ)));
					uint num8 = CellNoise.probLookup(num7);
					for (uint num9 = 0u; num9 < num8; num9 += 1u)
					{
						num7 = CellNoise.lcgRandom(num7);
						Vector3 vector;
						vector.x = num7 / 4.2949673E+09f;
						num7 = CellNoise.lcgRandom(num7);
						vector.y = num7 / 4.2949673E+09f;
						num7 = CellNoise.lcgRandom(num7);
						vector.z = num7 / 4.2949673E+09f;
						Vector3 arg = new Vector3(vector.x + (float)num4, vector.y + (float)num5, vector.z + (float)num6);
						CellNoise.insert(distanceArray, distanceFunc(input, arg));
					}
				}
			}
		}
	}

	// Token: 0x06001317 RID: 4887 RVA: 0x00082708 File Offset: 0x00080B08
	public static float EuclidianDistanceFunc(Vector3 p1, Vector3 p2)
	{
		return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
	}

	// Token: 0x06001318 RID: 4888 RVA: 0x00082774 File Offset: 0x00080B74
	public static float ManhattanDistanceFunc(Vector3 p1, Vector3 p2)
	{
		return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
	}

	// Token: 0x06001319 RID: 4889 RVA: 0x000827B4 File Offset: 0x00080BB4
	public static float ChebyshevDistanceFunc(Vector3 p1, Vector3 p2)
	{
		Vector3 vector = p1 - p2;
		return Math.Max(Math.Max(Math.Abs(vector.x), Math.Abs(vector.y)), Math.Abs(vector.z));
	}

	// Token: 0x0600131A RID: 4890 RVA: 0x000827F8 File Offset: 0x00080BF8
	private static uint probLookup(uint value)
	{
		if (value < 393325350u)
		{
			return 1u;
		}
		if (value < 1022645910u)
		{
			return 2u;
		}
		if (value < 1861739990u)
		{
			return 3u;
		}
		if (value < 2700834071u)
		{
			return 4u;
		}
		if (value < 3372109335u)
		{
			return 5u;
		}
		if (value < 3819626178u)
		{
			return 6u;
		}
		if (value < 4075350088u)
		{
			return 7u;
		}
		if (value < 4203212043u)
		{
			return 8u;
		}
		return 9u;
	}

	// Token: 0x0600131B RID: 4891 RVA: 0x00082870 File Offset: 0x00080C70
	private static void insert(float[] arr, float value)
	{
		for (int i = arr.Length - 1; i >= 0; i--)
		{
			if (value > arr[i])
			{
				break;
			}
			float num = arr[i];
			arr[i] = value;
			if (i + 1 < arr.Length)
			{
				arr[i + 1] = num;
			}
		}
	}

	// Token: 0x0600131C RID: 4892 RVA: 0x000828BA File Offset: 0x00080CBA
	private static uint lcgRandom(uint lastValue)
	{
		return (uint)((ulong)(1103515245u * lastValue + 12345u) % 4294967296UL);
	}

	// Token: 0x0600131D RID: 4893 RVA: 0x000828D5 File Offset: 0x00080CD5
	private static uint hash(uint i, uint j, uint k)
	{
		return (((2166136261u ^ i) * 16777619u ^ j) * 16777619u ^ k) * 16777619u;
	}

	// Token: 0x04000F06 RID: 3846
	private const uint OFFSET_BASIS = 2166136261u;

	// Token: 0x04000F07 RID: 3847
	private const uint FNV_PRIME = 16777619u;
}
