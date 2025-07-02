using Blocks;

public class BlockWithTypeCountModelFeatureType<T> : SingleBlockTypeIntModelFeatureType
{
	protected override void Update(string blockType)
	{
		if (!Block.blockNameTypeMap.TryGetValue(blockType, out var typeFromHandle))
		{
			typeFromHandle = typeof(Block);
		}
		if (typeof(T).IsAssignableFrom(typeFromHandle))
		{
			value++;
		}
	}
}
