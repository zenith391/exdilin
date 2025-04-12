using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleJSON
{
	// Token: 0x02000287 RID: 647
	public static class JSONDecoder
	{
		// Token: 0x06001E53 RID: 7763 RVA: 0x000D8670 File Offset: 0x000D6A70
		public static JObject Decode(string json)
		{
			return JSONDecoder.Scan(json, 0).Result;
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x000D868C File Offset: 0x000D6A8C
		private static JSONDecoder.ScannerData Scan(string json, int index)
		{
			index = JSONDecoder.SkipWhitespace(json, index);
			char c = json[index];
			if (c == '"')
			{
				return JSONDecoder.ScanString(json, index + 1);
			}
			if (c == '[')
			{
				return JSONDecoder.ScanArray(json, index);
			}
			if (c == 'f')
			{
				return JSONDecoder.ScanFalse(json, index);
			}
			if (c == 'n')
			{
				return JSONDecoder.ScanNull(json, index);
			}
			if (c == 't')
			{
				return JSONDecoder.ScanTrue(json, index);
			}
			if (c == '{')
			{
				return JSONDecoder.ScanObject(json, index);
			}
			if (JSONDecoder.IsNumberStart(c))
			{
				return JSONDecoder.ScanNumber(json, index);
			}
			throw new ParseError("Unexpected token " + c, index);
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x000D873C File Offset: 0x000D6B3C
		private static JSONDecoder.ScannerData ScanString(string json, int index)
		{
			string str;
			index = JSONDecoder.ScanBareString(json, index, out str);
			return new JSONDecoder.ScannerData(JObject.CreateString(str), index + 1);
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x000D8762 File Offset: 0x000D6B62
		private static JSONDecoder.ScannerData ScanTrue(string json, int index)
		{
			return new JSONDecoder.ScannerData(JObject.CreateBoolean(true), JSONDecoder.ExpectConstant(json, index, "true"));
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x000D877B File Offset: 0x000D6B7B
		private static JSONDecoder.ScannerData ScanFalse(string json, int index)
		{
			return new JSONDecoder.ScannerData(JObject.CreateBoolean(false), JSONDecoder.ExpectConstant(json, index, "false"));
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x000D8794 File Offset: 0x000D6B94
		private static JSONDecoder.ScannerData ScanNull(string json, int index)
		{
			return new JSONDecoder.ScannerData(JObject.CreateNull(), JSONDecoder.ExpectConstant(json, index, "null"));
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x000D87AC File Offset: 0x000D6BAC
		private static JSONDecoder.ScannerData ScanNumber(string json, int index)
		{
			bool isNegative = false;
			bool isFractional = false;
			bool negativeExponent = false;
			if (json[index] == '-')
			{
				isNegative = true;
				index++;
			}
			ulong num = 0UL;
			if (json[index] != '0')
			{
				while (json.Length > index && char.IsNumber(json[index]))
				{
					num = num * 10UL + (ulong)((long)(json[index] - '0'));
					index++;
				}
			}
			else
			{
				index++;
			}
			ulong num2 = 0UL;
			int num3 = 0;
			if (json.Length > index && json[index] == '.')
			{
				isFractional = true;
				index++;
				while (json.Length > index && char.IsNumber(json[index]))
				{
					num2 = num2 * 10UL + (ulong)((long)(json[index] - '0'));
					index++;
					num3++;
				}
			}
			ulong num4 = 0UL;
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
					num4 = num4 * 10UL + (ulong)((long)(json[index] - '0'));
					index++;
				}
			}
			return new JSONDecoder.ScannerData(JObject.CreateNumber(isNegative, isFractional, negativeExponent, num, num2, num3, num4), index);
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x000D8944 File Offset: 0x000D6D44
		private static JSONDecoder.ScannerData ScanArray(string json, int index)
		{
			List<JObject> list = new List<JObject>();
			int num = JSONDecoder.SkipWhitespace(json, index + 1);
			if (json[num] == ']')
			{
				return new JSONDecoder.ScannerData(JObject.CreateArray(list), num + 1);
			}
			while (json[index] != ']')
			{
				index++;
				JSONDecoder.ScannerData scannerData = JSONDecoder.Scan(json, index);
				index = JSONDecoder.SkipWhitespace(json, scannerData.Index);
				if (json[index] != ',' && json[index] != ']')
				{
					int num2 = (index - 200 >= 0) ? (index - 200) : 0;
					throw new ParseError("Expecting array separator (,) or array end (]) near json: " + json.Substring(num2, index - num2), index);
				}
				list.Add(scannerData.Result);
			}
			return new JSONDecoder.ScannerData(JObject.CreateArray(list), index + 1);
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x000D8A1C File Offset: 0x000D6E1C
		private static JSONDecoder.ScannerData ScanObject(string json, int index)
		{
			Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>();
			int num = JSONDecoder.SkipWhitespace(json, index + 1);
			if (json[num] == '}')
			{
				return new JSONDecoder.ScannerData(JObject.CreateObject(dictionary), num + 1);
			}
			while (json[index] != '}')
			{
				index = JSONDecoder.SkipWhitespace(json, index + 1);
				if (json[index] != '"')
				{
					throw new ParseError("Object keys must be strings", index);
				}
				string key;
				index = JSONDecoder.ScanBareString(json, index + 1, out key) + 1;
				index = JSONDecoder.SkipWhitespace(json, index);
				if (json[index] != ':')
				{
					throw new ParseError("Expecting object separator (:)", index);
				}
				index++;
				JSONDecoder.ScannerData scannerData = JSONDecoder.Scan(json, index);
				index = JSONDecoder.SkipWhitespace(json, scannerData.Index);
				if (json[index] != '}' && json[index] != ',')
				{
					throw new ParseError("Expecting object pair separator (,) or object end (})", index);
				}
				dictionary[key] = scannerData.Result;
			}
			return new JSONDecoder.ScannerData(JObject.CreateObject(dictionary), index + 1);
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x000D8B20 File Offset: 0x000D6F20
		private static int SkipWhitespace(string json, int index)
		{
			while (char.IsWhiteSpace(json[index]))
			{
				index++;
			}
			return index;
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x000D8B40 File Offset: 0x000D6F40
		private static int ExpectConstant(string json, int index, string expected)
		{
			if (json.Substring(index, expected.Length) != expected)
			{
				throw new ParseError(string.Format("Expected '{0}' got '{1}'", expected, json.Substring(index, expected.Length)), index);
			}
			return index + expected.Length;
		}

		// Token: 0x06001E5E RID: 7774 RVA: 0x000D8B8C File Offset: 0x000D6F8C
		private static bool IsNumberStart(char b)
		{
			return b == '-' || (b >= '0' && b <= '9');
		}

		// Token: 0x06001E5F RID: 7775 RVA: 0x000D8BAC File Offset: 0x000D6FAC
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
					if (JSONDecoder.EscapeChars.ContainsKey(json[num]) || json[num] == 'u')
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
					if (JSONDecoder.EscapeChars.ContainsKey(json[index]))
					{
						stringBuilder.Append(JSONDecoder.EscapeChars[json[index]]);
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

		// Token: 0x04001880 RID: 6272
		private const char ObjectStart = '{';

		// Token: 0x04001881 RID: 6273
		private const char ObjectEnd = '}';

		// Token: 0x04001882 RID: 6274
		private const char ObjectPairSeparator = ',';

		// Token: 0x04001883 RID: 6275
		private const char ObjectSeparator = ':';

		// Token: 0x04001884 RID: 6276
		private const char ArrayStart = '[';

		// Token: 0x04001885 RID: 6277
		private const char ArrayEnd = ']';

		// Token: 0x04001886 RID: 6278
		private const char ArraySeparator = ',';

		// Token: 0x04001887 RID: 6279
		private const char StringStart = '"';

		// Token: 0x04001888 RID: 6280
		private const char NullStart = 'n';

		// Token: 0x04001889 RID: 6281
		private const char TrueStart = 't';

		// Token: 0x0400188A RID: 6282
		private const char FalseStart = 'f';

		// Token: 0x0400188B RID: 6283
		private static readonly Dictionary<char, string> EscapeChars = new Dictionary<char, string>
		{
			{
				'"',
				"\""
			},
			{
				'\\',
				"\\"
			},
			{
				'b',
				"\b"
			},
			{
				'f',
				"\f"
			},
			{
				'n',
				"\n"
			},
			{
				'r',
				"\r"
			},
			{
				't',
				"\t"
			}
		};

		// Token: 0x02000288 RID: 648
		private struct ScannerData
		{
			// Token: 0x06001E61 RID: 7777 RVA: 0x000D8D8C File Offset: 0x000D718C
			public ScannerData(JObject result, int index)
			{
				this.Result = result;
				this.Index = index;
			}

			// Token: 0x0400188C RID: 6284
			public readonly JObject Result;

			// Token: 0x0400188D RID: 6285
			public readonly int Index;
		}
	}
}
