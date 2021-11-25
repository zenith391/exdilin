using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000296 RID: 662
	public class CharacterHoverState : CharacterBaseState
	{
		// Token: 0x06001ECB RID: 7883 RVA: 0x000DC6BD File Offset: 0x000DAABD
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			parent.PlayAnimation(this.animations[0], interrupt);
			parent.lastHoverAnim = 0;
		}

		// Token: 0x06001ECC RID: 7884 RVA: 0x000DC6E2 File Offset: 0x000DAAE2
		public override void Exit(CharacterStateHandler parent)
		{
			parent.lastHoverAnim = -1;
			base.Exit(parent);
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x000DC6F4 File Offset: 0x000DAAF4
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			MoveDirection moveDirection = MoveDirection.None;
			if (Mathf.Abs(parent.desiredGoto.z) < Mathf.Abs(parent.desiredGoto.x) || parent.walkStrafe || Mathf.Abs(parent.turnPower) > 0f)
			{
				if (parent.desiredGoto.x > 0.03f || parent.turnPower < 0f)
				{
					moveDirection = MoveDirection.XPos;
				}
				else if (parent.desiredGoto.x < -0.03f || parent.turnPower > 0f)
				{
					moveDirection = MoveDirection.XNeg;
				}
			}
			else if (parent.desiredGoto.z > 0.03f)
			{
				moveDirection = MoveDirection.ZPos;
			}
			else if (parent.desiredGoto.z < -0.03f)
			{
				moveDirection = MoveDirection.ZNeg;
			}
			switch (moveDirection)
			{
			case MoveDirection.XPos:
				this.PlayAnimation(parent, 3);
				return true;
			case MoveDirection.XNeg:
				this.PlayAnimation(parent, 4);
				return true;
			case MoveDirection.ZPos:
				this.PlayAnimation(parent, 1);
				return true;
			case MoveDirection.ZNeg:
				this.PlayAnimation(parent, 2);
				return true;
			}
			this.PlayAnimation(parent, 0);
			return true;
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x000DC844 File Offset: 0x000DAC44
		private void PlayAnimation(CharacterStateHandler parent, int animIndex)
		{
			if (parent.lastHoverAnim != animIndex)
			{
				string animName = this.animations[animIndex];
				if (!parent.IsPlayingAnimation(animName))
				{
					parent.lastHoverAnim = animIndex;
					parent.PlayAnimation(animName, false);
				}
			}
		}

		// Token: 0x040018D0 RID: 6352
		public List<string> animations;
	}
}
