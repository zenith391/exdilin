using System;
using System.Collections.Generic;

// Token: 0x02000153 RID: 339
public class BidiIntStringConverter
{
	// Token: 0x060014DE RID: 5342 RVA: 0x00092C44 File Offset: 0x00091044
	public BidiIntStringConverter(string[] strings, int startingValue, int step)
	{
		for (int i = 0; i < strings.Length; i++)
		{
			this.table[i * step + startingValue] = strings[i];
		}
	}

	// Token: 0x060014DF RID: 5343 RVA: 0x00092C8C File Offset: 0x0009108C
	public string StringValue(int key)
	{
		string result;
		if (this.table.TryGetValue(key, out result))
		{
			return result;
		}
		return string.Empty;
	}

	// Token: 0x060014E0 RID: 5344 RVA: 0x00092CB4 File Offset: 0x000910B4
	public int IntValue(string s)
	{
		foreach (KeyValuePair<int, string> keyValuePair in this.table)
		{
			if (keyValuePair.Value == s)
			{
				return keyValuePair.Key;
			}
		}
		return 0;
	}

	// Token: 0x04001068 RID: 4200
	private Dictionary<int, string> table = new Dictionary<int, string>();
}
