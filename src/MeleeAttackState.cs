using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002A8 RID: 680
public class MeleeAttackState
{
	// Token: 0x06001F95 RID: 8085 RVA: 0x000E3820 File Offset: 0x000E1C20
	public void Enter(StateHandlerBase parent, bool interrupt)
	{
		if (this.animationState == null)
		{
			BWLog.Error("No animation set for attack state");
			return;
		}
		int animatorLayer = parent.GetAnimatorLayer();
		interrupt |= (parent.IsPlayingAnimation(this.animationState) || parent.targetController.IsInTransition(animatorLayer));
		parent.PlayAnimation(this.animationState, interrupt);
		Block leftHandAttachment = parent.combatController.GetLeftHandAttachment();
		Block rightHandAttachment = parent.combatController.GetRightHandAttachment();
		if ((leftHandAttachment != null && this.isLeftHanded) || (rightHandAttachment != null && this.isRightHanded))
		{
			int delay = 10;
			if (this.animationState.StartsWith("Chop"))
			{
				delay = 15;
			}
			else if (this.animationState.StartsWith("SwordLunge"))
			{
				delay = 20;
			}
			else if (this.animationState.StartsWith("SwordJump"))
			{
				delay = 25;
			}
			parent.targetObject.PlayPositionedSoundAfterDelay(delay, this.swishSFX[Mathf.FloorToInt(UnityEngine.Random.value * (float)this.swishSFX.Length)], 0.4f, 1f);
		}
		this.ClearAttackHitBlocks(parent.combatController);
	}

	// Token: 0x06001F96 RID: 8086 RVA: 0x000E3948 File Offset: 0x000E1D48
	public void Exit(StateHandlerBase parent)
	{
		this.SetAttackingFlag(parent.combatController, false);
		this.ClearAttackHitBlocks(parent.combatController);
	}

	// Token: 0x06001F97 RID: 8087 RVA: 0x000E3964 File Offset: 0x000E1D64
	public bool Update(StateHandlerBase parent)
	{
		float normalizedTime = parent.targetController.GetCurrentAnimatorStateInfo(parent.GetAnimatorLayer()).normalizedTime;
		bool flag = normalizedTime >= this.damageStartNormalizedTime && normalizedTime < this.damageEndNormalizedTime;
		flag &= !parent.IsPlayingAnimation(this.recoilAnimation);
		this.SetAttackingFlag(parent.combatController, flag);
		if (flag)
		{
			Block leftHandAttachment = parent.combatController.GetLeftHandAttachment();
			Block rightHandAttachment = parent.combatController.GetRightHandAttachment();
			if (leftHandAttachment != null && this.isLeftHanded)
			{
				HashSet<Block> hashSet = parent.combatController.CheckLeftHandMeleeCollisions();
				if (hashSet != null)
				{
					Vector3 attackDirection = parent.combatController.LeftHandDelta();
					this.HandleWeaponHits(parent, hashSet, leftHandAttachment, leftHandAttachment.GetPosition(), attackDirection);
				}
			}
			if (rightHandAttachment != null && this.isRightHanded)
			{
				HashSet<Block> hashSet2 = parent.combatController.CheckRightHandMeleeCollisions();
				if (hashSet2 != null)
				{
					Vector3 attackDirection2 = parent.combatController.RightHandDelta();
					this.HandleWeaponHits(parent, hashSet2, rightHandAttachment, rightHandAttachment.GetPosition(), attackDirection2);
				}
			}
			if (this.isLeftFooted)
			{
				HashSet<Block> hashSet3 = parent.combatController.CheckLeftFootCollisions();
				if (hashSet3 != null)
				{
					Vector3 attackDirection3 = parent.combatController.LeftFootDelta();
					this.HandleWeaponHits(parent, hashSet3, null, parent.combatController.leftFootAttachmentParent.position, attackDirection3);
				}
			}
			if (this.isRightFooted)
			{
				HashSet<Block> hashSet4 = parent.combatController.CheckRightFootCollisions();
				if (hashSet4 != null)
				{
					Vector3 attackDirection4 = parent.combatController.RightFootDelta();
					this.HandleWeaponHits(parent, hashSet4, null, parent.combatController.rightFootAttachmentParent.position, attackDirection4);
				}
			}
		}
		bool flag2 = parent.IsPlayingAnimation(this.animationState) || parent.IsPlayingAnimation(this.recoilAnimation);
		return parent.targetController.IsInTransition(parent.GetAnimatorLayer()) || parent.TimeInCurrentState() < 0.15f || (flag2 && normalizedTime < 1f);
	}

