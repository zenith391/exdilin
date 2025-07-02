namespace Blocks;

public class CharacterPlayAnimState : CharacterBaseState
{
	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.playAnimFinished = false;
		parent.animationHash = parent.PlayAnimation(parent.playAnimCurrent, interrupt);
	}

	public override void Exit(CharacterStateHandler parent)
	{
		parent.ClearAnimation();
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if ((parent.animationHash != parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash || parent.targetController.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) && !parent.targetController.IsInTransition(0) && parent.stateTime > 0.25f)
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
