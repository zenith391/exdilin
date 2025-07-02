using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterJumpState : CharacterBaseState
{
	public enum JumpState
	{
		Enter,
		Up,
		Peak,
		Down,
		Land,
		LandRunning
	}

	public List<string> animations;

	public List<float> blendSpeeds;

	public bool CanAddJumpForce(CharacterStateHandler parent)
	{
		JumpState currentJumpState = parent.currentJumpState;
		if (currentJumpState == JumpState.Enter || currentJumpState == JumpState.Up)
		{
			return !parent.hasDoubleJumped;
		}
		return false;
	}

	public bool HeadingUp(CharacterStateHandler parent)
	{
		return parent.currentJumpState < JumpState.Peak;
	}

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		int index = 0;
		parent.currentJumpState = JumpState.Enter;
		parent.desiredAnimSpeed = 1f;
		if (blendSpeeds != null)
		{
			parent.stateBlend = blendSpeeds[index];
		}
		if (parent.desiredJumpForce < 0f)
		{
			index = 2;
			parent.currentJumpState = JumpState.Down;
			if (parent.targetObject.NearGround(0.1f) > 0f)
			{
				parent.stateBlend = 0.3f;
				index = 3;
				parent.currentJumpState = JumpState.Land;
			}
			else
			{
				parent.stateBlend = 0.7f;
			}
		}
		parent.PlayAnimation(animations[index], interrupt);
		parent.hasDoubleJumped = false;
		parent.allowDouble = false;
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public void DoubleJump(CharacterStateHandler parent)
	{
		if (CanAddJumpForce(parent) && !parent.hasDoubleJumped && parent.allowDouble)
		{
			parent.hasDoubleJumped = true;
			parent.allowDouble = false;
			parent.PlayAnimation(animations[6]);
			parent.targetObject.walkController.AddJumpForce(15f * parent.desiredJumpForce);
		}
	}

	public void Bounce(CharacterStateHandler parent)
	{
		parent.PlayAnimation(animations[6]);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		parent.allowDouble = false;
		float y = parent.rb.velocity.y;
		if (parent.currentJumpState < JumpState.Peak && parent.targetObject.goT.up.y < 0.999f)
		{
			Transform goT = parent.targetObject.goT;
			Vector3 vector = goT.forward;
			if (goT.up.y < 0.5f)
			{
				vector = ((!(goT.forward.y > 0f)) ? goT.up : (-goT.up));
			}
			vector.y = 0f;
			Quaternion b = Quaternion.LookRotation(vector.normalized, Vector3.up);
			parent.targetObject.goT.rotation = Quaternion.Slerp(parent.targetObject.goT.rotation, b, 0.2f);
		}
		int shortNameHash = parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash;
		switch (parent.currentJumpState)
		{
		case JumpState.Enter:
			if (parent.animationHash != shortNameHash)
			{
				parent.animationHash = shortNameHash;
				if (blendSpeeds != null)
				{
					parent.stateBlend = blendSpeeds[1];
				}
				parent.currentJumpState = JumpState.Up;
				parent.targetObject.walkController.AddJumpForce(15f * parent.desiredJumpForce);
			}
			break;
		case JumpState.Up:
			if (!(y < 0.001f))
			{
				break;
			}
			if (parent.hasDoubleJumped)
			{
				if (blendSpeeds != null)
				{
					parent.stateBlend = blendSpeeds[2];
				}
				parent.PlayAnimation(animations[1]);
				parent.currentJumpState = JumpState.Peak;
			}
			else
			{
				parent.currentJumpState = JumpState.Down;
			}
			break;
		case JumpState.Peak:
			if (parent.animationHash != shortNameHash)
			{
				if (blendSpeeds != null)
				{
					parent.stateBlend = blendSpeeds[3];
				}
				parent.animationHash = shortNameHash;
				parent.currentJumpState = JumpState.Down;
			}
			break;
		case JumpState.Down:
		{
			if (!parent.targetObject.IsOnGround())
			{
				break;
			}
			if (blendSpeeds != null)
			{
				parent.stateBlend = blendSpeeds[4];
			}
			float a = ((parent.rb.velocity.y <= 0f) ? parent.rb.velocity.magnitude : 0f);
			parent.maxDownSpeedDuringJump = Mathf.Max(a, parent.maxDownSpeedDuringJump);
			parent.stateTime = 0f;
			parent.blendStart = 0f;
			float bounciness = 0f;
			if (parent.targetObject.CanBounce(out bounciness))
			{
				parent.Bounce(bounciness);
			}
			else if (parent.desiresMove)
			{
				if (blendSpeeds != null)
				{
					parent.stateBlend = blendSpeeds[5];
				}
				parent.currentJumpState = JumpState.LandRunning;
				parent.PlayAnimation((parent.currentVelocity.z <= 9f) ? animations[4] : animations[5]);
			}
			else
			{
				parent.PlayAnimation(animations[3]);
				parent.currentJumpState = JumpState.Land;
			}
			if (parent.currentJumpState == JumpState.Land || parent.currentJumpState == JumpState.LandRunning)
			{
				bool flag = parent.desiredGoto.sqrMagnitude > 0.1f;
				Vector3 downSlopeDirection = parent.targetObject.walkController.GetDownSlopeDirection();
				Vector3 forward = parent.targetObject.goT.forward;
				float num;
				float num2;
				if (flag)
				{
					num = 0.3f;
					num2 = 0.25f;
				}
				else
				{
					num = 0.1f;
					num2 = 0.45f;
				}
				if (downSlopeDirection.sqrMagnitude < Mathf.Epsilon)
				{
					num += num2;
					num2 = 0f;
				}
				Vector3 vector2 = num * forward;
				Vector3 vector3 = num2 * downSlopeDirection;
				parent.targetObject.walkController.slideVelocity = parent.maxDownSpeedDuringJump * (vector2 + vector3);
			}
			parent.desiredAnimSpeed = 1f;
			break;
		}
		case JumpState.Land:
			if (parent.animationHash != shortNameHash && !parent.isTransitioning && parent.stateTime > 0.1f)
			{
				return false;
			}
			if (parent.desiresMove && !parent.isTransitioning && parent.stateTime > 0.1f)
			{
				return false;
			}
			break;
		case JumpState.LandRunning:
			if (parent.animationHash != shortNameHash && !parent.isTransitioning && parent.stateTime > 0.05f)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void OnScreenDebug(CharacterStateHandler parent)
	{
		ActionDebug.AddMessage("Jump State", string.Empty + parent.currentJumpState);
	}
}
