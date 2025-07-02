using System;

namespace Exdilin;

public class BlockEntry
{
	public string id;

	public string modelName;

	public BlockMetaData metaData;

	public Type blockType;

	public bool hasDefaultTiles;

	public Mod originator;
}
