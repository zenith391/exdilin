using System;

// Token: 0x020002A7 RID: 679
public class UpperBodyAttackState : UpperBodyBaseState
{
	// Token: 0x06001F90 RID: 8080 RVA: 0x000E3797 File Offset: 0x000E1B97
	public UpperBodyAttackState(MeleeAttackState state)
	{
		this.attackState = state;
	}

	// Token: 0x06001F91 RID: 8081 RVA: 0x000E37A6 File Offset: 0x000E1BA6
	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		this.attackState.Enter(parent, interrupt);
	}

	// Token: 0x06001F92 RID: 8082 RVA: 0x000E37BD File Offset: 0x000E1BBD
	public override void Exit(UpperBodyStateHandler parent)
	{
		this.attackState.Exit(parent);
		base.Exit(parent);
	}

	// Token: 0x06001F93 RID: 8083 RVA: 0x000E37D2 File Offset: 0x000E1BD2
	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		return this.attackState.Update(parent);
	}

	// Token: 0x040019B7 RID: 6583
	public MeleeAttackState attackState;
}
