using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterHoverState : CharacterBaseState
{
	public List<string> animations;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.PlayAnimation(animations[0], interrupt);
		parent.lastHoverAnim = 0;
	}

	public override void Exit(CharacterStateHandler parent)
	{
		parent.lastHoverAnim = -1;
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		MoveDirection moveDirection = MoveDirection.None;
		if (Mathf.Abs(parent.desiredGoto.z) < Mathf.Abs(parent.desiredGoto.x) || parent.walkStrafe || Mathf.Abs(parent.turnPower) > 0f)
		{
			if (parent.desiredGoto.x > 0.03f || parent.turnPower < 0f)
			{
				moveDirection = MoveDirection.XPos;
			}
			else if (parent.desiredGoto.x < -0.03f || parent.turnPower > 0f)
			{
				moveDirection = MoveDirection.XNeg;
			}
		}
		else if (parent.desiredGoto.z > 0.03f)
		{
			moveDirection = MoveDirection.ZPos;
		}
		else if (parent.desiredGoto.z < -0.03f)
		{
			moveDirection = MoveDirection.ZNeg;
		}
		switch (moveDirection)
		{
		case MoveDirection.XPos:
			PlayAnimation(parent, 3);
			return true;
		case MoveDirection.XNeg:
			PlayAnimation(parent, 4);
			return true;
		case MoveDirection.ZPos:
			PlayAnimation(parent, 1);
			return true;
		case MoveDirection.ZNeg:
			PlayAnimation(parent, 2);
			return true;
		default:
			PlayAnimation(parent, 0);
			return true;
		}
	}

	private void PlayAnimation(CharacterStateHandler parent, int animIndex)
	{
		if (parent.lastHoverAnim != animIndex)
		{
			string animName = animations[animIndex];
			if (!parent.IsPlayingAnimation(animName))
			{
				parent.lastHoverAnim = animIndex;
				parent.PlayAnimation(animName);
			}
		}
	}
}
