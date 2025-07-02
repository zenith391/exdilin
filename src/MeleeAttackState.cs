using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class MeleeAttackState
{
	public string animationState;

	public string recoilAnimation;

	public float damageStartNormalizedTime;

	public float damageEndNormalizedTime;

	public float interruptNormalizedTime;

	public string[] swishSFX = new string[1] { "Sword Swish" };

	public string[] recoilSFX = new string[2] { "Sword Clang A", "Sword Clang B" };

	public bool isRightHanded;

	public bool isLeftHanded;

	public bool isRightFooted;

	public bool isLeftFooted;

	public void Enter(StateHandlerBase parent, bool interrupt)
	{
		if (animationState == null)
		{
			BWLog.Error("No animation set for attack state");
			return;
		}
		int animatorLayer = parent.GetAnimatorLayer();
		interrupt |= parent.IsPlayingAnimation(animationState) || parent.targetController.IsInTransition(animatorLayer);
		parent.PlayAnimation(animationState, interrupt);
		Block leftHandAttachment = parent.combatController.GetLeftHandAttachment();
		Block rightHandAttachment = parent.combatController.GetRightHandAttachment();
		if ((leftHandAttachment != null && isLeftHanded) || (rightHandAttachment != null && isRightHanded))
		{
			int delay = 10;
			if (animationState.StartsWith("Chop"))
			{
				delay = 15;
			}
			else if (animationState.StartsWith("SwordLunge"))
			{
				delay = 20;
			}
			else if (animationState.StartsWith("SwordJump"))
			{
				delay = 25;
			}
			parent.targetObject.PlayPositionedSoundAfterDelay(delay, swishSFX[Mathf.FloorToInt(Random.value * (float)swishSFX.Length)], 0.4f);
		}
		ClearAttackHitBlocks(parent.combatController);
	}

	public void Exit(StateHandlerBase parent)
	{
		SetAttackingFlag(parent.combatController, attacking: false);
		ClearAttackHitBlocks(parent.combatController);
	}

	public bool Update(StateHandlerBase parent)
	{
		float normalizedTime = parent.targetController.GetCurrentAnimatorStateInfo(parent.GetAnimatorLayer()).normalizedTime;
		bool flag = normalizedTime >= damageStartNormalizedTime && normalizedTime < damageEndNormalizedTime;
		flag &= !parent.IsPlayingAnimation(recoilAnimation);
		SetAttackingFlag(parent.combatController, flag);
		if (flag)
		{
			Block leftHandAttachment = parent.combatController.GetLeftHandAttachment();
			Block rightHandAttachment = parent.combatController.GetRightHandAttachment();
			if (leftHandAttachment != null && isLeftHanded)
			{
				HashSet<Block> hashSet = parent.combatController.CheckLeftHandMeleeCollisions();
				if (hashSet != null)
				{
					Vector3 attackDirection = parent.combatController.LeftHandDelta();
					HandleWeaponHits(parent, hashSet, leftHandAttachment, leftHandAttachment.GetPosition(), attackDirection);
				}
			}
			if (rightHandAttachment != null && isRightHanded)
			{
				HashSet<Block> hashSet2 = parent.combatController.CheckRightHandMeleeCollisions();
				if (hashSet2 != null)
				{
					Vector3 attackDirection2 = parent.combatController.RightHandDelta();
					HandleWeaponHits(parent, hashSet2, rightHandAttachment, rightHandAttachment.GetPosition(), attackDirection2);
				}
			}
			if (isLeftFooted)
			{
				HashSet<Block> hashSet3 = parent.combatController.CheckLeftFootCollisions();
				if (hashSet3 != null)
				{
					Vector3 attackDirection3 = parent.combatController.LeftFootDelta();
					HandleWeaponHits(parent, hashSet3, null, parent.combatController.leftFootAttachmentParent.position, attackDirection3);
				}
			}
			if (isRightFooted)
			{
				HashSet<Block> hashSet4 = parent.combatController.CheckRightFootCollisions();
				if (hashSet4 != null)
				{
					Vector3 attackDirection4 = parent.combatController.RightFootDelta();
					HandleWeaponHits(parent, hashSet4, null, parent.combatController.rightFootAttachmentParent.position, attackDirection4);
				}
			}
		}
		bool flag2 = parent.IsPlayingAnimation(animationState) || parent.IsPlayingAnimation(recoilAnimation);
		if (!parent.targetController.IsInTransition(parent.GetAnimatorLayer()) && !(parent.TimeInCurrentState() < 0.15f))
		{
			if (flag2)
			{
				return normalizedTime < 1f;
			}
			return false;
		}
		return true;
	}

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
		foreach (Block hitBlock in hitBlocks)
		{
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(hitBlock);
			if (blockAnimatedCharacter?.CanRepelAttack(attackPosition, normalized) ?? hitBlock.CanRepelAttack(attackPosition, normalized))
			{
				if (blockAnimatedCharacter != null && !hashSet.Contains(blockAnimatedCharacter))
				{
					blockAnimatedCharacter.ShieldHitReact(hitBlock);
					hashSet.Add(blockAnimatedCharacter);
				}
				flag = true;
			}
		}
		if (flag)
		{
			parent.PlayAnimation(recoilAnimation, interrupt: true);
			parent.targetObject.PlayPositionedSound(recoilSFX[Mathf.FloorToInt(Random.value * (float)recoilSFX.Length)], 0.8f);
			return;
		}
		foreach (Block hitBlock2 in hitBlocks)
		{
			BlockAnimatedCharacter blockAnimatedCharacter2 = BlockAnimatedCharacter.FindBlockOwner(hitBlock2);
			Block block = ((blockAnimatedCharacter2 == null) ? hitBlock2 : blockAnimatedCharacter2);
			if (!parent.combatController.AlreadyHitDuringAttack(block))
			{
				block.OnAttacked(attackPosition, normalized);
				if (isLeftFooted || isRightFooted)
				{
					parent.combatController.OnHitByFoot(block);
				}
				else
				{
					parent.combatController.OnHitByMeleeAttack(block, weaponBlock);
				}
			}
		}
	}

	private void SetAttackingFlag(CombatController combatController, bool attacking)
	{
		if (isLeftHanded)
		{
			combatController.leftHandAttachmentIsAttacking = attacking;
		}
		if (isRightHanded)
		{
			combatController.rightHandAttachmentIsAttacking = attacking;
		}
	}

	private void ClearAttackHitBlocks(CombatController combatController)
	{
		if (isLeftHanded)
		{
			combatController.ClearLeftHandAttackHitBlocks();
		}
		if (isRightHanded)
		{
			combatController.ClearRightHandAttackHitBlocks();
		}
		if (isLeftFooted || isRightFooted)
		{
			combatController.ClearFeetAttackHitBlocks();
		}
	}
}
