using UnityEngine;

public class FloatArgumentPatternMatch : ArgumentPatternMatch
{
	private enum MatchType
	{
		SpecificValue = 1,
		Any = 2,
		Positive = 4,
		Negative = 8
	}

	private float _value;

	private MatchType _matchType;

	public FloatArgumentPatternMatch(string patternStr)
	{
		switch (patternStr)
		{
		case "*":
			_matchType = MatchType.Any;
			return;
		case "PositiveValue":
			_value = 0f;
			_matchType = (MatchType)5;
			return;
		case "NegativeValue":
			_value = 0f;
			_matchType = MatchType.Negative;
			return;
		}
		_matchType = MatchType.SpecificValue;
		if (!float.TryParse(patternStr, out _value))
		{
			BWLog.Error("Failed to parse float argument: " + patternStr);
		}
	}

	public override bool Matches(object o)
	{
		if (o is float num)
		{
			if ((_matchType & MatchType.Any) == MatchType.Any)
			{
				return true;
			}
			if ((_matchType & MatchType.SpecificValue) == MatchType.SpecificValue && Mathf.Abs(num - _value) < Mathf.Epsilon)
			{
				return true;
			}
			if ((_matchType & MatchType.Positive) == MatchType.Positive && num > 0f)
			{
				return true;
			}
			if ((_matchType & MatchType.Negative) == MatchType.Negative && num < 0f)
			{
				return true;
			}
		}
		return false;
	}
}
