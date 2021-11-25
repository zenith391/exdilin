using System;
using UnityEngine;

// Token: 0x02000161 RID: 353
internal static class RectExtensions
{
	// Token: 0x06001540 RID: 5440 RVA: 0x00094634 File Offset: 0x00092A34
	public static bool Intersects(this Rect r1, Rect r2)
	{
		return r2.xMin <= r1.xMax && r2.xMax >= r1.xMin && r2.yMin <= r1.yMax && r2.yMax >= r1.yMin;
	}
}
