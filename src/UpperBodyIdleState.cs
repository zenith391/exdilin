using System;

// Token: 0x020002AD RID: 685
public class UpperBodyIdleState : UpperBodyBaseState
{
	// Token: 0x06001FAF RID: 8111 RVA: 0x000E4196 File Offset: 0x000E2596
	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		parent.PlayAnimation(this.animationState, interrupt);
	}

	// Token: 0x06001FB0 RID: 8112 RVA: 0x000E41A6 File Offset: 0x000E25A6
	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		return true;
	}

	// Token: 0x040019D0 RID: 6608
	public string animationState;
}
