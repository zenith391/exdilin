using System.Collections;
using System.Collections.Generic;

public class PredicateSet : IEnumerable<Predicate>, IEnumerable
{
	private BitArray arr;

	public PredicateSet(IEnumerable<Predicate> e)
	{
		arr = new BitArray(Predicate.predicateCount);
		foreach (Predicate item in e)
		{
			Add(item);
		}
	}

	public PredicateSet()
	{
		arr = new BitArray(Predicate.predicateCount);
	}

	public IEnumerator<Predicate> GetEnumerator()
	{
		for (int i = 0; i < Predicate.predicateCount; i++)
		{
			if (arr.Get(i))
			{
				yield return PredicateRegistry.IndexToPredicate[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(Predicate pred)
	{
		arr.Set(pred.index, value: true);
	}

	public void Remove(Predicate pred)
	{
		arr.Set(pred.index, value: false);
	}

	public bool Contains(Predicate pred)
	{
		return arr.Get(pred.index);
	}

	public void Clear()
	{
		arr.SetAll(value: false);
	}

	public void UnionWith(HashSet<Predicate> predicates)
	{
		foreach (Predicate predicate in predicates)
		{
			Add(predicate);
		}
	}
}
