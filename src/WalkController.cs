using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x02000343 RID: 835
public class WalkController
{
	// Token: 0x0600257B RID: 9595 RVA: 0x00110FFC File Offset: 0x0010F3FC
	public WalkController(BlockAbstractLegs legs)
	{
		this.legs = legs;
		this.currentDirection = legs.goT.forward;
	}

	// Token: 0x0600257C RID: 9596 RVA: 0x001111D8 File Offset: 0x0010F5D8
	public Vector3 GetRigidBodyBelowVelocity()
	{
		return (!(this.rigidBodyBelow == null)) ? this.rigidBodyBelowVelocity : Vector3.zero;
	}

	// Token: 0x0600257D RID: 9597 RVA: 0x001111FC File Offset: 0x0010F5FC
	public float GetWantedSpeedSqr()
	{
		float result = this.wantedVel.sqrMagnitude;
		WalkController.WalkControllerMode walkControllerMode = this.mode;
		if (walkControllerMode == WalkController.WalkControllerMode.Idle)
		{
			result = 0f;
		}
		return result;
	}

	// Token: 0x0600257E RID: 9598 RVA: 0x00111234 File Offset: 0x0010F634
	private float IncApplications(WalkController.WalkControllerMode mode, float analog)
	{
		float num;
		if (this.applicationCounts.TryGetValue(mode, out num))
		{
			num += analog;
		}
		else
		{
			num = analog;
		}
		this.applicationCounts[mode] = num;
		return num;
	}

	// Token: 0x0600257F RID: 9599 RVA: 0x00111270 File Offset: 0x0010F670
	private float GetApplications(WalkController.WalkControllerMode mode)
	{
		float result;
		if (this.applicationCounts.TryGetValue(mode, out result))
		{
			return result;
		}
		return 0f;
	}

	// Token: 0x06002580 RID: 9600 RVA: 0x00111297 File Offset: 0x0010F697
	public void SetDefaultWackiness(float w)
	{
		this.defaultWackiness = w;
	}

	// Token: 0x06002581 RID: 9601 RVA: 0x001112A0 File Offset: 0x0010F6A0
	private bool InFirstPerson()
	{
		return this.legs == Blocksworld.blocksworldCamera.firstPersonCharacter;
	}

	// Token: 0x06002582 RID: 9602 RVA: 0x001112B4 File Offset: 0x0010F6B4
	private void AddJumpForce(float force, bool swim = false)
	{
		this.totalJumpEnergy += force / (float)(1 + this.jumpCountThisFrame * ((!swim) ? 1 : 4));
		this.jumpEnergyLeft = this.totalJumpEnergy;
		this.jumpTimer = 0.5f;
		this.jumpCountThisFrame++;
		this.legs.jumpCountdown = this.legs.startJumpCountdown;
		for (int i = 0; i < this.chunkControllers.Count; i++)
		{
			WalkController walkController = this.chunkControllers[i];
			walkController.legs.jumpCountdown = walkController.legs.startJumpCountdown;
		}
	}

	// Token: 0x06002583 RID: 9603 RVA: 0x00111364 File Offset: 0x0010F764
	public void Jump(float force)
	{
		WalkController.JumpMode jumpMode = this.jumpMode;
		if (jumpMode == WalkController.JumpMode.Ready)
		{
			if (!this.climbing)
			{
				float num = Mathf.Clamp(this.legs.modelMass, 1f, 5f);
				if (this.onGround && this.legs.upright)
				{
					this.AddJumpForce(force * 100f * num, false);
					this.jumpUp = Vector3.up;
				}
				else if (BlockWater.BlockWithinWater(this.legs, false))
				{
					this.AddJumpForce(force * 100f * num, true);
					this.jumpUp = this.legs.goT.up;
				}
			}
		}
		this.jumpPressed = true;
	}

	// Token: 0x06002584 RID: 9604 RVA: 0x00111427 File Offset: 0x0010F827
	public float GetWackiness()
	{
		return (this.IsActive() && this.onGround && this.jumpMode == WalkController.JumpMode.Ready) ? this.wackiness : 1f;
	}

	// Token: 0x06002585 RID: 9605 RVA: 0x0011145C File Offset: 0x0010F85C
	public void SetChunk()
	{
		this.chunk = this.legs.chunk;
		this.chunkBlocks = new HashSet<Block>();
		foreach (Block block in this.chunk.blocks)
		{
			this.chunkBlocks.Add(block);
			if (block is BlockAbstractLegs && block != this.legs)
			{
				BlockAbstractLegs blockAbstractLegs = (BlockAbstractLegs)block;
				if (blockAbstractLegs.walkController != null)
				{
					this.chunkControllers.Add(blockAbstractLegs.walkController);
				}
			}
		}
		foreach (WalkController walkController in this.chunkControllers)
		{
			float num = Vector3.Dot(walkController.legs.goT.up, this.legs.goT.up);
			if (num > 0.99f)
			{
				this.sameUpControllers.Add(walkController);
			}
			else if (num < -0.99f)
			{
				this.conflictingUpControllers.Add(walkController);
			}
		}
	}

	// Token: 0x06002586 RID: 9606 RVA: 0x001115BC File Offset: 0x0010F9BC
	public bool IsActive()
	{
		return this.mode != WalkController.WalkControllerMode.Idle || this.turnMode != WalkController.TurnMode.None || this.vicinityMode != WalkController.VicinityMode.None;
	}

	// Token: 0x06002587 RID: 9607 RVA: 0x001115E4 File Offset: 0x0010F9E4
	public Vector3 GetTargetVelocity()
	{
		WalkController.WalkControllerMode walkControllerMode = this.mode;
		if (walkControllerMode != WalkController.WalkControllerMode.Idle)
		{
			return this.targetVelocity;
		}
		return Vector3.zero;
	}

	// Token: 0x06002588 RID: 9608 RVA: 0x00111610 File Offset: 0x0010FA10
	public Vector3 GetTotalForce()
	{
		WalkController.WalkControllerMode walkControllerMode = this.mode;
		if (walkControllerMode != WalkController.WalkControllerMode.Idle)
		{
			return this.totalForce;
		}
		return Vector3.zero;
	}

	// Token: 0x06002589 RID: 9609 RVA: 0x0011163C File Offset: 0x0010FA3C
	public Vector3 GetTotalCmTorque()
	{
		WalkController.WalkControllerMode walkControllerMode = this.mode;
		if (walkControllerMode != WalkController.WalkControllerMode.Idle)
		{
			return this.totalCmTorque;
		}
		return Vector3.zero;
	}

