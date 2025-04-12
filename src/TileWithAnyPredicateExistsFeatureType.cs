using System;
using System.Collections.Generic;

// Token: 0x020001DA RID: 474
public class TileWithAnyPredicateExistsFeatureType : SingleTileModelFeatureType
{
	// Token: 0x0600188E RID: 6286 RVA: 0x000AC58D File Offset: 0x000AA98D
	protected override void Update(Tile tile)
	{
		this.triggered = (this.triggered || this.predicates.Contains(tile.gaf.Predicate));
	}

	// Token: 0x0400137A RID: 4986
	public HashSet<Predicate> predicates = new HashSet<Predicate>();
}
