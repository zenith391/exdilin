using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterGetUpState : CharacterBaseState
{
	public List<string> animations;

	public List<float> timing;

	private bool smoothParentTransition;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		if (animations.Count == 0)
		{
			BWLog.Error("Attempting to start animation, but none found in list!");
			return;
		}
		parent.animationHash = -1;
		parent.startRotation = parent.targetObject.goT.rotation;
		EnterSideAnim(parent);
	}

	protected void EnterSideAnim(CharacterStateHandler parent)
	{
		int num = parent.lastSideAnim;
		if (num < 0)
		{
			num = 0;
		}
		parent.getUpAnim = timing[num];
		parent.PlayAnimation(animations[num], interrupt: true);
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if (parent.animationHash == -1 || (parent.targetObject != null && parent.targetObject.broken))
		{
			return false;
		}
		if (parent.isTransitioning || parent.stateTime < 0.1f)
		{
			return true;
		}
		AnimatorStateInfo currentAnimatorStateInfo = parent.targetController.GetCurrentAnimatorStateInfo(0);
		return parent.animationHash == currentAnimatorStateInfo.shortNameHash;
	}
}
