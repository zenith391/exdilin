using System;
using UnityEngine;

// Token: 0x02000232 RID: 562
public class ParameterValueVisualizer
{
	// Token: 0x06001ADD RID: 6877 RVA: 0x000C4E4C File Offset: 0x000C324C
	public virtual void Update()
	{
	}

	// Token: 0x06001ADE RID: 6878 RVA: 0x000C4E4E File Offset: 0x000C324E
	public virtual void Destroy()
	{
		if (this.visualizerGo != null)
		{
			UnityEngine.Object.Destroy(this.visualizerGo);
			this.visualizerGo = null;
		}
	}

	// Token: 0x04001687 RID: 5767
	protected GameObject visualizerGo;
}
