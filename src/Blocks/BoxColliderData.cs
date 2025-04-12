using System;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200004C RID: 76
	public class BoxColliderData
	{
		// Token: 0x0600065E RID: 1630 RVA: 0x0002ACE1 File Offset: 0x000290E1
		public BoxColliderData(BoxCollider c)
		{
			this.size = c.size;
			this.center = c.center;
		}

		// Token: 0x04000498 RID: 1176
		public Vector3 size;

		// Token: 0x04000499 RID: 1177
		public Vector3 center;
	}
}
