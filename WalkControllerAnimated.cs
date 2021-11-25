using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x02000349 RID: 841
public class WalkControllerAnimated
{
	// Token: 0x060025A8 RID: 9640 RVA: 0x00113ECC File Offset: 0x001122CC
	public WalkControllerAnimated(BlockWalkable target)
	{
		this.legs = target;
		this.character = (target as BlockAnimatedCharacter);
		this.legsRb = this.legs.GetRigidBody();
		this.legsTransform = this.legs.goT;
		this.currentMoveDirection = this.legs.goT.forward;
		this.currentFaceDirection = this.currentMoveDirection;
	}

	// Token: 0x060025A9 RID: 9641 RVA: 0x001140BF File Offset: 0x001124BF
	public Vector3 GetRigidBodyBelowVelocity()
	{
		return (!(this.rigidBodyBelow == null)) ? this.rigidBodyBelowVelocity : Vector3.zero;
	}

	// Token: 0x060025AA RID: 9642 RVA: 0x001140E4 File Offset: 0x001124E4
	public float GetWantedSpeedSqr()
	{
		return this.wantedVel.sqrMagnitude;
	}

	// Token: 0x060025AB RID: 9643 RVA: 0x001140FE File Offset: 0x001124FE
	public bool IsFPC()
	{
		return this.legs == Blocksworld.blocksworldCamera.firstPersonBlock && !this.legs.unmoving && !this.legs.IsFixed();
	}

	// Token: 0x060025AC RID: 9644 RVA: 0x00114138 File Offset: 0x00112538
	private void AddJumpForce(float force, bool swim)
	{
		this.totalJumpEnergy += force / (float)(1 + this.jumpCountThisFrame * ((!swim) ? 1 : 4));
		this.jumpEnergyLeft = this.totalJumpEnergy;
		this.jumpTimer = 0.5f;
		this.jumpCountThisFrame++;
		this.legs.jumpCountdown = this.legs.startJumpCountdown;
		for (int i = 0; i < this.chunkControllers.Count; i++)
		{
			WalkControllerAnimated walkControllerAnimated = this.chunkControllers[i];
			walkControllerAnimated.legs.jumpCountdown = walkControllerAnimated.legs.startJumpCountdown;
		}
	}

	// Token: 0x060025AD RID: 9645 RVA: 0x001141E8 File Offset: 0x001125E8
	public void Jump(float force)
	{
		this.jumpPressed = true;
		float massM = this.legs.GetBlockMetaData().massM;
		WalkControllerAnimated.JumpMode jumpMode = this.jumpMode;
		if (jumpMode == WalkControllerAnimated.JumpMode.Ready)
		{
			if (!this.climbing)
			{
				if (this.onGround && this.legs.upright)
				{
					this.AddJumpForce(force * 50f * massM, false);
					this.jumpUp = Vector3.up;
				}
				else if (BlockWater.BlockWithinWater(this.legs, false))
				{
					this.AddJumpForce(force * 50f * massM, true);
					this.jumpUp = this.legs.goT.up;
				}
			}
		}
	}

	// Token: 0x060025AE RID: 9646 RVA: 0x001142A4 File Offset: 0x001126A4
	public void SetChunk()
	{
		this.chunk = this.legs.chunk;
		this.chunkBlocks = new HashSet<Block>();
		foreach (Block block in this.chunk.blocks)
		{
			this.chunkBlocks.Add(block);
			if (block is BlockWalkable && block != this.legs)
			{
				BlockWalkable blockWalkable = (BlockWalkable)block;
				if (blockWalkable.walkController != null)
				{
					this.chunkControllers.Add(blockWalkable.walkController);
				}
			}
		}
		foreach (WalkControllerAnimated walkControllerAnimated in this.chunkControllers)
		{
			float num = Vector3.Dot(walkControllerAnimated.legs.goT.up, this.legs.goT.up);
			if (num > 0.99f)
			{
				this.sameUpControllers.Add(walkControllerAnimated);
			}
			else if (num < -0.99f)
			{
				this.conflictingUpControllers.Add(walkControllerAnimated);
			}
		}
	}

	// Token: 0x060025AF RID: 9647 RVA: 0x00114404 File Offset: 0x00112804
	public bool IsActive()
	{
		return this.requestingTurn || this.requestingTranslate || this.requestingTurnToTag || this.requestingDPadControl || this.requestingTurnAlongCamera || this.vicinityMode != WalkControllerAnimated.VicinityMode.None;
	}

	// Token: 0x060025B0 RID: 9648 RVA: 0x00114457 File Offset: 0x00112857
	public bool GotDeliberateMovement()
	{
		return this.translating;
	}

	// Token: 0x060025B1 RID: 9649 RVA: 0x0011445F File Offset: 0x0011285F
	public void StartPull()
	{
		this.beingPulled = true;
	}

	// Token: 0x060025B2 RID: 9650 RVA: 0x00114468 File Offset: 0x00112868
	public void StopPull()
	{
		this.beingPulled = false;
		this.wasPulled = true;
	}

	// Token: 0x060025B3 RID: 9651 RVA: 0x00114478 File Offset: 0x00112878
	public void CancelPull()
	{
		this.beingPulled = false;
		this.wasPulled = false;
	}

	// Token: 0x060025B4 RID: 9652 RVA: 0x00114488 File Offset: 0x00112888
	public bool WasPulled()
	{
		return this.wasPulled;
	}

	// Token: 0x060025B5 RID: 9653 RVA: 0x00114490 File Offset: 0x00112890
	public void Translate(Vector3 dir, float maxSpeed)
	{
		this.requestingTranslate |= (maxSpeed > 0f);
		this.aggregatedTranslateRequest += maxSpeed * dir.normalized;
	}

	// Token: 0x060025B6 RID: 9654 RVA: 0x001144C8 File Offset: 0x001128C8
	public void AvoidTag(string tagName, float avoidDistance, float maxSpeed, float analog)
	{
		this.vicinityMode = WalkControllerAnimated.VicinityMode.AvoidTag;
		float num;
		float num2;
		if (this.avoidDistances.TryGetValue(tagName, out num))
		{
			this.avoidDistances[tagName] = num + analog * avoidDistance;
			num2 = this.avoidApplications[tagName] + analog;
		}
		else
		{
			this.avoidDistances[tagName] = analog * avoidDistance;
			num2 = analog;
		}
		this.avoidApplications[tagName] = num2;
		this.avoidMaxSpeed = Mathf.Max(analog * maxSpeed + 2.5f * Mathf.Max(0f, num2 - 1f), this.avoidMaxSpeed);
	}

	// Token: 0x060025B7 RID: 9655 RVA: 0x00114563 File Offset: 0x00112963
	public void Turn(float speed)
	{
		if (Mathf.Abs(speed) > Mathf.Epsilon)
		{
			this.requestingTurn = true;
			this.turnRequestSpeed += speed;
		}
	}

	// Token: 0x060025B8 RID: 9656 RVA: 0x0011458A File Offset: 0x0011298A
	public void TurnTowardsTag(string tagName)
	{
		this.requestingTurnToTag = true;
		this.turnToTagRequestStr = tagName;
	}