	// Token: 0x0600258A RID: 9610 RVA: 0x00111668 File Offset: 0x0010FA68
	public void Translate(Vector3 dir, float maxSpeed)
	{
		this.strafe = (Mathf.Abs(dir.x) > 0.5f && Mathf.Abs(dir.z) < 0.1f);
		if (this.InFirstPerson())
		{
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(dir.x) < firstPersonDeadZone.x)
			{
				dir.x = 0f;
			}
			else
			{
				bool flag = dir.x < 0f;
				dir.x = (Mathf.Abs(dir.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					dir.x = -dir.x;
				}
			}
			if (Mathf.Abs(dir.y) < firstPersonDeadZone.y)
			{
				dir.y = 0f;
			}
			else
			{
				bool flag2 = dir.y < 0f;
				dir.y = (Mathf.Abs(dir.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					dir.y = -dir.y;
				}
			}
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.x = blocksworldCamera.firstPersonDpad.x + maxSpeed * dir.x;
			BlocksworldCamera blocksworldCamera2 = Blocksworld.blocksworldCamera;
			blocksworldCamera2.firstPersonDpad.y = blocksworldCamera2.firstPersonDpad.y + maxSpeed * dir.z;
		}
		this.mode = WalkController.WalkControllerMode.Translate;
		this.gotoMaxSpeed = Mathf.Max(maxSpeed, this.gotoMaxSpeed);
		Vector3 a = Vector3.Cross(this.currentDirection, Vector3.up);
		Vector3 a2 = this.currentDirection * dir.z + a * dir.x;
		this.gotoOffset += maxSpeed * 5f * a2;
		this.gotoOffset.y = 0f;
		this.slowDownAtTarget = true;
	}

	// Token: 0x0600258B RID: 9611 RVA: 0x00111870 File Offset: 0x0010FC70
	public void AvoidTag(string tagName, float avoidDistance, float maxSpeed, float analog)
	{
		this.vicinityMode = WalkController.VicinityMode.AvoidTag;
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
		this.avoidMaxSpeed = Mathf.Max(analog * maxSpeed + this.speedIncPerApplication * Mathf.Max(0f, num2 - 1f), this.avoidMaxSpeed);
	}

	// Token: 0x0600258C RID: 9612 RVA: 0x0011190C File Offset: 0x0010FD0C
	public void Turn(float speed)
	{
		this.turnMode = WalkController.TurnMode.Turn;
		if (this.InFirstPerson())
		{
			bool flag = speed < 0f;
			float num = Mathf.Abs(speed) / 4f;
			if (num < 0.3f)
			{
				num = 0f;
			}
			else
			{
				if (num > 1f)
				{
					num = 1f;
				}
				num = (num - 0.3f) / 0.7f;
				num = Mathf.Pow(num, 9f);
				if (flag)
				{
					num *= -1f;
				}
			}
			num *= 0.001f;
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(num) < firstPersonDeadZone.x)
			{
				num = 0f;
			}
			else
			{
				bool flag2 = num < 0f;
				num = (Mathf.Abs(num) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag2)
				{
					num = -num;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += num;
		}
		this.currentDirection = Quaternion.AngleAxis(speed, Vector3.up) * this.currentDirection;
		this.currentDirection.y = 0f;
		if (this.currentDirection.sqrMagnitude > 0.01f)
		{
			this.currentDirection.Normalize();
		}
	}

	// Token: 0x0600258D RID: 9613 RVA: 0x00111A50 File Offset: 0x0010FE50
	public void TurnTowardsTag(string tagName)
	{
		this.turnMode = WalkController.TurnMode.TurnTowardsTag;
		Vector3 a = default(Vector3);
		if (this.TryGetClosestBlockWithTag(tagName, out a, false))
		{
			Vector3 vector = a - this.legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude > 0.01f)
			{
				this.currentDirection = vector.normalized;
			}
		}
	}

	// Token: 0x0600258E RID: 9614 RVA: 0x00111ABC File Offset: 0x0010FEBC
	public void DPadControl(string key, float maxSpeed, float wackiness)
	{
		if (this.InFirstPerson())
		{
			Vector2 vector = (!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(vector.x) < firstPersonDeadZone.x)
			{
				vector.x = 0f;
			}
			else
			{
				bool flag = vector.x < 0f;
				vector.x = (Mathf.Abs(vector.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					vector.x = -vector.x;
				}
			}
			if (Mathf.Abs(vector.y) < firstPersonDeadZone.y)
			{
				vector.y = 0f;
			}
			else
			{
				bool flag2 = vector.y < 0f;
				vector.y = (Mathf.Abs(vector.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					vector.y = -vector.y;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += Mathf.Pow(vector.x, 3f) / 2f;
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.y = blocksworldCamera.firstPersonDpad.y + maxSpeed * Mathf.Pow(vector.y, 3f);
		}
		this.mode = WalkController.WalkControllerMode.DPad;
		this.dPadMoveKey = key;
		float num = this.IncApplications(this.mode, 1f);
		this.gotoMaxSpeed = Mathf.Max(maxSpeed + this.speedIncPerApplication * Mathf.Max(0f, num - 1f), this.gotoMaxSpeed);
		this.slowDownAtTarget = true;
		this.wackiness = wackiness;
	}

	// Token: 0x0600258F RID: 9615 RVA: 0x00111CA8 File Offset: 0x001100A8
	public void TiltMover(Vector2 tiltVector)
	{
		float magnitude = tiltVector.magnitude;
		tiltVector = tiltVector.normalized;
		if (this.InFirstPerson())
		{
			Vector3 firstPersonDeadZone = Blocksworld.blocksworldCamera.firstPersonDeadZone;
			if (Mathf.Abs(tiltVector.x) < firstPersonDeadZone.x)
			{
				tiltVector.x = 0f;
			}
			else
			{
				bool flag = tiltVector.x < 0f;
				tiltVector.x = (Mathf.Abs(tiltVector.x) - firstPersonDeadZone.x) / (1f - firstPersonDeadZone.x);
				if (flag)
				{
					tiltVector.x = -tiltVector.x;
				}
			}
			if (Mathf.Abs(tiltVector.y) < firstPersonDeadZone.y)
			{
				tiltVector.y = 0f;
			}
			else
			{
				bool flag2 = tiltVector.y < 0f;
				tiltVector.y = (Mathf.Abs(tiltVector.y) - firstPersonDeadZone.y) / (1f - firstPersonDeadZone.y);
				if (flag2)
				{
					tiltVector.y = -tiltVector.y;
				}
			}
			Blocksworld.blocksworldCamera.firstPersonRotation += Mathf.Pow(tiltVector.x, 3f) / 2f;
			BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
			blocksworldCamera.firstPersonDpad.y = blocksworldCamera.firstPersonDpad.y + magnitude * Mathf.Pow(tiltVector.y, 3f);
		}
		this.mode = WalkController.WalkControllerMode.DPad;
		this.dPadMoveKey = "L";
		float num = this.IncApplications(this.mode, 1f);
		this.gotoMaxSpeed = Mathf.Max(magnitude + this.speedIncPerApplication * Mathf.Max(0f, num - 1f), this.gotoMaxSpeed);
		this.slowDownAtTarget = true;
		this.wackiness = this.wackiness;
	}

	// Token: 0x06002590 RID: 9616 RVA: 0x00111E80 File Offset: 0x00110280
	public bool TryGetClosestBlockWithTag(string tagName, out Vector3 target, bool allowChunk = false)
	{
		List<Block> blocksWithTag = TagManager.GetBlocksWithTag(tagName);
		target = default(Vector3);
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
						target = position2;
						result = true;
					}
				}
			}
			return result;
		}
		return false;
	}

	// Token: 0x06002591 RID: 9617 RVA: 0x00111F3C File Offset: 0x0011033C
	public void GotoTag(string tagName, float maxSpeed, float wackiness, float analog, bool slowDown = true)
	{
		this.mode = WalkController.WalkControllerMode.GotoTag;
		float num = this.IncApplications(this.mode, analog);
		this.gotoMaxSpeed = Mathf.Max(analog * maxSpeed + this.speedIncPerApplication * Mathf.Max(0f, num - 1f), this.gotoMaxSpeed);
		this.wackiness = wackiness;
		this.slowDownAtTarget = slowDown;
		Vector3 a = default(Vector3);
		if (this.TryGetClosestBlockWithTag(tagName, out a, false))
		{
			this.gotoOffset += a - this.legs.goT.position;
			this.gotoOffset.y = 0f;
			if (this.gotoOffset.sqrMagnitude > 1E-05f)
			{
				this.currentDirection = this.gotoOffset.normalized;
			}
		}
	}

	// Token: 0x06002592 RID: 9618 RVA: 0x00112010 File Offset: 0x00110410
	public void GotoTap(float maxSpeed, float wackiness, bool slowDown = true)
	{
		this.mode = WalkController.WalkControllerMode.GotoTap;
		float num = this.IncApplications(this.mode, 1f);
		this.gotoMaxSpeed = Mathf.Max(maxSpeed + this.speedIncPerApplication * Mathf.Max(0f, num - 1f), this.gotoMaxSpeed);
		this.wackiness = wackiness;
		this.slowDownAtTarget = slowDown;
	}

	// Token: 0x06002593 RID: 9619 RVA: 0x0011206F File Offset: 0x0011046F
	public void TurnTowardsTap()
	{
		this.turnMode = WalkController.TurnMode.TurnTowardsTap;
	}

	// Token: 0x06002594 RID: 9620 RVA: 0x00112078 File Offset: 0x00110478
	public void TurnAlongCamera()
	{
		this.turnMode = WalkController.TurnMode.TurnAlongCamera;
		Vector3 vector = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
		if (vector.sqrMagnitude > 0.001f)
		{
			this.currentDirection = vector.normalized;
		}
	}

	// Token: 0x06002595 RID: 9621 RVA: 0x001120BA File Offset: 0x001104BA
	public void AddIgnoreCollider(Collider c)
	{
		if (this.ignoreColliders == null)
		{
			this.GatherIgnoreColliders();
		}
		this.ignoreColliders.Add(c);
	}

	// Token: 0x06002596 RID: 9622 RVA: 0x001120DC File Offset: 0x001104DC
	private void GatherIgnoreColliders()
	{
		this.ignoreColliders = new HashSet<Collider>();
		for (int i = 0; i < 2 * this.legs.legPairCount; i++)
		{
			this.ignoreColliders.Add(this.legs.feet[i].collider);
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
			foreach (WalkController walkController in this.chunkControllers)
			{
				BlockAbstractLegs blockAbstractLegs = walkController.legs;
				for (int k = 0; k < 2 * blockAbstractLegs.legPairCount; k++)
				{
					this.ignoreColliders.Add(walkController.legs.feet[k].collider);
				}
			}
		}
	}

	// Token: 0x06002597 RID: 9623 RVA: 0x00112228 File Offset: 0x00110628
	private Vector3 GetHoverForceAndUpdateOnGround(float mass)
	{
		this.hoverResult.Set(0f, 0f, 0f);
		float maxDistance = this.legs.onGroundHeight + 1f;
		if (this.ignoreColliders == null)
		{
			this.GatherIgnoreColliders();
		}
		Vector3 up = this.legsTransform.up;
		Vector3 vector = this.legsTransform.position + up * this.heightTestOffset;
		if (this.climbOn && this.wantedVel.sqrMagnitude > 0.001f)
		{
			this.downRayOffset = this.wantedVel.normalized * 0.65f;
			vector += this.downRayOffset;
		}
		else
		{
			this.downRayOffset = Vector3.zero;
		}
		RaycastHit[] array = Physics.RaycastAll(vector, -up, maxDistance, BlockAbstractLegs.raycastMask);
		RaycastHit hit = default(RaycastHit);
		float num = float.MaxValue;
		bool flag = false;
		if (array.Length > 0)
		{
			foreach (RaycastHit raycastHit in array)
			{
				Collider collider = raycastHit.collider;
				if (!(collider == null) && !collider.isTrigger && !this.ignoreColliders.Contains(collider))
				{
					float sqrMagnitude = (raycastHit.point - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						hit = raycastHit;
						flag = true;
					}
				}
			}
		}
		this.climbing = false;
		this.rigidBodyBelow = null;
		this.rigidBodyBelowVelocity = Vector3.zero;
		this.onGroundBlock = null;
		if (!flag)
		{
			this.prevHeightError = 0f;
			this.onGround = false;
			return this.hoverResult;
		}
		GameObject gameObject = hit.collider.gameObject;
		Transform parent = gameObject.transform.parent;
		if (parent != null)
		{
			this.AddRbBelowVelocity(hit, parent.gameObject.GetComponent<Rigidbody>(), false);
		}
		else
		{
			this.AddRbBelowVelocity(hit, gameObject.GetComponent<Rigidbody>(), true);
		}
		float magnitude = (vector - hit.point).magnitude;
		float num2 = Mathf.Clamp(Vector3.Dot(hit.normal, Vector3.up), 0f, 1f);
		this.onGround = (num2 > 0.2f && magnitude < this.legs.onGroundHeight + 0.25f + this.heightTestOffset);
		this.onGroundNormal = hit.normal;
		bool flag2 = magnitude < this.legs.onGroundHeight + 0.6f;
		this.onGroundBlock = ((!flag2) ? null : BWSceneManager.FindBlock(gameObject, false));
		float num3 = this.legs.onGroundHeight + 0.5f;
		if (magnitude < num3)
		{
			float modelMass = this.legs.modelMass;
			float num4 = this.legs.onGroundHeight + 0.5f - magnitude;
			float num5 = num4 - this.prevHeightError;
			this.prevHeightError = num4;
			float num6 = 30f * modelMass;
			float num7 = 50f * modelMass;
			float num8 = 100f;
			float num9 = Mathf.Min(num2 * num2 * 1.2f, 1f);
			float d = num9 * Mathf.Clamp((num6 * num4 + num7 * num5) * 0.5f * mass, -num8, num8);
			this.hoverResult += up * d;
			if (this.climbOn && magnitude < 0.9f && num2 > 0.2f)
			{
				this.climbing = true;
			}
		}
		else
		{
			this.prevHeightError = 0f;
		}
		return this.hoverResult;
	}

	// Token: 0x06002598 RID: 9624 RVA: 0x001125EC File Offset: 0x001109EC
	private void AddRbBelowVelocity(RaycastHit hit, Rigidbody rb, bool checkSurfaceBlocks = false)
	{
		this.rigidBodyBelow = rb;
		if (this.rigidBodyBelow != null && !this.rigidBodyBelow.isKinematic)
		{
			this.rigidBodyBelowVelocity = this.rigidBodyBelow.velocity;
			Vector3 rhs = hit.point - this.rigidBodyBelow.worldCenterOfMass;
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

	// Token: 0x06002599 RID: 9625 RVA: 0x001126CC File Offset: 0x00110ACC
	private void DPadFixedUpdate(string key)
	{
		Vector2 vector = (!Blocksworld.UI.Controls.IsDPadActive(key)) ? Vector2.zero : Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
		float maxSpeed = this.gotoMaxSpeed * Mathf.Min(1f, vector.magnitude);
		Vector3 cameraUp = Blocksworld.cameraUp;
		Vector3 cameraRight = Blocksworld.cameraRight;
		Vector3 cameraForward = Blocksworld.cameraForward;
		float num = vector.y;
		if (cameraForward.y > 0f)
		{
			num *= -1f;
		}
		Vector3 a = Util.ProjectOntoPlane(cameraUp, Vector3.up).normalized * num;
		Vector3 b = Util.ProjectOntoPlane(cameraRight, Vector3.up).normalized * vector.x;
		Vector3 a2 = a + b;
		this.slowDownAtTarget = false;
		this.gotoOffset += a2 * 100f;
		if (this.turnMode == WalkController.TurnMode.None && a2.sqrMagnitude > 0.001f)
		{
			this.currentDirection = a2.normalized;
		}
		this.GotoFixedUpdate(maxSpeed);
	}

	// Token: 0x0600259A RID: 9626 RVA: 0x001127F8 File Offset: 0x00110BF8
	private void GotoFixedUpdate(float maxSpeed)
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
		if (this.gotoMaxSpeed > 0.001f)
		{
			this.speedFraction = maxSpeed / this.gotoMaxSpeed;
		}
		else
		{
			this.speedFraction = 0f;
		}
		Vector3 worldCenterOfMass = this.legsRb.worldCenterOfMass;
		bool flag = this.InFirstPerson() && !this.legs.unmoving && !this.legs.IsFixed();
		if (flag && (this.mode == WalkController.WalkControllerMode.DPad || (this.mode == WalkController.WalkControllerMode.Translate && !this.strafe)))
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
			BlockCharacter firstPersonCharacter = Blocksworld.blocksworldCamera.firstPersonCharacter;
			if (firstPersonCharacter != null && !firstPersonCharacter.GetThinksOnGround() && Mathf.Abs(num) > 0.05f && Mathf.Abs(firstPersonDpad.y) < 0.1f)
			{
				firstPersonDpad.y = 0.1f;
			}
			this.currentDirection = 0.2f * num * right + this.currentDirection;
			this.currentDirection.y = 0f;
			this.currentDirection.Normalize();
			this.gotoOffset = 100f * (firstPersonDpad.x * right + firstPersonDpad.y * forward);
		}
		Vector3 a = worldCenterOfMass + this.gotoOffset;
		Vector3 vector = Vector3.zero;
		this.totalForce = Vector3.zero;
		this.totalCmTorque = Vector3.zero;
		Vector3 vector2 = a - worldCenterOfMass;
		vector2.y = 0f;
		float magnitude = vector2.magnitude;
		Vector3 normalized = vector2.normalized;
		Vector3 velocity = this.legsRb.velocity;
		velocity.y = 0f;
		this.currentVelFiltered = 0.9f * this.currentVelFiltered + 0.100000024f * velocity;
		float num2 = maxSpeed;
		if (this.slowDownAtTarget)
		{
			if (magnitude < 5f)
			{
				num2 = magnitude / 5f;
			}
			else if (magnitude < 10f)
			{
				num2 = maxSpeed * (magnitude / 10f);
			}
			this.speedFraction = num2 / this.gotoMaxSpeed;
		}
		Vector3 vector3 = normalized * num2;
		float sqrMagnitude = this.avoidVector.sqrMagnitude;
		bool flag2 = sqrMagnitude > 0.01f;
		if (flag2)
		{
			if (sqrMagnitude > 1f)
			{
				this.avoidVector.Normalize();
			}
			vector3 += this.avoidVector * this.avoidMaxSpeed;
			float num3 = Mathf.Max(this.avoidMaxSpeed, this.gotoMaxSpeed);
			if (vector3.sqrMagnitude > num3 * num3)
			{
				vector3 = vector3.normalized * num3;
			}
		}
		if (this.onGround && this.onGroundBlock != null)
		{
			Collider component = this.onGroundBlock.go.GetComponent<Collider>();
			PhysicMaterial material = component.material;
			float dynamicFriction = material.dynamicFriction;
			if (dynamicFriction < 0.4f && this.legsRb.velocity.magnitude > 0.1f)
			{
				float t = Mathf.Pow(dynamicFriction / 0.4f, 3f);
				vector3 = Vector3.Lerp(this.wantedVel, vector3, t);
			}
		}
		this.wantedVel = vector3;
		float num4 = 0f;
		if (this.gotoMaxSpeed > 0.01f)
		{
			num4 = this.currentVelFiltered.magnitude / this.gotoMaxSpeed;
		}
		float num5 = 1f - this.wackiness;
		this.highSpeedFraction = num4 * Mathf.Clamp((this.gotoMaxSpeed - 7f) * 0.05f, 0f, 1f);
		if (this.highSpeedFraction > 0.01f)
		{
			num5 = Mathf.Min(1f, num5 + this.highSpeedFraction);
		}
		Vector3 b = num5 * this.GetHoverForceAndUpdateOnGround(this.legsRb.mass) * 4f;
		vector += b;
		if (this.rigidBodyBelow != null)
		{
			this.rigidBodyBelowVelocity.y = 0f;
			float sqrMagnitude2 = this.rigidBodyBelowVelocity.sqrMagnitude;
			if (sqrMagnitude2 > 0.01f)
			{
				Vector3 b2 = new Vector3(this.rigidBodyBelowVelocity.x, 0f, this.rigidBodyBelowVelocity.z);
				this.wantedVel += b2;
			}
		}
		if (!this.onGround)
		{
			this.prevHeightError = 0f;
		}
		Vector3 vector4 = this.wantedVel - this.currentVelFiltered;
		vector4.y = 0f;
		float magnitude2 = vector4.magnitude;
		Vector3 normalized2 = vector4.normalized;
		float num6 = magnitude2 - this.prevVelError;
		this.prevVelError = magnitude2;
		Vector3 up = this.legsTransform.up;
		float num7 = Vector3.Angle(Vector3.up, up);
		float num8 = Mathf.Max(1f - Mathf.Abs(num7) / 135f, 0f);
		float num9 = num8;
		if (!this.onGround)
		{
			num8 *= 0.5f;
			num9 = 0.6f;
		}
		float num10 = 1f + this.highSpeedFraction;
		bool flag3 = flag2 || this.gotoOffset.sqrMagnitude > 0.0001f || (magnitude2 > 1f && this.onGround);
		float num11 = 50f;
		if (flag3 && (this.onGround || magnitude2 > 0f))
		{
			Vector3 vector5 = normalized2;
			float d = num10 * num8 * Mathf.Clamp(magnitude2 * 10f + num6 * 10f, -num11, num11);
			vector5 *= d;
			vector += vector5;
		}
		if (this.onGround && this.onGroundBlock != null)
		{
			Collider componentInChildren = this.onGroundBlock.go.GetComponentInChildren<Collider>();
			float num12 = (!(componentInChildren != null) || !(componentInChildren.sharedMaterial != null)) ? 0f : componentInChildren.sharedMaterial.bounciness;
			float num13 = 4f - 3f * num12;
			Vector3 velocity2 = this.legsRb.velocity;
			float num14 = -Vector3.Dot(this.onGroundNormal, velocity2);
			if (num12 > 0f && num14 > num13)
			{
				float num15 = velocity2.magnitude * num12;
				Vector3 a2 = Vector3.Reflect(velocity2.normalized, this.onGroundNormal);
				this.legsRb.AddForce(1.5f * num15 * a2 - velocity2, ForceMode.VelocityChange);
				this.onGround = false;
				return;
			}
		}
		if (this.onGround || flag3)
		{
			Vector3 a3 = Vector3.Cross(up, Vector3.up);
			float num16 = Mathf.Clamp(num7 - this.prevAngleError, -5f, 5f);
			this.prevAngleError = num7;
			float num17 = 70f * num10;
			float num18 = num10 * (0.1f + 0.9f * num9) * (num7 * 2f + num16 * 8f + 40f);
			num18 = Mathf.Clamp(num18, -num17, num17);
			this.totalCmTorque += a3 * num18;
			Vector3 angularVelocity = this.legsRb.angularVelocity;
			float magnitude3 = angularVelocity.magnitude;
			float num19 = Mathf.Clamp(magnitude3, -10f, 10f);
			this.totalCmTorque += -num9 * num19 * angularVelocity;
			float num20 = (!this.slowDownAtTarget) ? 1f : 3f;
			if (this.strafe)
			{
				this.currentDirection = this.legsTransform.forward;
			}
			else if (((double)magnitude2 > 0.02 && magnitude > num20) || this.turnMode != WalkController.TurnMode.None || flag)
			{
				Vector3 forward2 = this.legsTransform.forward;
				if ((double)Mathf.Abs(Vector3.Dot(forward2, Vector3.up)) < 0.25)
				{
					forward2.y = 0f;
					Vector3 vector6 = (this.turnMode == WalkController.TurnMode.None && !flag) ? normalized : this.currentDirection;
					float num21 = Util.AngleBetween(forward2, vector6, Vector3.up);
					float num22 = Mathf.Clamp(num21 - this.prevForwardAngleError, -5f, 5f) / Blocksworld.fixedDeltaTime;
					this.prevForwardAngleError = num21;
					if (Mathf.Abs(num21) < 25f)
					{
						this.iForwardAngleSum = Mathf.Clamp(this.iForwardAngleSum + Blocksworld.fixedDeltaTime * num21, -1000f, 1000f);
					}
					else
					{
						this.iForwardAngleSum = 0f;
					}
					a3 = Vector3.Cross(forward2, vector6);
					float num23 = Mathf.Clamp(num21 * 2f + num22 * 0.1f + this.iForwardAngleSum * 5f, -num17, num17);
					if (flag)
					{
						num23 *= Blocksworld.blocksworldCamera.firstPersonTorque;
					}
					bool ignoreRotation = this.legs.ignoreRotation;
					num23 = ((!ignoreRotation) ? num23 : 0f);
					this.totalCmTorque += num9 * up * num23 * (1f + this.highSpeedFraction);
					Vector3 position = this.legsTransform.position;
					Vector3 a4 = position - worldCenterOfMass;
					a4.y = 0f;
					float magnitude4 = a4.magnitude;
					if (magnitude4 > 0.25f && !ignoreRotation)
					{
						float num24 = 0.5f * num11;
						float d2 = num9 * (1f + this.highSpeedFraction) * Mathf.Clamp(-0.1f * num23 * magnitude4, -num24, num24);
						Vector3 vector7 = d2 * Vector3.Cross(a4 / magnitude4, Vector3.up);
						this.totalForce += vector7;
						this.legsRb.AddForceAtPosition(vector7, position);
					}
				}
			}
		}
		else
		{
			this.prevAngleError = 0f;
		}
		this.legsRb.AddForceAtPosition(vector, worldCenterOfMass);
		this.legsRb.AddTorque(this.totalCmTorque * this.torqueMultiplier * this.addedTorqueMult);
		this.totalForce += vector;
		this.applicationCounts.Clear();
	}

	// Token: 0x0600259B RID: 9627 RVA: 0x0011334C File Offset: 0x0011174C
	private void GotoTapFixedUpdate()
	{
		if (TapControlGesture.HasWorldTapPos() && this.tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= this.tapActivatedTime)
		{
			this.gotoOffset += TapControlGesture.GetWorldTapPos() - this.legs.goT.position;
			this.gotoOffset.y = 0f;
			if (this.turnMode == WalkController.TurnMode.None && this.gotoOffset.sqrMagnitude > 1E-05f)
			{
				this.currentDirection = this.gotoOffset.normalized;
			}
		}
		this.GotoFixedUpdate(this.gotoMaxSpeed);
	}

	// Token: 0x0600259C RID: 9628 RVA: 0x001133FC File Offset: 0x001117FC
	private void TurnTowardsTapFixedUpdate()
	{
		if (TapControlGesture.HasWorldTapPos() && this.tapActivatedTime >= 0f && TapControlGesture.GetWorldTapTime() >= this.tapActivatedTime)
		{
			Vector3 vector = TapControlGesture.GetWorldTapPos() - this.legs.goT.position;
			vector.y = 0f;
			if (vector.sqrMagnitude > 0.01f)
			{
				this.currentDirection = vector.normalized;
			}
		}
	}

	// Token: 0x0600259D RID: 9629 RVA: 0x00113478 File Offset: 0x00111878
	private void AddJumpForce()
	{
		float num = 7f * this.totalJumpEnergy * Blocksworld.fixedDeltaTime;
		Vector3 force = num * this.jumpUp + 0.2f * num * this.sideJumpVector;
		Rigidbody component = this.legs.body.GetComponent<Rigidbody>();
		Vector3 worldCenterOfMass = component.worldCenterOfMass;
		Vector3 position = 0.7f * worldCenterOfMass + 0.3f * this.legs.goT.position;
		component.AddForceAtPosition(force, position);
		this.jumpEnergyLeft -= num;
	}

	// Token: 0x0600259E RID: 9630 RVA: 0x00113518 File Offset: 0x00111918
	private void JumpFixedUpdate()
	{
		WalkController.JumpMode jumpMode = this.jumpMode;
		if (jumpMode != WalkController.JumpMode.AddingForce)
		{
			if (jumpMode != WalkController.JumpMode.Ready)
			{
				if (jumpMode == WalkController.JumpMode.WaitingForReady)
				{
					this.jumpTimer -= Blocksworld.fixedDeltaTime;
					if (this.jumpTimer <= 0f)
					{
						this.jumpMode = WalkController.JumpMode.Ready;
					}
				}
			}
			else if (this.jumpEnergyLeft > 0f)
			{
				this.AddJumpForce();
				if (this.gotoOffset.sqrMagnitude > 1E-05f)
				{
					this.sideJumpVector = this.gotoOffset.normalized * this.speedFraction;
				}
				else
				{
					this.sideJumpVector = Vector3.zero;
				}
				this.jumpMode = WalkController.JumpMode.AddingForce;
				this.jumpCountThisFrame = 0;
			}
		}
		else
		{
			this.AddJumpForce();
			if (this.jumpEnergyLeft <= 0f || !this.jumpPressed)
			{
				this.jumpEnergyLeft = 0f;
				this.totalJumpEnergy = 0f;
				this.jumpMode = WalkController.JumpMode.WaitingForReady;
			}
		}
		this.jumpPressed = false;
	}

	// Token: 0x0600259F RID: 9631 RVA: 0x00113628 File Offset: 0x00111A28
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

	// Token: 0x060025A0 RID: 9632 RVA: 0x001137A4 File Offset: 0x00111BA4
	public void FixedUpdate()
	{
		WalkController.VicinityMode vicinityMode = this.vicinityMode;
		if (vicinityMode != WalkController.VicinityMode.AvoidTag)
		{
			if (vicinityMode == WalkController.VicinityMode.None)
			{
				this.avoidMaxSpeed = this.defaultMaxSpeed;
			}
		}
		else
		{
			this.AvoidTagFixedUpdate();
		}
		if (this.InFirstPerson())
		{
			this.GotoFixedUpdate(this.gotoMaxSpeed);
		}
		else
		{
			WalkController.TurnMode turnMode = this.turnMode;
			if (turnMode == WalkController.TurnMode.TurnTowardsTap)
			{
				this.TurnTowardsTapFixedUpdate();
				if (this.previousTurnMode != WalkController.TurnMode.TurnTowardsTap)
				{
					this.tapActivatedTime = Time.time;
				}
			}
			switch (this.mode)
			{
			case WalkController.WalkControllerMode.Idle:
				if (this.turnMode != WalkController.TurnMode.None || this.vicinityMode != WalkController.VicinityMode.None)
				{
					this.GotoFixedUpdate(this.gotoMaxSpeed);
				}
				this.gotoMaxSpeed = this.defaultMaxSpeed;
				this.speedFraction = 1f;
				this.prevVelError = 0f;
				this.wantedVel = Vector3.zero;
				break;
			case WalkController.WalkControllerMode.GotoTap:
				this.GotoTapFixedUpdate();
				if (this.previousMode != WalkController.WalkControllerMode.GotoTap)
				{
					this.tapActivatedTime = Time.time;
				}
				break;
			case WalkController.WalkControllerMode.GotoTag:
				this.GotoFixedUpdate(this.gotoMaxSpeed);
				break;
			case WalkController.WalkControllerMode.DPad:
				this.DPadFixedUpdate(this.dPadMoveKey);
				break;
			case WalkController.WalkControllerMode.Translate:
				this.GotoFixedUpdate(this.gotoMaxSpeed);
				break;
			}
		}
		this.JumpFixedUpdate();
		if (this.mode == WalkController.WalkControllerMode.Idle && this.turnMode == WalkController.TurnMode.None)
		{
			this.tapActivatedTime = -1f;
			this.prevAngleError = 0f;
		}
		this.previousMode = this.mode;
		this.previousTurnMode = this.turnMode;
		this.mode = WalkController.WalkControllerMode.Idle;
		this.turnMode = WalkController.TurnMode.None;
		this.vicinityMode = WalkController.VicinityMode.None;
		this.gotoOffset = Vector3.zero;
		this.avoidVector = Vector3.zero;
		this.strafe = false;
		this.gotoMaxSpeed = 0f;
		if (this.InFirstPerson())
		{
			Blocksworld.blocksworldCamera.firstPersonDpad = Vector2.zero;
			Blocksworld.blocksworldCamera.firstPersonRotation = 0f;
		}
	}

	// Token: 0x17000174 RID: 372
	// (set) Token: 0x060025A1 RID: 9633 RVA: 0x001139B6 File Offset: 0x00111DB6
	public float SetAddedTorqueMultiplier
	{
		set
		{
			this.addedTorqueMult = value * this.legs.modelMass;
		}
	}

	// Token: 0x060025A2 RID: 9634 RVA: 0x001139CC File Offset: 0x00111DCC
	public float GetAndResetHighSpeedFraction()
	{
		float result = this.highSpeedFraction;
		this.highSpeedFraction = 1f;
		return result;
	}

	// Token: 0x060025A3 RID: 9635 RVA: 0x001139EC File Offset: 0x00111DEC
	private void Climp(int handIndex, Vector3 shoulderPoint, Vector3 climbPoint, Vector3 signedRight, ref float alpha)
	{
		Vector3 normalized = (shoulderPoint - climbPoint).normalized;
		this.handTargets[handIndex] = climbPoint + 0.3f * normalized + signedRight * 0.3f;
		alpha = 0.5f;
	}

	// Token: 0x060025A4 RID: 9636 RVA: 0x00113A44 File Offset: 0x00111E44
	public void SetHandPosition(int handIndex, GameObject hand, float handWidth, float handHeight)
	{
		Transform transform = hand.transform;
		Vector3 position = transform.position;
		Transform goT = this.legs.goT;
		Vector3 up = goT.up;
		Vector3 forward = goT.forward;
		Vector3 right = goT.right;
		int num = handIndex * 2 - 1;
		Vector3 vector = right * (float)(-(float)num);
		Vector3 position2 = goT.position;
		Vector3 vector2 = position2 + up * handHeight + vector * handWidth + forward * -0.1f;
		int num2 = 4;
		float num3 = 0.2f;
		HandInfo handInfo = this.handInfos[handIndex];
		if (this.handCounters[handIndex] % num2 == 0)
		{
			Rigidbody rigidBody = this.legs.GetRigidBody();
			float num4 = 0.65f;
			Vector3 vector3 = vector2 + (-up * 2.5f + vector).normalized * num4;
			Vector3 vector4 = Util.ProjectOntoPlane(rigidBody.velocity - this.GetRigidBodyBelowVelocity(), Vector3.up);
			float magnitude = vector4.magnitude;
			Vector3 normalized = vector4.normalized;
			float num5 = Mathf.Min(1f, magnitude * 0.06f);
			handInfo.lastParentUpdatePos = position2;
			if (this.onGround)
			{
				handInfo.pendlumTimer += num5;
				if (handInfo.pendlumTimer > 6.28318548f)
				{
					handInfo.pendlumTimer -= 6.28318548f;
				}
				handInfo.pendlumAmp = Mathf.Min(0.7f, magnitude * 0.15f);
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					handInfo.pendlumTimer *= 0.98f;
					handInfo.pendlumAmp *= 0.98f;
				}
			}
			Vector3 b = handInfo.pendlumAmp * forward * (float)num * Mathf.Sin(handInfo.pendlumTimer * (float)num2);
			vector3 += b;
			Vector3 rhs = vector4;
			float num6 = 5f;
			if (magnitude > num6)
			{
				rhs = normalized * num6;
			}
			Vector3 a = 0.1f * (forward * Vector3.Dot(forward, rhs));
			Vector3 vector5 = 0.1f * (up * magnitude * Mathf.Clamp(Vector3.Dot(up, normalized), -0.5f, 1f));
			if (this.onGround)
			{
				a *= 0.2f;
				vector5 *= 0.2f;
			}
			Vector3 b2 = a + vector5;
			vector3 += b2;
			Vector3 vector6 = up - Vector3.up;
			vector6 = Vector3.Dot(forward, vector6) * forward + Mathf.Max(0f, Vector3.Dot(vector, vector6)) * vector + Vector3.Dot(up, vector6) * up;
			vector3 += vector6;
			Vector3 vector7 = vector3 - vector2;
			float magnitude2 = vector7.magnitude;
			Vector3 normalized2 = vector7.normalized;
			float num7 = magnitude2 - num4;
			float num8 = 1f;
			float num9 = 0.6f;
			if (magnitude2 > num8)
			{
				vector3 = vector2 + normalized2 * num8;
			}
			else if (magnitude2 < num9)
			{
				vector3 = vector2 + normalized2 * num9;
			}
			else
			{
				vector3 += -0.1f * num7 * normalized2;
			}
			this.handTargets[handIndex] = vector3;
		}
		else
		{
			Vector3 b3 = position2 - handInfo.lastParentUpdatePos;
			this.handTargets[handIndex] += b3;
			handInfo.lastParentUpdatePos = position2;
		}
		this.handCounters[handIndex]++;
		transform.position = num3 * this.handTargets[handIndex] + (1f - num3) * position;
		transform.LookAt(vector2, vector);
		transform.Rotate(0f, 0f, 90f);
	}

