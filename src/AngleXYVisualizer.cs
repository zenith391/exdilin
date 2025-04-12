using System;
using UnityEngine;

// Token: 0x02000236 RID: 566
public class AngleXYVisualizer : AngleVisualizer
{
	// Token: 0x06001AE6 RID: 6886 RVA: 0x000C5233 File Offset: 0x000C3633
	public AngleXYVisualizer(float sign = 1f) : base(sign)
	{
	}

	// Token: 0x06001AE7 RID: 6887 RVA: 0x000C523C File Offset: 0x000C363C
	protected override Quaternion GetArrowRotation(float angle, Transform blockT)
	{
		return blockT.rotation * Quaternion.Euler(90f, this.sign * angle, 0f);
	}
}
