using System.Collections.Generic;

public abstract class SingleTileModelFeatureType : BooleanModelFeatureType
{
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		Update(tile);
	}

	protected abstract void Update(Tile tile);
}
