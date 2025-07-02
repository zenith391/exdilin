using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class BWEncript
{
	private static int xorKeyLength = 23;

	private static byte[] xorKey = new byte[23]
	{
		157, 40, 243, 183, 126, 24, 225, 170, 100, 116,
		91, 122, 236, 18, 57, 11, 239, 57, 90, 113,
		119, 192, 56
	};

	public static string GetChecksumStr(byte[] inputData)
	{
		string text = null;
		using MD5 mD = MD5.Create();
		byte[] inputData2 = mD.ComputeHash(inputData);
		byte[] array = XOR(inputData2);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	private static byte[] XOR(byte[] inputData)
	{
		byte[] array = new byte[inputData.Length];
		for (int i = 0; i < inputData.Length; i++)
		{
			array[i] = (byte)(inputData[i] ^ xorKey[i % xorKeyLength]);
		}
		return array;
	}

	public static bool VerifyChecksum(string inputChecksum, byte[] inputData)
	{
		if (string.IsNullOrEmpty(inputChecksum) || inputData == null)
		{
			Debug.Log("No input checksum");
			return false;
		}
		string checksumStr = GetChecksumStr(inputData);
		StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
		bool flag = ordinalIgnoreCase.Compare(checksumStr, inputChecksum) == 0;
		if (!flag)
		{
			BWLog.Info("Checksum verification failed");
		}
		return flag;
	}
}