	// Token: 0x060025A5 RID: 9637 RVA: 0x00113E92 File Offset: 0x00112292
	public bool IsOnGround()
	{
		return this.onGround;
	}

	// Token: 0x060025A6 RID: 9638 RVA: 0x00113E9A File Offset: 0x0011229A
	public void ClearIgnoreColliders()
	{
		if (this.ignoreColliders != null)
		{
			this.ignoreColliders.Clear();
			this.ignoreColliders = null;
		}
	}

	// Token: 0x04002011 RID: 8209
	private Chunk chunk;

	// Token: 0x04002012 RID: 8210
	private Rigidbody rigidBodyBelow;

	// Token: 0x04002013 RID: 8211
	private Vector3 rigidBodyBelowVelocity = Vector3.zero;

	// Token: 0x04002014 RID: 8212
	private HandInfo[] handInfos = new HandInfo[]
	{
		new HandInfo(),
		new HandInfo()
	};

	// Token: 0x04002015 RID: 8213
	private HashSet<Collider> ignoreColliders;

	// Token: 0x04002016 RID: 8214
	private Dictionary<WalkController.WalkControllerMode, float> applicationCounts = new Dictionary<WalkController.WalkControllerMode, float>();

	// Token: 0x04002017 RID: 8215
	private WalkController.JumpMode jumpMode;

