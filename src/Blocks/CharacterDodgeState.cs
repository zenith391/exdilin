using System;

namespace Blocks
{
	// Token: 0x02000294 RID: 660
	public class CharacterDodgeState : CharacterBaseState
	{
		// Token: 0x06001EC2 RID: 7874 RVA: 0x000DC4D5 File Offset: 0x000DA8D5
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			parent.PlayAnimation(this.animation, interrupt);
			parent.dodgeSpeed = 0f;
		}

		// Token: 0x06001EC3 RID: 7875 RVA: 0x000DC4F8 File Offset: 0x000DA8F8
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
			parent.dodgeSpeed = 0f;
		}

		// Token: 0x06001EC4 RID: 7876 RVA: 0x000DC50C File Offset: 0x000DA90C
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			float num = parent.StateNormalizedTime();
			if (num > this.dodgeStartNormalizedTime && num < this.dodgeEndNormalizedTime)
			{
				parent.dodgeSpeed = this.speed;
			}
			else
			{
				parent.dodgeSpeed = 0f;
			}
			return parent.stateTime < 0.15f || (parent.IsPlayingAnimation(this.animation) && num < 1f);
		}

		// Token: 0x040018C9 RID: 6345
		public string animation;

		// Token: 0x040018CA RID: 6346
		public float speed = 5f;

		// Token: 0x040018CB RID: 6347
		public float dodgeStartNormalizedTime;

		// Token: 0x040018CC RID: 6348
		public float dodgeEndNormalizedTime = 1f;
	}
}