	// Token: 0x060025B9 RID: 9657 RVA: 0x0011459C File Offset: 0x0011299C
	public void DPadControl(string key, float maxSpeed)
	{
		if (key == "L" && maxSpeed > 0f)
		{
			Vector2 vector = (!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			this.requestingDPadControl = (vector.sqrMagnitude > Mathf.Epsilon);
			if (this.requestingDPadControl)
			{
				this.dPadControlRequestDir = vector.normalized;
				this.dPadControlRequestSpeed = maxSpeed * Mathf.Min(1f, vector.magnitude);
			}
		}
	}

	// Token: 0x060025BA RID: 9658 RVA: 0x00114639 File Offset: 0x00112A39
	public void TiltMoverControl(Vector2 tiltVector)
	{
		this.requestingDPadControl = true;
		this.dPadControlRequestDir = tiltVector.normalized;
		this.dPadControlRequestSpeed = tiltVector.magnitude;
	}

	// Token: 0x060025BB RID: 9659 RVA: 0x0011465C File Offset: 0x00112A5C
	public bool TryGetClosestBlockWithTag(string tagName, out Block targetBlock, bool allowChunk = false)
	{
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		targetBlock = null;
		if (blocksWithTag.Count > 0)
		{
			float num = 1E+09f;
			Vector3 position = this.legs.goT.position;
			bool result = false;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				if (allowChunk || !this.chunkBlocks.Contains(block))
				{
					Vector3 position2 = block.goT.position;
					float sqrMagnitude = (position - position2).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						targetBlock = block;
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	// Token: 0x060025BC RID: 9660 RVA: 0x00114710 File Offset: 0x00112B10
	public bool AtBlock(Block block, out Vector3 gotoPoint, float distCheck = 1f)
	{
		Collider component = block.go.GetComponent<Collider>();
		Vector3 position = this.legs.goT.position;
		Bounds bounds = component.bounds;
		gotoPoint = ((!bounds.Contains(position)) ? bounds.ClosestPoint(position) : block.GetPosition());
		if ((gotoPoint - position).magnitude < distCheck)
		{
			return true;
		}
		Ray ray = new Ray(position, (gotoPoint - position).normalized);
		RaycastHit raycastHit;
		return component.Raycast(ray, out raycastHit, distCheck);
	}

	// Token: 0x060025BD RID: 9661 RVA: 0x001147B0 File Offset: 0x00112BB0
	public bool GotoTag(string tagName, float maxSpeed)
	{
		this.gotoTagBlock = null;
		this.requestingGoToTag = this.TryGetClosestBlockWithTag(tagName, out this.gotoTagBlock, false);
		if (this.requestingGoToTag)
		{
			this.gotoTagRequestStr = tagName;
			this.gotoTagRequestSpeed = maxSpeed;
			bool flag = this.AtBlock(this.gotoTagBlock, out this.gotoTagTarget, 1.5f);
			if (flag)
			{
				this.requestingGoToTag = false;
			}
			return !flag;
		}
		return false;
	}

	// Token: 0x060025BE RID: 9662 RVA: 0x0011481C File Offset: 0x00112C1C
	public void ChaseTag(string tagName, float maxSpeed)
	{
		this.gotoTagBlock = null;
		this.requestingGoToTag = this.TryGetClosestBlockWithTag(tagName, out this.gotoTagBlock, false);
		if (this.requestingGoToTag)
		{
			this.gotoTagTarget = this.gotoTagBlock.GetCenter();
			Vector3 vector = this.gotoTagTarget - this.legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude < 0.25f)
			{
				this.requestingGoToTag = false;
			}
			else
			{
				this.gotoTagRequestStr = tagName;
				this.gotoTagRequestSpeed = maxSpeed;
			}
		}
	}

	// Token: 0x060025BF RID: 9663 RVA: 0x001148B3 File Offset: 0x00112CB3
	public void GotoTap(float maxSpeed)
	{
		this.requestingGoToTap = true;
		this.gotoTapRequestSpeed = maxSpeed;
	}

	// Token: 0x060025C0 RID: 9664 RVA: 0x001148C3 File Offset: 0x00112CC3
	public void TurnTowardsTap()
	{
		this.requestingTurnToTap = true;
	}

	// Token: 0x060025C1 RID: 9665 RVA: 0x001148CC File Offset: 0x00112CCC
	public void TurnAlongCamera()
	{
		this.requestingTurnAlongCamera = true;
	}

	// Token: 0x060025C2 RID: 9666 RVA: 0x001148D5 File Offset: 0x00112CD5
	public void SetCapsuleCollider(CapsuleCollider c)
	{
		this.capsule = c;
	}

	// Token: 0x060025C3 RID: 9667 RVA: 0x001148DE File Offset: 0x00112CDE
	public void AddIgnoreCollider(Collider c)
	{
		if (this.ignoreColliders == null)
		{
			this.GatherIgnoreColliders();
		}
		this.ignoreColliders.Add(c);
	}

	// Token: 0x060025C4 RID: 9668 RVA: 0x00114900 File Offset: 0x00112D00
	private void GatherIgnoreColliders()
	{
		this.ignoreColliders = new HashSet<Collider>();
		if (this.legs.feet != null && this.legs.feet.Length > 0)
		{
			for (int i = 0; i < 2 * this.legs.legPairCount; i++)
			{
				if (this.legs.feet[i].collider != null)
				{
					this.ignoreColliders.Add(this.legs.feet[i].collider);
				}
			}
		}
		if (this.legs.body != null)
		{
			foreach (Transform transform in this.legs.body.GetComponentsInChildren<Transform>())
			{
				Collider component = transform.gameObject.GetComponent<Collider>();
				if (component != null)
				{
					this.ignoreColliders.Add(component);
				}
			}
			foreach (WalkControllerAnimated walkControllerAnimated in this.chunkControllers)
			{
				BlockWalkable blockWalkable = walkControllerAnimated.legs;
				for (int k = 0; k < 2 * blockWalkable.legPairCount; k++)
				{
					this.ignoreColliders.Add(walkControllerAnimated.legs.feet[k].collider);
				}
			}
		}
	}

	// Token: 0x060025C5 RID: 9669 RVA: 0x00114A8C File Offset: 0x00112E8C
	private bool DoGroundRaycast(Vector3 fromPosition, Vector3 direction, float maxDistance, out RaycastHit hit)
	{
		RaycastHit[] array = Physics.RaycastAll(fromPosition, direction, maxDistance, BlockWalkable.raycastMask);
		if (this.ignoreColliders == null)
		{
			this.GatherIgnoreColliders();
		}
		hit = default(RaycastHit);
		float num = float.MaxValue;
		bool result = false;
		if (array.Length > 0)
		{
			foreach (RaycastHit raycastHit in array)
			{
				Collider collider = raycastHit.collider;
				if (!(collider == null) && !(collider.gameObject == null) && !collider.isTrigger && !this.ignoreColliders.Contains(collider))
				{
					float sqrMagnitude = (raycastHit.point - fromPosition).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						hit = raycastHit;
						result = true;
					}
				}
			}
		}
		return result;
	}

	// Token: 0x060025C6 RID: 9670 RVA: 0x00114B70 File Offset: 0x00112F70
	private bool DoGroundRaycastWithNormalCheck(Vector3 fromPosition, Vector3 direction, float maxDistance, ref float closestDist, ref Vector3 closestPoint, ref Vector3 closestPointNormal, ref GameObject closestPointGameObject)
	{
		RaycastHit raycastHit = default(RaycastHit);
		if (!this.DoGroundRaycast(fromPosition, direction, maxDistance, out raycastHit))
		{
			return false;
		}
		Vector3 vector = fromPosition - raycastHit.point;
		float num = Mathf.Clamp(Vector3.Dot(raycastHit.normal, Vector3.up), 0f, 1f);
		if (num > 0.2f)
		{
			if (vector.magnitude < closestDist)
			{
				closestDist = vector.magnitude;
				closestPoint = raycastHit.point;
				closestPointNormal = raycastHit.normal;
				closestPointGameObject = raycastHit.collider.gameObject;
			}
			return true;
		}
		return false;
	}

	// Token: 0x060025C7 RID: 9671 RVA: 0x00114C18 File Offset: 0x00113018
	private bool DoOnGroundCheck()
	{
		float num = 1f;
		float d = num * 0.45f;
		float num2 = num * 0.3f;
		float num3 = num * 2f + num2;
		Vector3 vector = this.legsTransform.position + this.legsTransform.up * this.capsule.center.y;
		vector -= this.legsTransform.up * (this.capsule.height / 2f - this.capsule.radius);
		vector += Vector3.down * (this.capsule.radius - num2);
		Vector3 fromPosition = vector;
		float num4 = num3;
		Vector3 zero = Vector3.zero;
		Vector3 up = Vector3.up;
		GameObject gameObject = null;
		Vector3 direction = -Vector3.up;
		bool flag = this.DoGroundRaycastWithNormalCheck(fromPosition, direction, num3, ref num4, ref zero, ref up, ref gameObject);
		fromPosition = vector + this.legsTransform.forward * d;
		flag |= this.DoGroundRaycastWithNormalCheck(fromPosition, direction, num3, ref num4, ref zero, ref up, ref gameObject);
		fromPosition = vector - this.legsTransform.forward * d;
		flag |= this.DoGroundRaycastWithNormalCheck(fromPosition, direction, num3, ref num4, ref zero, ref up, ref gameObject);
		fromPosition = vector + this.legsTransform.right * d;
		flag |= this.DoGroundRaycastWithNormalCheck(fromPosition, direction, num3, ref num4, ref zero, ref up, ref gameObject);
		fromPosition = vector - this.legsTransform.right * d;
		flag |= this.DoGroundRaycastWithNormalCheck(fromPosition, direction, num3, ref num4, ref zero, ref up, ref gameObject);
		this.rigidBodyBelow = null;
		this.rigidBodyBelowVelocity = Vector3.zero;
		this.onGroundBlock = null;
		if (!flag)
		{
			this.prevHeightError = 0f;
			this.onGround = false;
			this.nearGround = false;
			this.groundNormal = Vector3.zero;
			return false;
		}
		Transform parent = gameObject.transform.parent;
		if (parent != null)
		{
			this.AddRbBelowVelocity(zero, parent.gameObject.GetComponent<Rigidbody>(), false);
		}
		else
		{
			this.AddRbBelowVelocity(zero, gameObject.GetComponent<Rigidbody>(), true);
		}
		this.onGroundPoint = zero;
		this.groundNormal = up;
		this.onGroundHeight = Mathf.Max(Mathf.Epsilon, num4 - num2);
		this.nearGround = true;
		this.onGround = (this.onGroundHeight < 0.2f * num);
		bool flag2 = this.onGroundHeight < num * (this.legs.onGroundHeight + 0.6f);
		this.onGroundBlock = ((!flag2) ? null : BWSceneManager.FindBlock(gameObject, false));
		if (this.onGroundBlock != null)
		{
			Collider component = this.onGroundBlock.go.GetComponent<Collider>();
			PhysicMaterial material = component.material;
			this.groundFriction = material.dynamicFriction;
			this.groundBounciness = material.bounciness;
		}
		else
		{
			this.groundFriction = 0.4f;
			this.groundBounciness = 0f;
		}
		return this.onGround;
	}

	// Token: 0x060025C8 RID: 9672 RVA: 0x00114F40 File Offset: 0x00113340
	private void AddRbBelowVelocity(Vector3 hitPoint, Rigidbody rb, bool checkSurfaceBlocks = false)
	{
		this.rigidBodyBelow = rb;
		if (this.rigidBodyBelow != null && !this.rigidBodyBelow.isKinematic)
		{
			this.rigidBodyBelowVelocity = this.rigidBodyBelow.velocity;
			Vector3 rhs = hitPoint - this.rigidBodyBelow.worldCenterOfMass;
			this.rigidBodyBelowVelocity += Vector3.Cross(this.rigidBodyBelow.angularVelocity, rhs);
			if (checkSurfaceBlocks)
			{
				Block block = BWSceneManager.FindBlock(rb.gameObject, true);
				BlockTankTreadsWheel blockTankTreadsWheel = block as BlockTankTreadsWheel;
				BlockTankTreadsWheel.TreadLink treadLink;
				if (blockTankTreadsWheel != null && blockTankTreadsWheel.treadsInfo != null && blockTankTreadsWheel.treadsInfo.gameObjectToTreadLink.TryGetValue(rb.gameObject, out treadLink))
				{
					this.rigidBodyBelowVelocity += treadLink.GetTreadVelocity();
				}
			}
		}
	}

	// Token: 0x060025C9 RID: 9673 RVA: 0x0011501C File Offset: 0x0011341C
	private void GotoFixedUpdate(float maxSpeed)
	{
		Vector3 worldCenterOfMass = this.legsRb.worldCenterOfMass;
		if (this.character == null)
		{
			return;
		}
		if (this.gotoMaxSpeed > 0.001f)
		{
			this.speedFraction = maxSpeed / this.gotoMaxSpeed;
		}
		else
		{
			this.speedFraction = 0f;
		}
		if (this.character.stateHandler.IsSwimming())
		{
			this.speedFraction *= 4f;
		}
		if (this.IsFPC() && (this.requestingTranslate || this.requestingDPadControl))
		{
			Vector3 forward = this.legsTransform.forward;
			Vector3 right = this.legsTransform.right;
			Vector2 firstPersonDpad = Blocksworld.blocksworldCamera.firstPersonDpad;
			float num = Blocksworld.blocksworldCamera.firstPersonTurnScale * Blocksworld.blocksworldCamera.firstPersonRotation;
			if (Blocksworld.blocksworldCamera.firstPersonMode == 2)
			{
				firstPersonDpad.x = 1.5f * num;
				num = -0.0005f * Blocksworld.blocksworldCamera.firstPersonLookOffset.x;
			}
			BlockAnimatedCharacter firstPersonAnimatedCharacter = Blocksworld.blocksworldCamera.firstPersonAnimatedCharacter;
			if (firstPersonAnimatedCharacter != null && !firstPersonAnimatedCharacter.GetThinksOnGround() && Mathf.Abs(num) > 0.05f && Mathf.Abs(firstPersonDpad.y) < 0.1f)
			{
				firstPersonDpad.y = 0.1f;
			}
			this.currentMoveDirection = 0.2f * num * right + this.currentMoveDirection;
			this.currentMoveDirection.y = 0f;
			this.currentMoveDirection.Normalize();
			if (!this.character.stateHandler.walkStrafe)
			{
				this.currentFaceDirection = this.currentMoveDirection;
			}
			this.gotoOffset = 100f * (firstPersonDpad.x * right + firstPersonDpad.y * forward);
		}
		if (this.gotoOffset.z < 0f && Mathf.Abs(this.gotoOffset.x) <= 0.0001f)
		{
			this.gotoOffset.x = -0.05f * this.gotoOffset.z;
		}
		Vector3 vector = worldCenterOfMass + this.gotoOffset;
		this.character.stateHandler.desiredGoto = this.legsTransform.InverseTransformPoint(vector);
		Vector3 vector2 = Vector3.zero;
		this.totalForce = Vector3.zero;
		this.totalCmTorque = Vector3.zero;
		Vector3 vector3 = vector - worldCenterOfMass;
		vector3.y = 0f;
		float magnitude = vector3.magnitude;
		Vector3 normalized = vector3.normalized;
		Vector3 velocity = this.legsRb.velocity;
		velocity.y = 0f;
		this.currentVelFiltered = 0.9f * this.currentVelFiltered + 0.100000024f * velocity;
		float d = maxSpeed * this.character.stateHandler.GetSpeedForceModifier();
		Vector3 vector4 = normalized * d;
		float sqrMagnitude = this.avoidVector.sqrMagnitude;
		bool flag = sqrMagnitude > 0.01f;
		if (flag)
		{
			if (sqrMagnitude > 1f)
			{
				this.avoidVector.Normalize();
			}
			vector4 += this.avoidVector * this.avoidMaxSpeed;
			float num2 = Mathf.Max(this.avoidMaxSpeed, this.gotoMaxSpeed);
			if (vector4.sqrMagnitude > num2 * num2)
			{
				vector4 = this.wantedVel.normalized * num2;
			}
		}
		float num3 = 0f;
		if (this.gotoMaxSpeed > 0.01f)
		{
			num3 = this.currentVelFiltered.magnitude / this.gotoMaxSpeed;
		}
		float num4 = 0.6f;
		this.highSpeedFraction = num3 * Mathf.Clamp((this.gotoMaxSpeed - 7f) * 0.05f, 0f, 1f);
		if (this.highSpeedFraction > 0.01f)
		{
			num4 = Mathf.Min(1f, num4 + this.highSpeedFraction);
		}
		this.DoOnGroundCheck();
		if (this.onGround || this.wantedVel.sqrMagnitude < Mathf.Epsilon)
		{
			if (this.groundFriction < 0.4f)
			{
				float t = Mathf.Pow(this.groundFriction / 0.4f, 3f);
				vector4 = Vector3.Lerp(this.wantedVel, vector4, t);
			}
			this.wantedVel = vector4;
		}
		else
		{
			this.wantedVel = Vector3.Lerp(this.wantedVel, vector4, 0.06f);
			this.wantedVel.y = vector4.y;
		}
		if (!this.onGround)
		{
			this.prevHeightError = 0f;
		}
		Vector3 vector5 = this.wantedVel - this.currentVelFiltered;
		vector5.y = 0f;
		float magnitude2 = vector5.magnitude;
		Vector3 normalized2 = vector5.normalized;
		float num5 = magnitude2 - this.prevVelError;
		this.prevVelError = magnitude2;
		Vector3 up = this.legsTransform.up;
		float num6 = Vector3.Angle(Vector3.up, up);
		float num7 = 1f;
		if (!this.onGround)
		{
			num7 = 0.6f;
			if (this.character != null && this.character.stateHandler.IsSwimming())
			{
				num7 *= 0.2f;
			}
		}
		float num8 = 1f + this.highSpeedFraction;
		this.translating = (flag || this.gotoOffset.sqrMagnitude > 0.0001f || (magnitude2 > 1f && this.onGround));
		if (this.rigidBodyBelow != null)
		{
			this.rigidBodyBelowVelocity.y = 0f;
			float sqrMagnitude2 = this.rigidBodyBelowVelocity.sqrMagnitude;
			if (sqrMagnitude2 > 0.01f)
			{
				Vector3 a = new Vector3(this.rigidBodyBelowVelocity.x, 0f, this.rigidBodyBelowVelocity.z);
				float d2 = Mathf.Clamp01((this.rigidBodyBelow.mass - 1f) / 5f);
				if (this.onGroundBlock != null)
				{
					BlockAbstractPlatform blockAbstractPlatform = this.onGroundBlock as BlockAbstractPlatform;
					if (blockAbstractPlatform != null)
					{
						d2 = 1f;
					}
				}
				vector2 += d2 * a;
				this.onMovingObject = true;
			}
			else
			{
				this.onMovingObject = false;
			}
		}
		if (this.onGround)
		{
			Vector3 lhs = Vector3.Cross(Vector3.up, this.wantedVel);
			Vector3 vector6 = Vector3.Cross(lhs, this.groundNormal);
			Debug.DrawRay(this.legsRb.worldCenterOfMass, this.wantedVel, Color.red);
			Debug.DrawRay(this.legsRb.worldCenterOfMass, vector6, Color.blue);
			float num9 = Mathf.Clamp(Vector3.Dot(this.groundNormal, Vector3.up), 0f, 1f);
			float d3 = (num9 >= 0.6f) ? Mathf.Min(num9 * num9 * 1.2f, 1f) : 0f;
			this.wantedVel = vector6 * d3;
		}
		vector2 += this.wantedVel;
		if (this.onGround && this.groundFriction < 0.4f)
		{
			float num10 = 1f - Mathf.Clamp(Vector3.Dot(this.groundNormal, Vector3.up), 0f, 1f);
			if (num10 < Mathf.Epsilon)
			{
				this.slideVelocity.y = 0f;
				float d4 = 1f - 0.25f * this.groundFriction / 0.4f;
				this.slideVelocity *= d4;
			}
			else
			{
				Vector3 vector7 = Vector3.Cross(Vector3.Cross(Vector3.up, this.groundNormal), this.groundNormal);
				this.slideVelocity += 10f * num10 * vector7.normalized * (0.4f - this.groundFriction) * Physics.gravity.magnitude * Time.fixedDeltaTime;
			}
			if (this.slideVelocity.magnitude >= 10f)
			{
				this.slideVelocity = this.slideVelocity.normalized * 10f;
			}
			vector2 += this.slideVelocity;
		}
		else
		{
			this.slideVelocity = Vector3.zero;
		}
		if (this.onGround || this.translating)
		{
			Vector3 a2 = Vector3.Cross(up, Vector3.up);
			float num11 = Mathf.Clamp(num6 - this.prevAngleError, -5f, 5f);
			this.prevAngleError = 0f;
			float num12 = 700f * num8;
			float num13 = num8 * (0.1f + 0.9f * num7) * (num6 * 2f + num11 * 8f + 40f);
			num13 = Mathf.Clamp(num13, -num12, num12);
			Vector3 b = (this.character != null) ? (a2 * num13 * 10f) : (a2 * num13);
			this.totalCmTorque += b;
			Vector3 angularVelocity = this.legsRb.angularVelocity;
			float num14 = (this.character == null) ? 10f : 100f;
			float value = angularVelocity.magnitude * num14;
			float num15 = (this.character == null) ? Mathf.Clamp(value, -num14, num14) : 10f;
			this.totalCmTorque += -num7 * num15 * angularVelocity;
			float num16 = 3f;
			bool flag2;
			if (this.character.stateHandler.walkStrafe)
			{
				float num17 = Vector3.Angle(this.currentFaceDirection, this.legsTransform.forward);
				flag2 = (num17 > 0.1f);
			}
			else
			{
				flag2 = (((double)magnitude2 > 0.02 && magnitude > num16) || this.requestingTurn || this.requestingTurnToTag || this.requestingTurnToTap || this.requestingDPadControl || this.requestingTurnAlongCamera || this.IsFPC());
			}
			if (flag2)
			{
				Vector3 forward2 = this.legsTransform.forward;
				if (Mathf.Abs(Vector3.Dot(forward2, Vector3.up)) < 0.25f)
				{
					forward2.y = 0f;
					float num18 = Util.AngleBetween(forward2, this.currentFaceDirection, Vector3.up);
					float num19 = Mathf.Clamp(num18 - this.prevForwardAngleError, -50f, 50f) / Blocksworld.fixedDeltaTime;
					this.prevForwardAngleError = 0f;
					if (Mathf.Abs(num18) < 5f)
					{
						this.iForwardAngleSum = Mathf.Clamp(this.iForwardAngleSum + Blocksworld.fixedDeltaTime * num18, -1000f, 1000f);
					}
					else
					{
						this.iForwardAngleSum = 0f;
					}
					a2 = Vector3.Cross(forward2, this.currentFaceDirection);
					float num20 = Mathf.Clamp(num18 * 2f + num19 * 0.1f + this.iForwardAngleSum * 5f, -num12, num12);
					if (this.IsFPC())
					{
						num20 *= Blocksworld.blocksworldCamera.firstPersonTorque;
					}
					num20 = ((!this.legs.ignoreRotation) ? num20 : 0f);
					this.totalCmTorque += num7 * up * num20 * (1f + this.highSpeedFraction);
				}
			}
		}
		else
		{
			this.prevAngleError = 0f;
		}
		this.character.stateHandler.requestedMoveVelocity = vector2;
		Vector3 force = vector2 - this.legsRb.velocity;
		float num21 = 0f;
		if (this.character.stateHandler.IsWalking())
		{
			num21 = this.GetStepForce(vector2, this.legsRb.velocity);
		}
		if (this.jumpPressed)
		{
			force.y = 0f;
		}
		else if (!this.onGround)
		{
			if (num21 < Mathf.Epsilon && this.legsRb.velocity.y > 2f && this.onGroundHeight > 0.1f)
			{
				force.y = -0.25f * (this.legsRb.velocity.y - 2f);
			}
			else
			{
				force.y = 0f;
			}
		}
		if (num21 > 0f && num21 > force.y)
		{
			force.y = num21;
		}
		this.legsRb.AddForce(force, ForceMode.VelocityChange);
		this.legsRb.AddTorque(this.totalCmTorque * this.torqueMultiplier * this.addedTorqueMult);
		this.totalForce = force;
	}

	// Token: 0x060025CA RID: 9674 RVA: 0x00115D54 File Offset: 0x00114154
	public Vector3 GetDownSlopeDirection()
	{
		if (!this.onGround)
		{
			return Vector3.zero;
		}
		float num = 1f - Mathf.Clamp(Vector3.Dot(this.groundNormal, Vector3.up), 0f, 1f);
		if (num < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		Vector3 normalized = Vector3.Cross(Vector3.up, this.groundNormal).normalized;
		return Vector3.Cross(normalized, this.groundNormal).normalized;
	}

	// Token: 0x060025CB RID: 9675 RVA: 0x00115DDC File Offset: 0x001141DC
	private float GetStepForce(Vector3 desiredVelocity, Vector3 currentVelocity)
	{
		if (!this.nearGround)
		{
			return 0f;
		}
		float maxDistance = Mathf.Clamp(desiredVelocity.magnitude * 0.2f, 0.8f, 3f);
		Vector3 stepFwd = Vector3.Cross(this.legsTransform.right.normalized, Vector3.up);
		Vector3 vector = this.onGroundPoint + Vector3.up * 0.1f;
		Vector3 fromPosition = vector + Vector3.up * 0.95f / 2f;
		Vector3 fromPosition2 = vector + Vector3.up * 0.95f;
		RaycastHit raycastHit;
		bool flag = this.DoGroundRaycast(vector, stepFwd, maxDistance, out raycastHit);
		RaycastHit raycastHit2;
		bool flag2 = this.DoGroundRaycast(fromPosition, stepFwd, maxDistance, out raycastHit2);
		RaycastHit raycastHit3;
		bool flag3 = this.DoGroundRaycast(fromPosition2, stepFwd, maxDistance, out raycastHit3);
		Func<Vector3, bool> func = (Vector3 normal) => Vector3.Dot(normal, -stepFwd) > 0.35f;
		flag &= func(raycastHit.normal);
		flag2 &= func(raycastHit2.normal);
		flag3 &= func(raycastHit3.normal);
		flag &= !(raycastHit.collider is MeshCollider);
		flag2 &= !(raycastHit2.collider is MeshCollider);
		if (!flag || flag3)
		{
			return 0f;
		}
		float num = (!flag2) ? 1f : 4f;
		Vector3 vector2 = desiredVelocity - Vector3.up * desiredVelocity.y;
		Vector3 a = desiredVelocity - currentVelocity;
		Vector3 vector3 = a - Vector3.up * a.y;
		float num2 = Mathf.Max(0f, (vector2.magnitude - 0.15f) * 8f);
		float num3 = Mathf.Max(0f, (vector3.magnitude - 0.15f) * 12f);
		float num4 = Mathf.Clamp(num * (num2 + num3), 0f, 8f);
		float num5 = Mathf.Clamp01(2f - this.onGroundHeight);
		num4 *= num5 * num5;
		return num4 / 20f;
	}

	// Token: 0x060025CC RID: 9676 RVA: 0x00116030 File Offset: 0x00114430
	private void IdleFixedUpdate()
	{
		this.DoOnGroundCheck();
		if (this.beingPulled || this.wasPulled)
		{
			return;
		}
		float num = Vector3.Angle(this.legsTransform.up, Vector3.up);
		if (num < 10f)
		{
			this.legsRb.freezeRotation = true;
			if (this.character != null && this.rigidBodyBelow == null)
			{
				this.legsRb.AddForce(-this.legsRb.velocity, ForceMode.VelocityChange);
			}
		}
	}

	// Token: 0x060025CD RID: 9677 RVA: 0x001160C0 File Offset: 0x001144C0
	private void StandingAttackFixedUpdate()
	{
		if (this.beingPulled || this.wasPulled)
		{
			return;
		}
		this.legsRb.freezeRotation = true;
		CharacterStateHandler stateHandler = (this.legs as BlockAnimatedCharacter).stateHandler;
		float magnitude = this.legsRb.velocity.magnitude;
		float d = magnitude;
		if (magnitude > stateHandler.standingAttackMaxSpeed)
		{
			d = Mathf.Lerp(magnitude, stateHandler.standingAttackMaxSpeed, 6f * Time.fixedDeltaTime);
		}
		else if (magnitude < stateHandler.standingAttackMinSpeed)
		{
			d = Mathf.Lerp(magnitude, stateHandler.standingAttackMinSpeed, 6f * Time.fixedDeltaTime);
		}
		this.legsRb.AddForce(d * stateHandler.standingAttackForward - this.legsRb.velocity, ForceMode.VelocityChange);
	}

	// Token: 0x060025CE RID: 9678 RVA: 0x0011618C File Offset: 0x0011458C
	public void AddJumpForce(float energy)
	{
		float num = 7f * energy * Blocksworld.fixedDeltaTime;
		Vector3 force = num * this.jumpUp + 0.2f * num * this.sideJumpVector;
		Rigidbody component = this.legs.body.GetComponent<Rigidbody>();
		Vector3 worldCenterOfMass = component.worldCenterOfMass;
		Vector3 position = 0.7f * worldCenterOfMass + 0.3f * this.legs.goT.position;
		component.AddForceAtPosition(force, position);
		this.jumpEnergyLeft -= num;
	}

	// Token: 0x060025CF RID: 9679 RVA: 0x00116228 File Offset: 0x00114628
	public Vector3 GetBounceVector(float bounciness)
	{
		this.jumpEnergyLeft = this.totalJumpEnergy;
		this.jumpCountThisFrame = 0;
		Rigidbody component = this.legs.body.GetComponent<Rigidbody>();
		Vector3 velocity = component.velocity;
		Vector3 vector = this.groundNormal;
		float d = velocity.magnitude * bounciness;
		Vector3 a = Vector3.Reflect(velocity.normalized, this.groundNormal);
		return d * a;
	}

	// Token: 0x060025D0 RID: 9680 RVA: 0x00116290 File Offset: 0x00114690
	public void Bounce(Vector3 bounceVector)
	{
		Rigidbody component = this.legs.body.GetComponent<Rigidbody>();
		component.AddForce(bounceVector - component.velocity, ForceMode.VelocityChange);
	}

	// Token: 0x060025D1 RID: 9681 RVA: 0x001162C4 File Offset: 0x001146C4
	private void JumpFixedUpdate()
	{
		WalkControllerAnimated.JumpMode jumpMode = this.jumpMode;
		if (jumpMode != WalkControllerAnimated.JumpMode.AddingForce)
		{
			if (jumpMode != WalkControllerAnimated.JumpMode.Ready)
			{
				if (jumpMode == WalkControllerAnimated.JumpMode.WaitingForReady)
				{
					this.jumpTimer -= Blocksworld.fixedDeltaTime;
					if (this.jumpTimer <= 0f)
					{
						this.jumpMode = WalkControllerAnimated.JumpMode.Ready;
					}
				}
			}
			else if (this.jumpEnergyLeft > 0f)
			{
				if (this.character != null)
				{
					this.character.stateHandler.StartJump(this.totalJumpEnergy);
					this.jumpMode = WalkControllerAnimated.JumpMode.WaitingForReady;
					this.totalJumpEnergy = 0f;
					this.jumpEnergyLeft = 0f;
				}
				else
				{
					this.AddJumpForce(this.totalJumpEnergy);
					this.jumpMode = WalkControllerAnimated.JumpMode.AddingForce;
				}
				this.jumpCountThisFrame = 0;
				if (this.gotoOffset.sqrMagnitude > 1E-05f)
				{
					this.sideJumpVector = this.gotoOffset.normalized * this.speedFraction;
				}
				else
				{
					this.sideJumpVector = Vector3.zero;
				}
			}
		}
		else
		{
			this.AddJumpForce(this.totalJumpEnergy);
			if (this.jumpEnergyLeft <= 0f || !this.jumpPressed)
			{
				this.jumpEnergyLeft = 0f;
				this.totalJumpEnergy = 0f;
				this.jumpMode = WalkControllerAnimated.JumpMode.WaitingForReady;
			}
		}
		this.jumpPressed = false;
	}

	// Token: 0x060025D2 RID: 9682 RVA: 0x00116424 File Offset: 0x00114824
	private void AvoidTagFixedUpdate()
	{
		foreach (KeyValuePair<string, float> keyValuePair in this.avoidDistances)
		{
			string key = keyValuePair.Key;
			float num = this.avoidDistances[key];
			List<Block> blocksWithTag = TagManager.GetBlocksWithTag(key);
			Vector3 position = this.legs.goT.position;
			for (int i = 0; i < blocksWithTag.Count; i++)
			{
				Block block = blocksWithTag[i];
				if (!this.chunkBlocks.Contains(block))
				{
					Collider component = block.go.GetComponent<Collider>();
					Vector3 b;
					if (component != null)
					{
						b = component.ClosestPointOnBounds(position);
					}
					else
					{
						b = block.goT.position;
					}
					Vector3 vector = position - b;
					vector.y = 0f;
					float magnitude = vector.magnitude;
					if (magnitude > 0.01f && magnitude < num)
					{
						Vector3 normalized = vector.normalized;
						float d = 1f - magnitude / num;
						this.avoidVector += normalized * d;
					}
				}
			}
		}
		this.avoidDistances.Clear();
		this.avoidApplications.Clear();
	}

	// Token: 0x060025D3 RID: 9683 RVA: 0x001165A0 File Offset: 0x001149A0
	public void FixedUpdate()
	{
		if (this.legsRb == null)
		{
			this.legsRb = this.legs.GetRigidBody();
			this.legsTransform = this.legs.goT;
			if (this.legsRb == null)
			{
				return;
			}
		}
		this.character.stateHandler.requestedMoveVelocity = Vector3.zero;
		this.legsRb.freezeRotation = false;
		WalkControllerAnimated.VicinityMode vicinityMode = this.vicinityMode;
		if (vicinityMode != WalkControllerAnimated.VicinityMode.AvoidTag)
		{
			if (vicinityMode == WalkControllerAnimated.VicinityMode.None)
			{
				this.avoidMaxSpeed = this.defaultMaxSpeed;
			}
		}
		else
		{
			this.AvoidTagFixedUpdate();
		}
		bool flag = false;
		if (this.character.stateHandler.IsOnSide() || this.character.stateHandler.IsGetUpState())
		{
			this.SideFixedUpdate();
		}
		else if (this.character.stateHandler.IsImmobile())
		{
			this.IdleFixedUpdate();
		}
		else if (this.character.stateHandler.InStandingAttack())
		{
			this.StandingAttackFixedUpdate();
		}
		else if (this.character.isHovering)
		{
			this.DoOnGroundCheck();
		}
		else if (!this.beingPulled && !this.wasPulled && !this.character.isHovering)
		{
			flag = true;
			this.MoveFixedUpdate();
		}
		if (this.wasPulled)
		{
			RaycastHit raycastHit;
			if (this.DoGroundRaycast(this.legsTransform.position + 0.5f * Vector3.up, -Vector3.up, 3f, out raycastHit))
			{
				this.onGround = true;
				this.wasPulled = false;
			}
			else
			{
				this.onGround = false;
			}
		}
		this.JumpFixedUpdate();
		if (!this.requestingTranslate && !this.requestingDPadControl && !this.requestingTurn && !this.requestingTurnToTag && !this.requestingDPadControl && !this.requestingTurnToTap && !this.requestingTurnAlongCamera)
		{
			this.prevAngleError = 0f;
		}
		this.previousVicinityMode = this.vicinityMode;
		this.vicinityMode = WalkControllerAnimated.VicinityMode.None;
		this.gotoOffset = Vector3.zero;
		this.avoidVector = Vector3.zero;
		this.currentFaceDirection = this.legs.goT.forward;
		this.ResetMovementRequests();
		if (!flag)
		{
			this.wantedVel = Vector3.zero;
			this.prevVelError = 0f;
			this.character.stateHandler.desiredGoto = Vector3.zero;
		}
		this.gotoMaxSpeed = 0f;
		if (this.IsFPC())
		{
			Blocksworld.blocksworldCamera.firstPersonDpad = Vector2.zero;
			Blocksworld.blocksworldCamera.firstPersonRotation = 0f;
		}
	}

	// Token: 0x060025D4 RID: 9684 RVA: 0x00116874 File Offset: 0x00114C74
	private void MoveFixedUpdate()
	{
		bool isActive = Blocksworld.orbitDuringControlGesture.IsActive;
		float num = 0f;
		this.gotoMaxSpeed = this.defaultMaxSpeed;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = this.currentFaceDirection;
		bool flag = false;
		if (this.requestingTranslate || this.requestingDPadControl)
		{
			this.tapActivatedTime = -1f;
		}
		else if ((this.requestingGoToTap || this.requestingTurnToTap) && !this.wasRequestingTapAction)
		{
			this.tapActivatedTime = Time.time;
		}
		bool flag2 = TapControlGesture.HasWorldTapPos() && this.tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= this.tapActivatedTime;
		if (this.requestingTranslate)
		{
			num = this.aggregatedTranslateRequest.magnitude;
			Vector3 normalized = this.aggregatedTranslateRequest.normalized;
			if (this.IsFPC())
			{
				Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
				if (Mathf.Abs(normalized.x) < firstPersonDeadZone.x)
				{
					normalized.x = 0f;
				}
				else
				{
					normalized.x = Mathf.Sign(normalized.x) * (Mathf.Abs(normalized.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				}
				if (Mathf.Abs(normalized.y) < firstPersonDeadZone.y)
				{
					normalized.y = 0f;
				}
				else
				{
					normalized.y = Mathf.Sign(normalized.y) * (Mathf.Abs(normalized.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				}
				BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
				blocksworldCamera.firstPersonDpad.x = blocksworldCamera.firstPersonDpad.x - num * normalized.x;
				BlocksworldCamera blocksworldCamera2 = Blocksworld.blocksworldCamera;
				blocksworldCamera2.firstPersonDpad.y = blocksworldCamera2.firstPersonDpad.y + num * normalized.z;
			}
			Vector3 a = Vector3.Cross(this.currentFaceDirection, Vector3.up);
			Vector3 a2 = this.currentFaceDirection * normalized.z + a * normalized.x;
			vector += this.aggregatedTranslateRequest.magnitude * 5f * a2;
		}
		if (this.requestingGoToTag)
		{
			Vector3 vector3 = this.gotoTagTarget - this.legs.goT.position;
			vector3.y = 0f;
			vector += vector3.normalized * this.gotoTagRequestSpeed;
			num = this.SlowdownAtTarget(this.gotoTagRequestSpeed, vector3.magnitude);
		}
		if (this.requestingGoToTap && flag2)
		{
			Vector3 b = TapControlGesture.GetWorldTapPos() - this.legs.goT.position;
			b.y = 0f;
			if (b.sqrMagnitude > 0.25f)
			{
				vector += b;
				num = this.SlowdownAtTarget(this.gotoTapRequestSpeed, b.magnitude);
			}
		}
		if (this.requestingTurnToTag && !flag)
		{
			Block block = null;
			if (this.TryGetClosestBlockWithTag(this.turnToTagRequestStr, out block, false))
			{
				Vector3 vector4 = block.goT.position - this.legs.goT.position;
				vector4.y = 0f;
				if (vector4.sqrMagnitude > 0.01f)
				{
					vector2 = vector4.normalized;
					flag = true;
				}
			}
		}
		if (this.requestingTurnAlongCamera && !flag)
		{
			Vector3 vector5 = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
			if (vector5.sqrMagnitude > 0.001f)
			{
				vector2 = vector5.normalized;
				flag = true;
			}
		}
		if (this.requestingDPadControl)
		{
			if (this.IsFPC())
			{
				float angle = this.dPadControlRequestSpeed * (this.dPadControlRequestDir.x * 0.45f);
				Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
				vector2 = rotation * this.currentFaceDirection;
				vector2.y = 0f;
				if (vector2.sqrMagnitude > 0.01f)
				{
					vector2.Normalize();
				}
				flag = true;
				Vector3 firstPersonDeadZone2 = Blocksworld.blocksworldCamera.firstPersonDeadZone;
				if (Mathf.Abs(this.dPadControlRequestDir.y) < firstPersonDeadZone2.y)
				{
					this.dPadControlRequestDir.y = 0f;
				}
				else
				{
					this.dPadControlRequestDir.y = Mathf.Sign(this.dPadControlRequestDir.y) * (Mathf.Abs(this.dPadControlRequestDir.y) - firstPersonDeadZone2.y) / (1f - firstPersonDeadZone2.y);
				}
				BlocksworldCamera blocksworldCamera3 = Blocksworld.blocksworldCamera;
				blocksworldCamera3.firstPersonDpad.y = blocksworldCamera3.firstPersonDpad.y + this.dPadControlRequestSpeed * Mathf.Pow(this.dPadControlRequestDir.y, 3f);
			}
			Vector3 cameraUp = Blocksworld.cameraUp;
			Vector3 cameraRight = Blocksworld.cameraRight;
			Vector3 cameraForward = Blocksworld.cameraForward;
			float num2 = this.dPadControlRequestDir.y;
			if (cameraForward.y > 0f)
			{
				num2 *= -1f;
			}
			Vector3 a3 = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized * num2;
			Vector3 b2 = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized * this.dPadControlRequestDir.x;
			Vector3 a4 = a3 + b2;
			if (!this.requestingTranslate)
			{
				vector += 100f * a4;
			}
			num = this.dPadControlRequestSpeed;
			if (!flag)
			{
				if (isActive)
				{
					vector2 = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized;
				}
				else
				{
					vector2 = a4.normalized;
					flag = true;
				}
			}
		}
		if (this.requestingTurn && !flag)
		{
			Quaternion rotation2 = Quaternion.AngleAxis(this.turnRequestSpeed, Vector3.up);
			vector2 = rotation2 * this.currentFaceDirection;
			vector2.y = 0f;
			if (vector2.sqrMagnitude > 0.01f)
			{
				vector2.Normalize();
			}
			flag = true;
			if (vector.sqrMagnitude > Mathf.Epsilon)
			{
				vector = rotation2 * vector;
			}
			this.character.stateHandler.turnPower = this.turnRequestSpeed;
		}
		if ((this.requestingTurnToTap || this.requestingGoToTap) && flag2 && !flag)
		{
			Vector3 vector6 = TapControlGesture.GetWorldTapPos() - this.legs.goT.position;
			vector6.y = 0f;
			if (vector6.sqrMagnitude > 0.01f)
			{
				vector2 = vector6.normalized;
				flag = true;
			}
		}
		if (this.requestingGoToTag && !flag)
		{
			Block block2 = null;
			if (this.TryGetClosestBlockWithTag(this.gotoTagRequestStr, out block2, false))
			{
				Vector3 vector7 = block2.goT.position - this.legs.goT.position;
				vector7.y = 0f;
				if (vector7.sqrMagnitude > 0.01f)
				{
					vector2 = vector7.normalized;
					flag = true;
				}
			}
		}
		if (flag && ((this.requestingTurnToTap && flag2) || this.requestingTurnToTag || this.requestingTurnAlongCamera) && !this.IsFPC())
		{
			Vector3 vector8 = this.legs.goT.InverseTransformDirection(vector2.normalized);
			float num3 = vector8.x;
			float num4 = (!this.requestingTurnAlongCamera) ? 0.45f : 0.05f;
			float num5 = (!this.requestingTurnAlongCamera) ? 1f : 5f;
			num3 *= num5;
			if (Mathf.Abs(num3) < num4)
			{
				num3 = 0f;
			}
			if (vector8.z < 0f && num3 == 0f)
			{
				num3 = -1f;
			}
			this.character.stateHandler.turnPower = num3;
		}
		this.gotoOffset = (this.currentMoveDirection = vector);
		this.currentFaceDirection = vector2;
		float num6 = Vector3.Angle(this.currentMoveDirection, this.currentFaceDirection);
		bool walkStrafe = ((!this.requestingDPadControl && !this.requestingTurnAlongCamera) || !this.IsFPC()) && vector.sqrMagnitude > 0.05f && num6 > 10f;
		this.character.stateHandler.walkStrafe = walkStrafe;
		this.gotoMaxSpeed = Mathf.Max(this.gotoMaxSpeed, num);
		this.GotoFixedUpdate(num);
	}

	// Token: 0x060025D5 RID: 9685 RVA: 0x00117131 File Offset: 0x00115531
	private float SlowdownAtTarget(float speed, float distanceToTarget)
	{
		if (distanceToTarget > 2f)
		{
			return speed;
		}
		return Mathf.Lerp(Mathf.Min(speed, 0.15f), speed, distanceToTarget / 2f);
	}

	// Token: 0x060025D6 RID: 9686 RVA: 0x00117158 File Offset: 0x00115558
	private void ResetMovementRequests()
	{
		this.wasRequestingTapAction = ((this.requestingGoToTap || this.requestingTurnToTap) && this.tapActivatedTime > 0f);
		this.requestingTranslate = false;
		this.requestingDPadControl = false;
		this.requestingTurn = false;
		this.requestingTurnToTag = false;
		this.requestingTurnToTap = false;
		this.requestingGoToTag = false;
		this.requestingGoToTap = false;
		this.requestingTurnAlongCamera = false;
		this.gotoTagBlock = null;
		this.gotoTagTarget = Vector3.zero;
		this.aggregatedTranslateRequest = Vector3.zero;
		this.turnRequestSpeed = 0f;
	}

	// Token: 0x060025D7 RID: 9687 RVA: 0x001171F4 File Offset: 0x001155F4
	private void SideFixedUpdate()
	{
		this.DoOnGroundCheck();
		if (this.beingPulled || this.wasPulled)
		{
			return;
		}
		this.legsRb.freezeRotation = true;
		this.character.goT.rotation = this.character.stateHandler.GetSideAnimRotation();
	}

	// Token: 0x17000175 RID: 373
	// (set) Token: 0x060025D8 RID: 9688 RVA: 0x0011724B File Offset: 0x0011564B
	public float SetAddedTorqueMultiplier
	{
		set
		{
			this.addedTorqueMult = value * this.legs.GetBlockMetaData().massM;
		}
	}

	// Token: 0x060025D9 RID: 9689 RVA: 0x00117268 File Offset: 0x00115668
	public float GetAndResetHighSpeedFraction()
	{
		float result = this.highSpeedFraction;
		this.highSpeedFraction = 1f;
		return result;
	}

	// Token: 0x060025DA RID: 9690 RVA: 0x00117288 File Offset: 0x00115688
	public bool IsOnGround()
	{
		return this.onGround;
	}

	// Token: 0x060025DB RID: 9691 RVA: 0x00117290 File Offset: 0x00115690
	public float OnGroundHeight()
	{
		return this.onGroundHeight;
	}

	// Token: 0x060025DC RID: 9692 RVA: 0x00117298 File Offset: 0x00115698
	public void ClearIgnoreColliders()
	{
		if (this.ignoreColliders != null)
		{
			this.ignoreColliders.Clear();
			this.ignoreColliders = null;
		}
	}

	// Token: 0x04002075 RID: 8309
	private bool requestingTranslate;

	// Token: 0x04002076 RID: 8310
	private Vector3 aggregatedTranslateRequest;

	// Token: 0x04002077 RID: 8311
	private bool requestingDPadControl;

	// Token: 0x04002078 RID: 8312
	private Vector2 dPadControlRequestDir;

	// Token: 0x04002079 RID: 8313
	private float dPadControlRequestSpeed;

	// Token: 0x0400207A RID: 8314
	private bool requestingTurn;

	// Token: 0x0400207B RID: 8315
	private float turnRequestSpeed;

	// Token: 0x0400207C RID: 8316
	private bool requestingTurnToTag;

	// Token: 0x0400207D RID: 8317
	private string turnToTagRequestStr;

	// Token: 0x0400207E RID: 8318
	private bool requestingTurnToTap;

	// Token: 0x0400207F RID: 8319
	private bool requestingGoToTag;

	// Token: 0x04002080 RID: 8320
	private Block gotoTagBlock;

	// Token: 0x04002081 RID: 8321
	private Vector3 gotoTagTarget;

	// Token: 0x04002082 RID: 8322
	private string gotoTagRequestStr;

	// Token: 0x04002083 RID: 8323
	private float gotoTagRequestSpeed;

	// Token: 0x04002084 RID: 8324
	private bool requestingGoToTap;

	// Token: 0x04002085 RID: 8325
	private float gotoTapRequestSpeed;

	// Token: 0x04002086 RID: 8326
	private bool requestingTurnAlongCamera;

	// Token: 0x04002087 RID: 8327
	private bool wasRequestingTapAction;

	// Token: 0x04002088 RID: 8328
	private Chunk chunk;

	// Token: 0x04002089 RID: 8329
	private Rigidbody rigidBodyBelow;

	// Token: 0x0400208A RID: 8330
	private Vector3 rigidBodyBelowVelocity = Vector3.zero;

	// Token: 0x0400208B RID: 8331
	private HandInfo[] handInfos = new HandInfo[]
	{
		new HandInfo(),
		new HandInfo()
	};

	// Token: 0x0400208C RID: 8332
	private HashSet<Collider> ignoreColliders;

	// Token: 0x0400208D RID: 8333
	private WalkControllerAnimated.JumpMode jumpMode;

	// Token: 0x0400208E RID: 8334
	private Vector3 jumpUp = Vector3.up;

	// Token: 0x0400208F RID: 8335
	private bool jumpPressed;

	// Token: 0x04002090 RID: 8336
	private int jumpCountThisFrame;

	// Token: 0x04002091 RID: 8337
	private float jumpTimer;

	// Token: 0x04002092 RID: 8338
	private float jumpEnergyLeft;

	// Token: 0x04002093 RID: 8339
	private float totalJumpEnergy;

	// Token: 0x04002094 RID: 8340
	private Vector3 sideJumpVector = Vector3.up;

	// Token: 0x04002095 RID: 8341
	public float heightTestOffset = 0.5f;

	// Token: 0x04002096 RID: 8342
	public bool climbOn;

	// Token: 0x04002097 RID: 8343
	public Vector3 downRayOffset = Vector3.zero;

	// Token: 0x04002098 RID: 8344
	public bool climbing;

	// Token: 0x04002099 RID: 8345
	public Vector3 currentMoveDirection = Vector3.forward;

	// Token: 0x0400209A RID: 8346
	public Vector3 currentFaceDirection = Vector3.forward;

	// Token: 0x0400209B RID: 8347
	private Vector3 wantedVel = Vector3.zero;

	// Token: 0x0400209C RID: 8348
	public float defaultMaxSpeed = 5f;

	// Token: 0x0400209D RID: 8349
	public float defaultAvoidDistance = 3f;

	// Token: 0x0400209E RID: 8350
	private Dictionary<string, float> avoidDistances = new Dictionary<string, float>();

	// Token: 0x0400209F RID: 8351
	private Dictionary<string, float> avoidApplications = new Dictionary<string, float>();

	// Token: 0x040020A0 RID: 8352
	public float avoidMaxSpeed = 5f;

	// Token: 0x040020A1 RID: 8353
	public float defaultDPadMaxSpeed = 5f;

	// Token: 0x040020A2 RID: 8354
	public float torqueMultiplier = 1f;

	// Token: 0x040020A3 RID: 8355
	public float addedTorqueMult = 1f;

	// Token: 0x040020A4 RID: 8356
	private float highSpeedFraction;

	// Token: 0x040020A5 RID: 8357
	private float tapActivatedTime = -1f;

	// Token: 0x040020A6 RID: 8358
	private bool onGround;

	// Token: 0x040020A7 RID: 8359
	private bool nearGround;

	// Token: 0x040020A8 RID: 8360
	public Vector3 groundNormal = Vector3.zero;

	// Token: 0x040020A9 RID: 8361
	private float onGroundHeight;

	// Token: 0x040020AA RID: 8362
	private Vector3 onGroundPoint;

	// Token: 0x040020AB RID: 8363
	public float groundFriction;

	// Token: 0x040020AC RID: 8364
	private const float defaultGroundFriction = 0.4f;

	// Token: 0x040020AD RID: 8365
	public float groundBounciness;

	// Token: 0x040020AE RID: 8366
	public Vector3 slideVelocity;

	// Token: 0x040020AF RID: 8367
	public bool onMovingObject;

	// Token: 0x040020B0 RID: 8368
	public Block onGroundBlock;

	// Token: 0x040020B1 RID: 8369
	private string dPadMoveKey = "L";

	// Token: 0x040020B2 RID: 8370
	public WalkControllerAnimated.VicinityMode vicinityMode;

	// Token: 0x040020B3 RID: 8371
	public WalkControllerAnimated.VicinityMode previousVicinityMode;

	// Token: 0x040020B4 RID: 8372
	private Vector3 gotoOffset = Vector3.zero;

	// Token: 0x040020B5 RID: 8373
	private Vector3 avoidVector = Vector3.zero;

	// Token: 0x040020B6 RID: 8374
	private float gotoMaxSpeed = 8f;

	// Token: 0x040020B7 RID: 8375
	public BlockWalkable legs;

	// Token: 0x040020B8 RID: 8376
	public BlockAnimatedCharacter character;

	// Token: 0x040020B9 RID: 8377
	public List<WalkControllerAnimated> chunkControllers = new List<WalkControllerAnimated>();

	// Token: 0x040020BA RID: 8378
	private List<WalkControllerAnimated> sameUpControllers = new List<WalkControllerAnimated>();

	// Token: 0x040020BB RID: 8379
	private List<WalkControllerAnimated> conflictingUpControllers = new List<WalkControllerAnimated>();

	// Token: 0x040020BC RID: 8380
	private HashSet<Block> chunkBlocks = new HashSet<Block>();

	// Token: 0x040020BD RID: 8381
	private bool translating;

	// Token: 0x040020BE RID: 8382
	private bool beingPulled;

	// Token: 0x040020BF RID: 8383
	private bool wasPulled;

	// Token: 0x040020C0 RID: 8384
	private float prevAngleError;

	// Token: 0x040020C1 RID: 8385
	private Vector3 iUpAngleError;

	// Token: 0x040020C2 RID: 8386
	private float prevVelError;

	// Token: 0x040020C3 RID: 8387
	private float prevForwardAngleError;

	// Token: 0x040020C4 RID: 8388
	private float iForwardAngleSum;

	// Token: 0x040020C5 RID: 8389
	private float prevHeightError;

	// Token: 0x040020C6 RID: 8390
	private Vector3 currentVelFiltered = Vector3.zero;

	// Token: 0x040020C7 RID: 8391
	private Vector3 targetVelocity = Vector3.zero;

	// Token: 0x040020C8 RID: 8392
	public Vector3 totalForce = Vector3.zero;

	// Token: 0x040020C9 RID: 8393
	private Vector3 totalCmTorque = Vector3.zero;

	// Token: 0x040020CA RID: 8394
	public float speedFraction = 1f;

	// Token: 0x040020CB RID: 8395
	private CapsuleCollider capsule;

	// Token: 0x040020CC RID: 8396
	private const float SPEED_FRACTION_THRESHOLD = 0.001f;

	// Token: 0x040020CD RID: 8397
	private const float VEL_FILTER_ALPHA = 0.9f;

	// Token: 0x040020CE RID: 8398
	private const float TRANSLATE_CONTROL_K = 10f;

	// Token: 0x040020CF RID: 8399
	private const float TRANSLATE_CONTROL_D = 10f;

	// Token: 0x040020D0 RID: 8400
	private const float ANGLE_CONTROL_K = 2f;

	// Token: 0x040020D1 RID: 8401
	private const float ANGLE_CONTROL_D = 8f;

	// Token: 0x040020D2 RID: 8402
	private const float FORWARD_ANGLE_CONTROL_K = 2f;

	// Token: 0x040020D3 RID: 8403
	private const float FORWARD_ANGLE_CONTROL_D = 0.1f;

	// Token: 0x040020D4 RID: 8404
	private const float FORWARD_ANGLE_CONTROL_I = 5f;

	// Token: 0x040020D5 RID: 8405
	public Rigidbody legsRb;

	// Token: 0x040020D6 RID: 8406
	private Transform legsTransform;

	// Token: 0x040020D7 RID: 8407
	private const float BODY_HEIGHT_ADJUST = 4f;

	// Token: 0x040020D8 RID: 8408
	private const float standingAttackSpeedChange = 6f;

	// Token: 0x040020D9 RID: 8409
	private int[] handCounters = new int[2];

	// Token: 0x040020DA RID: 8410
	private Vector3[] handTargets = new Vector3[2];

	// Token: 0x0200034A RID: 842
	public enum VicinityMode
	{
		// Token: 0x040020DC RID: 8412
		None,
		// Token: 0x040020DD RID: 8413
		AvoidTag
	}

	// Token: 0x0200034B RID: 843
	private enum JumpMode
	{
		// Token: 0x040020DF RID: 8415
		Ready,
		// Token: 0x040020E0 RID: 8416
		AddingForce,
		// Token: 0x040020E1 RID: 8417
		WaitingForReady
	}
}
