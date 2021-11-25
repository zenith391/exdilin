using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000C7 RID: 199
	public class BlockRotatingPlatform : BlockAbstractRotatingPlatform, ITreasureAnimationDriver
	{
		// Token: 0x06000F1C RID: 3868 RVA: 0x00065C80 File Offset: 0x00064080
		public BlockRotatingPlatform(List<List<Tile>> tiles) : base(tiles)
		{
			this.controlsVelocity = true;
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x00065CB4 File Offset: 0x000640B4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.AtAngle", (Block b) => new PredicateSensorDelegate(((BlockRotatingPlatform)b).AtAngle), null, new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Angle"
			}, null);
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.IncreaseAngle", null, (Block b) => new PredicateActionDelegate(((BlockRotatingPlatform)b).IncreaseAngleDurational), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Angles",
				"Duration"
			}, null);
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.IncreaseAngleNonDurational", null, (Block b) => new PredicateActionDelegate(((BlockRotatingPlatform)b).IncreaseAngleNonDurational), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Velocity"
			}, null);
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.ReturnAngle", null, (Block b) => new PredicateActionDelegate(((BlockRotatingPlatform)b).ReturnAngleDurational), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Angles",
				"Duration"
			}, null);
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.ReturnAngleNonDurational", null, (Block b) => new PredicateActionDelegate(((BlockRotatingPlatform)b).ReturnAngleNonDurational), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Speed"
			}, null);
			PredicateRegistry.Add<BlockRotatingPlatform>("RotatingPlatform.FreeSpin", null, (Block b) => new PredicateActionDelegate(((BlockRotatingPlatform)b).FreeSpin), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Mass Multiplier"
			}, null);
			Block.AddSimpleDefaultTiles(new GAF("RotatingPlatform.IncreaseAngleNonDurational", new object[]
			{
				45f
			}), new string[]
			{
				"Rotating Platform"
			});
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x00065EDD File Offset: 0x000642DD
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.AlignPlatform();
			this.spinFree = false;
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x00065EF2 File Offset: 0x000642F2
		private RigidbodyConstraints GetConstraint(Vector3 v)
		{
			if (Mathf.Abs(Vector3.Dot(v, Vector3.up)) > 0.01f)
			{
				return RigidbodyConstraints.FreezeRotationY;
			}
			if (Mathf.Abs(Vector3.Dot(v, Vector3.right)) > 0.01f)
			{
				return RigidbodyConstraints.FreezeRotationX;
			}
			return RigidbodyConstraints.FreezeRotationZ;
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x00065F30 File Offset: 0x00064330
		private void SetConstraints()
		{
			if (this.chunkRigidBody != null && !this.broken)
			{
				this.goT.localPosition += Vector3.right * 0.05f;
				RigidbodyConstraints constraints = RigidbodyConstraints.FreezePosition | this.GetConstraint(this.upDirection) | this.GetConstraint(this.forwardDirection);
				this.chunkRigidBody.constraints = constraints;
			}
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00065FA8 File Offset: 0x000643A8
		public override void Play2()
		{
			base.Play2();
			this.movedCm = false;
			if (!this.modelBlock.ContainsTileWithPredicate(Block.predicateFreeze) || base.ContainsTileWithPredicate(Block.predicateFreeze))
			{
				this.SetConstraints();
			}
			this.errorSum = 0f;
			this.origRight = this.goT.right;
			this.origPos = this.goT.position;
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x0006601C File Offset: 0x0006441C
		protected void MoveCM()
		{
			Vector3 worldCenterOfMass = this.chunkRigidBody.worldCenterOfMass;
			Vector3 position = this.goT.position;
			Vector3 b = position - worldCenterOfMass;
			Vector3 centerOfMass = this.chunkRigidBody.centerOfMass + b;
			this.chunkRigidBody.centerOfMass = centerOfMass;
			this.chunkRigidBody.inertiaTensorRotation = this.chunkRigidBody.rotation;
			this.chunkCM = centerOfMass;
			this.movedCm = true;
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x0006608C File Offset: 0x0006448C
		protected override void AlignPlatform()
		{
			if (!this.enabled || this.broken)
			{
				return;
			}
			this.colliding = false;
			Vector3 up = this.goT.up;
			if (this.chunkRigidBody != null && !this.chunkRigidBody.isKinematic)
			{
				if (!this.movedCm)
				{
					this.MoveCM();
				}
				if (this.spinFree)
				{
					this.targetAngle = -Util.AngleBetween(up, this.upDirection, this.goT.right);
					this.targetSteps = (float)base.GetSteps(this.targetAngle);
					this.lastErrorAngle = 0f;
				}
				else
				{
					Quaternion rotation = Quaternion.AngleAxis(this.targetAngle, this.origRight);
					Vector3 vector = rotation * this.upDirection;
					float value = Vector3.Angle(up, vector);
					float num = Mathf.Clamp(value, -20f, 20f);
					Vector3 a = Vector3.Cross(up, vector);
					if (a.sqrMagnitude > 1E-07f)
					{
						a = a.normalized;
					}
					else
					{
						a = Vector3.zero;
					}
					float num2 = 0.25f;
					float num3 = 0.002f;
					float num4 = Mathf.Abs(num);
					float num5 = this.errorSum;
					if (num4 < 1f)
					{
						this.errorSum *= num4;
					}
					else if (num4 < 3f)
					{
						this.errorSum = Mathf.Clamp(this.errorSum + num, -100f, 100f);
					}
					else
					{
						this.errorSum *= 3f / num4;
					}
					Vector3 vector2 = a * (num2 * num + num3 * this.errorSum);
					float a2 = Mathf.Abs(this.targetAngle - this.prevTargetAngle);
					float num6 = Mathf.Max(a2, 10f);
					if (num4 > num6 && CollisionManager.bumpedObject.Overlaps(this.chunk.blocks))
					{
						this.chunkRigidBody.angularVelocity = vector2 * 0.1f;
						this.targetAngle = this.prevTargetAngle;
						this.targetSteps = this.prevTargetSteps;
						this.errorSum = num5;
						this.colliding = true;
					}
					else
					{
						this.chunkRigidBody.angularVelocity = vector2;
					}
					this.lastErrorAngle = num;
				}
			}
			this.prevTargetAngle = this.targetAngle;
			this.prevTargetSteps = this.targetSteps;
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x000662FC File Offset: 0x000646FC
		public Vector3 GetTreasurePositionOffset(TreasureHandler.TreasureState state)
		{
			Vector3 position = state.transform.position;
			Vector3 a = this.goT.position - position;
			return -a;
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x0006632D File Offset: 0x0006472D
		public Quaternion GetTreasureRotation(TreasureHandler.TreasureState state)
		{
			return Quaternion.AngleAxis(this.targetAngle, this.origRight);
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x00066340 File Offset: 0x00064740
		public bool TreasureAnimationActivated()
		{
			return this.enabled;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x00066348 File Offset: 0x00064748
		protected override void Vanishing(float scale)
		{
			base.Vanishing(scale);
			this.SetChunkPose(scale);
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x00066358 File Offset: 0x00064758
		protected override void Appearing(float scale)
		{
			base.Appearing(scale);
			this.SetChunkPose(scale);
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x00066368 File Offset: 0x00064768
		protected void SetChunkPose(float scale = 1f)
		{
			if (this.enabled && !this.didFix && !this.isTreasure && !this.broken)
			{
				Quaternion rotation = Quaternion.AngleAxis(this.targetAngle, this.origRight);
				Transform transform = this.chunk.go.transform;
				transform.rotation = rotation;
				Vector3 b = this.origPos - this.goT.position;
				transform.position += b;
			}
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x000663F4 File Offset: 0x000647F4
		public override void Appeared()
		{
			base.Appeared();
			if (this.chunkRigidBody != null)
			{
				this.chunkRigidBody.centerOfMass = this.chunkCM;
			}
			this.SetChunkPose(1f);
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x00066429 File Offset: 0x00064829
		protected override void ModelBlockAppeared()
		{
			this.SetChunkPose(1f);
			if (this.chunkRigidBody != null)
			{
				this.chunkRigidBody.centerOfMass = this.chunkCM;
			}
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x00066458 File Offset: 0x00064858
		protected override void ModelBlockAppearing(float scale)
		{
			this.SetChunkPose(scale);
		}

		// Token: 0x06000F2D RID: 3885 RVA: 0x00066461 File Offset: 0x00064861
		protected override void ModelBlockVanishing(float scale)
		{
			this.SetChunkPose(scale);
		}

		// Token: 0x06000F2E RID: 3886 RVA: 0x0006646A File Offset: 0x0006486A
		public override void ChunkInModelFrozen()
		{
			base.ChunkInModelFrozen();
			this.enabled = false;
		}

		// Token: 0x06000F2F RID: 3887 RVA: 0x0006647C File Offset: 0x0006487C
		public override void ChunkInModelUnfrozen()
		{
			base.ChunkInModelUnfrozen();
			foreach (Block block in Block.connectedCache[this])
			{
				if (block.didFix)
				{
					return;
				}
			}
			this.enabled = true;
		}

		// Token: 0x04000BBC RID: 3004
		private bool movedCm;

		// Token: 0x04000BBD RID: 3005
		private Vector3 chunkCM = Vector3.zero;

		// Token: 0x04000BBE RID: 3006
		private float errorSum;

		// Token: 0x04000BBF RID: 3007
		private Vector3 origRight = Vector3.right;

		// Token: 0x04000BC0 RID: 3008
		private Vector3 origPos = Vector3.zero;
	}
}
