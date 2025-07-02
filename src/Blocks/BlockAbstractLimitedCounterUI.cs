using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractLimitedCounterUI : BlockAbstractCounterUI
{
	protected int minValue;

	protected int maxValue = 5;

	public BlockAbstractLimitedCounterUI(List<List<Tile>> tiles, int index)
		: base(tiles, index)
	{
	}

	public override void Play()
	{
		base.Play();
		minValue = 0;
		maxValue = 5;
	}

	public virtual TileResultCode ValueEqualsMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (isDefined)
		{
			if (maxValue == currentValue + extraValue)
			{
				return TileResultCode.True;
			}
			return TileResultCode.False;
		}
		return TileResultCode.False;
	}

	public bool ValueEqualsMaxValue()
	{
		if (isDefined && maxValue > 0 && maxValue != int.MaxValue)
		{
			return maxValue == currentValue + extraValue;
		}
		return false;
	}

	public bool ValueEqualsMinValue()
	{
		if (isDefined && minValue != int.MinValue)
		{
			return minValue == currentValue + extraValue;
		}
		return false;
	}

	public void SetValueToMax(float floatArg)
	{
		if (maxValue != int.MaxValue)
		{
			int num = maxValue;
			if (floatArg < 1f)
			{
				num = Mathf.Clamp(Mathf.RoundToInt(floatArg * (float)num), 0, maxValue);
			}
			SetValue(num);
		}
	}

	public void SetValueToMin(float floatArg)
	{
		if (minValue != int.MinValue)
		{
			int num = minValue;
			if (floatArg < 1f)
			{
				num = Mathf.Clamp(Mathf.RoundToInt(floatArg * (float)num), minValue, maxValue);
			}
			SetValue(num);
		}
	}

	protected virtual void SetMaxValue(int newMax)
	{
		int num = maxValue;
		maxValue = newMax;
		dirty = dirty || num != maxValue;
		if (!isDefined)
		{
			SetValue(0);
		}
		if (currentValue + extraValue > maxValue)
		{
			SetValue(maxValue);
		}
	}

	protected virtual void SetMinValue(int newMin)
	{
		int num = minValue;
		minValue = newMin;
		dirty = dirty || num != minValue;
		if (!isDefined)
		{
			SetValue(0);
		}
		if (currentValue + extraValue < minValue)
		{
			SetValue(minValue);
		}
	}

	public TileResultCode SetMaxValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = ((args.Length == 0) ? 5 : ((int)args[0]));
		SetMaxValue(num);
		return TileResultCode.True;
	}

	public TileResultCode SetMinValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int num = ((args.Length != 0) ? ((int)args[0]) : 0);
		SetMinValue(num);
		return TileResultCode.True;
	}

	public void SetValue(int value, float floatArg)
	{
		int value2 = Mathf.Clamp((floatArg >= 1f) ? value : Mathf.RoundToInt((float)value * floatArg), minValue, maxValue);
		SetValue(value2);
	}

	public override TileResultCode SetValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int value = ((args.Length != 0) ? ((int)args[0]) : 0);
		SetValue(value, eInfo.floatArg);
		return TileResultCode.True;
	}

	public override void IncrementValue(int inc)
	{
		if (isDefined)
		{
			int num = currentValue;
			long num2 = currentValue;
			currentValue = AdjustCurrentLongValue(num2 + inc);
			dirty = dirty || num != currentValue;
		}
	}

	protected virtual int AdjustCurrentLongValue(long currentLongValue)
	{
		long num = currentLongValue + extraValue;
		if (num < minValue)
		{
			currentLongValue -= num;
		}
		else if (num > maxValue)
		{
			currentLongValue -= num - maxValue;
		}
		return (int)currentLongValue;
	}

	protected void AdjustCurrentValue()
	{
		int num = currentValue + extraValue;
		if (num < minValue)
		{
			currentValue -= num;
		}
		else if (num > maxValue)
		{
			currentValue -= num - maxValue;
		}
	}

	public override TileResultCode IncrementValue(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int inc = ((args.Length == 0) ? 1 : ((int)args[0]));
		IncrementValue(inc);
		return TileResultCode.True;
	}

	public virtual float GetFraction()
	{
		return (float)(currentValue + extraValue) / (float)maxValue;
	}

	public virtual TileResultCode GetFraction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Min(eInfo.floatArg, GetFraction());
		if (!(eInfo.floatArg <= 0f))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}
}
