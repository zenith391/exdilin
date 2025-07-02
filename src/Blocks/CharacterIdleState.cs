using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterIdleState : CharacterBaseState
{
	public List<string> animations;

	public string leftIdle;

	public string rightIdle;

	public string frontIdle;

	public string backIdle;

	public string topIdle;

	public List<Vector3> sideOffsets;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		if (animations.Count == 0)
		{
			BWLog.Error("Attempting to start animation, but none found in list!");
			return;
		}
		parent.sideAnim = -1;
		if (parent.targetObject.goT.up.y > 0.8f)
		{
			parent.animationHash = parent.PlayAnimation(animations[0], interrupt);
			parent.timeInAnim = Random.Range(10, 25);
		}
		else
		{
			EnterSideAnim(parent);
		}
	}

	protected void EnterSideAnim(CharacterStateHandler parent)
	{
		string empty = string.Empty;
		int sideAnim = parent.sideAnim;
		parent.stateBlend = 0.1f;
		if (parent.targetObject.goT.up.y < -0.5f)
		{
			sideAnim = 0;
			empty = ((!(topIdle != string.Empty)) ? animations[0] : topIdle);
		}
		else if (parent.targetObject.goT.forward.y < -0.5f)
		{
			sideAnim = 1;
			empty = ((!(frontIdle != string.Empty)) ? animations[0] : frontIdle);
		}
		else if (parent.targetObject.goT.forward.y > 0.5f)
		{
			sideAnim = 2;
			empty = ((!(backIdle != string.Empty)) ? animations[0] : backIdle);
		}
		else if (parent.targetObject.goT.right.y > 0.5f)
		{
			sideAnim = 3;
			empty = ((!(leftIdle != string.Empty)) ? animations[0] : leftIdle);
		}
		else
		{
			if (parent.targetObject.goT.right.y >= -0.5f)
			{
				return;
			}
			sideAnim = 4;
			empty = ((!(rightIdle != string.Empty)) ? animations[0] : rightIdle);
		}
		if (sideAnim != parent.sideAnim)
		{
			parent.stateTime = 0f;
			parent.timeInAnim = 3f;
			parent.lastForward = parent.targetObject.goT.forward;
			parent.lastRight = parent.targetObject.goT.right;
			parent.lastUp = parent.targetObject.goT.up;
			parent.sideAnim = sideAnim;
			parent.PlayAnimation(empty);
		}
	}

	public bool IsOnSide(CharacterStateHandler parent)
	{
		return parent.sideAnim != -1;
	}

	public override Vector3 GetOffset(CharacterStateHandler parent)
	{
		if (!IsOnSide(parent) || sideOffsets == null)
		{
			return base.GetOffset(parent);
		}
		return sideOffsets[parent.sideAnim];
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if (parent.targetController.IsInTransition(0) || parent.currentState == CharacterState.EditSitting)
		{
			return true;
		}
		bool flag = false;
		if (parent.sideAnim != -1)
		{
			parent.lastSideAnim = parent.sideAnim;
			if (!parent.IsPulling() && !parent.WasPulling())
			{
				Quaternion identity = Quaternion.identity;
				Vector3 lastForward = parent.lastForward;
				lastForward.y = 0f;
				lastForward = lastForward.normalized;
				Vector3 lastUp = parent.lastUp;
				lastUp.y = 0f;
				lastUp = lastUp.normalized;
				Vector3 vector = parent.lastForward;
				switch (parent.sideAnim)
				{
				case 0:
					vector = parent.lastForward;
					break;
				case 1:
					vector = parent.lastUp;
					break;
				case 2:
					vector = -parent.lastUp;
					break;
				case 3:
					vector = parent.lastForward;
					break;
				case 4:
					vector = parent.lastForward;
					break;
				}
				vector.y = 0f;
				identity = Quaternion.LookRotation(vector.normalized, Vector3.up);
				parent.SetSideAnimRotation(identity);
			}
			if (parent.stateTime > parent.timeInAnim)
			{
				parent.InterruptQueue(CharacterState.GetUp);
				return false;
			}
		}
		else if (parent.timeInAnim > 0f)
		{
			flag = parent.stateTime > parent.timeInAnim;
			flag &= parent.upperBody.GetState() == UpperBodyState.BaseLayer && parent.upperBody.stateTime > parent.timeInAnim;
		}
		else
		{
			flag = !parent.targetController.IsInTransition(0) && parent.animationHash != parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash;
		}
		if (flag && Blocksworld.blocksworldCamera.firstPersonBlock != parent.targetObject)
		{
			parent.sideAnim = -1;
			int num = 0;
			if (parent.timeInAnim > 0f)
			{
				num = Random.Range(0, animations.Count);
			}
			parent.stateBlend = 0.1f;
			string animName = animations[num];
			parent.animationHash = parent.PlayAnimation(animName);
			if (num == 0)
			{
				parent.timeInAnim = Random.Range(10, 25);
				parent.stateTime = 0f;
			}
			else
			{
				parent.timeInAnim = -1f;
			}
		}
		return true;
	}
}
