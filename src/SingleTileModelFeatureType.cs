using System;
using System.Collections.Generic;

// Token: 0x020001D9 RID: 473
public abstract class SingleTileModelFeatureType : BooleanModelFeatureType
{
	// Token: 0x0600188B RID: 6283 RVA: 0x000AC571 File Offset: 0x000AA971
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		this.Update(tile);
	}

	// Token: 0x0600188C RID: 6284
	protected abstract void Update(Tile tile);
}
