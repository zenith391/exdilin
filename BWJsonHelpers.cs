using System;
using System.Collections.Generic;
using System.Globalization;
using SimpleJSON;

// Token: 0x020003A6 RID: 934
public static class BWJsonHelpers
{
	// Token: 0x0600289E RID: 10398 RVA: 0x0012ACB2 File Offset: 0x001290B2
	public static BlocksInventory PropertyIfExists(BlocksInventory property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			return BlocksInventory.FromString(json[key].StringValue, true);
		}
		return property;
	}

	// Token: 0x0600289F RID: 10399 RVA: 0x0012ACD4 File Offset: 0x001290D4
	public static bool PropertyIfExists(bool property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jobject = json[key];
		if (jobject.Kind == JObjectKind.Boolean)
		{
			return jobject.BooleanValue;
		}
		if (jobject.Kind == JObjectKind.Number)
		{
			return jobject.IntValue > 0;
		}
		if (jobject.Kind == JObjectKind.String)
		{
			return jobject.StringValue.ToLowerInvariant() == "true";
		}
		return property;
	}

	// Token: 0x060028A0 RID: 10400 RVA: 0x0012AD50 File Offset: 0x00129150
	public static int PropertyIfExists(int property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jobject = json[key];
		if (jobject.Kind == JObjectKind.String)
		{
			int result = 0;
			if (!int.TryParse(jobject.StringValue, out result))
			{
				BWLog.Error("Failed to parse int property: " + key + " value " + jobject.StringValue);
			}
			return result;
		}
		return jobject.IntValue;
	}

	// Token: 0x060028A1 RID: 10401 RVA: 0x0012ADB8 File Offset: 0x001291B8
	public static float PropertyIfExists(float property, string key, JObject json)
	{
		if (!json.ContainsKey(key))
		{
			return property;
		}
		JObject jobject = json[key];
		if (jobject.Kind == JObjectKind.String)
		{
			float result = 0f;
			if (!float.TryParse(jobject.StringValue, out result))
			{
				BWLog.Error("Failed to parse float property: " + key + " value " + jobject.StringValue);
			}
			return result;
		}
		return jobject.FloatValue;
	}

	// Token: 0x060028A2 RID: 10402 RVA: 0x0012AE24 File Offset: 0x00129224
	public static string IDPropertyAsStringIfExists(string property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jobject = json[key];
			if (jobject.Kind == JObjectKind.Number)
			{
				int intValue = jobject.IntValue;
				if (intValue == 0)
				{
					return null;
				}
				return intValue.ToString();
			}
			else if (jobject.Kind == JObjectKind.String)
			{
				return jobject.StringValue;
			}
		}
		return property;
	}

	// Token: 0x060028A3 RID: 10403 RVA: 0x0012AE82 File Offset: 0x00129282
	public static string PropertyIfExists(string property, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			return json[key].StringValue;
		}
		return property;
	}

	// Token: 0x060028A4 RID: 10404 RVA: 0x0012AEA0 File Offset: 0x001292A0
	public static string PropertyIfExists(string property, string key1, string key2, JObject json)
	{
		if (json.ContainsKey(key1))
		{
			JObject jobject = json[key1];
			if (jobject.Kind == JObjectKind.Object && jobject.ContainsKey(key2))
			{
				return Util.FixNonAscii(jobject[key2].StringValue);
			}
		}
		return property;
	}

	// Token: 0x060028A5 RID: 10405 RVA: 0x0012AEEC File Offset: 0x001292EC
	public static DateTime PropertyIfExists(DateTime property, string key, JObject json)
	{
		string text = null;
		text = BWJsonHelpers.PropertyIfExists(text, key, json);
		if (!string.IsNullOrEmpty(text))
		{
			DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out property);
		}
		return property;
	}

	// Token: 0x060028A6 RID: 10406 RVA: 0x0012AF20 File Offset: 0x00129320
	public static List<string> PropertyIfExists(List<string> list, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jobject = json[key];
			if (jobject.Kind == JObjectKind.Array)
			{
				List<string> list2 = new List<string>();
				foreach (JObject jobject2 in jobject.ArrayValue)
				{
					list2.Add(jobject2.ToString());
				}
				return list2;
			}
		}
		return list;
	}

	// Token: 0x060028A7 RID: 10407 RVA: 0x0012AFAC File Offset: 0x001293AC
	public static List<int> PropertyIfExists(List<int> list, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			JObject jobject = json[key];
			if (jobject.Kind == JObjectKind.Array)
			{
				List<int> list2 = new List<int>();
				foreach (JObject jobject2 in jobject.ArrayValue)
				{
					if (jobject2.Kind == JObjectKind.Number)
					{
						list2.Add(jobject2.IntValue);
					}
				}
				return list2;
			}
		}
		return list;
	}

	// Token: 0x060028A8 RID: 10408 RVA: 0x0012B044 File Offset: 0x00129444
	public static void AddForEachInArray<T>(List<T> list, string key, JObject json, Func<JObject, T> newT)
	{
		if (json.ContainsKey(key))
		{
			List<JObject> arrayValue = json[key].ArrayValue;
			foreach (JObject arg in arrayValue)
			{
				list.Add(newT(arg));
			}
		}
	}

	// Token: 0x060028A9 RID: 10409 RVA: 0x0012B0BC File Offset: 0x001294BC
	public static void AddForEachInArray(List<int> list, string key, JObject json)
	{
		if (json.ContainsKey(key))
		{
			List<JObject> arrayValue = json[key].ArrayValue;
			foreach (JObject jobject in arrayValue)
			{
				list.Add(jobject.IntValue);
			}
		}
	}
}
