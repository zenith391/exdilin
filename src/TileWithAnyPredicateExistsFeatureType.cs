using System.Collections.Generic;

public class TileWithAnyPredicateExistsFeatureType : SingleTileModelFeatureType
{
	public HashSet<Predicate> predicates = new HashSet<Predicate>();

	protected override void Update(Tile tile)
	{
		triggered = triggered || predicates.Contains(tile.gaf.Predicate);
	}
}
