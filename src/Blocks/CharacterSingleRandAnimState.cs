using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterSingleRandAnimState : CharacterBaseState
{
	public List<string> animations;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		if (animations.Count == 0)
		{
			BWLog.Error("Attempting to start animation, but none found in list!");
			return;
		}
		parent.animationHash = parent.PlayAnimation(animations[0], interrupt);
		parent.timeInAnim = Random.Range(10, 25);
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if (parent.targetController.IsInTransition(0))
		{
			return true;
		}
		if (parent.stateTime > parent.timeInAnim)
		{
			int index = Random.Range(0, animations.Count);
			parent.stateBlend = 0.1f;
			string animName = animations[index];
			parent.animationHash = parent.PlayAnimation(animName);
			parent.timeInAnim = Random.Range(10, 25);
			parent.stateTime = 0f;
		}
		return true;
	}
}
