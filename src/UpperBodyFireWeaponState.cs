public class UpperBodyFireWeaponState : UpperBodyBaseState
{
	public string animationState;

	public float fireNormalizedTime;

	public float interruptNormalizedTime;

	public bool isLeftHanded;

	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.combatController.SetAttackDamageDone(done: false);
		interrupt |= parent.IsPlayingAnimation(animationState) || parent.targetController.IsInTransition(parent.GetAnimatorLayer());
		parent.PlayAnimation(animationState, interrupt);
	}

	public override void Exit(UpperBodyStateHandler parent)
	{
		base.Exit(parent);
		parent.combatController.SetAttackDamageDone(done: false);
	}

	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		float normalizedTime = parent.targetController.GetCurrentAnimatorStateInfo(parent.GetAnimatorLayer()).normalizedTime;
		if (!parent.combatController.AttackDamageDone() && normalizedTime >= fireNormalizedTime)
		{
			parent.combatController.SetAttackDamageDone(done: true);
			if (isLeftHanded)
			{
				parent.combatController.OnFireLeftHandWeapon();
			}
			else
			{
				parent.combatController.OnFireRightHandWeapon();
			}
		}
		bool flag = parent.IsPlayingAnimation(animationState);
		return parent.targetController.IsInTransition(parent.GetAnimatorLayer()) || parent.stateTime < 0.15f || flag;
	}
}
