using System.Collections.Generic;
using UnityEngine;

public class TableBidiIntFloatConverter : BidiIntFloatConverter
{
	public float defaultFloatIfIntNotExist;

	public TableBidiIntFloatConverter(float[] values, int intMin, int intMax, bool onlyShowPositive)
	{
		TableBidiIntFloatConverter tableBidiIntFloatConverter = this;
		Dictionary<int, float> table = new Dictionary<int, float>();
		for (int i = 0; i < Mathf.Min(values.Length, intMax - intMin + 1); i++)
		{
			int num = i + intMin;
			float num2 = values[i];
			table[num] = num2;
			if (onlyShowPositive)
			{
				table[-num] = 0f - num2;
			}
		}
		intToFloat = (int key) => table.TryGetValue(key, out var value) ? value : tableBidiIntFloatConverter.defaultFloatIfIntNotExist;
		floatToInt = delegate(float f)
		{
			int result = 0;
			float num3 = float.MaxValue;
			foreach (KeyValuePair<int, float> item in table)
			{
				float num4 = Mathf.Abs(item.Value - f);
				if (num4 < num3)
				{
					num3 = num4;
					result = item.Key;
				}
			}
			return result;
		};
	}
}
