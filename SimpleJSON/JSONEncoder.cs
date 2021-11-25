using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleJSON
{
	// Token: 0x02000289 RID: 649
	public class JSONEncoder
	{
		// Token: 0x06001E62 RID: 7778 RVA: 0x000D8D9C File Offset: 0x000D719C
		private JSONEncoder()
		{
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x000D8DB0 File Offset: 0x000D71B0
		public static string Encode(object obj)
		{
			JSONEncoder jsonencoder = new JSONEncoder();
			jsonencoder.EncodeObject(obj);
			return jsonencoder._buffer.ToString();
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x000D8DD8 File Offset: 0x000D71D8
		private void EncodeObject(object obj)
		{
			if (obj == null)
			{
				this.EncodeNull();
			}
			else if (obj is IJSONEncodable)
			{
				this.EncodeDictionary(((IJSONEncodable)obj).ToJSON());
			}
			else if (obj is string)
			{
				this.EncodeString((string)obj);
			}
			else if (obj is float)
			{
				this.EncodeFloat((float)obj);
			}
			else if (obj is double)
			{
				this.EncodeDouble((double)obj);
			}
			else if (obj is int)
			{
				this.EncodeLong((long)((int)obj));
			}
			else if (obj is uint)
			{
				this.EncodeULong((ulong)((uint)obj));
			}
			else if (obj is long)
			{
				this.EncodeLong((long)obj);
			}
			else if (obj is ulong)
			{
				this.EncodeULong((ulong)obj);
			}
			else if (obj is short)
			{
				this.EncodeLong((long)((short)obj));
			}
			else if (obj is ushort)
			{
				this.EncodeULong((ulong)((ushort)obj));
			}
			else if (obj is byte)
			{
				this.EncodeULong((ulong)((byte)obj));
			}
			else if (obj is bool)
			{
				this.EncodeBool((bool)obj);
			}
			else if (obj is IDictionary)
			{
				this.EncodeDictionary((IDictionary)obj);
			}
			else if (obj is IEnumerable)
			{
				this.EncodeEnumerable((IEnumerable)obj);
			}
			else if (obj is Enum)
			{
				this.EncodeObject(Convert.ChangeType(obj, Enum.GetUnderlyingType(obj.GetType())));
			}
			else
			{
				if (!(obj is JObject))
				{
					throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, "obj");
				}
				JObject jobject = (JObject)obj;
				switch (jobject.Kind)
				{
				case JObjectKind.Object:
					this.EncodeDictionary(jobject.ObjectValue);
					break;
				case JObjectKind.Array:
					this.EncodeEnumerable(jobject.ArrayValue);
					break;
				case JObjectKind.String:
					this.EncodeString(jobject.StringValue);
					break;
				case JObjectKind.Number:
					if (jobject.IsFractional)
					{
						this.EncodeDouble(jobject.DoubleValue);
					}
					else if (jobject.IsNegative)
					{
						this.EncodeLong(jobject.LongValue);
					}
					else
					{
						this.EncodeULong(jobject.ULongValue);
					}
					break;
				case JObjectKind.Boolean:
					this.EncodeBool(jobject.BooleanValue);
					break;
				case JObjectKind.Null:
					this.EncodeNull();
					break;
				default:
					throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, "obj");
				}
			}
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x000D90C8 File Offset: 0x000D74C8
		private void EncodeNull()
		{
			this._buffer.Append("null");
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x000D90DC File Offset: 0x000D74DC
		private void EncodeString(string str)
		{
			this._buffer.Append('"');
			foreach (char c in str)
			{
				if (JSONEncoder.EscapeChars.ContainsKey(c))
				{
					this._buffer.Append(JSONEncoder.EscapeChars[c]);
				}
				else if (c > '\u0080' || c < ' ')
				{
					this._buffer.Append("\\u" + Convert.ToString((int)c, 16).ToUpper(CultureInfo.InvariantCulture).PadLeft(4, '0'));
				}
				else
				{
					this._buffer.Append(c);
				}
			}
			this._buffer.Append('"');
		}

		// Token: 0x06001E67 RID: 7783 RVA: 0x000D91A7 File Offset: 0x000D75A7
		private void EncodeFloat(float f)
		{
			this._buffer.Append(f.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x000D91C1 File Offset: 0x000D75C1
		private void EncodeDouble(double d)
		{
			this._buffer.Append(d.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x000D91DB File Offset: 0x000D75DB
		private void EncodeLong(long l)
		{
			this._buffer.Append(l.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x000D91F5 File Offset: 0x000D75F5
		private void EncodeULong(ulong l)
		{
			this._buffer.Append(l.ToString(CultureInfo.InvariantCulture));
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x000D920F File Offset: 0x000D760F
		private void EncodeBool(bool b)
		{
			this._buffer.Append((!b) ? "false" : "true");
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x000D9234 File Offset: 0x000D7634
		private void EncodeDictionary(IDictionary d)
		{
			bool flag = true;
			this._buffer.Append('{');
			IDictionaryEnumerator enumerator = d.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
					if (!(dictionaryEntry.Key is string))
					{
						throw new ArgumentException("Dictionary keys must be strings", "d");
					}
					if (!flag)
					{
						this._buffer.Append(',');
					}
					this.EncodeString((string)dictionaryEntry.Key);
					this._buffer.Append(':');
					this.EncodeObject(dictionaryEntry.Value);
					flag = false;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this._buffer.Append('}');
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x000D9310 File Offset: 0x000D7710
		private void EncodeEnumerable(IEnumerable e)
		{
			bool flag = true;
			this._buffer.Append('[');
			IEnumerator enumerator = e.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					if (!flag)
					{
						this._buffer.Append(',');
					}
					this.EncodeObject(obj);
					flag = false;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this._buffer.Append(']');
		}

		// Token: 0x0400188E RID: 6286
		private readonly StringBuilder _buffer = new StringBuilder();

		// Token: 0x0400188F RID: 6287
		internal static readonly Dictionary<char, string> EscapeChars = new Dictionary<char, string>
		{
			{
				'"',
				"\\\""
			},
			{
				'\\',
				"\\\\"
			},
			{
				'\b',
				"\\b"
			},
			{
				'\f',
				"\\f"
			},
			{
				'\n',
				"\\n"
			},
			{
				'\r',
				"\\r"
			},
			{
				'\t',
				"\\t"
			},
			{
				'\u2028',
				"\\u2028"
			},
			{
				'\u2029',
				"\\u2029"
			}
		};
	}
}
