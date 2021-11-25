using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020001D8 RID: 472
public abstract class SingleBlockTypeIntModelFeatureType : IntModelFeatureType
{
	// Token: 0x06001888 RID: 6280 RVA: 0x000AA9F6 File Offset: 0x000A8DF6
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (tile.gaf.Predicate == Block.predicateCreate)
		{
			this.Update((string)tile.gaf.Args[0]);
		}
	}

	// Token: 0x06001889 RID: 6281
	protected abstract void Update(string blockType);
}
