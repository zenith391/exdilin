namespace Blocks;

public class CharacterChainAnimState : CharacterBaseState
{
	public int animationHash;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		parent.playAnimFinished = false;
		base.Enter(parent, interrupt);
		parent.PlayAnimation(animationHash, interrupt);
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

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
		if (animationHash == parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash)
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
}
