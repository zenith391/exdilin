using System;
using System.Collections.Generic;

// Token: 0x020001D6 RID: 470
public abstract class SameRowPredicatesBooleanModelFeatureType : BooleanModelFeatureType
{
	// Token: 0x06001882 RID: 6274 RVA: 0x000AC4C0 File Offset: 0x000AA8C0
	public override void Reset()
	{
		base.Reset();
		this.foundPredicates.Clear();
		this.currentRow = -1;
	}

	// Token: 0x06001883 RID: 6275 RVA: 0x000AC4DC File Offset: 0x000AA8DC
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (this.triggered)
		{
			return;
		}
		if (rowIndex != this.currentRow)
		{
			this.foundPredicates.Clear();
			this.currentRow = rowIndex;
		}
		if (this.predicates.Contains(tile.gaf.Predicate))
		{
			this.foundPredicates.Add(tile.gaf.Predicate);
			if (this.foundPredicates.Count == this.predicates.Count)
			{
				this.triggered = true;
			}
		}
	}

	// Token: 0x04001377 RID: 4983
	public HashSet<Predicate> predicates = new HashSet<Predicate>();

	// Token: 0x04001378 RID: 4984
	private int currentRow;

	// Token: 0x04001379 RID: 4985
	private HashSet<Predicate> foundPredicates = new HashSet<Predicate>();
}
