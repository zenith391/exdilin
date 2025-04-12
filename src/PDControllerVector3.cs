using System;
using UnityEngine;

// Token: 0x020000F5 RID: 245
public class PDControllerVector3
{
	// Token: 0x060011FC RID: 4604 RVA: 0x0007B320 File Offset: 0x00079720
	public PDControllerVector3(float p, float d)
	{
		this.p = p;
		this.d = d;
	}

	// Token: 0x060011FD RID: 4605 RVA: 0x0007B338 File Offset: 0x00079738
	public Vector3 Update(Vector3 currentError, float deltaTime)
	{
		Vector3 a = (currentError - this.lastError) / deltaTime;
		this.lastError = currentError;
		return currentError * this.p + a * this.d;
	}

	// Token: 0x060011FE RID: 4606 RVA: 0x0007B37C File Offset: 0x0007977C
	public void Reset()
	{
		this.lastError = Vector3.zero;
	}

	// Token: 0x04000E47 RID: 3655
	private Vector3 lastError;

	// Token: 0x04000E48 RID: 3656
	private float p;

	// Token: 0x04000E49 RID: 3657
	private float d;
}