	// Token: 0x04002018 RID: 8216
	private Vector3 jumpUp = Vector3.up;

	// Token: 0x04002019 RID: 8217
	private bool jumpPressed;

	// Token: 0x0400201A RID: 8218
	private int jumpCountThisFrame;

	// Token: 0x0400201B RID: 8219
	private float jumpTimer;

	// Token: 0x0400201C RID: 8220
	private float jumpEnergyLeft;

	// Token: 0x0400201D RID: 8221
	private float totalJumpEnergy;

	// Token: 0x0400201E RID: 8222
	private Vector3 sideJumpVector = Vector3.up;

	// Token: 0x0400201F RID: 8223
	public float heightTestOffset = 0.5f;

	// Token: 0x04002020 RID: 8224
	public bool climbOn;

	// Token: 0x04002021 RID: 8225
	public Vector3 downRayOffset = Vector3.zero;

	// Token: 0x04002022 RID: 8226
	public bool climbing;

	// Token: 0x04002023 RID: 8227
	public Vector3 currentDirection = Vector3.forward;

	// Token: 0x04002024 RID: 8228
	private Vector3 wantedVel = Vector3.zero;

	// Token: 0x04002025 RID: 8229
	public float defaultMaxSpeed = 5f;

	// Token: 0x04002026 RID: 8230
	public float defaultAvoidDistance = 3f;

