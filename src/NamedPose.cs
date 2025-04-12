using System;
using UnityEngine;

// Token: 0x02000257 RID: 599
public class NamedPose
{
	// Token: 0x06001B3E RID: 6974 RVA: 0x000C66B7 File Offset: 0x000C4AB7
	public NamedPose(string name, Vector3 position, Vector3 direction)
	{
		this.name = name;
		this.position = position;
		this.direction = direction;
	}

	// Token: 0x0400172A RID: 5930
	public string name = string.Empty;

	// Token: 0x0400172B RID: 5931
	public Vector3 position;

	// Token: 0x0400172C RID: 5932
	public Vector3 direction;
}
