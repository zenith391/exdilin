using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200004E RID: 78
	public class BlockAbstractAntiGravity : BlockAbstractHover
	{
		// Token: 0x06000660 RID: 1632 RVA: 0x0002B17C File Offset: 0x0002957C
		public BlockAbstractAntiGravity(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0002B2C0 File Offset: 0x000296C0
		public new static void Register()
		{
			BlockAbstractAntiGravity.predicateTiltMover = PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.TiltMover", null, (Block b) => new PredicateActionDelegate(b.TiltMoverControl), new Type[]
			{
				typeof(float),
				typeof(int)
			}, null, null);
			BlockAbstractAntiGravity.predicateAlignToTilt = PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.AlignToTilt", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignToTilt), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.ConstrainTranslation", (Block b) => new PredicateSensorDelegate(b.IsConstrainTranslation), (Block b) => new PredicateActionDelegate(b.ConstrainTranslation), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.FreeTranslation", (Block b) => new PredicateSensorDelegate(b.IsFreeTranslation), (Block b) => new PredicateActionDelegate(b.FreeTranslation), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.ConstrainRotation", (Block b) => new PredicateSensorDelegate(b.IsConstrainRotation), (Block b) => new PredicateActionDelegate(b.ConstrainRotation), new Type[]
			{
				typeof(int)
			}, null, null);
			PredicateRegistry.Add<BlockAbstractAntiGravity>("AntiGravity.FreeRotation", (Block b) => new PredicateSensorDelegate(b.IsFreeRotation), (Block b) => new PredicateActionDelegate(b.FreeRotation), new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0002B4C8 File Offset: 0x000298C8
		public override void OnCreate()
		{
			base.OnCreate();
			this.metaData = this.go.GetComponent<AntigravityMetaData>();
			if (this.metaData != null)
			{
				this.rotation = Quaternion.Euler(this.metaData.orientation);
			}
			else
			{
				BWLog.Info("Could not find antigravity meta data component in " + base.BlockType());
			}
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0002B530 File Offset: 0x00029930
		public override void Play()
		{
			base.Play();
			if (this.chunk != null)
			{
				this.chunk.UpdateCenterOfMass(true);
			}
			this.treatAsVehicleStatus = -1;
			this.tiltAlignHeadingController = new PDControllerVector3(50f, 4f);
			this.tiltAlignUpController = new PDControllerVector3(50f, 4f);
			this.tiltAlignAngVelController = new PDControllerVector3(4f, 0f);
			this.hoverInFieldDistanceOffsets = Util.nullVector3;
			this.modelAntigravityBlocks.Clear();
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				if (block is BlockAbstractAntiGravity)
				{
					this.modelAntigravityBlocks.Add((BlockAbstractAntiGravity)block);
				}
			}
			this.modelAntigravityBlocks.Sort(new BlockNameComparer<BlockAbstractAntiGravity>());
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0002B614 File Offset: 0x00029A14
		private void ApplyChunkVelocityForce(Vector3 targetVelocity)
		{
			if (Mathf.Abs(this.targetVelocityApplications) > 0.001f && this.chunkRigidBody != null && !this.chunkRigidBody.isKinematic)
			{
				Vector3 velocity = this.chunkRigidBody.velocity;
				Vector3 normalized = targetVelocity.normalized;
				base.UpdateConnectedCache();
				Vector3 modelAcceleration = BlockAccelerations.GetModelAcceleration(Block.connectedCache[this]);
				targetVelocity += modelAcceleration;
				Vector3 vector = targetVelocity - velocity;
				Vector3 vector2 = Vector3.Dot(normalized, vector) * normalized;
				Vector3 a = vector - vector2;
				Vector3 a2 = vector2 + 0.2f * a;
				float mass = this.chunkRigidBody.mass;
				float d = 25f * this.targetVelocityApplications;
				if (a2.magnitude > 4f)
				{
					a2 = a2.normalized * 4f;
				}
				Vector3 force = a2 * mass * d;
				this.chunkRigidBody.AddForce(force);
				Vector3 vector3 = targetVelocity * this.targetVelocityApplications;
				vector3 = Util.ProjectOntoPlane(vector3, Vector3.up);
				Blocksworld.blocksworldCamera.AddForceDirectionHint(this.chunk, vector3);
				float num = Mathf.Abs(force.magnitude / mass);
				this.sfxLoopStrength += num / 20f;
			}
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0002B774 File Offset: 0x00029B74
		private void ApplyChunkAngularVelocityTorque(Vector3 targetAngVel)
		{
			if (Mathf.Abs(this.targetAngVelApplications) > 0.001f && this.chunkRigidBody != null && !this.chunkRigidBody.isKinematic)
			{
				Vector3 angularVelocity = this.chunkRigidBody.angularVelocity;
				Vector3 a = targetAngVel - angularVelocity;
				float mass = this.chunkRigidBody.mass;
				Vector3 vector = a * mass;
				this.chunkRigidBody.AddTorque(vector);
				float magnitude = (vector / mass).magnitude;
				this.sfxLoopStrength += magnitude / 100f;
			}
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0002B814 File Offset: 0x00029C14
		private void ApplyChunkExtraTorque(Vector3 extraTorque)
		{
			if (extraTorque.sqrMagnitude > 0.001f && this.chunkRigidBody != null && !this.chunkRigidBody.isKinematic)
			{
				float mass = this.chunkRigidBody.mass;
				Vector3 torque = extraTorque * mass;
				this.chunkRigidBody.AddTorque(torque);
				if (this.isTrackingTiltAlign)
				{
					Vector3 normalized = torque.normalized;
					float angle = torque.magnitude * Time.fixedDeltaTime;
					this.tiltAlignBaseRotation *= Quaternion.AngleAxis(angle, normalized);
				}
				float magnitude = extraTorque.magnitude;
				this.sfxLoopStrength += magnitude / 100f;
			}
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0002B8CC File Offset: 0x00029CCC
		protected void ApplyChunkAlignTorque(Vector3 field, Vector3 localUp, float applications, float maxAngVelMag = -1f)
		{
			float num = Mathf.Abs(applications);
			if (num > 0.01f && this.chunkRigidBody != null)
			{
				Vector3 vector = this.goT.TransformDirection(localUp);
				float a = Vector3.Angle(vector, -field);
				Vector3 vector2 = Vector3.Cross(field, vector);
				if (vector2.sqrMagnitude > 0.001f)
				{
					vector2 = vector2.normalized;
				}
				else
				{
					vector2 = this.goT.forward;
				}
				float mass = this.chunkRigidBody.mass;
				float num2 = num;
				float num3 = 0.5f * num2;
				vector2 *= num3 * Mathf.Min(a, 90f) * mass;
				Vector3 angularVelocity = this.chunkRigidBody.angularVelocity;
				Vector3 normalized = field.normalized;
				Vector3 a2 = Util.ProjectOntoPlane(angularVelocity, -normalized);
				float magnitude = a2.magnitude;
				if (maxAngVelMag > 0f && magnitude > maxAngVelMag)
				{
					float num4 = magnitude - maxAngVelMag;
					vector2 *= 1f / (1f + 5f * num4);
				}
				float num5 = num2;
				float num6 = magnitude * num5 * mass;
				vector2 += -num6 * a2;
				this.chunkRigidBody.AddTorque(vector2);
			}
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0002BA0C File Offset: 0x00029E0C
		protected void ApplyTiltAlignChunkTorque()
		{
			if (!this.applyTiltAlign)
			{
				return;
			}
			if (this.chunkRigidBody == null || !TiltManager.Instance.IsMonitoring())
			{
				return;
			}
			Quaternion quaternion = Quaternion.Inverse(this.tiltAlignBaseAttitude) * TiltManager.Instance.GetCurrentAttitude();
			quaternion *= this.tiltAttitudeCorrect;
			Quaternion quaternion2 = this.tiltAlignBaseRotation * quaternion;
			bool flag = false;
			if (flag)
			{
				quaternion2 = Quaternion.Slerp(this.chunkRigidBody.rotation, quaternion2, 8f * Time.fixedDeltaTime);
				this.chunkRigidBody.MoveRotation(quaternion2);
				return;
			}
			Vector3 currentError = -this.chunkRigidBody.angularVelocity;
			Vector3 torque = this.tiltAlignAngVelController.Update(currentError, Time.fixedDeltaTime);
			Vector3 rhs = quaternion2 * Vector3.forward;
			Vector3 currentError2 = Vector3.Cross(this.chunkRigidBody.transform.forward, rhs);
			Vector3 torque2 = this.tiltAlignHeadingController.Update(currentError2, Time.fixedDeltaTime);
			Vector3 rhs2 = quaternion2 * Vector3.up;
			Vector3 currentError3 = Vector3.Cross(this.chunkRigidBody.transform.up, rhs2);
			Vector3 torque3 = this.tiltAlignUpController.Update(currentError3, Time.fixedDeltaTime);
			this.chunkRigidBody.AddTorque(torque, ForceMode.Acceleration);
			this.chunkRigidBody.AddTorque(torque2, ForceMode.Acceleration);
			this.chunkRigidBody.AddTorque(torque3, ForceMode.Acceleration);
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0002BB70 File Offset: 0x00029F70
		private bool GetClosestGroundHit(Vector3 pos, ref RaycastHit hit, float maxDist = 100f)
		{
			RaycastHit[] array = Physics.RaycastAll(pos, Vector3.down, maxDist);
			Util.SmartSort(array, pos);
			foreach (RaycastHit hitt in array)
			{
				Block block = BWSceneManager.FindBlock(hitt.collider.gameObject, false);
				if (block != null)
				{
					if (block.isTerrain)
					{
						return true;
					}
					if (block.modelBlock != this.modelBlock)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0002BBF0 File Offset: 0x00029FF0
		private float GetDistanceToGround(Vector3 pos)
		{
			if (this.GetClosestGroundHit(pos, ref BlockAbstractAntiGravity.tempHit, 100f))
			{
				return BlockAbstractAntiGravity.tempHit.distance;
			}
			return 200f;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0002BC18 File Offset: 0x0002A018
		private void ApplyChunkPositionForces()
		{
			for (int i = 0; i < 3; i++)
			{
				float num = this.positionInFieldChunkApplications[i];
				float num2 = this.prevPositionInFieldChunkApplications[i];
				if (num > 0.01f && this.chunkRigidBody != null)
				{
					bool flag = this.positionInFieldHover[i];
					Vector3 worldCenterOfMass = this.chunkRigidBody.worldCenterOfMass;
					float num3 = this.hoverInFieldDistanceOffsets[i];
					Vector3 zero = Vector3.zero;
					zero[i] = 1f;
					if (num2 < 0.01f)
					{
						if (flag && Util.IsNullVector3Component(num3, i))
						{
							num3 = Mathf.Min(50f, this.GetDistanceToGround(worldCenterOfMass)) + 2.5f * (1f + this.extraModelGravityMultiplier);
							this.hoverInFieldDistanceOffsets[i] = num3;
						}
						else
						{
							this.positionInFieldPositions[i] = worldCenterOfMass;
						}
						this.positionInFieldChunkOffsets[i] = 0f;
					}
					float num4 = this.positionInFieldChunkOffsets[i];
					float num5 = this.positionInFieldChunkOffsetTargets[i];
					float num6 = num5 - num4;
					float num7 = Mathf.Abs(num6);
					float num8 = this.positionInFieldChunkOffsetIncrements[i];
					if (num7 < num8)
					{
						ref Vector3 ptr = ref this.positionInFieldChunkOffsets;
						int index;
						this.positionInFieldChunkOffsets[index = i] = ptr[index] + num6;
					}
					else
					{
						ref Vector3 ptr = ref this.positionInFieldChunkOffsets;
						int index2;
						this.positionInFieldChunkOffsets[index2 = i] = ptr[index2] + Mathf.Sign(num6) * num8;
					}
					float num9 = this.positionInFieldChunkOffsets[i];
					Vector3 b = zero * num9;
					Vector3 a;
					if (!flag)
					{
						a = this.positionInFieldPositions[i] + b;
					}
					else
					{
						float distanceToGround = this.GetDistanceToGround(worldCenterOfMass);
						if (distanceToGround > 100f)
						{
							a = this.positionInFieldPositions[i] + b;
						}
						else
						{
							a = worldCenterOfMass + zero * (this.hoverInFieldDistances[i] + num3 + num9 - distanceToGround);
							this.positionInFieldPositions[i] = worldCenterOfMass - b;
						}
					}
					Vector3 vector = a - worldCenterOfMass;
					vector = zero * Vector3.Dot(zero, vector);
					float magnitude = vector.magnitude;
					if ((double)magnitude > 0.1)
					{
						float num10 = Vector3.Dot(vector, zero);
						float num11 = num * this.chunkPositionErrorControlFactor;
						float num12 = num * 4f;
						float num13 = Mathf.Clamp(num11 * num10, -num12, num12);
						float num14 = Vector3.Dot(this.chunkRigidBody.velocity, zero);
						float num15 = num13 - num14;
						float num16 = 4f * num;
						float num17 = (!flag) ? this.chunkRigidBody.mass : this.totalMassModel;
						float num18 = 25f * num17 * num;
						float num19 = Mathf.Clamp(num17 * (num16 * num15), -num18, num18);
						this.chunkRigidBody.AddForce(zero * num19);
						float num20 = Mathf.Abs(num19 / num17);
						this.sfxLoopStrength += num20 / 20f;
					}
				}
				this.prevPositionInFieldChunkApplications[i] = num;
			}
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x0002BF6C File Offset: 0x0002A36C
		private void ApplyModelGravityForce()
		{
			if (this.modelAntigravityBlocks[0] != this)
			{
				return;
			}
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < this.modelAntigravityBlocks.Count; i++)
			{
				BlockAbstractAntiGravity blockAbstractAntiGravity = this.modelAntigravityBlocks[i];
				float a = blockAbstractAntiGravity.extraModelGravityMultiplier;
				num = Mathf.Min(a, num);
				num2 = Mathf.Max(a, num2);
			}
			float num3 = 0f;
			float num4 = Mathf.Abs(num);
			if (num4 > num2)
			{
				num3 = num;
			}
			else if (num4 < num2)
			{
				num3 = num2;
			}
			if (Mathf.Abs(num3) > 0.01f)
			{
				foreach (Rigidbody rigidbody in this.allRigidbodies)
				{
					if (!(rigidbody == null))
					{
						base.AddGravityForce(rigidbody, num3, rigidbody.mass);
						List<Block> list;
						if (this.varyingMassBlocksModel.Count > 0 && this.varyingMassBlocksModel.TryGetValue(rigidbody, out list))
						{
							float varyingMassOffset = base.GetVaryingMassOffset(list);
							base.AddGravityForce(rigidbody, num3, varyingMassOffset);
						}
					}
				}
			}
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x0002C0C0 File Offset: 0x0002A4C0
		private void ApplyChunkGravityForce()
		{
			if (Mathf.Abs(this.extraChunkGravityMultiplier) > 0.01f && this.chunkRigidBody != null)
			{
				base.AddGravityForce(this.chunkRigidBody, this.extraChunkGravityMultiplier, this.chunkRigidBody.mass);
				if (this.varyingMassBlocksChunk.Count > 0)
				{
					base.AddGravityForce(this.chunkRigidBody, this.extraChunkGravityMultiplier, base.GetVaryingMassOffset(this.varyingMassBlocksChunk));
				}
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0002C140 File Offset: 0x0002A540
		private void ApplyChunkTurnTowardsTag()
		{
			float num = Mathf.Abs(this.turnTowardsTagChunkApplications);
			if (num > 0f && this.turnTowardsTag.Length > 0)
			{
				Vector3 position = this.goT.position;
				if (this.chunkBlocksSet == null)
				{
					if (this.chunk.go != null)
					{
						this.chunkBlocksSet = new HashSet<Block>(this.chunk.blocks);
					}
					else
					{
						this.chunkBlocksSet = new HashSet<Block>();
					}
				}
				Block block;
				if (TagManager.TryGetClosestBlockWithTag(this.turnTowardsTag, position, out block, this.chunkBlocksSet))
				{
					Vector3 vector = block.goT.position - position;
					if (vector.sqrMagnitude > 0.0001f)
					{
						this.ApplyChunkAlignTorque(Mathf.Sign(this.turnTowardsTagChunkApplications) * vector.normalized, -(this.rotation * Vector3.forward), Mathf.Abs(this.turnTowardsTagChunkApplications), -1f);
					}
				}
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0002C248 File Offset: 0x0002A648
		private void ApplyChunkTurnAlongDirection()
		{
			if (this.turnAlongDirection.sqrMagnitude > 0.1f)
			{
				float magnitude = this.turnAlongDirection.magnitude;
				this.ApplyChunkAlignTorque(this.turnAlongDirection.normalized, -(this.rotation * Vector3.forward), magnitude, this.turnAlongMaxAngVel);
			}
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0002C2A3 File Offset: 0x0002A6A3
		public override void ResetFrame()
		{
			base.ResetFrame();
			this.ResetActions();
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0002C2B4 File Offset: 0x0002A6B4
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.ApplyModelGravityForce();
			if (this.chunk != null && this.chunk.mobileCharacter != null)
			{
				this.chunk.mobileCharacter.isHovering = (!this.didFix && this.targetVelocity.magnitude > 0f);
			}
			if (!this.didFix)
			{
				if (!this.broken)
				{
					this.ApplyChunkGravityForce();
					Vector3 vector = this.EARTH_GRAVITY;
					vector = this.alignRotation * vector;
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = -Vector3.up;
					}
					this.ApplyTiltAlignChunkTorque();
					this.ApplyChunkAlignTorque(vector, this.rotation * Vector3.up, this.alignInFieldChunkApplications, -1f);
					this.ApplyChunkTurnTowardsTag();
					this.ApplyChunkTurnAlongDirection();
					this.ApplyChunkPositionForces();
					this.ApplyChunkAngularVelocityTorque(this.targetAngVel);
					this.ApplyChunkExtraTorque(this.extraTorqueChunk);
					this.ApplyChunkVelocityForce(this.targetVelocity);
					this.currentDpad = Vector2.Lerp(Vector2.zero, this.currentDpad, 0.95f);
				}
			}
			this.isTrackingTiltAlign = this.applyTiltAlign;
			this.applyTiltAlign = false;
			base.UpdateSFXs();
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0002C3FB File Offset: 0x0002A7FB
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.chunkBlocksSet = null;
			this.currentDpad.Set(0f, 0f);
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0002C420 File Offset: 0x0002A820
		private void ResetActions()
		{
			this.extraModelGravityMultiplier = 0f;
			this.extraChunkGravityMultiplier = 0f;
			this.alignInFieldChunkApplications = 0f;
			this.positionInFieldChunkApplications.Set(0f, 0f, 0f);
			this.positionInFieldHover[0] = false;
			this.positionInFieldHover[1] = false;
			this.positionInFieldHover[2] = false;
			this.turnTowardsTagChunkApplications = 0f;
			this.turnAlongDirection.Set(0f, 0f, 0f);
			this.turnAlongMaxAngVel = -1f;
			this.targetVelocityApplications = 0f;
			this.targetAngVelApplications = 0f;
			this.targetAngVel.Set(0f, 0f, 0f);
			this.extraTorqueChunk.Set(0f, 0f, 0f);
			this.targetVelocity.Set(0f, 0f, 0f);
			this.alignRotation.Set(0f, 0f, 0f, 1f);
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x0002C538 File Offset: 0x0002A938
		public static HashSet<Predicate> GetInertiaPredicates()
		{
			if (BlockAbstractAntiGravity.inertiaPredicates == null)
			{
				BlockAbstractAntiGravity.inertiaPredicates = new HashSet<Predicate>
				{
					BlockFlightYoke.predicateBankTurn,
					BlockAntiGravity.predicateAntigravityBankTurn,
					BlockAntiGravityColumn.predicateAntigravityColumnBankTurn,
					BlockFlightYoke.predicateFlightSim,
					BlockFlightYoke.predicateTiltFlightSim
				};
			}
			return BlockAbstractAntiGravity.inertiaPredicates;
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0002C59C File Offset: 0x0002A99C
		public override void Play2()
		{
			base.Play2();
			this.ResetActions();
			this.prevPositionInFieldChunkApplications = Vector3.zero;
			this.positionInFieldChunkOffsets = Vector3.zero;
			this.positionInFieldChunkOffsetTargets = Vector3.zero;
			this.positionInFieldChunkOffsetIncrements = Vector3.one * Blocksworld.fixedDeltaTime;
			if (base.ContainsTileWithAnyPredicateInPlayMode2(BlockAbstractAntiGravity.GetInertiaPredicates()))
			{
				Transform goT = this.goT;
				this.inertiaBank = this.GetInertia(goT.TransformDirection(this.rotation * Vector3.forward));
				this.inertiaTurn = this.GetInertia(goT.TransformDirection(this.rotation * Vector3.up));
			}
			else
			{
				this.inertiaBank = 1f;
				this.inertiaTurn = 1f;
			}
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0002C664 File Offset: 0x0002AA64
		public TileResultCode TurnTowardsTagChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.turnTowardsTag = ((args.Length <= 0) ? string.Empty : ((string)args[0]));
			float num = (args.Length <= 1) ? 1f : ((float)args[1]);
			this.turnTowardsTagChunkApplications += eInfo.floatArg * num;
			return TileResultCode.True;
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0002C6C4 File Offset: 0x0002AAC4
		public TileResultCode AlignInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			this.alignInFieldChunkApplications += eInfo.floatArg * num;
			return TileResultCode.True;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0002C704 File Offset: 0x0002AB04
		public TileResultCode AlignTerrainChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			this.alignInFieldChunkApplications += eInfo.floatArg * num;
			Vector3 position = this.goT.position;
			if (this.GetClosestGroundHit(position, ref BlockAbstractAntiGravity.tempHit, 50f))
			{
				float num2 = 3f / Mathf.Max(BlockAbstractAntiGravity.tempHit.distance, 3f);
				this.alignRotation = Quaternion.FromToRotation(Vector3.up, num2 * BlockAbstractAntiGravity.tempHit.normal + (1f - num2) * Vector3.up);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0002C7B8 File Offset: 0x0002ABB8
		public TileResultCode AlignToTilt(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.chunkRigidBody == null || !TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.True;
			}
			if (!this.isTrackingTiltAlign)
			{
				bool flag = Util.GetIntArg(args, 0, 0) > 0;
				this.tiltAlignBaseAttitude = TiltManager.Instance.GetCurrentAttitude();
				if (flag)
				{
					Vector3 gravityVector = TiltManager.Instance.GetGravityVector();
					this.tiltAlignBaseAttitude *= Quaternion.FromToRotation(gravityVector, -Vector3.forward);
				}
				this.tiltAttitudeCorrect = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
				this.tiltAttitudeCorrect *= Quaternion.Inverse(this.goT.rotation);
				this.tiltAttitudeCorrect *= Quaternion.Inverse(this.rotation);
				this.tiltAlignBaseAttitude *= this.tiltAttitudeCorrect;
				this.tiltAlignBaseRotation = this.chunkRigidBody.rotation;
				this.tiltAlignTorque = (this.tiltAlignTorqueDelta = Vector3.zero);
				this.tiltAlignHeadingController.Reset();
				this.tiltAlignUpController.Reset();
				this.tiltAlignAngVelController.Reset();
			}
			Blocksworld.UI.Controls.UpdateTiltPrompt();
			this.isTrackingTiltAlign = true;
			this.applyTiltAlign = true;
			return TileResultCode.True;
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0002C90D File Offset: 0x0002AD0D
		private float GetGravityInfluence(object[] args, float arg1)
		{
			return arg1 * ((args.Length <= 0) ? 0f : ((float)args[0]));
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0002C92C File Offset: 0x0002AD2C
		public TileResultCode IncreaseModelGravityInfluence(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.extraModelGravityMultiplier += this.GetGravityInfluence(args, eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0002C949 File Offset: 0x0002AD49
		public TileResultCode IncreaseChunkGravityInfluence(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.extraChunkGravityMultiplier += this.GetGravityInfluence(args, eInfo.floatArg);
			return TileResultCode.True;
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x0002C968 File Offset: 0x0002AD68
		public TileResultCode IncreaseChunkGlobalAngularVelocity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 vector = (args.Length <= 0) ? Vector3.zero : ((Vector3)args[0]);
			float num = (args.Length <= 1) ? 1f : ((float)args[1]);
			vector *= eInfo.floatArg * num;
			this.targetAngVel += vector;
			this.targetAngVelApplications += eInfo.floatArg;
			return TileResultCode.True;
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x0002C9E4 File Offset: 0x0002ADE4
		public TileResultCode IncreaseLocalAngularVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 vector = (args.Length <= 0) ? Vector3.zero : ((Vector3)args[0]);
			float num = (args.Length <= 1) ? 2f : ((float)args[1]);
			vector *= eInfo.floatArg * num;
			Vector3 b = this.goT.TransformDirection(vector);
			this.targetAngVel += b;
			this.targetAngVelApplications += eInfo.floatArg;
			return TileResultCode.True;
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x0002CA6C File Offset: 0x0002AE6C
		public TileResultCode IncreaseLocalTorqueChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 vector = (args.Length <= 0) ? Vector3.zero : ((Vector3)args[0]);
			float num = (args.Length <= 1) ? 2f : ((float)args[1]);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset("L");
			vector *= eInfo.floatArg * num;
			this.IncreaseLocalTorque(vector);
			return TileResultCode.True;
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x0002CAE4 File Offset: 0x0002AEE4
		private void IncreaseLocalTorque(Vector3 localTorqueInc)
		{
			Vector3 b = this.goT.TransformDirection(localTorqueInc);
			this.extraTorqueChunk += b;
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x0002CB10 File Offset: 0x0002AF10
		public TileResultCode IncreaseLocalVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 vector = (args.Length <= 0) ? Vector3.zero : ((Vector3)args[0]);
			float num = (args.Length <= 1) ? 4f : ((float)args[1]);
			vector *= eInfo.floatArg * num;
			Vector3 b = this.goT.TransformDirection(vector);
			this.targetVelocity += b;
			this.targetVelocityApplications += eInfo.floatArg;
			return TileResultCode.True;
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x0002CB98 File Offset: 0x0002AF98
		public TileResultCode DPadIncreaseTorqueChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			float num = (args.Length <= 1) ? 3f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			Vector3 b = num * eInfo.floatArg * Vector3.Cross(Vector3.up, worldDPadOffset);
			this.extraTorqueChunk += b;
			return TileResultCode.True;
		}

		// Token: 0x06000683 RID: 1667 RVA: 0x0002CC44 File Offset: 0x0002B044
		public TileResultCode DPadIncreaseVelocityChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			float d = (args.Length <= 1) ? 4f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			Vector3 b = d * worldDPadOffset * eInfo.floatArg;
			this.targetVelocity += b;
			this.targetVelocityApplications += worldDPadOffset.magnitude;
			return TileResultCode.True;
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0002CCFC File Offset: 0x0002B0FC
		public TileResultCode AlignAlongDPadChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			float floatArg = Util.GetFloatArg(args, 1, -1f);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			this.turnAlongDirection += worldDPadOffset;
			if (floatArg > 0f)
			{
				if (this.turnAlongMaxAngVel <= 0f)
				{
					this.turnAlongMaxAngVel = floatArg;
				}
				else
				{
					this.turnAlongMaxAngVel += floatArg;
				}
				float num = Mathf.Max(0f, (this.turnAlongMaxAngVel - 5f) / 5f);
				if (num > 0f)
				{
					this.turnAlongDirection += worldDPadOffset * num;
				}
			}
			return TileResultCode.True;
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0002CDF4 File Offset: 0x0002B1F4
		protected override void HandleTiltMover(float xTilt, float yTilt, float zTilt)
		{
			Vector3 cameraUp = Blocksworld.cameraUp;
			Vector3 cameraForward = Blocksworld.cameraForward;
			Vector3 cameraRight = Blocksworld.cameraRight;
			Vector3 forward = this.goT.forward;
			Vector3 forward2 = this.goT.forward;
			Quaternion rotation = Quaternion.AngleAxis(xTilt * 90f, Vector3.up);
			Quaternion rotation2 = Quaternion.AngleAxis(yTilt * 90f, Blocksworld.cameraRight);
			Vector3 b = rotation * (rotation2 * Blocksworld.cameraForward);
			this.turnAlongDirection += b;
			this.bankAngle = -zTilt * 30f;
			this.alignInFieldChunkApplications = 1f;
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0002CE9C File Offset: 0x0002B29C
		public TileResultCode BankTurnChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			float floatArg = Util.GetFloatArg(args, 1, -1f);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			Vector3 worldDPadOffset = Blocksworld.UI.Controls.GetWorldDPadOffset(key);
			Transform goT = this.goT;
			Vector3 vec = goT.TransformDirection(this.rotation * Vector3.forward);
			Vector3 normalized = Util.ProjectOntoPlane(vec, Vector3.up).normalized;
			Vector3 normalized2 = Util.ProjectOntoPlane(Vector3.RotateTowards(normalized, worldDPadOffset, 1f, float.MaxValue), Vector3.up).normalized;
			this.turnAlongDirection += normalized2 * this.inertiaTurn;
			if (floatArg > 0f)
			{
				if (this.turnAlongMaxAngVel <= 0f)
				{
					this.turnAlongMaxAngVel = floatArg;
				}
				else
				{
					this.turnAlongMaxAngVel += floatArg;
				}
				float num = Mathf.Max(0f, (this.turnAlongMaxAngVel - 5f) / 5f);
				if (num > 0f)
				{
					this.turnAlongDirection += normalized2 * num * this.inertiaTurn;
				}
			}
			float num2 = 0.5f * (1f - Vector3.Dot(worldDPadOffset, normalized));
			num2 *= Mathf.Sign(Vector3.Cross(worldDPadOffset, normalized).y);
			if (this.chunkRigidBody != null && worldDPadOffset.sqrMagnitude > 0f)
			{
				this.bankAngle = num2 * 80f;
				this.bankAngle = Mathf.Clamp(this.bankAngle, -50f, 50f);
			}
			else
			{
				this.bankAngle = 0f;
			}
			this.alignRotation = Quaternion.AngleAxis(this.bankAngle, normalized);
			this.alignInFieldChunkApplications += this.inertiaBank * 0.15f;
			return TileResultCode.True;
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0002D0C4 File Offset: 0x0002B4C4
		public TileResultCode FlightSimChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string key = (args.Length <= 0) ? "L" : ((string)args[0]);
			float num = (args.Length <= 1) ? 2f : ((float)args[1]);
			Blocksworld.UI.Controls.EnableDPad(key, MoverDirectionMask.ALL);
			this.currentDpad = Blocksworld.UI.Controls.GetNormalizedDPadOffset(key);
			if (this.currentDpad.magnitude > 0.01f)
			{
				float num2 = eInfo.floatArg * num;
				float num3 = this.currentDpad.x * num2;
				float num4 = this.currentDpad.y * num2;
				Vector3 localTorqueInc = new Vector3(-num4, num3, num3);
				this.IncreaseLocalTorque(localTorqueInc);
			}
			return TileResultCode.True;
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x0002D184 File Offset: 0x0002B584
		public TileResultCode TiltFlightSimChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			if (this.chunkRigidBody == null || !TiltManager.Instance.IsMonitoring())
			{
				return TileResultCode.True;
			}
			float floatArg = Util.GetFloatArg(args, 1, 2f);
			Vector3 relativeGravityVector = TiltManager.Instance.GetRelativeGravityVector();
			float num = floatArg * TiltManager.Instance.GetTiltTwist();
			float num2 = -2f * floatArg * relativeGravityVector.y;
			float num3 = 2f * floatArg * relativeGravityVector.x + num;
			this.tiltAlignBaseRotation *= Quaternion.AngleAxis(num3 * Time.fixedDeltaTime, Vector3.up);
			return this.AlignToTilt(eInfo, args);
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000689 RID: 1673 RVA: 0x0002D22A File Offset: 0x0002B62A
		public Vector3 EARTH_GRAVITY
		{
			get
			{
				return -9.82f * Vector3.up;
			}
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0002D23C File Offset: 0x0002B63C
		private float GetInertia(Vector3 axis)
		{
			float num = 0f;
			List<Block> list = Block.connectedCache[this];
			for (int i = 0; i < list.Count; i++)
			{
				Block block = list[i];
				num += block.GetMomentOfInertia(block.goT.position, axis, false);
			}
			return Mathf.Clamp(num, 1f, 10000f);
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0002D2A4 File Offset: 0x0002B6A4
		public TileResultCode IncreasePositionInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int num = (args.Length <= 0) ? 1 : ((int)args[0]);
			float num2 = (args.Length <= 1) ? 1f : ((float)args[1]);
			float num3 = (args.Length <= 2) ? 1f : ((float)args[2]);
			float num4 = (args.Length <= 3) ? 1f : ((float)args[3]);
			ref Vector3 ptr = ref this.positionInFieldChunkApplications;
			int index;
			this.positionInFieldChunkApplications[index = num] = ptr[index] + num2;
			if (eInfo.timer < 0.001f)
			{
				ptr = ref this.positionInFieldChunkOffsetTargets;
				int index2;
				this.positionInFieldChunkOffsetTargets[index2 = num] = ptr[index2] + num3 * num4;
				this.positionInFieldChunkOffsetIncrements[num] = Mathf.Max(Mathf.Abs(num3), this.positionInFieldChunkOffsetIncrements[num]) * Blocksworld.fixedDeltaTime;
			}
			if (eInfo.timer >= num4)
			{
				this.positionInFieldChunkOffsetIncrements[num] = Blocksworld.fixedDeltaTime;
				return TileResultCode.True;
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x0002D3B4 File Offset: 0x0002B7B4
		private void PositionInGravityFieldChunk(object[] args, float arg1, int index)
		{
			float num = (args.Length <= 0) ? 1f : ((float)args[0]);
			ref Vector3 ptr = ref this.positionInFieldChunkApplications;
			this.positionInFieldChunkApplications[index] = ptr[index] + arg1 * num;
			this.positionInFieldHover[index] = false;
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0002D401 File Offset: 0x0002B801
		public TileResultCode PositionInGravityFieldXChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.PositionInGravityFieldChunk(args, eInfo.floatArg, 0);
			return TileResultCode.True;
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0002D412 File Offset: 0x0002B812
		public TileResultCode PositionInGravityFieldYChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.PositionInGravityFieldChunk(args, eInfo.floatArg, 1);
			return TileResultCode.True;
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0002D424 File Offset: 0x0002B824
		public TileResultCode HoverInGravityFieldChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.positionInFieldHover[1] = true;
			ref Vector3 ptr = ref this.positionInFieldChunkApplications;
			this.positionInFieldChunkApplications[1] = ptr[1] + 1.25f;
			this.hoverInFieldDistances[1] = Util.GetFloatArg(args, 1, 1f) * eInfo.floatArg;
			return TileResultCode.True;
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0002D475 File Offset: 0x0002B875
		public TileResultCode PositionInGravityFieldZChunk(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.PositionInGravityFieldChunk(args, eInfo.floatArg, 2);
			return TileResultCode.True;
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x0002D488 File Offset: 0x0002B888
		public override bool TreatAsVehicleLikeBlock()
		{
			return base.TreatAsVehicleLikeBlockWithStatus(ref this.treatAsVehicleStatus);
		}

		// Token: 0x0400049A RID: 1178
		public static Predicate predicateTiltMover;

		// Token: 0x0400049B RID: 1179
		public static Predicate predicateAlignToTilt;

		// Token: 0x0400049C RID: 1180
		protected float extraModelGravityMultiplier;

		// Token: 0x0400049D RID: 1181
		private float extraChunkGravityMultiplier;

		// Token: 0x0400049E RID: 1182
		protected float alignInFieldChunkApplications;

		// Token: 0x0400049F RID: 1183
		protected float alignToTiltChunkApplications;

		// Token: 0x040004A0 RID: 1184
		private Quaternion tiltAttitudeCorrect = Quaternion.identity;

		// Token: 0x040004A1 RID: 1185
		private Quaternion tiltAlignBaseAttitude;

		// Token: 0x040004A2 RID: 1186
		private Quaternion tiltAlignBaseRotation;

		// Token: 0x040004A3 RID: 1187
		private Vector3 tiltAlignTorque;

		// Token: 0x040004A4 RID: 1188
		private Vector3 tiltAlignTorqueDelta;

		// Token: 0x040004A5 RID: 1189
		private bool applyTiltAlign;

		// Token: 0x040004A6 RID: 1190
		private bool isTrackingTiltAlign;

		// Token: 0x040004A7 RID: 1191
		private PDControllerVector3 tiltAlignHeadingController;

		// Token: 0x040004A8 RID: 1192
		private PDControllerVector3 tiltAlignUpController;

		// Token: 0x040004A9 RID: 1193
		private PDControllerVector3 tiltAlignAngVelController;

		// Token: 0x040004AA RID: 1194
		protected Vector3 positionInFieldChunkApplications = Vector3.zero;

		// Token: 0x040004AB RID: 1195
		private Vector3 prevPositionInFieldChunkApplications = Vector3.zero;

		// Token: 0x040004AC RID: 1196
		protected float chunkPositionErrorControlFactor = 1f;

		// Token: 0x040004AD RID: 1197
		private Vector3 positionInFieldChunkOffsets = Vector3.zero;

		// Token: 0x040004AE RID: 1198
		private Vector3 positionInFieldChunkOffsetTargets = Vector3.zero;

		// Token: 0x040004AF RID: 1199
		private Vector3 positionInFieldChunkOffsetIncrements = Vector3.one;

		// Token: 0x040004B0 RID: 1200
		private float turnTowardsTagChunkApplications;

		// Token: 0x040004B1 RID: 1201
		private string turnTowardsTag = string.Empty;

		// Token: 0x040004B2 RID: 1202
		protected Vector3 turnAlongDirection = Vector3.zero;

		// Token: 0x040004B3 RID: 1203
		protected float turnAlongMaxAngVel = -1f;

		// Token: 0x040004B4 RID: 1204
		protected Vector3 targetAngVel = Vector3.zero;

		// Token: 0x040004B5 RID: 1205
		protected float targetAngVelApplications;

		// Token: 0x040004B6 RID: 1206
		private Vector3 targetVelocity = Vector3.zero;

		// Token: 0x040004B7 RID: 1207
		private float targetVelocityApplications;

		// Token: 0x040004B8 RID: 1208
		private Vector3 extraTorqueChunk = Vector3.zero;

		// Token: 0x040004B9 RID: 1209
		private Vector3[] positionInFieldPositions = new Vector3[]
		{
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		};

		// Token: 0x040004BA RID: 1210
		private bool[] positionInFieldHover = new bool[3];

		// Token: 0x040004BB RID: 1211
		private Vector3 hoverInFieldDistances = Vector3.zero;

		// Token: 0x040004BC RID: 1212
		private Vector3 hoverInFieldDistanceOffsets = Util.nullVector3;

		// Token: 0x040004BD RID: 1213
		private Quaternion alignRotation = Quaternion.identity;

		// Token: 0x040004BE RID: 1214
		private float inertiaBank = 1f;

		// Token: 0x040004BF RID: 1215
		private float inertiaTurn = 1f;

		// Token: 0x040004C0 RID: 1216
		private float bankAngle;

		// Token: 0x040004C1 RID: 1217
		protected Vector2 currentDpad = Vector2.zero;

		// Token: 0x040004C2 RID: 1218
		private int treatAsVehicleStatus = -1;

		// Token: 0x040004C3 RID: 1219
		private AntigravityMetaData metaData;

		// Token: 0x040004C4 RID: 1220
		private List<BlockAbstractAntiGravity> modelAntigravityBlocks = new List<BlockAbstractAntiGravity>();

		// Token: 0x040004C5 RID: 1221
		private static RaycastHit tempHit = default(RaycastHit);

		// Token: 0x040004C6 RID: 1222
		private static HashSet<Predicate> inertiaPredicates = null;

		// Token: 0x040004C7 RID: 1223
		private const float BANK_ANGLE_FACTOR = 80f;

		// Token: 0x040004C8 RID: 1224
		private const float BANK_ANGLE_LIMIT = 50f;

		// Token: 0x040004C9 RID: 1225
		private const float ALIGN_PER_INERTIA_FACTOR = 0.15f;

		// Token: 0x040004CA RID: 1226
		private const float MINIMUM_TOLERANCE = 0.01f;

		// Token: 0x040004CB RID: 1227
		private const float TOTAL_INERTIA_MIN_VALUE = 1f;

		// Token: 0x040004CC RID: 1228
		private const float TOTAL_INERTIA_MAX_VALUE = 10000f;
	}
}
