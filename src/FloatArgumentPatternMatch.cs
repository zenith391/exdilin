using System;
using UnityEngine;

// Token: 0x0200016B RID: 363
public class FloatArgumentPatternMatch : ArgumentPatternMatch
{
	// Token: 0x0600157F RID: 5503 RVA: 0x00096600 File Offset: 0x00094A00
	public FloatArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			this._matchType = FloatArgumentPatternMatch.MatchType.Any;
		}
		else if (patternStr == "PositiveValue")
		{
			this._value = 0f;
			this._matchType = (FloatArgumentPatternMatch.MatchType)5;
		}
		else if (patternStr == "NegativeValue")
		{
			this._value = 0f;
			this._matchType = FloatArgumentPatternMatch.MatchType.Negative;
		}
		else
		{
			this._matchType = FloatArgumentPatternMatch.MatchType.SpecificValue;
			if (!float.TryParse(patternStr, out this._value))
			{
				BWLog.Error("Failed to parse float argument: " + patternStr);
			}
		}
	}

	// Token: 0x06001580 RID: 5504 RVA: 0x000966A8 File Offset: 0x00094AA8
	public override bool Matches(object o)
	{
		if (o is float)
		{
			float num = (float)o;
			if ((this._matchType & FloatArgumentPatternMatch.MatchType.Any) == FloatArgumentPatternMatch.MatchType.Any)
			{
				return true;
			}
			if ((this._matchType & FloatArgumentPatternMatch.MatchType.SpecificValue) == FloatArgumentPatternMatch.MatchType.SpecificValue && Mathf.Abs(num - this._value) < Mathf.Epsilon)
			{
				return true;
			}
			if ((this._matchType & FloatArgumentPatternMatch.MatchType.Positive) == FloatArgumentPatternMatch.MatchType.Positive && num > 0f)
			{
				return true;
			}
			if ((this._matchType & FloatArgumentPatternMatch.MatchType.Negative) == FloatArgumentPatternMatch.MatchType.Negative && num < 0f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x040010BF RID: 4287
	private float _value;

	// Token: 0x040010C0 RID: 4288
	private FloatArgumentPatternMatch.MatchType _matchType;

	// Token: 0x0200016C RID: 364
	private enum MatchType
	{
		// Token: 0x040010C2 RID: 4290
		SpecificValue = 1,
		// Token: 0x040010C3 RID: 4291
		Any,
		// Token: 0x040010C4 RID: 4292
		Positive = 4,
		// Token: 0x040010C5 RID: 4293
		Negative = 8
	}
}