	// Token: 0x04002027 RID: 8231
	private Dictionary<string, float> avoidDistances = new Dictionary<string, float>();

	// Token: 0x04002028 RID: 8232
	private Dictionary<string, float> avoidApplications = new Dictionary<string, float>();

	// Token: 0x04002029 RID: 8233
	public float avoidMaxSpeed = 5f;

	// Token: 0x0400202A RID: 8234
	public float defaultDPadMaxSpeed = 5f;

	// Token: 0x0400202B RID: 8235
	public float speedIncPerApplication = 2.5f;

	// Token: 0x0400202C RID: 8236
	public float wackiness = 1f;

	// Token: 0x0400202D RID: 8237
	public float defaultWackiness = 1f;

	// Token: 0x0400202E RID: 8238
	public float torqueMultiplier = 1f;

	// Token: 0x0400202F RID: 8239
	public float addedTorqueMult = 1f;

	// Token: 0x04002030 RID: 8240
	private float highSpeedFraction;

	// Token: 0x04002031 RID: 8241
	private float tapActivatedTime = -1f;

	// Token: 0x04002032 RID: 8242
	private bool onGround;

	// Token: 0x04002033 RID: 8243
	private Vector3 onGroundNormal;

	// Token: 0x04002034 RID: 8244
	private bool strafe;

