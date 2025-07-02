using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class CharacterBaseState
{
	public CharacterState desiredState;

	public List<Vector3> rootOffsets;

	public virtual void Enter(CharacterStateHandler parent, bool interrupt)
	{
		parent.targetController.speed = 1f;
		parent.desiredAnimSpeed = 1f;
		Blocksworld.blocksworldCamera.fpcTilt = 0f;
		parent.firstFrame = true;
	}

	public virtual void Exit(CharacterStateHandler parent)
	{
		Blocksworld.blocksworldCamera.fpcTilt = 0f;
		parent.desiredAnimSpeed = 1f;
	}

	public virtual bool Update(CharacterStateHandler parent)
	{
		return true;
	}

	public virtual Vector3 GetOffset(CharacterStateHandler parent)
	{
		if (rootOffsets != null && rootOffsets.Count > 0)
		{
			return rootOffsets[0];
		}
		return Vector3.zero;
	}

	public virtual void OnScreenDebug(CharacterStateHandler parent)
	{
	}
}
