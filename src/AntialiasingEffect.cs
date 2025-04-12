using System;
using UnityEngine;

// Token: 0x02000031 RID: 49
[RequireComponent(typeof(Camera))]
public class AntialiasingEffect : ScreenPostEffect
{
	// Token: 0x060001CB RID: 459 RVA: 0x0000A594 File Offset: 0x00008994
	protected override string GetShaderName()
	{
		return "Blocksworld/AntialiasingEffect";
	}
}
