using System;
using System.Collections.Generic;
using System.Globalization;
using SimpleJSON;

public static class BWJsonHelpers
{
	public static BlocksInventory PropertyIfExists(BlocksInventory property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			return BlocksInventory.FromString(json[key].StringValue);
		}
		return property;
	}

	public static bool PropertyIfExists(bool property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jObject = json[key];
		if (jObject.Kind == JObjectKind.Boolean)
		{
			return jObject.BooleanValue;
		}
		if (jObject.Kind == JObjectKind.Number)
		{
			return jObject.IntValue > 0;
		}
		if (jObject.Kind == JObjectKind.String)
		{
			return jObject.StringValue.ToLowerInvariant() == "true";
		}
		return property;
	}

	public static int PropertyIfExists(int property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jObject = json[key];
		if (jObject.Kind == JObjectKind.String)
		{
			int result = 0;
			if (!int.TryParse(jObject.StringValue, out result))
			{
				BWLog.Error("Failed to parse int property: " + key + " value " + jObject.StringValue);
			}
			return result;
		}
		return jObject.IntValue;
	}

	public static float PropertyIfExists(float property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jObject = json[key];
		if (jObject.Kind == JObjectKind.String)
		{
			float result = 0f;
			if (!float.TryParse(jObject.StringValue, out result))
			{
				BWLog.Error("Failed to parse float property: " + key + " value " + jObject.StringValue);
			}
			return result;
		}
		return jObject.FloatValue;
	}

	public static string IDPropertyAsStringIfExists(string property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jObject = json[key];
			if (jObject.Kind == JObjectKind.Number)
			{
				int intValue = jObject.IntValue;
				if (intValue == 0)
				{
					return null;
				}
				return intValue.ToString();
			}
			if (jObject.Kind == JObjectKind.String)
			{
				return jObject.StringValue;
			}
		}
		return property;
	}

	public static string PropertyIfExists(string property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			return json[key].StringValue;
		}
		return property;
	}

	public static string PropertyIfExists(string property, string key1, string key2, JObject json)
	{
		if (json.ContainsKey(key1))
		{
			JObject jObject = json[key1];
			if (jObject.Kind == JObjectKind.Object && jObject.ContainsKey(key2))
			{
				return Util.FixNonAscii(jObject[key2].StringValue);
			}
		}
		return property;
	}

	public static DateTime PropertyIfExists(DateTime property, string key, JObject json)
	{
		string property2 = null;
		property2 = PropertyIfExists(property2, key, json);
		if (!string.IsNullOrEmpty(property2))
		{
			DateTime.TryParse(property2, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out property);
		}
		return property;
	}

	public static List<string> PropertyIfExists(List<string> list, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jObject = json[key];
			if (jObject.Kind == JObjectKind.Array)
			{
				List<string> list2 = new List<string>();
				{
					foreach (JObject item in jObject.ArrayValue)
					{
						list2.Add(item.ToString());
					}
					return list2;
				}
			}
		}
		return list;
	}

	public static List<int> PropertyIfExists(List<int> list, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jObject = json[key];
			if (jObject.Kind == JObjectKind.Array)
			{
				List<int> list2 = new List<int>();
				{
					foreach (JObject item in jObject.ArrayValue)
					{
						if (item.Kind == JObjectKind.Number)
						{
							list2.Add(item.IntValue);
						}
					}
					return list2;
				}
			}
		}
		return list;
	}

	public static void AddForEachInArray<T>(List<T> list, string key, JObject json, Func<JObject, T> newT)
	{
		if (!json.ContainsKey(key))
		{
			return;
		}
		List<JObject> arrayValue = json[key].ArrayValue;
		foreach (JObject item in arrayValue)
		{
			list.Add(newT(item));
		}
	}

	public static void AddForEachInArray(List<int> list, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return;
		}
		List<JObject> arrayValue = json[key].ArrayValue;
		foreach (JObject item in arrayValue)
		{
			list.Add(item.IntValue);
		}
	}
}
