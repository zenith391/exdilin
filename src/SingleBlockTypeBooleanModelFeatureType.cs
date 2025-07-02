using System.Collections.Generic;
using Blocks;

public abstract class SingleBlockTypeBooleanModelFeatureType : BooleanModelFeatureType
{
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (tile.gaf.Predicate == Block.predicateCreate)
		{
			Update((string)tile.gaf.Args[0]);
		}
	}

	protected abstract void Update(string blockType);
}
