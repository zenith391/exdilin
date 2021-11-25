using System;
using UnityEngine;

// Token: 0x02000213 RID: 531
[Serializable]
public class MaterialTextureDefinition
{
	// Token: 0x040015F3 RID: 5619
	public string name;

	// Token: 0x040015F4 RID: 5620
	public PhysicMaterial material;

	// Token: 0x040015F5 RID: 5621
	public bool canApplyToNonTerrain;

	// Token: 0x040015F6 RID: 5622
	public string terrainTextureSuffix = string.Empty;

	// Token: 0x040015F7 RID: 5623
	public string blockTextureSuffix = string.Empty;

	// Token: 0x040015F8 RID: 5624
	public bool notOnTerrainBlocks;
}
