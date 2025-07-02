using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterSingleAnimState : CharacterBaseState
{
	public int animationHash;

	public List<string> animations;

	public override void Enter(CharacterStateHandler parent, bool interrupt)
	{
		base.Enter(parent, interrupt);
		parent.animationHash = parent.PlayAnimation(animations[Random.Range(0, animations.Count)], interrupt);
		parent.timeInAnim = Random.Range(15, 45);
		if (Blocksworld.CurrentState != State.Play)
		{
			parent.targetController.speed = Random.Range(0.9f, 1.05f);
		}
	}

	public override void Exit(CharacterStateHandler parent)
	{
		base.Exit(parent);
	}

	public override bool Update(CharacterStateHandler parent)
	{
		base.Update(parent);
		if (Blocksworld.CurrentState != State.Play)
		{
			if (parent.stateTime > parent.timeInAnim)
			{
				int index = Random.Range(0, animations.Count);
				parent.stateBlend = 0.1f;
				string animName = animations[index];
				parent.animationHash = parent.PlayAnimation(animName);
				parent.timeInAnim = Random.Range(15, 45);
				parent.stateTime = 0f;
				parent.targetController.speed = Random.Range(0.9f, 1.05f);
			}
			return true;
		}
		if (animationHash != parent.targetController.GetCurrentAnimatorStateInfo(0).shortNameHash && !parent.targetController.IsInTransition(0))
		{
			return parent.stateTime <= 0.25f;
		}
		return true;
	}
}
