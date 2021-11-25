using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020001D4 RID: 468
public abstract class NonConditionalTileModelFeatureType : BooleanModelFeatureType
{
	// Token: 0x0600187D RID: 6269 RVA: 0x000AC432 File Offset: 0x000AA832
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (model[blockIndex][rowIndex][0].gaf.Predicate == Block.predicateThen)
		{
			this.UpdateNonConditionalTile(tile);
		}
	}

	// Token: 0x0600187E RID: 6270
	protected abstract void UpdateNonConditionalTile(Tile tile);
}
