using System;
using System.Collections.Generic;

// Token: 0x020001D5 RID: 469
public class NonConditionalTileWithAnyPredicateExistsFeatureType : NonConditionalTileModelFeatureType
{
	// Token: 0x06001880 RID: 6272 RVA: 0x000AC476 File Offset: 0x000AA876
	protected override void UpdateNonConditionalTile(Tile tile)
	{
		this.triggered = (this.triggered || this.predicates.Contains(tile.gaf.Predicate));
	}

	// Token: 0x04001376 RID: 4982
	public HashSet<Predicate> predicates = new HashSet<Predicate>();
}