	// Token: 0x04002035 RID: 8245
	public Block onGroundBlock;

	// Token: 0x04002036 RID: 8246
	private string dPadMoveKey = "L";

	// Token: 0x04002037 RID: 8247
	private WalkController.WalkControllerMode mode;

	// Token: 0x04002038 RID: 8248
	private WalkController.WalkControllerMode previousMode;

	// Token: 0x04002039 RID: 8249
	private WalkController.TurnMode turnMode;

	// Token: 0x0400203A RID: 8250
	private WalkController.TurnMode previousTurnMode;

	// Token: 0x0400203B RID: 8251
	private WalkController.VicinityMode vicinityMode;

	// Token: 0x0400203C RID: 8252
	private Vector3 gotoOffset = Vector3.zero;

	// Token: 0x0400203D RID: 8253
	private Vector3 avoidVector = Vector3.zero;

	// Token: 0x0400203E RID: 8254
	private float gotoMaxSpeed = 8f;

	// Token: 0x0400203F RID: 8255
	private bool slowDownAtTarget = true;

	// Token: 0x04002040 RID: 8256
	public BlockAbstractLegs legs;

	// Token: 0x04002041 RID: 8257
	public List<WalkController> chunkControllers = new List<WalkController>();

	// Token: 0x04002042 RID: 8258
	private List<WalkController> sameUpControllers = new List<WalkController>();

