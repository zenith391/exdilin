using System;
using Blocks;
using UnityEngine;

// Token: 0x020002A9 RID: 681
public abstract class StateHandlerBase
{
	// Token: 0x06001F9C RID: 8092
	public abstract bool IsPlayingAnimation(string animName);

	// Token: 0x06001F9D RID: 8093
	public abstract int PlayAnimation(string animName, bool interrupt = false);

	// Token: 0x06001F9E RID: 8094
	public abstract int GetAnimatorLayer();

	// Token: 0x06001F9F RID: 8095
	public abstract float TimeInCurrentState();

	// Token: 0x040019C3 RID: 6595
	public CombatController combatController;

	// Token: 0x040019C4 RID: 6596
	public Animator targetController;

	// Token: 0x040019C5 RID: 6597
	public BlockAnimatedCharacter targetObject;
}
