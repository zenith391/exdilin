using System;
using Blocks;

// Token: 0x020001C2 RID: 450
public class BlockWithTypeCountModelFeatureType<T> : SingleBlockTypeIntModelFeatureType
{
	// Token: 0x06001817 RID: 6167 RVA: 0x000AAC50 File Offset: 0x000A9050
	protected override void Update(string blockType)
	{
		Type typeFromHandle;
		if (!Block.blockNameTypeMap.TryGetValue(blockType, out typeFromHandle))
		{
			typeFromHandle = typeof(Block);
		}
		if (typeof(T).IsAssignableFrom(typeFromHandle))
		{
			this.value++;
		}
	}
}
