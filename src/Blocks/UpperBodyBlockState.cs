using System;
using System.Collections.Generic;

namespace Blocks
{
	// Token: 0x020002AB RID: 683
	public class UpperBodyBlockState : UpperBodyBaseState
	{
		// Token: 0x06001FA5 RID: 8101 RVA: 0x000E3DC0 File Offset: 0x000E21C0
		public override void Enter(UpperBodyStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			if (parent.IsPlayingAnimationFromList(new List<string>
			{
				this.animationLoop,
				this.animationHitReact
			}))
			{
				parent.PlayAnimation(this.animationLoop, interrupt);
			}
			else if (!parent.IsPlayingAnimation(this.animationIn))
			{
				parent.PlayAnimation(this.animationIn, interrupt);
			}
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x000E3E31 File Offset: 0x000E2231
		public override void Exit(UpperBodyStateHandler parent)
		{
			parent.combatController.ClearShieldHitFlags();
			this.DeactivateShield(parent);
			base.Exit(parent);
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x000E3E4C File Offset: 0x000E224C
		public override bool Update(UpperBodyStateHandler parent)
		{
			base.Update(parent);
			if (parent.IsPlayingAnimation(this.animationIn) && parent.stateTime > 0.2f)
			{
				this.ActivateShield(parent);
				parent.PlayAnimation(this.animationLoop, false);
			}
			else if (parent.IsPlayingAnimation(this.animationLoop) && !parent.combatController.IsShielding() && parent.stateTime > 0.4f)
			{
				parent.PlayAnimation(this.animationOut, true);
				this.DeactivateShield(parent);
			}
			else if (parent.IsPlayingAnimation(this.animationLoop))
			{
				this.ActivateShield(parent);
				if (!parent.IsPlayingAnimationFromList(new List<string>
				{
					this.animationIn,
					this.animationLoop,
					this.animationHitReact
				}))
				{
					parent.PlayAnimation(this.animationIn, true);
				}
				if (!parent.IsPlayingAnimation(this.animationHitReact) && parent.combatController.IsShieldHit())
				{
					parent.PlayAnimation(this.animationHitReact, true);
					parent.combatController.ClearShieldHitFlags();
				}
			}
			int animatorLayer = parent.GetAnimatorLayer();
			return parent.stateTime < 0.5f || parent.targetController.IsInTransition(animatorLayer) || parent.IsPlayingAnimationFromList(this.animations);
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x000E3FB8 File Offset: 0x000E23B8
		private void ActivateShield(UpperBodyStateHandler parent)
		{
			if (this.isLeftHanded)
			{
				if (!parent.combatController.leftHandAttachmentIsShielding)
				{
					parent.combatController.ActivateLeftShield();
				}
			}
			else if (!parent.combatController.rightHandAttachmentIsShielding)
			{
				parent.combatController.ActivateRightShield();
			}
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x000E400C File Offset: 0x000E240C
		private void DeactivateShield(UpperBodyStateHandler parent)
		{
			if (this.isLeftHanded)
			{
				if (parent.combatController.leftHandAttachmentIsShielding)
				{
					parent.combatController.DeactivateLeftShield();
				}
			}
			else if (parent.combatController.rightHandAttachmentIsShielding)
			{
				parent.combatController.DeactivateRightShield();
			}
		}

		// Token: 0x040019C6 RID: 6598
		public List<string> animations;

		// Token: 0x040019C7 RID: 6599
		public string animationIn;

		// Token: 0x040019C8 RID: 6600
		public string animationLoop;

		// Token: 0x040019C9 RID: 6601
		public string animationOut;

		// Token: 0x040019CA RID: 6602
		public string animationHitReact;

		// Token: 0x040019CB RID: 6603
		public bool isLeftHanded;
	}
}
