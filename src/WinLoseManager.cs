using System;
using UnityEngine;

// Token: 0x02000360 RID: 864
public class WinLoseManager
{
	// Token: 0x06002654 RID: 9812 RVA: 0x0011ADFC File Offset: 0x001191FC
	public static ParticleSystem GetParticleSystem()
	{
		if (WinLoseManager.winParticleSystem == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Blocksworld.starsReward.gameObject);
			WinLoseManager.winParticleSystem = gameObject.GetComponent<ParticleSystem>();
		}
		return WinLoseManager.winParticleSystem;
	}

	// Token: 0x06002655 RID: 9813 RVA: 0x0011AE39 File Offset: 0x00119239
	public static bool IsFinished()
	{
		return WinLoseManager.winning || WinLoseManager.losing || WinLoseManager.ending;
	}

	// Token: 0x06002656 RID: 9814 RVA: 0x0011AE57 File Offset: 0x00119257
	public static void Reset()
	{
		WinLoseManager.winning = false;
		WinLoseManager.losing = false;
		WinLoseManager.ending = false;
		WinLoseManager.messageWindow = null;
		WinLoseManager.playedStinger = false;
		WinLoseManager.buttonPressed = false;
	}

	// Token: 0x04002190 RID: 8592
	public static bool winning;

	// Token: 0x04002191 RID: 8593
	public static bool losing;

	// Token: 0x04002192 RID: 8594
	public static bool ending;

	// Token: 0x04002193 RID: 8595
	public static UISpeechBubble messageWindow;

	// Token: 0x04002194 RID: 8596
	public static bool playedStinger;

	// Token: 0x04002195 RID: 8597
	public static bool buttonPressed;

	// Token: 0x04002196 RID: 8598
	private static ParticleSystem winParticleSystem;
}
