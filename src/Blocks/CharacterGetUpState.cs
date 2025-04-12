using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000295 RID: 661
	public class CharacterGetUpState : CharacterBaseState
	{
		// Token: 0x06001EC6 RID: 7878 RVA: 0x000DC598 File Offset: 0x000DA998
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			if (this.animations.Count == 0)
			{
				BWLog.Error("Attempting to start animation, but none found in list!");
				return;
			}
			parent.animationHash = -1;
			parent.startRotation = parent.targetObject.goT.rotation;
			this.EnterSideAnim(parent);
		}

		// Token: 0x06001EC7 RID: 7879 RVA: 0x000DC5EC File Offset: 0x000DA9EC
		protected void EnterSideAnim(CharacterStateHandler parent)
		{
			int num = parent.lastSideAnim;
			if (num < 0)
			{
				num = 0;
			}
			parent.getUpAnim = this.timing[num];
			parent.PlayAnimation(this.animations[num], true);
		}

		// Token: 0x06001EC8 RID: 7880 RVA: 0x000DC62F File Offset: 0x000DAA2F
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x000DC638 File Offset: 0x000DAA38
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (parent.animationHash == -1 || (parent.targetObject != null && parent.targetObject.broken))
			{
				return false;
			}
			if (parent.isTransitioning || parent.stateTime < 0.1f)
			{
				return true;
			}
			AnimatorStateInfo currentAnimatorStateInfo = parent.targetController.GetCurrentAnimatorStateInfo(0);
			return parent.animationHash == currentAnimatorStateInfo.shortNameHash;
		}

		// Token: 0x040018CD RID: 6349
		public List<string> animations;

		// Token: 0x040018CE RID: 6350
		public List<float> timing;

		// Token: 0x040018CF RID: 6351
		private bool smoothParentTransition;
	}
}
