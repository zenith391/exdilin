using System;
using System.Collections.Generic;

namespace SimpleJSON
{
	// Token: 0x02000285 RID: 645
	public class JObject
	{
		// Token: 0x06001E08 RID: 7688 RVA: 0x000D7C15 File Offset: 0x000D6015
		private JObject(string str)
		{
			this.Kind = JObjectKind.String;
			this.StringValue = str;
		}

		// Token: 0x06001E09 RID: 7689 RVA: 0x000D7C2B File Offset: 0x000D602B
		private JObject(bool b)
		{
			this.Kind = JObjectKind.Boolean;
			this.BooleanValue = b;
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x000D7C41 File Offset: 0x000D6041
		private JObject()
		{
			this.Kind = JObjectKind.Null;
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x000D7C50 File Offset: 0x000D6050
		private JObject(bool isNegative, bool isFractional, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			this.Kind = JObjectKind.Number;
			if (!isFractional)
			{
				this.MakeInteger(isNegative, integerPart);
			}
			else
			{
				this.MakeFloat(isNegative, negativeExponent, integerPart, fractionalPart, fractionalPartLength, exponent);
			}
		}

		// Token: 0x06001E0C RID: 7692 RVA: 0x000D7C83 File Offset: 0x000D6083
		private JObject(List<JObject> list)
		{
			this.Kind = JObjectKind.Array;
			this.ArrayValue = list;
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x000D7C99 File Offset: 0x000D6099
		private JObject(Dictionary<string, JObject> dict)
		{
			this.Kind = JObjectKind.Object;
			this.ObjectValue = dict;
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06001E0E RID: 7694 RVA: 0x000D7CAF File Offset: 0x000D60AF
		// (set) Token: 0x06001E0F RID: 7695 RVA: 0x000D7CB7 File Offset: 0x000D60B7
		public JObjectKind Kind { get; private set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06001E10 RID: 7696 RVA: 0x000D7CC0 File Offset: 0x000D60C0
		// (set) Token: 0x06001E11 RID: 7697 RVA: 0x000D7CC8 File Offset: 0x000D60C8
		public Dictionary<string, JObject> ObjectValue { get; private set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06001E12 RID: 7698 RVA: 0x000D7CD1 File Offset: 0x000D60D1
		// (set) Token: 0x06001E13 RID: 7699 RVA: 0x000D7CD9 File Offset: 0x000D60D9
		public List<JObject> ArrayValue { get; private set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06001E14 RID: 7700 RVA: 0x000D7CE2 File Offset: 0x000D60E2
		// (set) Token: 0x06001E15 RID: 7701 RVA: 0x000D7CEA File Offset: 0x000D60EA
		public string StringValue { get; private set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06001E16 RID: 7702 RVA: 0x000D7CF3 File Offset: 0x000D60F3
		// (set) Token: 0x06001E17 RID: 7703 RVA: 0x000D7CFB File Offset: 0x000D60FB
		public bool BooleanValue { get; private set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06001E18 RID: 7704 RVA: 0x000D7D04 File Offset: 0x000D6104
		public int Count
		{
			get
			{
				return (this.Kind != JObjectKind.Array) ? ((this.Kind != JObjectKind.Object) ? 0 : this.ObjectValue.Count) : this.ArrayValue.Count;
			}
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06001E19 RID: 7705 RVA: 0x000D7D3E File Offset: 0x000D613E
		// (set) Token: 0x06001E1A RID: 7706 RVA: 0x000D7D46 File Offset: 0x000D6146
		public double DoubleValue { get; private set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06001E1B RID: 7707 RVA: 0x000D7D4F File Offset: 0x000D614F
		// (set) Token: 0x06001E1C RID: 7708 RVA: 0x000D7D57 File Offset: 0x000D6157
		public float FloatValue { get; private set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06001E1D RID: 7709 RVA: 0x000D7D60 File Offset: 0x000D6160
		// (set) Token: 0x06001E1E RID: 7710 RVA: 0x000D7D68 File Offset: 0x000D6168
		public ulong ULongValue { get; private set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06001E1F RID: 7711 RVA: 0x000D7D71 File Offset: 0x000D6171
		// (set) Token: 0x06001E20 RID: 7712 RVA: 0x000D7D79 File Offset: 0x000D6179
		public long LongValue { get; private set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06001E21 RID: 7713 RVA: 0x000D7D82 File Offset: 0x000D6182
		// (set) Token: 0x06001E22 RID: 7714 RVA: 0x000D7D8A File Offset: 0x000D618A
		public uint UIntValue { get; private set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06001E23 RID: 7715 RVA: 0x000D7D93 File Offset: 0x000D6193
		// (set) Token: 0x06001E24 RID: 7716 RVA: 0x000D7D9B File Offset: 0x000D619B
		public int IntValue { get; private set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06001E25 RID: 7717 RVA: 0x000D7DA4 File Offset: 0x000D61A4
		// (set) Token: 0x06001E26 RID: 7718 RVA: 0x000D7DAC File Offset: 0x000D61AC
		public ushort UShortValue { get; private set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06001E27 RID: 7719 RVA: 0x000D7DB5 File Offset: 0x000D61B5
		// (set) Token: 0x06001E28 RID: 7720 RVA: 0x000D7DBD File Offset: 0x000D61BD
		public short ShortValue { get; private set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06001E29 RID: 7721 RVA: 0x000D7DC6 File Offset: 0x000D61C6
		// (set) Token: 0x06001E2A RID: 7722 RVA: 0x000D7DCE File Offset: 0x000D61CE
		public byte ByteValue { get; private set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06001E2B RID: 7723 RVA: 0x000D7DD7 File Offset: 0x000D61D7
		// (set) Token: 0x06001E2C RID: 7724 RVA: 0x000D7DDF File Offset: 0x000D61DF
		public sbyte SByteValue { get; private set; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06001E2D RID: 7725 RVA: 0x000D7DE8 File Offset: 0x000D61E8
		public object RawValue
		{
			get
			{
				switch (this.Kind)
				{
				case JObjectKind.Object:
					return this.ObjectValue;
				case JObjectKind.Array:
					return this.FlattenArray();
				case JObjectKind.String:
					return this.StringValue;
				case JObjectKind.Number:
					return this.DoubleValue;
				case JObjectKind.Boolean:
					return this.BooleanValue;
				case JObjectKind.Null:
					return null;
				default:
					return null;
				}
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06001E2E RID: 7726 RVA: 0x000D7E4F File Offset: 0x000D624F
		// (set) Token: 0x06001E2F RID: 7727 RVA: 0x000D7E57 File Offset: 0x000D6257
		public bool IsNegative { get; private set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06001E30 RID: 7728 RVA: 0x000D7E60 File Offset: 0x000D6260
		// (set) Token: 0x06001E31 RID: 7729 RVA: 0x000D7E68 File Offset: 0x000D6268
		public bool IsFractional { get; private set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06001E32 RID: 7730 RVA: 0x000D7E71 File Offset: 0x000D6271
		// (set) Token: 0x06001E33 RID: 7731 RVA: 0x000D7E79 File Offset: 0x000D6279
		public IntegerSize MinInteger { get; private set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06001E34 RID: 7732 RVA: 0x000D7E82 File Offset: 0x000D6282
		// (set) Token: 0x06001E35 RID: 7733 RVA: 0x000D7E8A File Offset: 0x000D628A
		public FloatSize MinFloat { get; private set; }

		// Token: 0x06001E36 RID: 7734 RVA: 0x000D7E93 File Offset: 0x000D6293
		public bool ContainsKey(string key)
		{
			return this.ObjectValue.ContainsKey(key);
		}

		// Token: 0x1700014F RID: 335
		public JObject this[string key]
		{
			get
			{
				return this.ObjectValue[key];
			}
		}

		// Token: 0x17000150 RID: 336
		public JObject this[int key]
		{
			get
			{
				return this.ArrayValue[key];
			}
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x000D7EBD File Offset: 0x000D62BD
		public static explicit operator string(JObject obj)
		{
			return obj.StringValue;
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x000D7EC5 File Offset: 0x000D62C5
		public static explicit operator bool(JObject obj)
		{
			return obj.BooleanValue;
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x000D7ECD File Offset: 0x000D62CD
		public static explicit operator double(JObject obj)
		{
			return obj.DoubleValue;
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x000D7ED5 File Offset: 0x000D62D5
		public static explicit operator float(JObject obj)
		{
			return obj.FloatValue;
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x000D7EDD File Offset: 0x000D62DD
		public static explicit operator ulong(JObject obj)
		{
			return obj.ULongValue;
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x000D7EE5 File Offset: 0x000D62E5
		public static explicit operator long(JObject obj)
		{
			return obj.LongValue;
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x000D7EED File Offset: 0x000D62ED
		public static explicit operator uint(JObject obj)
		{
			return obj.UIntValue;
		}

		// Token: 0x06001E40 RID: 7744 RVA: 0x000D7EF5 File Offset: 0x000D62F5
		public static explicit operator int(JObject obj)
		{
			return obj.IntValue;
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x000D7EFD File Offset: 0x000D62FD
		public static explicit operator ushort(JObject obj)
		{
			return obj.UShortValue;
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x000D7F05 File Offset: 0x000D6305
		public static explicit operator short(JObject obj)
		{
			return obj.ShortValue;
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x000D7F0D File Offset: 0x000D630D
		public static explicit operator byte(JObject obj)
		{
			return obj.ByteValue;
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x000D7F15 File Offset: 0x000D6315
		public static explicit operator sbyte(JObject obj)
		{
			return obj.SByteValue;
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x000D7F1D File Offset: 0x000D631D
		public static JObject CreateString(string str)
		{
			return new JObject(str);
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x000D7F25 File Offset: 0x000D6325
		public static JObject CreateBoolean(bool b)
		{
			return new JObject(b);
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x000D7F2D File Offset: 0x000D632D
		public static JObject CreateNull()
		{
			return new JObject();
		}

		// Token: 0x06001E48 RID: 7752 RVA: 0x000D7F34 File Offset: 0x000D6334
		public static JObject CreateNumber(bool isNegative, bool isFractional, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			return new JObject(isNegative, isFractional, negativeExponent, integerPart, fractionalPart, fractionalPartLength, exponent);
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x000D7F45 File Offset: 0x000D6345
		public static JObject CreateArray(List<JObject> list)
		{
			return new JObject(list);
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x000D7F4D File Offset: 0x000D634D
		public static JObject CreateObject(Dictionary<string, JObject> dict)
		{
			return new JObject(dict);
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x000D7F58 File Offset: 0x000D6358
		private void MakeInteger(bool isNegative, ulong integerPart)
		{
			this.IsNegative = isNegative;
			if (!this.IsNegative)
			{
				this.ULongValue = integerPart;
				this.MinInteger = IntegerSize.UInt64;
				if (this.ULongValue <= 9223372036854775807UL)
				{
					this.LongValue = (long)this.ULongValue;
					this.MinInteger = IntegerSize.Int64;
				}
				if (this.ULongValue <= 2147483647UL && this.ULongValue >= 0)
				{
					this.UIntValue = (uint)this.ULongValue;
					this.MinInteger = IntegerSize.UInt32;
				}
				if (this.ULongValue <= 2147483647UL)
				{
					this.IntValue = (int)this.ULongValue;
					this.MinInteger = IntegerSize.Int32;
				}
				if (this.ULongValue <= 65535UL)
				{
					this.UShortValue = (ushort)this.ULongValue;
					this.MinInteger = IntegerSize.UInt16;
				}
				if (this.ULongValue <= 32767UL)
				{
					this.ShortValue = (short)this.ULongValue;
					this.MinInteger = IntegerSize.Int16;
				}
				if (this.ULongValue <= 255UL)
				{
					this.ByteValue = (byte)this.ULongValue;
					this.MinInteger = IntegerSize.UInt8;
				}
				if (this.ULongValue <= 127UL)
				{
					this.SByteValue = (sbyte)this.ULongValue;
					this.MinInteger = IntegerSize.Int8;
				}
				this.DoubleValue = this.ULongValue;
				this.MinFloat = FloatSize.Double;
				if (this.DoubleValue <= 3.4028234663852886E+38)
				{
					this.FloatValue = (float)this.DoubleValue;
					this.MinFloat = FloatSize.Single;
				}
			}
			else
			{
				this.LongValue = (long)(-(long)integerPart);
				this.MinInteger = IntegerSize.Int64;
				if (this.LongValue >= -2147483648L)
				{
					this.IntValue = (int)this.LongValue;
					this.MinInteger = IntegerSize.Int32;
				}
				if (this.LongValue >= -32768L)
				{
					this.ShortValue = (short)this.LongValue;
					this.MinInteger = IntegerSize.Int16;
				}
				if (this.LongValue >= -128L)
				{
					this.SByteValue = (sbyte)this.LongValue;
					this.MinInteger = IntegerSize.Int8;
				}
				this.DoubleValue = (double)this.LongValue;
				this.MinFloat = FloatSize.Double;
				if (this.DoubleValue >= -3.4028234663852886E+38)
				{
					this.FloatValue = (float)this.DoubleValue;
					this.MinFloat = FloatSize.Single;
				}
			}
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x000D817C File Offset: 0x000D657C
		private void MakeFloat(bool isNegative, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			this.DoubleValue = (double)((!isNegative) ? 1 : -1) * (integerPart + fractionalPart / Math.Pow(10.0, (double)fractionalPartLength)) * Math.Pow(10.0, (double)(((!negativeExponent) ? 1L : -1L) * (long)exponent));
			this.MinFloat = FloatSize.Double;
			this.IsFractional = true;
			if (this.DoubleValue < 0.0)
			{
				this.IsNegative = true;
				if (this.DoubleValue >= -3.4028234663852886E+38)
				{
					this.FloatValue = (float)this.DoubleValue;
					this.MinFloat = FloatSize.Single;
				}
			}
			else if (this.DoubleValue <= 3.4028234663852886E+38)
			{
				this.FloatValue = (float)this.DoubleValue;
				this.MinFloat = FloatSize.Single;
			}
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x000D8258 File Offset: 0x000D6658
		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(obj, this))
			{
				return true;
			}
			if (!(obj is JObject))
			{
				return false;
			}
			JObject jobject = (JObject)obj;
			if (jobject.Kind != this.Kind)
			{
				return false;
			}
			switch (this.Kind)
			{
			case JObjectKind.Object:
				if (this.ObjectValue.Count != jobject.ObjectValue.Count)
				{
					return false;
				}
				foreach (KeyValuePair<string, JObject> keyValuePair in this.ObjectValue)
				{
					if (!jobject.ObjectValue.ContainsKey(keyValuePair.Key) || !keyValuePair.Value.Equals(jobject.ObjectValue[keyValuePair.Key]))
					{
						return false;
					}
				}
				return true;
			case JObjectKind.Array:
				if (this.ArrayValue.Count != jobject.ArrayValue.Count)
				{
					return false;
				}
				for (int i = 0; i < this.ArrayValue.Count; i++)
				{
					if (!this.ArrayValue[i].Equals(jobject.ArrayValue[i]))
					{
						return false;
					}
				}
				return true;
			case JObjectKind.String:
				return this.StringValue == jobject.StringValue;
			case JObjectKind.Number:
				return JObject.EqualNumber(this, jobject);
			case JObjectKind.Boolean:
				return this.BooleanValue == jobject.BooleanValue;
			default:
				return true;
			}
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x000D83F8 File Offset: 0x000D67F8
		public override int GetHashCode()
		{
			switch (this.Kind)
			{
			case JObjectKind.Object:
				return this.ObjectValue.GetHashCode();
			case JObjectKind.Array:
				return this.ArrayValue.GetHashCode();
			case JObjectKind.String:
				return this.StringValue.GetHashCode();
			case JObjectKind.Number:
				if (this.IsFractional)
				{
					return this.DoubleValue.GetHashCode();
				}
				if (this.IsNegative)
				{
					return this.LongValue.GetHashCode();
				}
				return this.ULongValue.GetHashCode();
			case JObjectKind.Boolean:
				return this.BooleanValue.GetHashCode();
			case JObjectKind.Null:
				return 0;
			default:
				return 0;
			}
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x000D84C4 File Offset: 0x000D68C4
		public static bool EqualNumber(JObject o1, JObject o2)
		{
			if (o1.MinFloat != o2.MinFloat || o1.MinInteger != o2.MinInteger || o1.IsNegative != o2.IsNegative || o1.IsFractional != o2.IsFractional)
			{
				return false;
			}
			if (o1.IsFractional)
			{
				return o1.DoubleValue == o2.DoubleValue;
			}
			if (o1.IsNegative)
			{
				return o1.LongValue == o2.LongValue;
			}
			return o1.ULongValue == o2.ULongValue;
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x000D855C File Offset: 0x000D695C
		public object[] FlattenArray()
		{
			if (this.Kind != JObjectKind.Array)
			{
				throw new Exception("Must be array");
			}
			List<object> list = new List<object>();
			foreach (JObject jobject in this.ArrayValue)
			{
				list.Add(jobject.RawValue);
			}
			return list.ToArray();
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x000D85E0 File Offset: 0x000D69E0
		public override string ToString()
		{
			switch (this.Kind)
			{
			case JObjectKind.Object:
				return "JObject:Object";
			case JObjectKind.Array:
				return "JObject:Array";
			case JObjectKind.String:
				return this.StringValue;
			case JObjectKind.Number:
				return this.DoubleValue.ToString();
			case JObjectKind.Boolean:
				return this.BooleanValue.ToString();
			case JObjectKind.Null:
				return "null";
			default:
				return "JObject:unknown";
			}
		}
	}
}
