using System;
using Blocks;

// Token: 0x020001C3 RID: 451
public class BlockWithTypeExistsModelFeatureType<T> : SingleBlockTypeBooleanModelFeatureType
{
	// Token: 0x06001819 RID: 6169 RVA: 0x000AACA8 File Offset: 0x000A90A8
	protected override void Update(string blockType)
	{
		if (!this.triggered)
		{
			Type typeFromHandle;
			if (!Block.blockNameTypeMap.TryGetValue(blockType, out typeFromHandle))
			{
				typeFromHandle = typeof(Block);
			}
			this.triggered = (this.triggered || typeof(T).IsAssignableFrom(typeFromHandle));
		}
	}
}