	// Token: 0x04002043 RID: 8259
	private List<WalkController> conflictingUpControllers = new List<WalkController>();

	// Token: 0x04002044 RID: 8260
	private HashSet<Block> chunkBlocks = new HashSet<Block>();

	// Token: 0x04002045 RID: 8261
	private float prevAngleError;

	// Token: 0x04002046 RID: 8262
	private Vector3 iUpAngleError;

	// Token: 0x04002047 RID: 8263
	private float prevVelError;

	// Token: 0x04002048 RID: 8264
	private float prevForwardAngleError;

	// Token: 0x04002049 RID: 8265
	private float iForwardAngleSum;

	// Token: 0x0400204A RID: 8266
	private float prevHeightError;

	// Token: 0x0400204B RID: 8267
	private Vector3 currentVelFiltered = Vector3.zero;

	// Token: 0x0400204C RID: 8268
	private Vector3 targetVelocity = Vector3.zero;

	// Token: 0x0400204D RID: 8269
	private Vector3 totalForce = Vector3.zero;

	// Token: 0x0400204E RID: 8270
	private Vector3 totalCmTorque = Vector3.zero;

	// Token: 0x0400204F RID: 8271
	public float speedFraction = 1f;

	// Token: 0x04002050 RID: 8272
	private Vector3 hoverResult = Vector3.zero;

