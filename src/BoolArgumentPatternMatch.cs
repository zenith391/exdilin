public class BoolArgumentPatternMatch : ArgumentPatternMatch
{
	private bool _value;

	private bool _matchAny;

	public BoolArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			_matchAny = true;
		}
		else if (patternStr == "True")
		{
			_value = true;
		}
		else
		{
			_value = false;
		}
	}

	public override bool Matches(object o)
	{
		if (o is bool)
		{
			if (!_matchAny)
			{
				return ((bool)o).Equals(_value);
			}
			return true;
		}
		return false;
	}
}
