using Blocks;

public class BlockWithTypeExistsModelFeatureType<T> : SingleBlockTypeBooleanModelFeatureType
{
	protected override void Update(string blockType)
	{
		if (!triggered)
		{
			if (!Block.blockNameTypeMap.TryGetValue(blockType, out var value))
			{
				value = typeof(Block);
			}
			triggered = triggered || typeof(T).IsAssignableFrom(value);
		}
	}
}
