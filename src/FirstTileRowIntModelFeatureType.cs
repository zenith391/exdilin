using System;
using System.Collections.Generic;

// Token: 0x020001C7 RID: 455
public abstract class FirstTileRowIntModelFeatureType : IntModelFeatureType
{
	// Token: 0x06001832 RID: 6194 RVA: 0x000AAAF2 File Offset: 0x000A8EF2
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (rowIndex == 0 && columnIndex == 0)
		{
			this.Update(model[blockIndex][0]);
		}
	}

	// Token: 0x06001833 RID: 6195
	protected abstract void Update(List<Tile> firstRow);
}
