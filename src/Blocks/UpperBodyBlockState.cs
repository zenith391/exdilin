using System.Collections.Generic;

namespace Blocks;

public class UpperBodyBlockState : UpperBodyBaseState
{
	public List<string> animations;

	public string animationIn;

	public string animationLoop;

	public string animationOut;

	public string animationHitReact;

	public bool isLeftHanded;

	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		if (parent.IsPlayingAnimationFromList(new List<string> { animationLoop, animationHitReact }))
		{
			parent.PlayAnimation(animationLoop, interrupt);
		}
		else if (!parent.IsPlayingAnimation(animationIn))
		{
			parent.PlayAnimation(animationIn, interrupt);
		}
	}

	public override void Exit(UpperBodyStateHandler parent)
	{
		parent.combatController.ClearShieldHitFlags();
		DeactivateShield(parent);
		base.Exit(parent);
	}

	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		if (parent.IsPlayingAnimation(animationIn) && parent.stateTime > 0.2f)
		{
			ActivateShield(parent);
			parent.PlayAnimation(animationLoop);
		}
		else if (parent.IsPlayingAnimation(animationLoop) && !parent.combatController.IsShielding() && parent.stateTime > 0.4f)
		{
			parent.PlayAnimation(animationOut, interrupt: true);
			DeactivateShield(parent);
		}
		else if (parent.IsPlayingAnimation(animationLoop))
		{
			ActivateShield(parent);
			if (!parent.IsPlayingAnimationFromList(new List<string> { animationIn, animationLoop, animationHitReact }))
			{
				parent.PlayAnimation(animationIn, interrupt: true);
			}
			if (!parent.IsPlayingAnimation(animationHitReact) && parent.combatController.IsShieldHit())
			{
				parent.PlayAnimation(animationHitReact, interrupt: true);
				parent.combatController.ClearShieldHitFlags();
			}
		}
		int animatorLayer = parent.GetAnimatorLayer();
		if (!(parent.stateTime < 0.5f) && !parent.targetController.IsInTransition(animatorLayer))
		{
			return parent.IsPlayingAnimationFromList(animations);
		}
		return true;
	}

	private void ActivateShield(UpperBodyStateHandler parent)
	{
		if (isLeftHanded)
		{
			if (!parent.combatController.leftHandAttachmentIsShielding)
			{
				parent.combatController.ActivateLeftShield();
			}
		}
		else if (!parent.combatController.rightHandAttachmentIsShielding)
		{
			parent.combatController.ActivateRightShield();
		}
	}

	private void DeactivateShield(UpperBodyStateHandler parent)
	{
		if (isLeftHanded)
		{
			if (parent.combatController.leftHandAttachmentIsShielding)
			{
				parent.combatController.DeactivateLeftShield();
			}
		}
		else if (parent.combatController.rightHandAttachmentIsShielding)
		{
			parent.combatController.DeactivateRightShield();
		}
	}
}
