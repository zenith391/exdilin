using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000056 RID: 86
	public class RaycastDistanceComparer : IComparer<RaycastHit>
	{
		// Token: 0x0600072E RID: 1838 RVA: 0x00030DDB File Offset: 0x0002F1DB
		public RaycastDistanceComparer(Vector3 from)
		{
			this._from = from;
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x00030DEC File Offset: 0x0002F1EC
		public int Compare(RaycastHit a, RaycastHit b)
		{
			Vector3 vector = this._from - a.point;
			Vector3 vector2 = this._from - b.point;
			return (int)(vector.sqrMagnitude - vector2.sqrMagnitude);
		}

		// Token: 0x0400055D RID: 1373
		private Vector3 _from;
	}
}
