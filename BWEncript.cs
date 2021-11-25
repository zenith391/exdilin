using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Token: 0x020003A4 RID: 932
public class BWEncript
{
	// Token: 0x06002891 RID: 10385 RVA: 0x0012A9C0 File Offset: 0x00128DC0
	public static string GetChecksumStr(byte[] inputData)
	{
		string result = null;
		using (MD5 md = MD5.Create())
		{
			byte[] inputData2 = md.ComputeHash(inputData);
			byte[] array = BWEncript.XOR(inputData2);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			result = stringBuilder.ToString();
		}
		return result;
	}

	// Token: 0x06002892 RID: 10386 RVA: 0x0012AA48 File Offset: 0x00128E48
	private static byte[] XOR(byte[] inputData)
	{
		byte[] array = new byte[inputData.Length];
		for (int i = 0; i < inputData.Length; i++)
		{
			array[i] = (byte) (inputData[i] ^ BWEncript.xorKey[i % BWEncript.xorKeyLength]);
		}
		return array;
	}

	// Token: 0x06002893 RID: 10387 RVA: 0x0012AA88 File Offset: 0x00128E88
	public static bool VerifyChecksum(string inputChecksum, byte[] inputData)
	{
		if (string.IsNullOrEmpty(inputChecksum) || inputData == null)
		{
			Debug.Log("No input checksum");
			return false;
		}
		string checksumStr = BWEncript.GetChecksumStr(inputData);
		StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
		bool flag = 0 == ordinalIgnoreCase.Compare(checksumStr, inputChecksum);
		if (!flag)
		{
			BWLog.Info("Checksum verification failed");
		}
		return flag;
	}

	// Token: 0x0400237D RID: 9085
	private static int xorKeyLength = 23;

	// Token: 0x0400237E RID: 9086
	private static byte[] xorKey = new byte[]
	{
		157,
		40,
		243,
		183,
		126,
		24,
		225,
		170,
		100,
		116,
		91,
		122,
		236,
		18,
		57,
		11,
		239,
		57,
		90,
		113,
		119,
		192,
		56
	};
}
