using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005E RID: 94
	public class BlockAbstractLimitedCounterUI : BlockAbstractCounterUI
	{
		// Token: 0x060007BB RID: 1979 RVA: 0x00036417 File Offset: 0x00034817
		public BlockAbstractLimitedCounterUI(List<List<Tile>> tiles, int index) : base(tiles, index)
		{
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x00036428 File Offset: 0x00034828
		public override void Play()
		{
			base.Play();
			this.minValue = 0;
			this.maxValue = 5;
		}

		// Token: 0x060007BD RID: 1981 RVA: 0x0003643E File Offset: 0x0003483E
		public virtual TileResultCode ValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.isDefined)
			{
				return (this.maxValue != this.currentValue + this.extraValue) ? TileResultCode.False : TileResultCode.True;
			}
			return TileResultCode.False;
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x0003646C File Offset: 0x0003486C
		public bool ValueEqualsMaxValue()
		{
			return this.isDefined && (this.maxValue > 0 && this.maxValue != int.MaxValue) && this.maxValue == this.currentValue + this.extraValue;
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x000364BA File Offset: 0x000348BA
		public bool ValueEqualsMinValue()
		{
			return this.isDefined && this.minValue != int.MinValue && this.minValue == this.currentValue + this.extraValue;
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x000364F4 File Offset: 0x000348F4
		public void SetValueToMax(float floatArg)
		{
			if (this.maxValue == 2147483647)
			{
				return;
			}
			int num = this.maxValue;
			if (floatArg < 1f)
			{
				num = Mathf.Clamp(Mathf.RoundToInt(floatArg * (float)num), 0, this.maxValue);
			}
			this.SetValue(num);
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x00036544 File Offset: 0x00034944
		public void SetValueToMin(float floatArg)
		{
			if (this.minValue == -2147483648)
			{
				return;
			}
			int num = this.minValue;
			if (floatArg < 1f)
			{
				num = Mathf.Clamp(Mathf.RoundToInt(floatArg * (float)num), this.minValue, this.maxValue);
			}
			this.SetValue(num);
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00036598 File Offset: 0x00034998
		protected virtual void SetMaxValue(int newMax)
		{
			int num = this.maxValue;
			this.maxValue = newMax;
			this.dirty = (this.dirty || num != this.maxValue);
			if (!this.isDefined)
			{
				this.SetValue(0);
			}
			if (this.currentValue + this.extraValue > this.maxValue)
			{
				this.SetValue(this.maxValue);
			}
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x0003660C File Offset: 0x00034A0C
		protected virtual void SetMinValue(int newMin)
		{
			int num = this.minValue;
			this.minValue = newMin;
			this.dirty = (this.dirty || num != this.minValue);
			if (!this.isDefined)
			{
				this.SetValue(0);
			}
			if (this.currentValue + this.extraValue < this.minValue)
			{
				this.SetValue(this.minValue);
			}
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x00036680 File Offset: 0x00034A80
		public TileResultCode SetMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = (args.Length <= 0) ? 5 : ((int)args[0]);
			this.SetMaxValue(num);
			return TileResultCode.True;
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x000366B0 File Offset: 0x00034AB0
		public TileResultCode SetMinValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = (args.Length <= 0) ? 0 : ((int)args[0]);
			this.SetMinValue(num);
			return TileResultCode.True;
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x000366E0 File Offset: 0x00034AE0
		public void SetValue(int value, float floatArg)
		{
			int value2 = Mathf.Clamp((floatArg >= 1f) ? value : Mathf.RoundToInt((float)value * floatArg), this.minValue, this.maxValue);
			this.SetValue(value2);
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x00036720 File Offset: 0x00034B20
		public override TileResultCode SetValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int value = (args.Length <= 0) ? 0 : ((int)args[0]);
			this.SetValue(value, eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00036754 File Offset: 0x00034B54
		public override void IncrementValue(int inc)
		{
			if (this.isDefined)
			{
				int currentValue = this.currentValue;
				long num = (long)this.currentValue;
				this.currentValue = this.AdjustCurrentLongValue(num + (long)inc);
				this.dirty = (this.dirty || currentValue != this.currentValue);
			}
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x000367AC File Offset: 0x00034BAC
		protected virtual int AdjustCurrentLongValue(long currentLongValue)
		{
			long num = currentLongValue + (long)this.extraValue;
			if (num < (long)this.minValue)
			{
				currentLongValue -= num;
			}
			else if (num > (long)this.maxValue)
			{
				currentLongValue -= num - (long)this.maxValue;
			}
			return (int)currentLongValue;
		}

		// Token: 0x060007CA RID: 1994 RVA: 0x000367F8 File Offset: 0x00034BF8
		protected void AdjustCurrentValue()
		{
			int num = this.currentValue + this.extraValue;
			if (num < this.minValue)
			{
				this.currentValue -= num;
			}
			else if (num > this.maxValue)
			{
				this.currentValue -= num - this.maxValue;
			}
		}

		// Token: 0x060007CB RID: 1995 RVA: 0x00036854 File Offset: 0x00034C54
		public override TileResultCode IncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int inc = (args.Length <= 0) ? 1 : ((int)args[0]);
			this.IncrementValue(inc);
			return TileResultCode.True;
		}

		// Token: 0x060007CC RID: 1996 RVA: 0x00036884 File Offset: 0x00034C84
		public virtual float GetFraction()
		{
			return (float)(this.currentValue + this.extraValue) / (float)this.maxValue;
		}

		// Token: 0x060007CD RID: 1997 RVA: 0x000368A9 File Offset: 0x00034CA9
		public virtual TileResultCode GetFraction(ScriptRowExecutionInfo eInfo, object[] args)
		{
			eInfo.floatArg = Mathf.Min(eInfo.floatArg, this.GetFraction());
			return (eInfo.floatArg <= 0f) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x040005F4 RID: 1524
		protected int minValue;

		// Token: 0x040005F5 RID: 1525
		protected int maxValue = 5;
	}
}
