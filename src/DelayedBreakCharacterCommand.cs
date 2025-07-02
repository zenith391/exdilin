using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class DelayedBreakCharacterCommand : DelayedCommand
{
	private Block block;

	private Vector3 chunkPos;

	private Vector3 chunkVel;

	private Vector3 chunkAngVel;

	public DelayedBreakCharacterCommand(Block block, Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel)
		: base(1)
	{
		this.block = block;
		this.chunkPos = chunkPos;
		this.chunkVel = chunkVel;
		this.chunkAngVel = chunkAngVel;
	}

	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		if (block is BlockCharacter)
		{
			CharacterBreak(block as BlockCharacter);
		}
		else if (block is BlockAnimatedCharacter)
		{
			AnimatedCharacterBreak(block as BlockAnimatedCharacter);
		}
	}

	protected void CharacterBreak(BlockCharacter character)
	{
		FootInfo[] feet = character.feet;
		GameObject[] hands = character.hands;
		for (int i = 0; i < 2; i++)
		{
			GameObject gameObject = hands[i];
			Util.UnparentTransformSafely(gameObject.transform);
			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = gameObject.AddComponent<Rigidbody>();
			}
			if (Blocksworld.interpolateRigidBodies)
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}
			rigidbody.mass = 0.5f;
			gameObject.GetComponent<Collider>().enabled = true;
			Block.AddExplosiveForce(rigidbody, gameObject.transform.position, chunkPos, chunkVel, chunkAngVel);
			FootInfo footInfo = feet[i];
			if (footInfo.rb == null)
			{
				Util.UnparentTransformSafely(footInfo.go.transform);
				Rigidbody rigidbody2 = (footInfo.rb = footInfo.go.AddComponent<Rigidbody>());
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody2.interpolation = RigidbodyInterpolation.Interpolate;
				}
				rigidbody2.mass = 0.5f;
				if (footInfo.collider != null)
				{
					footInfo.collider.enabled = true;
				}
				Block.AddExplosiveForce(rigidbody2, footInfo.go.transform.position, chunkPos, chunkVel, chunkAngVel);
			}
		}
		GameObject middle = character.middle;
		Util.UnparentTransformSafely(middle.transform);
		Rigidbody rigidbody3 = middle.GetComponent<Rigidbody>();
		if (rigidbody3 == null)
		{
			rigidbody3 = middle.AddComponent<Rigidbody>();
		}
		if (Blocksworld.interpolateRigidBodies)
		{
			rigidbody3.interpolation = RigidbodyInterpolation.Interpolate;
		}
		rigidbody3.mass = 0.75f;
		Block.AddExplosiveForce(rigidbody3, middle.transform.position, chunkPos, chunkVel, chunkAngVel);
		if (character.BlockType().EndsWith("Headless"))
		{
			Blocksworld.blocksworldCamera.Unfollow(character.chunk);
			Blocksworld.chunks.Remove(character.chunk);
			character.chunk.Destroy(delayed: true);
			character.go.SetActive(value: false);
		}
		else if (character.collider != null)
		{
			character.collider.size = Vector3.one;
			character.collider.center = new Vector3(0f, 0.5f, -0.05f);
			character.chunk.UpdateCenterOfMass();
		}
	}

	protected void AnimatedCharacterBreak(BlockAnimatedCharacter animCharacter)
	{
		foreach (BlocksterBody.BodyPart allLimb in BlocksterBody.AllLimbs)
		{
			foreach (BlocksterBody.Bone item in BlocksterBody.GetBonesForBodyPart(allLimb))
			{
				List<GameObject> objectsForBone = animCharacter.bodyParts.GetObjectsForBone(item);
				GameObject gameObject = null;
				for (int i = 0; i < objectsForBone.Count; i++)
				{
					GameObject gameObject2 = objectsForBone[i];
					Util.UnparentTransformSafely(gameObject2.transform);
					Collider collider = gameObject2.GetComponent<Collider>();
					if (collider == null)
					{
						collider = gameObject2.AddComponent<BoxCollider>();
					}
					collider.enabled = true;
					if (i == 0)
					{
						gameObject = gameObject2;
					}
					else
					{
						gameObject2.transform.parent = gameObject.transform;
					}
				}
				if (gameObject != null)
				{
					Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
					if (rigidbody == null)
					{
						rigidbody = gameObject.AddComponent<Rigidbody>();
					}
					if (Blocksworld.interpolateRigidBodies)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
					rigidbody.mass = 0.5f;
					Block.AddExplosiveForce(rigidbody, gameObject.transform.position, chunkPos, chunkVel, chunkAngVel);
				}
			}
		}
		GameObject middle = animCharacter.middle;
		Util.UnparentTransformSafely(middle.transform);
		Rigidbody rigidbody2 = middle.GetComponent<Rigidbody>();
		if (rigidbody2 == null)
		{
			rigidbody2 = middle.AddComponent<Rigidbody>();
		}
		if (Blocksworld.interpolateRigidBodies)
		{
			rigidbody2.interpolation = RigidbodyInterpolation.Interpolate;
		}
		rigidbody2.mass = 0.75f;
		middle.GetComponent<Collider>().enabled = true;
		Block.AddExplosiveForce(rigidbody2, middle.transform.position, chunkPos, chunkVel, chunkAngVel);
		BoxCollider component = animCharacter.go.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.size = Vector3.one;
			component.center = new Vector3(0f, 0.5f, 0f);
			animCharacter.chunk.UpdateCenterOfMass();
		}
		else
		{
			BWLog.Warning("No collider found when blowing up character");
		}
	}
}
