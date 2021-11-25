using System;
using UnityEngine;

// Token: 0x02000167 RID: 359
public class GafPatternMatch
{
	// Token: 0x06001578 RID: 5496 RVA: 0x00096430 File Offset: 0x00094830
	public GafPatternMatch(int blockItemId, Predicate predicate, ArgumentPatternMatch[] argumentMatchers)
	{
		this._blockItemId = blockItemId;
		this._predicate = predicate;
		this._argumentMatchers = argumentMatchers;
	}

	// Token: 0x17000062 RID: 98
	// (get) Token: 0x06001579 RID: 5497 RVA: 0x0009644D File Offset: 0x0009484D
	public int BlockItemId
	{
		get
		{
			return this._blockItemId;
		}
	}

	// Token: 0x0600157A RID: 5498 RVA: 0x00096458 File Offset: 0x00094858
	public bool Matches(GAF gaf)
	{
		if (!gaf.Predicate.Equals(this._predicate))
		{
			return false;
		}
		int num = Mathf.Min(gaf.Args.Length, this._argumentMatchers.Length);
		for (int i = 0; i < num; i++)
		{
			if (gaf.Args[i] != null)
			{
				if (!this._argumentMatchers[i].Matches(gaf.Args[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x040010B5 RID: 4277
	private int _blockItemId;

	// Token: 0x040010B6 RID: 4278
	private Predicate _predicate;

	// Token: 0x040010B7 RID: 4279
	private ArgumentPatternMatch[] _argumentMatchers;
}
