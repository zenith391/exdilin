using System.Collections.Generic;

public class ObjectData
{
	private Dictionary<string, bool> bools = new Dictionary<string, bool>();

	private Dictionary<string, float> floats = new Dictionary<string, float>();

	private Dictionary<string, string> strings = new Dictionary<string, string>();

	public string GetString(string key)
	{
		if (strings.TryGetValue(key, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public void SetString(string key, string s)
	{
		strings[key] = s;
	}

	public bool TryGetString(string key, out string value)
	{
		return strings.TryGetValue(key, out value);
	}

	public bool GetBoolean(string key)
	{
		bool value;
		return bools.TryGetValue(key, out value) && value;
	}

	public void SetBoolean(string key, bool b)
	{
		bools[key] = b;
	}

	public bool TryGetFloat(string key, out float value)
	{
		return floats.TryGetValue(key, out value);
	}

	public float GetFloat(string key)
	{
		if (floats.TryGetValue(key, out var value))
		{
			return value;
		}
		return 0f;
	}

	public void SetFloat(string key, float f)
	{
		floats[key] = f;
	}

	public void RemoveFloat(string key)
	{
		floats.Remove(key);
	}
}
