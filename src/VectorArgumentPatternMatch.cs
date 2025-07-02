using UnityEngine;

public class VectorArgumentPatternMatch : ArgumentPatternMatch
{
	private Vector3 _value;

	private bool _matchAny;

	public VectorArgumentPatternMatch(string patternStr)
	{
		if (patternStr == "*")
		{
			_matchAny = true;
			return;
		}
		string[] array = patternStr.Split(',');
		if (array.Length == 3)
		{
			bool flag = float.TryParse(array[0], out var result);
			flag &= float.TryParse(array[1], out var result2);
			if (flag & float.TryParse(array[2], out var result3))
			{
				_value = new Vector3(result, result2, result3);
			}
		}
	}

	public override bool Matches(object o)
	{
		if (!(o is Vector3))
		{
			return false;
		}
		if (_matchAny)
		{
			return true;
		}
		Vector3 vector = (Vector3)o;
		return (vector - _value).sqrMagnitude < Mathf.Epsilon;
	}
}
