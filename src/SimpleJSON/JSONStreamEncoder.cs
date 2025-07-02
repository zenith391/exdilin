using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleJSON;

public class JSONStreamEncoder
{
	private struct EncoderContext
	{
		public bool IsObject;

		public bool IsEmpty;

		public EncoderContext(bool isObject, bool isEmpty)
		{
			IsObject = isObject;
			IsEmpty = isEmpty;
		}
	}

	private TextWriter _writer;

	private EncoderContext[] _contextStack;

	private int _contextStackPointer = -1;

	private bool _newlineInserted;

	public JSONStreamEncoder(TextWriter writer, int expectedNesting = 20)
	{
		_writer = writer;
		_contextStack = new EncoderContext[expectedNesting];
	}

	public void BeginArray()
	{
		WriteSeparator();
		PushContext(new EncoderContext(isObject: false, isEmpty: true));
		_writer.Write('[');
	}

	public void EndArray()
	{
		if (_contextStackPointer == -1)
		{
			throw new InvalidOperationException("EndArray called without BeginArray");
		}
		if (_contextStack[_contextStackPointer].IsObject)
		{
			throw new InvalidOperationException("EndArray called after BeginObject");
		}
		PopContext();
		WriteNewline();
		_writer.Write(']');
	}

	public void BeginObject()
	{
		WriteSeparator();
		PushContext(new EncoderContext(isObject: true, isEmpty: true));
		_writer.Write('{');
	}

	public void EndObject()
	{
		if (_contextStackPointer == -1)
		{
			throw new InvalidOperationException("EndObject called without BeginObject");
		}
		if (!_contextStack[_contextStackPointer].IsObject)
		{
			throw new InvalidOperationException("EndObject called after BeginArray");
		}
		PopContext();
		WriteNewline();
		_writer.Write('}');
	}

	public void WriteString(string str)
	{
		WriteSeparator();
		WriteBareString(str);
	}

	public void WriteKey(string str)
	{
		if (_contextStackPointer == -1)
		{
			throw new InvalidOperationException("WriteKey called without BeginObject");
		}
		if (!_contextStack[_contextStackPointer].IsObject)
		{
			throw new InvalidOperationException("WriteKey called after BeginArray");
		}
		WriteSeparator();
		WriteBareString(str);
		_writer.Write(':');
		_contextStack[_contextStackPointer].IsEmpty = true;
	}

	public void WriteNumber(long l)
	{
		WriteSeparator();
		_writer.Write(l);
	}

	public void WriteNumber(ulong l)
	{
		WriteSeparator();
		_writer.Write(l);
	}

	public void WriteNumber(double d)
	{
		WriteSeparator();
		WriteFractionalNumber(d, 1E-17);
	}

	public void WriteNumber(float f)
	{
		WriteSeparator();
		WriteFractionalNumber(f, 1E-05);
	}

	public void WriteNull()
	{
		WriteSeparator();
		_writer.Write("null");
	}

	public void WriteBool(bool b)
	{
		WriteSeparator();
		_writer.Write((!b) ? "false" : "true");
	}

	public void WriteJObject(JObject obj)
	{
		switch (obj.Kind)
		{
		case JObjectKind.Object:
			BeginObject();
			foreach (KeyValuePair<string, JObject> item in obj.ObjectValue)
			{
				WriteKey(item.Key);
				WriteJObject(item.Value);
			}
			EndObject();
			break;
		case JObjectKind.Array:
			BeginArray();
			foreach (JObject item2 in obj.ArrayValue)
			{
				WriteJObject(item2);
			}
			EndArray();
			break;
		case JObjectKind.String:
			WriteString(obj.StringValue);
			break;
		case JObjectKind.Number:
			if (obj.IsFractional)
			{
				WriteNumber(obj.DoubleValue);
			}
			else if (obj.IsNegative)
			{
				WriteNumber(obj.LongValue);
			}
			else
			{
				WriteNumber(obj.ULongValue);
			}
			break;
		case JObjectKind.Boolean:
			WriteBool(obj.BooleanValue);
			break;
		case JObjectKind.Null:
			WriteNull();
			break;
		}
	}

	public void InsertNewline()
	{
		_newlineInserted = true;
	}

	private void WriteBareString(string str)
	{
		_writer.Write('"');
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
					_writer.Write(str.Substring(num, i - num));
				}
				if (JSONEncoder.EscapeChars.ContainsKey(c))
				{
					_writer.Write(JSONEncoder.EscapeChars[c]);
				}
				else
				{
					_writer.Write("\\u" + Convert.ToString(c, 16).ToUpper(CultureInfo.InvariantCulture).PadLeft(4, '0'));
				}
				num = i + 1;
			}
		}
		if (num == 0 && i > num)
		{
			_writer.Write(str);
		}
		else if (i > num)
		{
			_writer.Write(str.Substring(num, i - num));
		}
		_writer.Write('"');
	}

	private void WriteFractionalNumber(double d, double tolerance)
	{
		if (d < 0.0)
		{
			_writer.Write('-');
			d = 0.0 - d;
		}
		else if (d == 0.0)
		{
			_writer.Write('0');
			return;
		}
		int num = (int)Math.Log10(d);
		if (num < 0)
		{
			_writer.Write("0.");
			for (int num2 = 0; num2 > num + 1; num2--)
			{
				_writer.Write('0');
			}
		}
		while (d > tolerance || num >= 0)
		{
			double num3 = Math.Pow(10.0, num);
			int num4 = (int)Math.Floor(d / num3);
			d -= (double)num4 * num3;
			_writer.Write((char)(48 + num4));
			if (num == 0 && (d > tolerance || num > 0))
			{
				_writer.Write('.');
			}
			num--;
		}
	}

	private void WriteSeparator()
	{
		if (_contextStackPointer != -1)
		{
			if (!_contextStack[_contextStackPointer].IsEmpty)
			{
				_writer.Write(',');
			}
			_contextStack[_contextStackPointer].IsEmpty = false;
			WriteNewline();
		}
	}

	private void WriteNewline()
	{
		if (_newlineInserted)
		{
			_writer.Write('\n');
			for (int i = 0; i < _contextStackPointer + 1; i++)
			{
				_writer.Write(' ');
			}
			_newlineInserted = false;
		}
	}

	private void PushContext(EncoderContext ctx)
	{
		if (_contextStackPointer + 1 == _contextStack.Length)
		{
			throw new StackOverflowException("Too much nesting for context stack, increase expected nesting when creating the encoder");
		}
		_contextStack[++_contextStackPointer] = ctx;
	}

	private void PopContext()
	{
		if (_contextStackPointer == -1)
		{
			throw new InvalidOperationException("Stack underflow");
		}
		_contextStackPointer--;
	}
}
