using System.Collections.Generic;

public abstract class FirstTileRowIntModelFeatureType : IntModelFeatureType
{
	public override void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
		if (rowIndex == 0 && columnIndex == 0)
		{
			Update(model[blockIndex][0]);
		}
	}

	protected abstract void Update(List<Tile> firstRow);
}
