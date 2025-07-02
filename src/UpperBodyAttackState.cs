public class UpperBodyAttackState : UpperBodyBaseState
{
	public MeleeAttackState attackState;

	public UpperBodyAttackState(MeleeAttackState state)
	{
		attackState = state;
	}

	public override void Enter(UpperBodyStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		attackState.Enter(parent, interrupt);
	}

	public override void Exit(UpperBodyStateHandler parent)
	{
		attackState.Exit(parent);
		base.Exit(parent);
	}

	public override bool Update(UpperBodyStateHandler parent)
	{
		base.Update(parent);
		return attackState.Update(parent);
	}
}
