public class StringArgumentPattenMatch : ArgumentPatternMatch
{
	private string _value;

	private bool _matchAny;

	private bool _multivalued;

	private string[] _multiValues;

	public StringArgumentPattenMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			_matchAny = true;
		}
		else if (patternStr.Contains(","))
		{
			_multivalued = true;
			_multiValues = patternStr.Split(',');
		}
		else
		{
			_value = patternStr;
		}
	}

	public override bool Matches(object o)
	{
		if (o is string)
		{
			if (_matchAny)
			{
				return true;
			}
			if (!_multivalued)
			{
				return (o as string).Equals(_value);
			}
			string text = o as string;
			string[] multiValues = _multiValues;
			foreach (string value in multiValues)
			{
				if (text.Equals(value))
				{
					return true;
				}
			}
		}
		return false;
	}
}
