namespace Blocks;

public class CharacterDodgeState : CharacterBaseState
{
	public string animation;

	public float speed = 5f;

	public float dodgeStartNormalizedTime;

	public float dodgeEndNormalizedTime = 1f;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.PlayAnimation(animation, interrupt);
		parent.dodgeSpeed = 0f;
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
		parent.dodgeSpeed = 0f;
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		float num = parent.StateNormalizedTime();
		if (num > dodgeStartNormalizedTime && num < dodgeEndNormalizedTime)
		{
			parent.dodgeSpeed = speed;
		}
		else
		{
			parent.dodgeSpeed = 0f;
		}
		if (!(parent.stateTime < 0.15f))
		{
			if (parent.IsPlayingAnimation(animation))
			{
				return num < 1f;
			}
			return false;
		}
		return true;
	}
}
