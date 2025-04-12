using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020001FD RID: 509
public class CharacterEventHandler : MonoBehaviour
{
	// Token: 0x06001A1B RID: 6683 RVA: 0x000C12B0 File Offset: 0x000BF6B0
	protected bool FindBlock()
	{
		this.targetBlock = (BWSceneManager.FindBlock(base.gameObject.transform.parent.gameObject, false) as BlockAnimatedCharacter);
		if (this.targetBlock == null)
		{
			return false;
		}
		this.legsRb = this.targetBlock.GetRigidBody();
		return this.legsRb != null;
	}

	// Token: 0x06001A1C RID: 6684 RVA: 0x000C1310 File Offset: 0x000BF710
	private void Impulse(float power)
	{
		if ((this.targetBlock == null || this.legsRb == null) && !this.FindBlock())
		{
			return;
		}
		this.legsRb.AddForceAtPosition(power * this.targetBlock.goT.forward, this.legsRb.worldCenterOfMass);
	}

	// Token: 0x06001A1D RID: 6685 RVA: 0x000C1374 File Offset: 0x000BF774
	private void StepLeft()
	{
		if (this.targetBlock == null && !this.FindBlock())
		{
			return;
		}
		List<string> possibleSfxs = this.targetBlock.GetPossibleSfxs("Legs Step", true);
		if (possibleSfxs.Count > 0)
		{
			string sfxName = possibleSfxs[0 % possibleSfxs.Count];
			this.targetBlock.PlayPositionedSound(sfxName, 0.2f, 1f);
		}
	}

	// Token: 0x06001A1E RID: 6686 RVA: 0x000C13DC File Offset: 0x000BF7DC
	private void StepRight()
	{
		if (this.targetBlock == null && !this.FindBlock())
		{
			return;
		}
		List<string> possibleSfxs = this.targetBlock.GetPossibleSfxs("Legs Step", true);
		if (possibleSfxs.Count > 0)
		{
			string sfxName = possibleSfxs[1 % possibleSfxs.Count];
			this.targetBlock.PlayPositionedSound(sfxName, 0.2f, 1f);
		}
	}

	// Token: 0x06001A1F RID: 6687 RVA: 0x000C1443 File Offset: 0x000BF843
	private void PlaySound(string soundName)
	{
		if (this.targetBlock == null && !this.FindBlock())
		{
			return;
		}
		this.targetBlock.PlayPositionedSound(soundName, 0.2f, 1f);
	}

	// Token: 0x06001A20 RID: 6688 RVA: 0x000C1472 File Offset: 0x000BF872
	private void StartRotation()
	{
		if (this.targetBlock == null && !this.FindBlock())
		{
			return;
		}
		if (this.targetBlock.stateHandler != null)
		{
		}
	}

	// Token: 0x040015A1 RID: 5537
	private BlockAnimatedCharacter targetBlock;

	// Token: 0x040015A2 RID: 5538
	private Rigidbody legsRb;
}
