public class UpperBodyIdleState : UpperBodyBaseState
{
	public string animationState;

	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		parent.PlayAnimation(animationState, interrupt);
	}

	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		return true;
	}
}
