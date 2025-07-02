using SimpleJSON;
using UnityEngine;

internal static class JSONExtensions
{
	public static void ToJSON(this Vector3 v, JSONStreamEncoder encoder, bool roundToHalf = false, bool compact = false, bool useRenamings = false)
	{
		float f = ((!roundToHalf) ? v.x : (Mathf.Round(v.x * 2f) / 2f));
		float f2 = ((!roundToHalf) ? v.y : (Mathf.Round(v.y * 2f) / 2f));
		float f3 = ((!roundToHalf) ? v.z : (Mathf.Round(v.z * 2f) / 2f));
		if (compact)
		{
			if (useRenamings && SymbolCompat.compactVectorsInv.TryGetValue(v, out var value))
			{
				encoder.WriteNumber(value);
				return;
			}
			encoder.BeginArray();
			encoder.WriteNumber(f);
			encoder.WriteNumber(f2);
			encoder.WriteNumber(f3);
			encoder.EndArray();
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
		if (SymbolCompat.compactVectors.TryGetValue(intValue, out var value))
		{
			return value;
		}
		BWLog.Info("Could not find compact vector for " + intValue);
		return Vector3.zero;
	}

	public static Quaternion QuaternionValue(this JObject obj)
	{
		return new Quaternion((float)obj["x"], (float)obj["y"], (float)obj["z"], (float)obj["w"]);
	}
}
