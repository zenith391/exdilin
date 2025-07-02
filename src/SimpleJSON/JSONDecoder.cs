using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleJSON;

public static class JSONDecoder
{
	private struct ScannerData
	{
		public readonly JObject Result;

		public readonly int Index;

		public ScannerData(JObject result, int index)
		{
			Result = result;
			Index = index;
		}
	}

	private const char ObjectStart = '{';

	private const char ObjectEnd = '}';

	private const char ObjectPairSeparator = ',';

	private const char ObjectSeparator = ':';

	private const char ArrayStart = '[';

	private const char ArrayEnd = ']';

	private const char ArraySeparator = ',';

	private const char StringStart = '"';

	private const char NullStart = 'n';

	private const char TrueStart = 't';

	private const char FalseStart = 'f';

	private static readonly Dictionary<char, string> EscapeChars = new Dictionary<char, string>
	{
		{ '"', "\"" },
		{ '\\', "\\" },
		{ 'b', "\b" },
		{ 'f', "\f" },
		{ 'n', "\n" },
		{ 'r', "\r" },
		{ 't', "\t" }
	};

	public static JObject Decode(string json)
	{
		return Scan(json, 0).Result;
	}

	private static ScannerData Scan(string json, int index)
	{
		index = SkipWhitespace(json, index);
		char c = json[index];
		switch (c)
		{
		case '"':
			return ScanString(json, index + 1);
		case '[':
			return ScanArray(json, index);
		case 'f':
			return ScanFalse(json, index);
		case 'n':
			return ScanNull(json, index);
		case 't':
			return ScanTrue(json, index);
		case '{':
			return ScanObject(json, index);
		default:
			if (IsNumberStart(c))
			{
				return ScanNumber(json, index);
			}
			throw new ParseError("Unexpected token " + c, index);
		}
	}

	private static ScannerData ScanString(string json, int index)
	{
		index = ScanBareString(json, index, out var result);
		return new ScannerData(JObject.CreateString(result), index + 1);
	}

	private static ScannerData ScanTrue(string json, int index)
	{
		return new ScannerData(JObject.CreateBoolean(b: true), ExpectConstant(json, index, "true"));
	}

	private static ScannerData ScanFalse(string json, int index)
	{
		return new ScannerData(JObject.CreateBoolean(b: false), ExpectConstant(json, index, "false"));
	}

	private static ScannerData ScanNull(string json, int index)
	{
		return new ScannerData(JObject.CreateNull(), ExpectConstant(json, index, "null"));
	}

	private static ScannerData ScanNumber(string json, int index)
	{
		bool isNegative = false;
		bool isFractional = false;
		bool negativeExponent = false;
		if (json[index] == '-')
		{
			isNegative = true;
			index++;
		}
		ulong num = 0uL;
		if (json[index] != '0')
		{
			while (json.Length > index && char.IsNumber(json[index]))
			{
				num = num * 10 + (ulong)(json[index] - 48);
				index++;
			}
		}
		else
		{
			index++;
		}
		ulong num2 = 0uL;
		int num3 = 0;
		if (json.Length > index && json[index] == '.')
		{
			isFractional = true;
			index++;
			while (json.Length > index && char.IsNumber(json[index]))
			{
				num2 = num2 * 10 + (ulong)(json[index] - 48);
				index++;
				num3++;
			}
		}
		ulong num4 = 0uL;
		if (json.Length > index && (json[index] == 'e' || json[index] == 'E'))
		{
			isFractional = true;
			index++;
			if (json[index] == '-')
			{
				negativeExponent = true;
				index++;
			}
			else if (json[index] == '+')
			{
				index++;
			}
			while (json.Length > index && char.IsNumber(json[index]))
			{
				num4 = num4 * 10 + (ulong)(json[index] - 48);
				index++;
			}
		}
		return new ScannerData(JObject.CreateNumber(isNegative, isFractional, negativeExponent, num, num2, num3, num4), index);
	}

