using System;

// Token: 0x02000169 RID: 361
public class IntArgumentPatternMatch : ArgumentPatternMatch
{
	// Token: 0x0600157D RID: 5501 RVA: 0x000964E4 File Offset: 0x000948E4
	public IntArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			this._matchType = IntArgumentPatternMatch.MatchType.Any;
		}
		else if (patternStr == "PositiveValue")
		{
			this._value = 0;
			this._matchType = (IntArgumentPatternMatch.MatchType)5;
		}
		else if (patternStr == "NegativeValue")
		{
			this._value = 0;
			this._matchType = IntArgumentPatternMatch.MatchType.Negative;
		}
		else
		{
			this._matchType = IntArgumentPatternMatch.MatchType.SpecificValue;
			if (!int.TryParse(patternStr, out this._value))
			{
				BWLog.Error("Failed to parse int argument: " + patternStr);
			}
		}
	}

	// Token: 0x0600157E RID: 5502 RVA: 0x00096584 File Offset: 0x00094984
	public override bool Matches(object o)
	{
		if (o is int)
		{
			int num = (int)o;
			if ((this._matchType & IntArgumentPatternMatch.MatchType.Any) == IntArgumentPatternMatch.MatchType.Any)
			{
				return true;
			}
			if ((this._matchType & IntArgumentPatternMatch.MatchType.SpecificValue) == IntArgumentPatternMatch.MatchType.SpecificValue && num == this._value)
			{
				return true;
			}
			if ((this._matchType & IntArgumentPatternMatch.MatchType.Positive) == IntArgumentPatternMatch.MatchType.Positive && num > 0)
			{
				return true;
			}
			if ((this._matchType & IntArgumentPatternMatch.MatchType.Negative) == IntArgumentPatternMatch.MatchType.Negative && num < 0)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x040010B8 RID: 4280
	private int _value;

	// Token: 0x040010B9 RID: 4281
	private IntArgumentPatternMatch.MatchType _matchType;

	// Token: 0x0200016A RID: 362
	private enum MatchType
	{
		// Token: 0x040010BB RID: 4283
		SpecificValue = 1,
		// Token: 0x040010BC RID: 4284
		Any,
		// Token: 0x040010BD RID: 4285
		Positive = 4,
		// Token: 0x040010BE RID: 4286
		Negative = 8
	}
}
