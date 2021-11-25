using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200012A RID: 298
public class DelayedBreakCharacterCommand : DelayedCommand
{
	// Token: 0x06001412 RID: 5138 RVA: 0x0008C4DE File Offset: 0x0008A8DE
	public DelayedBreakCharacterCommand(Block block, Vector3 chunkPos, Vector3 chunkVel, Vector3 chunkAngVel) : base(1)
	{
		this.block = block;
		this.chunkPos = chunkPos;
		this.chunkVel = chunkVel;
		this.chunkAngVel = chunkAngVel;
	}

	// Token: 0x06001413 RID: 5139 RVA: 0x0008C504 File Offset: 0x0008A904
	protected override void DelayedExecute()
	{
		base.DelayedExecute();
		if (this.block is BlockCharacter)
		{
			this.CharacterBreak(this.block as BlockCharacter);
		}
		else if (this.block is BlockAnimatedCharacter)
		{
			this.AnimatedCharacterBreak(this.block as BlockAnimatedCharacter);
		}
	}

	// Token: 0x06001414 RID: 5140 RVA: 0x0008C560 File Offset: 0x0008A960
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
			Block.AddExplosiveForce(rigidbody, gameObject.transform.position, this.chunkPos, this.chunkVel, this.chunkAngVel, 1f);
			FootInfo footInfo = feet[i];
			if (footInfo.rb == null)
			{
				Util.UnparentTransformSafely(footInfo.go.transform);
				Rigidbody rigidbody2 = footInfo.go.AddComponent<Rigidbody>();
				footInfo.rb = rigidbody2;
				if (Blocksworld.interpolateRigidBodies)
				{
					rigidbody2.interpolation = RigidbodyInterpolation.Interpolate;
				}
				rigidbody2.mass = 0.5f;
				if (footInfo.collider != null)
				{
					footInfo.collider.enabled = true;
				}
				Block.AddExplosiveForce(rigidbody2, footInfo.go.transform.position, this.chunkPos, this.chunkVel, this.chunkAngVel, 1f);
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
		Block.AddExplosiveForce(rigidbody3, middle.transform.position, this.chunkPos, this.chunkVel, this.chunkAngVel, 1f);
		if (character.BlockType().EndsWith("Headless"))
		{
			Blocksworld.blocksworldCamera.Unfollow(character.chunk);
			Blocksworld.chunks.Remove(character.chunk);
			character.chunk.Destroy(true);
			character.go.SetActive(false);
		}
		else if (character.collider != null)
		{
			character.collider.size = Vector3.one;
			character.collider.center = new Vector3(0f, 0.5f, -0.05f);
			character.chunk.UpdateCenterOfMass(true);
		}
	}

	// Token: 0x06001415 RID: 5141 RVA: 0x0008C7D4 File Offset: 0x0008ABD4
	protected void AnimatedCharacterBreak(BlockAnimatedCharacter animCharacter)
	{
		foreach (BlocksterBody.BodyPart bodyPart in BlocksterBody.AllLimbs)
		{
			foreach (BlocksterBody.Bone bone in BlocksterBody.GetBonesForBodyPart(bodyPart))
			{
				List<GameObject> objectsForBone = animCharacter.bodyParts.GetObjectsForBone(bone);
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
					Block.AddExplosiveForce(rigidbody, gameObject.transform.position, this.chunkPos, this.chunkVel, this.chunkAngVel, 1f);
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
		Block.AddExplosiveForce(rigidbody2, middle.transform.position, this.chunkPos, this.chunkVel, this.chunkAngVel, 1f);
		BoxCollider component = animCharacter.go.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.size = Vector3.one;
			component.center = new Vector3(0f, 0.5f, 0f);
			animCharacter.chunk.UpdateCenterOfMass(true);
		}
		else
		{
			BWLog.Warning("No collider found when blowing up character");
		}
	}

	// Token: 0x04000FB0 RID: 4016
	private Block block;

	// Token: 0x04000FB1 RID: 4017
	private Vector3 chunkPos;

	// Token: 0x04000FB2 RID: 4018
	private Vector3 chunkVel;

	// Token: 0x04000FB3 RID: 4019
	private Vector3 chunkAngVel;
}
