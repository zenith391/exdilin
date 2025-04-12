using System;

namespace Blocks
{
	// Token: 0x0200029A RID: 666
	public class CharacterPlayAnimState : CharacterBaseState
	{
		// Token: 0x06001EE0 RID: 7904 RVA: 0x000DD4E4 File Offset: 0x000DB8E4
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			base.Enter(parent, interrupt);
			parent.playAnimFinished = false;
			parent.animationHash = parent.PlayAnimation(parent.playAnimCurrent, interrupt);
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x000DD508 File Offset: 0x000DB908
		public override void Exit(CharacterStateHandler parent)
		{
			parent.ClearAnimation();
			base.Exit(parent);
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x000DD518 File Offset: 0x000DB918
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			bool flag = parent.animationHash != parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash || parent.targetController.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
			if (flag && !parent.targetController.IsInTransition(0) && parent.stateTime > 0.25f)
			{
				parent.playAnimFinished = true;
				if (!parent.requestingPlayAnim)
				{
					return false;
				}
			}
			return true;
		}
	}
}
