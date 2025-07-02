using UnityEngine;

public class GafPatternMatch
{
	private int _blockItemId;

	private Predicate _predicate;

	private ArgumentPatternMatch[] _argumentMatchers;

	public int BlockItemId => _blockItemId;

	public GafPatternMatch(int blockItemId, Predicate predicate, ArgumentPatternMatch[] argumentMatchers)
	{
		_blockItemId = blockItemId;
		_predicate = predicate;
		_argumentMatchers = argumentMatchers;
	}

	public bool Matches(GAF gaf)
	{
		if (!gaf.Predicate.Equals(_predicate))
		{
			return false;
		}
		int num = Mathf.Min(gaf.Args.Length, _argumentMatchers.Length);
		for (int i = 0; i < num; i++)
		{
			if (gaf.Args[i] != null && !_argumentMatchers[i].Matches(gaf.Args[i]))
			{
				return false;
			}
		}
		return true;
	}
}
