using System.Collections.Generic;

public class BidiIntStringConverter
{
	private Dictionary<int, string> table = new Dictionary<int, string>();

	public BidiIntStringConverter(string[] strings, int startingValue, int step)
	{
		for (int i = 0; i < strings.Length; i++)
		{
			table[i * step + startingValue] = strings[i];
		}
	}

	public string StringValue(int key)
	{
		if (table.TryGetValue(key, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public int IntValue(string s)
	{
		foreach (KeyValuePair<int, string> item in table)
		{
			if (item.Value == s)
			{
				return item.Key;
			}
		}
		return 0;
	}
}