	// Token: 0x04002051 RID: 8273
	private const float SPEED_FRACTION_THRESHOLD = 0.001f;

	// Token: 0x04002052 RID: 8274
	private const float VEL_FILTER_ALPHA = 0.9f;

	// Token: 0x04002053 RID: 8275
	private const float TRANSLATE_CONTROL_K = 10f;

	// Token: 0x04002054 RID: 8276
	private const float TRANSLATE_CONTROL_D = 10f;

	// Token: 0x04002055 RID: 8277
	private const float ANGLE_CONTROL_K = 2f;

	// Token: 0x04002056 RID: 8278
	private const float ANGLE_CONTROL_D = 8f;

	// Token: 0x04002057 RID: 8279
	private const float FORWARD_ANGLE_CONTROL_K = 2f;

	// Token: 0x04002058 RID: 8280
	private const float FORWARD_ANGLE_CONTROL_D = 0.1f;

	// Token: 0x04002059 RID: 8281
	private const float FORWARD_ANGLE_CONTROL_I = 5f;

	// Token: 0x0400205A RID: 8282
	private Rigidbody legsRb;

	// Token: 0x0400205B RID: 8283
	private Transform legsTransform;

	// Token: 0x0400205C RID: 8284
	private const float BODY_HEIGHT_ADJUST = 4f;

	// Token: 0x0400205D RID: 8285
	private int[] handCounters = new int[2];

	// Token: 0x0400205E RID: 8286
	private Vector3[] handTargets = new Vector3[2];

	// Token: 0x02000344 RID: 836
	private enum WalkControllerMode
	{
		// Token: 0x04002060 RID: 8288
		Idle,
		// Token: 0x04002061 RID: 8289
		GotoTap,
		// Token: 0x04002062 RID: 8290
		GotoTag,
		// Token: 0x04002063 RID: 8291
		DPad,
		// Token: 0x04002064 RID: 8292
		Translate
	}

	// Token: 0x02000345 RID: 837
	private enum TurnMode
	{
		// Token: 0x04002066 RID: 8294
		None,
		// Token: 0x04002067 RID: 8295
		Turn,
		// Token: 0x04002068 RID: 8296
		TurnTowardsTag,
		// Token: 0x04002069 RID: 8297
		TurnTowardsTap,
		// Token: 0x0400206A RID: 8298
		TurnAlongCamera
	}

	// Token: 0x02000346 RID: 838
	private enum VicinityMode
	{
		// Token: 0x0400206C RID: 8300
		None,
		// Token: 0x0400206D RID: 8301
		AvoidTag
	}

	// Token: 0x02000347 RID: 839
	private enum JumpMode
	{
		// Token: 0x0400206F RID: 8303
		Ready,
		// Token: 0x04002070 RID: 8304
		AddingForce,
		// Token: 0x04002071 RID: 8305
		WaitingForReady
	}
}
