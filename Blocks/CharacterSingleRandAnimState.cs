using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200029C RID: 668
	public class CharacterSingleRandAnimState : CharacterBaseState
	{
		// Token: 0x06001EE8 RID: 7912 RVA: 0x000DD720 File Offset: 0x000DBB20
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			if (this.animations.Count == 0)
			{
				BWLog.Error("Attempting to start animation, but none found in list!");
				return;
			}
			parent.animationHash = parent.PlayAnimation(this.animations[0], interrupt);
			parent.timeInAnim = (float)UnityEngine.Random.Range(10, 25);
		}

		// Token: 0x06001EE9 RID: 7913 RVA: 0x000DD779 File Offset: 0x000DBB79
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x000DD784 File Offset: 0x000DBB84
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (parent.targetController.IsInTransition(0))
			{
				return true;
			}
			bool flag = parent.stateTime > parent.timeInAnim;
			if (flag)
			{
				int index = UnityEngine.Random.Range(0, this.animations.Count);
				parent.stateBlend = 0.1f;
				string animName = this.animations[index];
				parent.animationHash = parent.PlayAnimation(animName, false);
				parent.timeInAnim = (float)UnityEngine.Random.Range(10, 25);
				parent.stateTime = 0f;
			}
			return true;
		}

		// Token: 0x040018E3 RID: 6371
		public List<string> animations;
	}
}
