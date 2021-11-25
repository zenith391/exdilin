using System;
using UnityEngine;

// Token: 0x0200028E RID: 654
[Serializable]
public class SkyBoxDefinition
{
	// Token: 0x17000151 RID: 337
	// (get) Token: 0x06001E93 RID: 7827 RVA: 0x000DB0FB File Offset: 0x000D94FB
	public string platformSpecificMaterialResourcePath
	{
		get
		{
			return this.materialResourcePath;
		}
	}

	// Token: 0x0400189C RID: 6300
	public string name;

	// Token: 0x0400189D RID: 6301
	public string materialResourcePath;

	// Token: 0x0400189E RID: 6302
	public string iOSMaterialResourcePath;

	// Token: 0x0400189F RID: 6303
	public Vector3 lightRotation;

	// Token: 0x040018A0 RID: 6304
	public string fogColor = "White";

	// Token: 0x040018A1 RID: 6305
	public float fogDensity = 50f;

	// Token: 0x040018A2 RID: 6306
	public float fogStart = 200f;
}
