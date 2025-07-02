using Blocks;
using UnityEngine;

public abstract class StateHandlerBase
{
	public CombatController combatController;

	public Animator targetController;

	public BlockAnimatedCharacter targetObject;

	public abstract bool IsPlayingAnimation(string animName);

	public abstract int PlayAnimation(string animName, bool interrupt = false);

	public abstract int GetAnimatorLayer();

	public abstract float TimeInCurrentState();
}
