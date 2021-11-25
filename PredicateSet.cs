using System;
using System.Collections;
using System.Collections.Generic;

// Token: 0x0200026C RID: 620
public class PredicateSet : IEnumerable<Predicate>, IEnumerable
{
	// Token: 0x06001D0C RID: 7436 RVA: 0x000CD1C4 File Offset: 0x000CB5C4
	public PredicateSet(IEnumerable<Predicate> e)
	{
		this.arr = new BitArray(Predicate.predicateCount);
		foreach (Predicate pred in e)
		{
			this.Add(pred);
		}
	}

	// Token: 0x06001D0D RID: 7437 RVA: 0x000CD230 File Offset: 0x000CB630
	public PredicateSet()
	{
		this.arr = new BitArray(Predicate.predicateCount);
	}

	// Token: 0x06001D0E RID: 7438 RVA: 0x000CD248 File Offset: 0x000CB648
	public IEnumerator<Predicate> GetEnumerator()
	{
		for (int i = 0; i < Predicate.predicateCount; i++)
		{
			if (this.arr.Get(i))
			{
				yield return PredicateRegistry.IndexToPredicate[i];
			}
		}
		yield break;
	}

	// Token: 0x06001D0F RID: 7439 RVA: 0x000CD263 File Offset: 0x000CB663
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	// Token: 0x06001D10 RID: 7440 RVA: 0x000CD26B File Offset: 0x000CB66B
	public void Add(Predicate pred)
	{
		this.arr.Set(pred.index, true);
	}

	// Token: 0x06001D11 RID: 7441 RVA: 0x000CD27F File Offset: 0x000CB67F
	public void Remove(Predicate pred)
	{
		this.arr.Set(pred.index, false);
	}

	// Token: 0x06001D12 RID: 7442 RVA: 0x000CD293 File Offset: 0x000CB693
	public bool Contains(Predicate pred)
	{
		return this.arr.Get(pred.index);
	}

	// Token: 0x06001D13 RID: 7443 RVA: 0x000CD2A6 File Offset: 0x000CB6A6
	public void Clear()
	{
		this.arr.SetAll(false);
	}

	// Token: 0x06001D14 RID: 7444 RVA: 0x000CD2B4 File Offset: 0x000CB6B4
	public void UnionWith(HashSet<Predicate> predicates)
	{
		foreach (Predicate pred in predicates)
		{
			this.Add(pred);
		}
	}

	// Token: 0x040017AE RID: 6062
	private BitArray arr;
}
