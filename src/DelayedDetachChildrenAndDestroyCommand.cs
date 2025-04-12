using System;
using UnityEngine;

// Token: 0x02000130 RID: 304
public class DelayedDetachChildrenAndDestroyCommand : DelayedCommand
{
	// Token: 0x06001422 RID: 5154 RVA: 0x0008CB57 File Offset: 0x0008AF57
	public DelayedDetachChildrenAndDestroyCommand(GameObject go) : base(3)
	{
		this.go = go;
	}

	// Token: 0x06001423 RID: 5155 RVA: 0x0008CB67 File Offset: 0x0008AF67
	private void DetachDestroy()
	{
		if (this.go != null)
		{
			this.go.transform.DetachChildren();
			UnityEngine.Object.Destroy(this.go);
			this.go = null;
		}
	}

	// Token: 0x06001424 RID: 5156 RVA: 0x0008CB9C File Offset: 0x0008AF9C
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		this.DetachDestroy();
	}

	// Token: 0x06001425 RID: 5157 RVA: 0x0008CBAA File Offset: 0x0008AFAA
	public override void Removed()
	{
		base.Removed();
		if (this.go != null)
		{
			this.DetachDestroy();
		}
	}

	// Token: 0x04000FC1 RID: 4033
	private GameObject go;
}
