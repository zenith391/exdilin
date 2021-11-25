using System;

// Token: 0x020002AC RID: 684
public class UpperBodyFireWeaponState : UpperBodyBaseState
{
	// Token: 0x06001FAB RID: 8107 RVA: 0x000E4068 File Offset: 0x000E2468
	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.combatController.SetAttackDamageDone(false);
		interrupt |= (parent.IsPlayingAnimation(this.animationState) || parent.targetController.IsInTransition(parent.GetAnimatorLayer()));
		parent.PlayAnimation(this.animationState, interrupt);
	}

	// Token: 0x06001FAC RID: 8108 RVA: 0x000E40C0 File Offset: 0x000E24C0
	public override void Exit(UpperBodyStateHandler parent)
	{
		base.Exit(parent);
		parent.combatController.SetAttackDamageDone(false);
	}

	// Token: 0x06001FAD RID: 8109 RVA: 0x000E40D8 File Offset: 0x000E24D8
	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		float normalizedTime = parent.targetController.GetCurrentAnimatorStateInfo(parent.GetAnimatorLayer()).normalizedTime;
		if (!parent.combatController.AttackDamageDone() && normalizedTime >= this.fireNormalizedTime)
		{
			parent.combatController.SetAttackDamageDone(true);
			if (this.isLeftHanded)
			{
				parent.combatController.OnFireLeftHandWeapon();
			}
			else
			{
				parent.combatController.OnFireRightHandWeapon();
			}
		}
		bool flag = parent.IsPlayingAnimation(this.animationState);
		return parent.targetController.IsInTransition(parent.GetAnimatorLayer()) || parent.stateTime < 0.15f || flag;
	}

	// Token: 0x040019CC RID: 6604
	public string animationState;

	// Token: 0x040019CD RID: 6605
	public float fireNormalizedTime;

	// Token: 0x040019CE RID: 6606
	public float interruptNormalizedTime;

	// Token: 0x040019CF RID: 6607
	public bool isLeftHanded;
}
