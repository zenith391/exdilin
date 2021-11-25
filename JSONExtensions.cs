using System;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000162 RID: 354
internal static class JSONExtensions
{
	// Token: 0x06001541 RID: 5441 RVA: 0x00094690 File Offset: 0x00092A90
	public static void ToJSON(this Vector3 v, JSONStreamEncoder encoder, bool roundToHalf = false, bool compact = false, bool useRenamings = false)
	{
		float f = (!roundToHalf) ? v.x : (Mathf.Round(v.x * 2f) / 2f);
		float f2 = (!roundToHalf) ? v.y : (Mathf.Round(v.y * 2f) / 2f);
		float f3 = (!roundToHalf) ? v.z : (Mathf.Round(v.z * 2f) / 2f);
		if (compact)
		{
			int num;
			if (useRenamings && SymbolCompat.compactVectorsInv.TryGetValue(v, out num))
			{
				encoder.WriteNumber((long)num);
			}
			else
			{
				encoder.BeginArray();
				encoder.WriteNumber(f);
				encoder.WriteNumber(f2);
				encoder.WriteNumber(f3);
				encoder.EndArray();
			}
		}
		else
		{
			encoder.BeginObject();
			encoder.WriteKey("x");
			encoder.WriteNumber(f);
			encoder.WriteKey("y");
			encoder.WriteNumber(f2);
			encoder.WriteKey("z");
			encoder.WriteNumber(f3);
			encoder.EndObject();
		}
	}

	// Token: 0x06001542 RID: 5442 RVA: 0x000947B4 File Offset: 0x00092BB4
	public static void ToJSON(this Quaternion q, JSONStreamEncoder encoder)
	{
		encoder.BeginObject();
		encoder.WriteKey("x");
		encoder.WriteNumber(q.x);
		encoder.WriteKey("y");
		encoder.WriteNumber(q.y);
		encoder.WriteKey("z");
		encoder.WriteNumber(q.z);
		encoder.WriteKey("w");
		encoder.WriteNumber(q.w);
		encoder.EndObject();
	}

	// Token: 0x06001543 RID: 5443 RVA: 0x00094830 File Offset: 0x00092C30
	public static Vector3 Vector3Value(this JObject obj)
	{
		if (obj.ArrayValue != null)
		{
			return new Vector3((float)obj[0], (float)obj[1], (float)obj[2]);
		}
		if (obj.ObjectValue != null)
		{
			return new Vector3((float)obj["x"], (float)obj["y"], (float)obj["z"]);
		}
		int intValue = obj.IntValue;
		Vector3 result;
		if (SymbolCompat.compactVectors.TryGetValue(intValue, out result))
		{
			return result;
		}
		BWLog.Info("Could not find compact vector for " + intValue);
		return Vector3.zero;
	}

	// Token: 0x06001544 RID: 5444 RVA: 0x000948E8 File Offset: 0x00092CE8
	public static Quaternion QuaternionValue(this JObject obj)
	{
		return new Quaternion((float)obj["x"], (float)obj["y"], (float)obj["z"], (float)obj["w"]);
	}
}
