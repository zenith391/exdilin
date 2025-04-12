using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleJSON
{
	// Token: 0x0200028A RID: 650
	public class JSONStreamEncoder
	{
		// Token: 0x06001E6F RID: 7791 RVA: 0x000D9433 File Offset: 0x000D7833
		public JSONStreamEncoder(TextWriter writer, int expectedNesting = 20)
		{
			this._writer = writer;
			this._contextStack = new JSONStreamEncoder.EncoderContext[expectedNesting];
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x000D9455 File Offset: 0x000D7855
		public void BeginArray()
		{
			this.WriteSeparator();
			this.PushContext(new JSONStreamEncoder.EncoderContext(false, true));
			this._writer.Write('[');
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x000D9478 File Offset: 0x000D7878
		public void EndArray()
		{
			if (this._contextStackPointer == -1)
			{
				throw new InvalidOperationException("EndArray called without BeginArray");
			}
			if (this._contextStack[this._contextStackPointer].IsObject)
			{
				throw new InvalidOperationException("EndArray called after BeginObject");
			}
			this.PopContext();
			this.WriteNewline();
			this._writer.Write(']');
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x000D94DB File Offset: 0x000D78DB
		public void BeginObject()
		{
			this.WriteSeparator();
			this.PushContext(new JSONStreamEncoder.EncoderContext(true, true));
			this._writer.Write('{');
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x000D9500 File Offset: 0x000D7900
		public void EndObject()
		{
			if (this._contextStackPointer == -1)
			{
				throw new InvalidOperationException("EndObject called without BeginObject");
			}
			if (!this._contextStack[this._contextStackPointer].IsObject)
			{
				throw new InvalidOperationException("EndObject called after BeginArray");
			}
			this.PopContext();
			this.WriteNewline();
			this._writer.Write('}');
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x000D9563 File Offset: 0x000D7963
		public void WriteString(string str)
		{
			this.WriteSeparator();
			this.WriteBareString(str);
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x000D9574 File Offset: 0x000D7974
		public void WriteKey(string str)
		{
			if (this._contextStackPointer == -1)
			{
				throw new InvalidOperationException("WriteKey called without BeginObject");
			}
			if (!this._contextStack[this._contextStackPointer].IsObject)
			{
				throw new InvalidOperationException("WriteKey called after BeginArray");
			}
			this.WriteSeparator();
			this.WriteBareString(str);
			this._writer.Write(':');
			this._contextStack[this._contextStackPointer].IsEmpty = true;
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x000D95EF File Offset: 0x000D79EF
		public void WriteNumber(long l)
		{
			this.WriteSeparator();
			this._writer.Write(l);
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x000D9603 File Offset: 0x000D7A03
		public void WriteNumber(ulong l)
		{
			this.WriteSeparator();
			this._writer.Write(l);
		}

		// Token: 0x06001E78 RID: 7800 RVA: 0x000D9617 File Offset: 0x000D7A17
		public void WriteNumber(double d)
		{
			this.WriteSeparator();
			this.WriteFractionalNumber(d, 1E-17);
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x000D962F File Offset: 0x000D7A2F
		public void WriteNumber(float f)
		{
			this.WriteSeparator();
			this.WriteFractionalNumber((double)f, 1E-05);
		}

		// Token: 0x06001E7A RID: 7802 RVA: 0x000D9648 File Offset: 0x000D7A48
		public void WriteNull()
		{
			this.WriteSeparator();
			this._writer.Write("null");
		}

		// Token: 0x06001E7B RID: 7803 RVA: 0x000D9660 File Offset: 0x000D7A60
		public void WriteBool(bool b)
		{
			this.WriteSeparator();
			this._writer.Write((!b) ? "false" : "true");
		}

		// Token: 0x06001E7C RID: 7804 RVA: 0x000D9688 File Offset: 0x000D7A88
		public void WriteJObject(JObject obj)
		{
			switch (obj.Kind)
			{
			case JObjectKind.Object:
				this.BeginObject();
				foreach (KeyValuePair<string, JObject> keyValuePair in obj.ObjectValue)
				{
					this.WriteKey(keyValuePair.Key);
					this.WriteJObject(keyValuePair.Value);
				}
				this.EndObject();
				break;
			case JObjectKind.Array:
				this.BeginArray();
				foreach (JObject obj2 in obj.ArrayValue)
				{
					this.WriteJObject(obj2);
				}
				this.EndArray();
				break;
			case JObjectKind.String:
				this.WriteString(obj.StringValue);
				break;
			case JObjectKind.Number:
				if (obj.IsFractional)
				{
					this.WriteNumber(obj.DoubleValue);
				}
				else if (obj.IsNegative)
				{
					this.WriteNumber(obj.LongValue);
				}
				else
				{
					this.WriteNumber(obj.ULongValue);
				}
				break;
			case JObjectKind.Boolean:
				this.WriteBool(obj.BooleanValue);
				break;
			case JObjectKind.Null:
				this.WriteNull();
				break;
			}
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x000D9808 File Offset: 0x000D7C08
		public void InsertNewline()
		{
			this._newlineInserted = true;
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x000D9814 File Offset: 0x000D7C14
		private void WriteBareString(string str)
		{
			this._writer.Write('"');
			int length = str.Length;
			int num = 0;
			int i;
			for (i = 0; i < length; i++)
			{
				char c = str[i];
				if (c > '\u0080' || c < ' ' || c == '"' || c == '\\')
				{
					if (i > num)
					{
						this._writer.Write(str.Substring(num, i - num));
					}
					if (JSONEncoder.EscapeChars.ContainsKey(c))
					{
						this._writer.Write(JSONEncoder.EscapeChars[c]);
					}
					else
					{
						this._writer.Write("\\u" + Convert.ToString((int)c, 16).ToUpper(CultureInfo.InvariantCulture).PadLeft(4, '0'));
					}
					num = i + 1;
				}
			}
			if (num == 0 && i > num)
			{
				this._writer.Write(str);
			}
			else if (i > num)
			{
				this._writer.Write(str.Substring(num, i - num));
			}
			this._writer.Write('"');
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x000D9938 File Offset: 0x000D7D38
		private void WriteFractionalNumber(double d, double tolerance)
		{
			if (d < 0.0)
			{
				this._writer.Write('-');
				d = -d;
			}
			else if (d == 0.0)
			{
				this._writer.Write('0');
				return;
			}
			int num = (int)Math.Log10(d);
			if (num < 0)
			{
				this._writer.Write("0.");
				for (int i = 0; i > num + 1; i--)
				{
					this._writer.Write('0');
				}
			}
			while (d > tolerance || num >= 0)
			{
				double num2 = Math.Pow(10.0, (double)num);
				int num3 = (int)Math.Floor(d / num2);
				d -= (double)num3 * num2;
				this._writer.Write((char)(48 + num3));
				if (num == 0 && (d > tolerance || num > 0))
				{
					this._writer.Write('.');
				}
				num--;
			}
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x000D9A34 File Offset: 0x000D7E34
		private void WriteSeparator()
		{
			if (this._contextStackPointer == -1)
			{
				return;
			}
			if (!this._contextStack[this._contextStackPointer].IsEmpty)
			{
				this._writer.Write(',');
			}
			this._contextStack[this._contextStackPointer].IsEmpty = false;
			this.WriteNewline();
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x000D9A94 File Offset: 0x000D7E94
		private void WriteNewline()
		{
			if (this._newlineInserted)
			{
				this._writer.Write('\n');
				for (int i = 0; i < this._contextStackPointer + 1; i++)
				{
					this._writer.Write(' ');
				}
				this._newlineInserted = false;
			}
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x000D9AE8 File Offset: 0x000D7EE8
		private void PushContext(JSONStreamEncoder.EncoderContext ctx)
		{
			if (this._contextStackPointer + 1 == this._contextStack.Length)
			{
				throw new StackOverflowException("Too much nesting for context stack, increase expected nesting when creating the encoder");
			}
			this._contextStack[++this._contextStackPointer] = ctx;
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x000D9B37 File Offset: 0x000D7F37
		private void PopContext()
		{
			if (this._contextStackPointer == -1)
			{
				throw new InvalidOperationException("Stack underflow");
			}
			this._contextStackPointer--;
		}

		// Token: 0x04001890 RID: 6288
		private TextWriter _writer;

		// Token: 0x04001891 RID: 6289
		private JSONStreamEncoder.EncoderContext[] _contextStack;

		// Token: 0x04001892 RID: 6290
		private int _contextStackPointer = -1;

		// Token: 0x04001893 RID: 6291
		private bool _newlineInserted;

		// Token: 0x0200028B RID: 651
		private struct EncoderContext
		{
			// Token: 0x06001E84 RID: 7812 RVA: 0x000D9B5E File Offset: 0x000D7F5E
			public EncoderContext(bool isObject, bool isEmpty)
			{
				this.IsObject = isObject;
				this.IsEmpty = isEmpty;
			}

			// Token: 0x04001894 RID: 6292
			public bool IsObject;

			// Token: 0x04001895 RID: 6293
			public bool IsEmpty;
		}
	}
}
