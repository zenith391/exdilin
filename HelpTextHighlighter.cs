using System;
using UnityEngine;

// Token: 0x020002DB RID: 731
public class HelpTextHighlighter
{
	// Token: 0x0600214E RID: 8526 RVA: 0x000F3D80 File Offset: 0x000F2180
	public virtual void Update()
	{
	}

	// Token: 0x0600214F RID: 8527 RVA: 0x000F3D82 File Offset: 0x000F2182
	public virtual bool IsVisible()
	{
		return this.go != null;
	}

	// Token: 0x06002150 RID: 8528 RVA: 0x000F3D90 File Offset: 0x000F2190
	public virtual void Show()
	{
		if (this.go == null)
		{
			this.go = new GameObject("Highlight");
		}
	}

	// Token: 0x06002151 RID: 8529 RVA: 0x000F3DB3 File Offset: 0x000F21B3
	public virtual void Destroy()
	{
		if (this.go != null)
		{
			UnityEngine.Object.Destroy(this.go);
			this.go = null;
		}
	}

	// Token: 0x04001C4C RID: 7244
	protected GameObject go;
}
