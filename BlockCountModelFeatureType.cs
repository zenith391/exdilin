using System;

// Token: 0x020001BF RID: 447
public class BlockCountModelFeatureType : SingleBlockTypeIntModelFeatureType
{
	// Token: 0x06001810 RID: 6160 RVA: 0x000AAA2D File Offset: 0x000A8E2D
	protected override void Update(string blockType)
	{
		this.value++;
	}
}
