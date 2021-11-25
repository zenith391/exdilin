using System;

namespace Blocks
{
	// Token: 0x02000293 RID: 659
	public class CharacterChainAnimState : CharacterBaseState
	{
		// Token: 0x06001EBE RID: 7870 RVA: 0x000DC418 File Offset: 0x000DA818
		public override void Enter(CharacterStateHandler parent, bool interrupt)
		{
			parent.playAnimFinished = false;
			base.Enter(parent, interrupt);
			parent.PlayAnimation(this.animationHash, interrupt);
		}

		// Token: 0x06001EBF RID: 7871 RVA: 0x000DC437 File Offset: 0x000DA837
		public override void Exit(CharacterStateHandler parent)
		{
			base.Exit(parent);
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x000DC440 File Offset: 0x000DA840
		public override bool Update(CharacterStateHandler parent)
		{
			base.Update(parent);
			if (parent.firstFrame)
			{
				parent.firstFrame = false;
				return true;
			}
			if (parent.stateTime < 0.2f)
			{
				return true;
			}
			if (this.animationHash == parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash)
			{
				return true;
			}
			if (parent.targetController.IsInTransition(0))
			{
				return true;
			}
			parent.playAnimFinished = true;
			return false;
		}

		// Token: 0x040018C8 RID: 6344
		public int animationHash;
	}
}
