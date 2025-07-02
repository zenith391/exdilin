using System.Collections.Generic;
using Blocks;

public abstract class NonConditionalTileModelFeatureType : BooleanModelFeatureType
{
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (model[blockIndex][rowIndex][0].gaf.Predicate == Block.predicateThen)
		{
			UpdateNonConditionalTile(tile);
		}
	}

	protected abstract void UpdateNonConditionalTile(Tile tile);
}
