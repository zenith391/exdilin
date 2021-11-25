using System;
using System.Collections.Generic;

// Token: 0x020001C0 RID: 448
public class BlockTypeExistsModelFeatureType : SingleBlockTypeBooleanModelFeatureType
{
	// Token: 0x06001812 RID: 6162 RVA: 0x000AAAC8 File Offset: 0x000A8EC8
	protected override void Update(string blockType)
	{
		this.triggered = (this.triggered || this.blockTypes.Contains(blockType));
	}

	// Token: 0x04001308 RID: 4872
	public HashSet<string> blockTypes = new HashSet<string>();
}