	private static ScannerData ScanArray(string json, int index)
	{
		List<JObject> list = new List<JObject>();
		int num = SkipWhitespace(json, index + 1);
		if (json[num] == ']')
		{
			return new ScannerData(JObject.CreateArray(list), num + 1);
		}
		while (json[index] != ']')
		{
			index++;
			ScannerData scannerData = Scan(json, index);
			index = SkipWhitespace(json, scannerData.Index);
			if (json[index] != ',' && json[index] != ']')
			{
				int num2 = ((index - 200 >= 0) ? (index - 200) : 0);
				throw new ParseError("Expecting array separator (,) or array end (]) near json: " + json.Substring(num2, index - num2), index);
			}
			list.Add(scannerData.Result);
		}
		return new ScannerData(JObject.CreateArray(list), index + 1);
	}

	private static ScannerData ScanObject(string json, int index)
	{
		Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>();
		int num = SkipWhitespace(json, index + 1);
		if (json[num] == '}')
		{
			return new ScannerData(JObject.CreateObject(dictionary), num + 1);
		}
		while (json[index] != '}')
		{
			index = SkipWhitespace(json, index + 1);
			if (json[index] != '"')
			{
				throw new ParseError("Object keys must be strings", index);
			}
			index = ScanBareString(json, index + 1, out var result) + 1;
			index = SkipWhitespace(json, index);
			if (json[index] != ':')
			{
				throw new ParseError("Expecting object separator (:)", index);
			}
			index++;
			ScannerData scannerData = Scan(json, index);
			index = SkipWhitespace(json, scannerData.Index);
			if (json[index] != '}' && json[index] != ',')
			{
				throw new ParseError("Expecting object pair separator (,) or object end (})", index);
			}
			dictionary[result] = scannerData.Result;
		}
		return new ScannerData(JObject.CreateObject(dictionary), index + 1);
	}

	private static int SkipWhitespace(string json, int index)
	{
		while (char.IsWhiteSpace(json[index]))
		{
			index++;
		}
		return index;
	}

	private static int ExpectConstant(string json, int index, string expected)
	{
		if (json.Substring(index, expected.Length) != expected)
		{
			throw new ParseError($"Expected '{expected}' got '{json.Substring(index, expected.Length)}'", index);
		}
		return index + expected.Length;
	}

	private static bool IsNumberStart(char b)
	{
		if (b != '-')
		{
			if (b >= '0')
			{
				return b <= '9';
			}
			return false;
		}
		return true;
	}

	private static int ScanBareString(string json, int index, out string result)
	{
		int num = index;
		bool flag = false;
		while (json[num] != '"')
		{
			if (json[num] == '\\')
			{
				flag = true;
				num++;
				if (EscapeChars.ContainsKey(json[num]) || json[num] == 'u')
				{
					num++;
				}
				else if (json[num] == 'u')
				{
					num += 5;
				}
			}
			else
			{
				num++;
			}
		}
		if (!flag)
		{
			result = json.Substring(index, num - index);
			return num;
		}
		StringBuilder stringBuilder = new StringBuilder(num - index);
		int num2 = index;
		while (json[index] != '"')
		{
			if (json[index] == '\\')
			{
				if (index > num2)
				{
					stringBuilder.Append(json, num2, index - num2);
				}
				index++;
				if (EscapeChars.ContainsKey(json[index]))
				{
					stringBuilder.Append(EscapeChars[json[index]]);
					index++;
				}
				else if (json[index] == 'u')
				{
					index++;
					int num3 = Convert.ToInt32(json.Substring(index, 4), 16);
					stringBuilder.Append((char)num3);
					index += 4;
				}
				num2 = index;
			}
			else
			{
				index++;
			}
		}
		if (num2 != index)
		{
			stringBuilder.Append(json, num2, index - num2);
		}
		result = stringBuilder.ToString();
		return index;
	}
}
