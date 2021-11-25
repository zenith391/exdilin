using System;
using UnityEngine;

// Token: 0x0200025A RID: 602
public static class NormalizedInput
{
	// Token: 0x17000090 RID: 144
	// (get) Token: 0x06001B50 RID: 6992 RVA: 0x000C6A01 File Offset: 0x000C4E01
	public static Vector3 mousePosition
	{
		get
		{
			return Input.mousePosition / NormalizedScreen.scale;
		}
	}
}
