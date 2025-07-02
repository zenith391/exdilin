using System.Collections.Generic;

public class NonConditionalTileWithAnyPredicateExistsFeatureType : NonConditionalTileModelFeatureType
{
	public HashSet<Predicate> predicates = new HashSet<Predicate>();

	protected override void UpdateNonConditionalTile(Tile tile)
	{
		triggered = triggered || predicates.Contains(tile.gaf.Predicate);
	}
}
