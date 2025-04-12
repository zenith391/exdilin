using System;
using UnityEngine;

// Token: 0x0200033D RID: 829
[RequireComponent(typeof(Camera))]
public class LetterboxingEffect : ScreenPostEffect
{
	// Token: 0x0600254E RID: 9550 RVA: 0x00110150 File Offset: 0x0010E550
	protected override string GetShaderName()
	{
		return "Blocksworld/LetterboxingEffect";
	}

	// Token: 0x0600254F RID: 9551 RVA: 0x00110157 File Offset: 0x0010E557
	public void SetFraction(float fraction)
	{
		if (this.shaderRGB != null)
		{
			base.material.SetFloat("_Fraction", fraction);
		}
	}
}
