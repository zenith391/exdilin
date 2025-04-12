using System;
using UnityEngine;

// Token: 0x020001F2 RID: 498
[Serializable]
public class BlockMeshMetaData
{
	// Token: 0x0400142D RID: 5165
	public string defaultPaint;

	// Token: 0x0400142E RID: 5166
	public string defaultTexture;

	// Token: 0x0400142F RID: 5167
	public bool canBeTextured;

	// Token: 0x04001430 RID: 5168
	public bool canBeMaterialTextured = true;

	// Token: 0x04001431 RID: 5169
	public Vector3 defaultTextureNormal;

	// Token: 0x04001432 RID: 5170
	public TextureSideRule[] textureSideRules;
}
