using System.Collections.Generic;

public abstract class SameRowPredicatesBooleanModelFeatureType : BooleanModelFeatureType
{
	public HashSet<Predicate> predicates = new HashSet<Predicate>();

	private int currentRow;

	private HashSet<Predicate> foundPredicates = new HashSet<Predicate>();

	public override void Reset()
	{
		base.Reset();
		foundPredicates.Clear();
		currentRow = -1;
	}

	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (triggered)
		{
			return;
		}
		if (rowIndex != currentRow)
		{
			foundPredicates.Clear();
			currentRow = rowIndex;
		}
		if (predicates.Contains(tile.gaf.Predicate))
		{
			foundPredicates.Add(tile.gaf.Predicate);
			if (foundPredicates.Count == predicates.Count)
			{
				triggered = true;
			}
		}
	}
}
