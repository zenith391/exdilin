using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000229 RID: 553
[Serializable]
public class ShapeCollisionMatrixEntry
{
	// Token: 0x04001645 RID: 5701
	public string shapeCategory;

	// Token: 0x04001646 RID: 5702
	public string[] noCollides;

	// Token: 0x04001647 RID: 5703
	public List<string> prefabsWithShapes = new List<string>();

	// Token: 0x04001648 RID: 5704
	[HideInInspector]
	public List<string> noCollideList = new List<string>();
}
