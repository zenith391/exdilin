public class IntArgumentPatternMatch : ArgumentPatternMatch
{
	private enum MatchType
	{
		SpecificValue = 1,
		Any = 2,
		Positive = 4,
		Negative = 8
	}

	private int _value;

	private MatchType _matchType;

	public IntArgumentPatternMatch(string patternStr)
	{
		switch (patternStr)
		{
		case "*":
			_matchType = MatchType.Any;
			return;
		case "PositiveValue":
			_value = 0;
			_matchType = (MatchType)5;
			return;
		case "NegativeValue":
			_value = 0;
			_matchType = MatchType.Negative;
			return;
		}
		_matchType = MatchType.SpecificValue;
		if (!int.TryParse(patternStr, out _value))
		{
			BWLog.Error("Failed to parse int argument: " + patternStr);
		}
	}

	public override bool Matches(object o)
	{
		if (o is int num)
		{
			if ((_matchType & MatchType.Any) == MatchType.Any)
			{
				return true;
			}
			if ((_matchType & MatchType.SpecificValue) == MatchType.SpecificValue && num == _value)
			{
				return true;
			}
			if ((_matchType & MatchType.Positive) == MatchType.Positive && num > 0)
			{
				return true;
			}
			if ((_matchType & MatchType.Negative) == MatchType.Negative && num < 0)
			{
				return true;
			}
		}
		return false;
	}
}
