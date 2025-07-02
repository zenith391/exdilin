using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class CharacterEventHandler : MonoBehaviour
{
	private BlockAnimatedCharacter targetBlock;

	private Rigidbody legsRb;

	protected bool FindBlock()
	{
		targetBlock = BWSceneManager.FindBlock(base.gameObject.transform.parent.gameObject) as BlockAnimatedCharacter;
		if (targetBlock == null)
		{
			return false;
		}
		legsRb = targetBlock.GetRigidBody();
		return legsRb != null;
	}

	private void Impulse(float power)
	{
		if ((targetBlock != null && !(legsRb == null)) || FindBlock())
		{
			legsRb.AddForceAtPosition(power * targetBlock.goT.forward, legsRb.worldCenterOfMass);
		}
	}

	private void StepLeft()
	{
		if (targetBlock != null || FindBlock())
		{
			List<string> possibleSfxs = targetBlock.GetPossibleSfxs("Legs Step");
			if (possibleSfxs.Count > 0)
			{
				string sfxName = possibleSfxs[0 % possibleSfxs.Count];
				targetBlock.PlayPositionedSound(sfxName, 0.2f);
			}
		}
	}

	private void StepRight()
	{
		if (targetBlock != null || FindBlock())
		{
			List<string> possibleSfxs = targetBlock.GetPossibleSfxs("Legs Step");
			if (possibleSfxs.Count > 0)
			{
				string sfxName = possibleSfxs[1 % possibleSfxs.Count];
				targetBlock.PlayPositionedSound(sfxName, 0.2f);
			}
		}
	}

	private void PlaySound(string soundName)
	{
		if (targetBlock != null || FindBlock())
		{
			targetBlock.PlayPositionedSound(soundName, 0.2f);
		}
	}

	private void StartRotation()
	{
		if (targetBlock != null || FindBlock())
		{
			_ = targetBlock.stateHandler;
		}
	}
}
