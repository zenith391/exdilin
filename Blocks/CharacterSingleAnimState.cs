using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200029B RID: 667
	public class CharacterSingleAnimState : CharacterBaseState
	{
		// Token: 0x06001EE4 RID: 7908 RVA: 0x000DD5B4 File Offset: 0x000DB9B4
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			parent.animationHash = parent.PlayAnimation(this.animations[UnityEngine.Random.Range(0, this.animations.Count)], interrupt);
			parent.timeInAnim = (float)UnityEngine.Random.Range(15, 45);
			if (Blocksworld.CurrentState != State.Play)
			{
				parent.targetController.speed = UnityEngine.Random.Range(0.9f, 1.05f);
			}
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x000DD627 File Offset: 0x000DBA27
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x000DD630 File Offset: 0x000DBA30
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (Blocksworld.CurrentState != State.Play)
			{
				bool flag = parent.stateTime > parent.timeInAnim;
				if (flag)
				{
					int index = UnityEngine.Random.Range(0, this.animations.Count);
					parent.stateBlend = 0.1f;
					string animName = this.animations[index];
					parent.animationHash = parent.PlayAnimation(animName, false);
					parent.timeInAnim = (float)UnityEngine.Random.Range(15, 45);
					parent.stateTime = 0f;
					parent.targetController.speed = UnityEngine.Random.Range(0.9f, 1.05f);
				}
				return true;
			}
			return this.animationHash == parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash || parent.targetController.IsInTransition(0) || parent.stateTime <= 0.25f;
		}

		// Token: 0x040018E1 RID: 6369
		public int animationHash;

		// Token: 0x040018E2 RID: 6370
		public List<string> animations;
	}
}
