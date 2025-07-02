using Blocks;
using UnityEngine;

public class CharacterAttackState : CharacterBaseState
{
	public MeleeAttackState attackState;

	public float maxForwardSpeed = 10f;

	public float minForwardSpeed;

	public float moveEndNormalizedTime;

	public CharacterAttackState(MeleeAttackState state)
	{
		attackState = state;
	}

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.standingAttackForward = parent.targetObject.goT.forward;
		parent.standingAttackMaxSpeed = maxForwardSpeed;
		parent.standingAttackMinSpeed = minForwardSpeed;
		attackState.Enter(parent, interrupt);
	}

	public override void Exit(CharacterStateHandler parent)
	{
		attackState.Exit(parent);
		parent.standingAttackForward = Vector3.zero;
		parent.standingAttackMaxSpeed = 0f;
		parent.standingAttackMinSpeed = 0f;
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		bool flag = parent.StateNormalizedTime() < moveEndNormalizedTime;
		parent.standingAttackMaxSpeed = ((!flag) ? 0f : maxForwardSpeed);
		parent.standingAttackMinSpeed = ((!flag) ? 0f : minForwardSpeed);
		return attackState.Update(parent);
	}
}
