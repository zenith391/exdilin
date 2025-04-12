using System;
using Blocks;
using UnityEngine;

// Token: 0x020002A6 RID: 678
public class CharacterAttackState : CharacterBaseState
{
	// Token: 0x06001F8C RID: 8076 RVA: 0x000E367E File Offset: 0x000E1A7E
	public CharacterAttackState(MeleeAttackState state)
	{
		this.attackState = state;
	}

	// Token: 0x06001F8D RID: 8077 RVA: 0x000E3698 File Offset: 0x000E1A98
	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.standingAttackForward = parent.targetObject.goT.forward;
		parent.standingAttackMaxSpeed = this.maxForwardSpeed;
		parent.standingAttackMinSpeed = this.minForwardSpeed;
		this.attackState.Enter(parent, interrupt);
	}

	// Token: 0x06001F8E RID: 8078 RVA: 0x000E36E8 File Offset: 0x000E1AE8
	public override void Exit(CharacterStateHandler parent)
	{
		this.attackState.Exit(parent);
		parent.standingAttackForward = Vector3.zero;
		parent.standingAttackMaxSpeed = 0f;
		parent.standingAttackMinSpeed = 0f;
		base.Exit(parent);
	}

	// Token: 0x06001F8F RID: 8079 RVA: 0x000E3720 File Offset: 0x000E1B20
	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		bool flag = parent.StateNormalizedTime() < this.moveEndNormalizedTime;
		parent.standingAttackMaxSpeed = ((!flag) ? 0f : this.maxForwardSpeed);
		parent.standingAttackMinSpeed = ((!flag) ? 0f : this.minForwardSpeed);
		return this.attackState.Update(parent);
	}

	// Token: 0x040019B3 RID: 6579
	public MeleeAttackState attackState;

	// Token: 0x040019B4 RID: 6580
	public float maxForwardSpeed = 10f;

	// Token: 0x040019B5 RID: 6581
	public float minForwardSpeed;

	// Token: 0x040019B6 RID: 6582
	public float moveEndNormalizedTime;
}
