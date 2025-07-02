using System;
using UnityEngine;

public class CellNoise
{
	private const uint OFFSET_BASIS = 2166136261u;

	private const uint FNV_PRIME = 16777619u;

	public static void CellNoiseFunc(Vector3 input, int seed, Func<Vector3, Vector3, float> distanceFunc, ref float[] distanceArray, int sizeX = 999999, int sizeY = 999999, int sizeZ = 999999)
	{
		for (int i = 0; i < distanceArray.Length; i++)
		{
			distanceArray[i] = 6666f;
		}
		int num = (int)Math.Floor(input.x);
		int num2 = (int)Math.Floor(input.y);
		int num3 = (int)Math.Floor(input.z);
		Vector3 vector = default(Vector3);
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				for (int l = -1; l < 2; l++)
				{
					int num4 = num + j;
					int num5 = num2 + k;
					int num6 = num3 + l;
					uint num7 = lcgRandom(hash((uint)((num4 + 14321) % sizeX + seed), (uint)((num5 + 34324) % sizeY), (uint)((num6 + 113231) % sizeZ)));
					uint num8 = probLookup(num7);
					for (uint num9 = 0u; num9 < num8; num9++)
					{
						num7 = lcgRandom(num7);
						vector.x = (float)num7 / 4.2949673E+09f;
						num7 = lcgRandom(num7);
						vector.y = (float)num7 / 4.2949673E+09f;
						num7 = lcgRandom(num7);
						vector.z = (float)num7 / 4.2949673E+09f;
						Vector3 arg = new Vector3(vector.x + (float)num4, vector.y + (float)num5, vector.z + (float)num6);
						insert(distanceArray, distanceFunc(input, arg));
					}
				}
			}
		}
	}

	public static float EuclidianDistanceFunc(Vector3 p1, Vector3 p2)
	{
		return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
	}

	public static float ManhattanDistanceFunc(Vector3 p1, Vector3 p2)
	{
		return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
	}

	public static float ChebyshevDistanceFunc(Vector3 p1, Vector3 p2)
	{
		Vector3 vector = p1 - p2;
		return Math.Max(Math.Max(Math.Abs(vector.x), Math.Abs(vector.y)), Math.Abs(vector.z));
	}

	private static uint probLookup(uint value)
	{
		if (value < 393325350)
		{
			return 1u;
		}
		if (value < 1022645910)
		{
			return 2u;
		}
		if (value < 1861739990)
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

	private static void insert(float[] arr, float value)
	{
		int num = arr.Length - 1;
		while (num >= 0 && !(value > arr[num]))
		{
			float num2 = arr[num];
			arr[num] = value;
			if (num + 1 < arr.Length)
			{
				arr[num + 1] = num2;
			}
			num--;
		}
	}

	private static uint lcgRandom(uint lastValue)
	{
		return (uint)((ulong)(1103515245 * lastValue + 12345) % 4294967296uL);
	}

	private static uint hash(uint i, uint j, uint k)
	{
		return (((((0x811C9DC5u ^ i) * 16777619) ^ j) * 16777619) ^ k) * 16777619;
	}
}
