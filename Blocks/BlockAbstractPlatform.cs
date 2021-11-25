using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000067 RID: 103
	public class BlockAbstractPlatform : BlockAbstractHover
	{
		// Token: 0x06000864 RID: 2148 RVA: 0x00039204 File Offset: 0x00037604
		public BlockAbstractPlatform(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x000392B0 File Offset: 0x000376B0
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

		// Token: 0x06000866 RID: 2150 RVA: 0x00039315 File Offset: 0x00037715
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.enabled = true;
			this.origChunkMasses.Clear();
			this.origChunkTensors.Clear();
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x0003933B File Offset: 0x0003773B
		public override void Play()
		{
			base.Play();
			this.enabled = true;
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0003934C File Offset: 0x0003774C
		public override void Play2()
		{
			base.Play2();
			this.forwardDirection = this.goT.forward;
			this.upDirection = this.goT.up;
			this.rightDirection = this.goT.right;
			this.chunkRigidBody = this.chunk.rb;
			if (this.chunkRigidBody != null)
			{
				this.forwardMassDistribution = base.CalculateMassDistribution(this.chunk, this.forwardDirection, null);
				this.rightMassDistribution = base.CalculateMassDistribution(this.chunk, this.rightDirection, null);
				this.upMassDistribution = base.CalculateMassDistribution(this.chunk, this.upDirection, null);
				this.targetPosition = this.GetPlatformPosition();
			}
			else
			{
				this.rightMassDistribution = 1f;
				this.forwardMassDistribution = 1f;
				this.upMassDistribution = 1f;
			}
			if (this.enabled)
			{
				base.UpdateConnectedCache();
				List<Block> list = Block.connectedCache[this];
				foreach (Block block in list)
				{
					if (block != this && block is BlockAbstractPlatform)
					{
						BlockAbstractPlatform blockAbstractPlatform = (BlockAbstractPlatform)block;
						blockAbstractPlatform.enabled = false;
					}
				}
			}
			this.massMultiplier = 1f;
			this.prevMassMultiplier = 1f;
			this.tensorMultiplier = 1f;
			this.prevTensorMultiplier = 1f;
			this.origChunkMasses = new Dictionary<Chunk, float>();
			this.origChunkTensors = new Dictionary<Chunk, Vector3>();
			base.UpdateConnectedCache();
			HashSet<Chunk> hashSet = Block.connectedChunks[this];
			foreach (Chunk chunk in hashSet)
			{
				GameObject go = chunk.go;
				if (go != null && go.GetComponent<Rigidbody>() != null)
				{
					this.origChunkMasses[this.chunk] = go.GetComponent<Rigidbody>().mass;
					this.origChunkTensors[this.chunk] = go.GetComponent<Rigidbody>().inertiaTensor;
				}
			}
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x000395AC File Offset: 0x000379AC
		protected virtual Vector3 GetPlatformPosition()
		{
			return this.goT.position;
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x000395BC File Offset: 0x000379BC
		private void ApplyModelGravityForce()
		{
			float gravityMultiplier = -1f;
			foreach (Rigidbody rigidbody in this.allRigidbodies)
			{
				if (!(rigidbody == null))
				{
					base.AddGravityForce(rigidbody, gravityMultiplier, rigidbody.mass);
					List<Block> list;
					if (this.varyingMassBlocksModel.Count > 0 && this.varyingMassBlocksModel.TryGetValue(rigidbody, out list))
					{
						float varyingMassOffset = base.GetVaryingMassOffset(list);
						base.AddGravityForce(rigidbody, gravityMultiplier, varyingMassOffset);
					}
				}
			}
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00039670 File Offset: 0x00037A70
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.enabled)
			{
				if (!this.controlsVelocity)
				{
					this.ApplyModelGravityForce();
					this.ApplyChunkPositionForces();
					this.AlignPlatform();
				}
				if (this.prevMassMultiplier != this.massMultiplier)
				{
					foreach (KeyValuePair<Chunk, float> keyValuePair in this.origChunkMasses)
					{
						Chunk key = keyValuePair.Key;
						GameObject go = key.go;
						if (go != null && go.GetComponent<Rigidbody>() != null)
						{
							go.GetComponent<Rigidbody>().mass = keyValuePair.Value * this.massMultiplier;
						}
					}
				}
				if (this.prevTensorMultiplier != this.tensorMultiplier)
				{
					foreach (KeyValuePair<Chunk, Vector3> keyValuePair2 in this.origChunkTensors)
					{
						Chunk key2 = keyValuePair2.Key;
						GameObject go2 = key2.go;
						if (go2 != null && go2.GetComponent<Rigidbody>() != null)
						{
							try
							{
								go2.GetComponent<Rigidbody>().inertiaTensor = keyValuePair2.Value * this.tensorMultiplier;
							}
							catch
							{
								BWLog.Info("Unable to set inertia tensor, possibly due to the use of rigidbody constraints in the world.");
							}
						}
					}
				}
				this.prevMassMultiplier = this.massMultiplier;
				this.prevTensorMultiplier = this.tensorMultiplier;
				this.tensorMultiplier = 1f;
				this.massMultiplier = 1f;
			}
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x00039840 File Offset: 0x00037C40
		protected virtual Vector3 GetTargetPositionOffset()
		{
			return Vector3.zero;
		}

		// Token: 0x0600086D RID: 2157 RVA: 0x00039848 File Offset: 0x00037C48
		private void ApplyChunkPositionForces()
		{
			if (this.chunkRigidBody != null)
			{
				Vector3 a = this.targetPosition + this.GetTargetPositionOffset();
				Vector3 a2 = a - this.GetPlatformPosition();
				float magnitude = a2.magnitude;
				if ((double)magnitude > 1E-05)
				{
					Vector3 a3 = a2 * 20f;
					Vector3 velocity = this.chunkRigidBody.velocity;
					Vector3 a4 = a3 - velocity;
					if (a4.sqrMagnitude > 400f)
					{
						a4 = a4.normalized * 20f;
					}
					float mass = this.chunkRigidBody.mass;
					float d = 20f;
					Vector3 force = a4 * mass * d;
					this.chunkRigidBody.AddForce(force);
				}
			}
		}

		// Token: 0x0600086E RID: 2158 RVA: 0x00039917 File Offset: 0x00037D17
		protected virtual Quaternion GetRotationOffset()
		{
			return Quaternion.identity;
		}

		// Token: 0x0600086F RID: 2159 RVA: 0x00039920 File Offset: 0x00037D20
		protected virtual void AlignPlatform()
		{
			Quaternion rotation = this.GetRotationOffset() * this.rotation;
			this.Align(this.upDirection, rotation * Vector3.up);
			this.Align(this.forwardDirection, rotation * Vector3.forward);
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x00039970 File Offset: 0x00037D70
		private float GetTorqueScaler(Vector3 normalizedTorque)
		{
			return Mathf.Abs(Vector3.Dot(normalizedTorque, this.forwardMassDistribution * this.goT.forward)) + Mathf.Abs(Vector3.Dot(normalizedTorque, this.rightMassDistribution * this.goT.right)) + Mathf.Abs(Vector3.Dot(normalizedTorque, this.upMassDistribution * this.goT.up));
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x000399E4 File Offset: 0x00037DE4
		protected virtual void Align(Vector3 target, Vector3 localUp)
		{
			if (this.chunkRigidBody != null)
			{
				Vector3 vector = this.goT.TransformDirection(localUp);
				float a = Vector3.Angle(vector, target);
				Vector3 vector2 = Vector3.Cross(vector, target);
				if (vector2.sqrMagnitude > 0.001f)
				{
					vector2 = vector2.normalized;
				}
				else
				{
					vector2 = this.goT.forward;
				}
				float torqueScaler = this.GetTorqueScaler(vector2);
				float num = 0.7f;
				Vector3 vector3 = num * Mathf.Min(a, 90f) * torqueScaler * vector2;
				Vector3 vector4 = this.chunkRigidBody.angularVelocity;
				vector4 = Util.ProjectOntoPlane(vector4, target.normalized);
				float magnitude = vector4.magnitude;
				if (magnitude > 0.001f)
				{
					Vector3 normalizedTorque = vector4 / magnitude;
					float torqueScaler2 = this.GetTorqueScaler(normalizedTorque);
					float num2 = 1f;
					float num3 = magnitude * num2 * torqueScaler2;
					vector3 += -num3 * vector4;
				}
				this.chunkRigidBody.AddTorque(vector3);
			}
		}

		// Token: 0x04000664 RID: 1636
		protected Vector3 targetPosition = Vector3.zero;

		// Token: 0x04000665 RID: 1637
		protected Vector3 rightDirection = Vector3.right;

		// Token: 0x04000666 RID: 1638
		protected Vector3 forwardDirection = Vector3.forward;

		// Token: 0x04000667 RID: 1639
		protected Vector3 upDirection = Vector3.up;

		// Token: 0x04000668 RID: 1640
		private float rightMassDistribution = 1f;

		// Token: 0x04000669 RID: 1641
		private float forwardMassDistribution = 1f;

		// Token: 0x0400066A RID: 1642
		private float upMassDistribution = 1f;

		// Token: 0x0400066B RID: 1643
		protected float massMultiplier = 1f;

		// Token: 0x0400066C RID: 1644
		protected float prevMassMultiplier = 1f;

		// Token: 0x0400066D RID: 1645
		protected float tensorMultiplier = 1f;

		// Token: 0x0400066E RID: 1646
		protected float prevTensorMultiplier = 1f;

		// Token: 0x0400066F RID: 1647
		protected Dictionary<Chunk, float> origChunkMasses = new Dictionary<Chunk, float>();

		// Token: 0x04000670 RID: 1648
		protected Dictionary<Chunk, Vector3> origChunkTensors = new Dictionary<Chunk, Vector3>();

		// Token: 0x04000671 RID: 1649
		private const float MAX_VEL_ERROR = 20f;

		// Token: 0x04000672 RID: 1650
		private AntigravityMetaData metaData;

		// Token: 0x04000673 RID: 1651
		protected bool enabled = true;

		// Token: 0x04000674 RID: 1652
		protected bool controlsVelocity;
	}
}
