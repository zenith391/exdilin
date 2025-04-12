using System;

// Token: 0x020002AA RID: 682
public class UpperBodyBaseState
{
	// Token: 0x06001FA1 RID: 8097 RVA: 0x000E3790 File Offset: 0x000E1B90
	public virtual void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
	}

	// Token: 0x06001FA2 RID: 8098 RVA: 0x000E3792 File Offset: 0x000E1B92
	public virtual void Exit(UpperBodyStateHandler parent)
	{
	}

	// Token: 0x06001FA3 RID: 8099 RVA: 0x000E3794 File Offset: 0x000E1B94
	public virtual bool Update(UpperBodyStateHandler parent)
	{
		return true;
	}
}
