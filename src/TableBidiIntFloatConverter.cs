using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000156 RID: 342
public class TableBidiIntFloatConverter : BidiIntFloatConverter
{
	// Token: 0x060014EA RID: 5354 RVA: 0x00092E34 File Offset: 0x00091234
	public TableBidiIntFloatConverter(float[] values, int intMin, int intMax, bool onlyShowPositive)
	{
		Dictionary<int, float> table = new Dictionary<int, float>();
		for (int j = 0; j < Mathf.Min(values.Length, intMax - intMin + 1); j++)
		{
			int num = j + intMin;
			float num2 = values[j];
			table[num] = num2;
			if (onlyShowPositive)
			{
				table[-num] = -num2;
			}
		}
		this.intToFloat = delegate(int i)
		{
			float result;
			if (table.TryGetValue(i, out result))
			{
				return result;
			}
			return this.defaultFloatIfIntNotExist;
		};
		this.floatToInt = delegate(float f)
		{
			int result = 0;
			float num3 = float.MaxValue;
			foreach (KeyValuePair<int, float> keyValuePair in table)
			{
				float num4 = Mathf.Abs(keyValuePair.Value - f);
				if (num4 < num3)
				{
					num3 = num4;
					result = keyValuePair.Key;
				}
			}
			return result;
		};
	}

	// Token: 0x0400106F RID: 4207
	public float defaultFloatIfIntNotExist;
}