	// Token: 0x06001F98 RID: 8088 RVA: 0x000E3B68 File Offset: 0x000E1F68
	private void HandleWeaponHits(StateHandlerBase parent, HashSet<Block> hitBlocks, Block weaponBlock, Vector3 attackPosition, Vector3 attackDirection)
	{
		if (hitBlocks.Count == 0)
		{
			return;
		}
		Vector3 forward = parent.targetController.transform.forward;
		Vector3 normalized = Vector3.Lerp(forward, attackDirection, 0.25f).normalized;
		bool flag = false;
		HashSet<BlockAnimatedCharacter> hashSet = new HashSet<BlockAnimatedCharacter>();
		foreach (Block block in hitBlocks)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(block);
			bool flag2 = (blockAnimatedCharacter == null) ? block.CanRepelAttack(attackPosition, normalized) : blockAnimatedCharacter.CanRepelAttack(attackPosition, normalized);
			if (flag2)
			{
				if (blockAnimatedCharacter != null && !hashSet.Contains(blockAnimatedCharacter))
				{
					blockAnimatedCharacter.ShieldHitReact(block);
					hashSet.Add(blockAnimatedCharacter);
				}
				flag = true;
			}
		}
		if (flag)
		{
			parent.PlayAnimation(this.recoilAnimation, true);
			parent.targetObject.PlayPositionedSound(this.recoilSFX[Mathf.FloorToInt(UnityEngine.Random.value * (float)this.recoilSFX.Length)], 0.8f, 1f);
		}
		else
		{
			foreach (Block block2 in hitBlocks)
			{
				BlockAnimatedCharacter blockAnimatedCharacter2 = BlockAnimatedCharacter.FindBlockOwner(block2);
				Block block3 = (blockAnimatedCharacter2 == null) ? block2 : blockAnimatedCharacter2;
				if (!parent.combatController.AlreadyHitDuringAttack(block3))
				{
					block3.OnAttacked(attackPosition, normalized);
					if (this.isLeftFooted || this.isRightFooted)
					{
						parent.combatController.OnHitByFoot(block3);
					}
					else
					{
						parent.combatController.OnHitByMeleeAttack(block3, weaponBlock);
					}
				}
			}
		}
	}

	// Token: 0x06001F99 RID: 8089 RVA: 0x000E3D50 File Offset: 0x000E2150
	private void SetAttackingFlag(CombatController combatController, bool attacking)
	{
		if (this.isLeftHanded)
		{
			combatController.leftHandAttachmentIsAttacking = attacking;
		}
		if (this.isRightHanded)
		{
			combatController.rightHandAttachmentIsAttacking = attacking;
		}
	}

	// Token: 0x06001F9A RID: 8090 RVA: 0x000E3D76 File Offset: 0x000E2176
	private void ClearAttackHitBlocks(CombatController combatController)
	{
		if (this.isLeftHanded)
		{
			combatController.ClearLeftHandAttackHitBlocks();
		}
		if (this.isRightHanded)
		{
			combatController.ClearRightHandAttackHitBlocks();
		}
		if (this.isLeftFooted || this.isRightFooted)
		{
			combatController.ClearFeetAttackHitBlocks();
		}
	}

	// Token: 0x040019B8 RID: 6584
	public string animationState;

	// Token: 0x040019B9 RID: 6585
	public string recoilAnimation;

	// Token: 0x040019BA RID: 6586
	public float damageStartNormalizedTime;

	// Token: 0x040019BB RID: 6587
	public float damageEndNormalizedTime;

	// Token: 0x040019BC RID: 6588
	public float interruptNormalizedTime;

	// Token: 0x040019BD RID: 6589
	public string[] swishSFX = new string[]
	{
		"Sword Swish"
	};

	// Token: 0x040019BE RID: 6590
	public string[] recoilSFX = new string[]
	{
		"Sword Clang A",
		"Sword Clang B"
	};

	// Token: 0x040019BF RID: 6591
	public bool isRightHanded;

	// Token: 0x040019C0 RID: 6592
	public bool isLeftHanded;

	// Token: 0x040019C1 RID: 6593
	public bool isRightFooted;

	// Token: 0x040019C2 RID: 6594
	public bool isLeftFooted;
}
