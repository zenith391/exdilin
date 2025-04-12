using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x020001D7 RID: 471
public abstract class SingleBlockTypeBooleanModelFeatureType : BooleanModelFeatureType
{
	// Token: 0x06001885 RID: 6277 RVA: 0x000AAA86 File Offset: 0x000A8E86
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (tile.gaf.Predicate == Block.predicateCreate)
		{
			this.Update((string)tile.gaf.Args[0]);
		}
	}

	// Token: 0x06001886 RID: 6278
	protected abstract void Update(string blockType);
}
