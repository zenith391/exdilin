using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000087 RID: 135
	public class BlockDriveAssist : BlockAbstractAntiGravity
	{
		// Token: 0x06000B74 RID: 2932 RVA: 0x00052C4C File Offset: 0x0005104C
		public BlockDriveAssist(List<List<Tile>> tiles) : base(tiles)
		{
			this.playLoop = false;
			this.informAboutVaryingGravity = false;
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x00052CB0 File Offset: 0x000510B0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.Assist", null, (Block b) => new PredicateActionDelegate(((BlockDriveAssist)b).Assist), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockDriveAssist>("DriveAssist.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x00052E8C File Offset: 0x0005128C
		public override void Play()
		{
			base.Play();
			this.assistApplications = 0f;
			this.appliedAssistApplications = 0f;
			this.cmOffset = Mathf.Abs(Vector3.Dot(Util.ComputeBounds(this.chunk.blocks).extents, this.go.transform.up));
			this.newColliders.Clear();
			this.oldColliders.Clear();
			DriveAssistMetaData component = this.go.GetComponent<DriveAssistMetaData>();
			if (component != null)
			{
				this.cmOffset = component.centerMassOffsetMultplier;
				this.dynamicFriction = component.dynamicFriction;
				this.staticFriction = component.staticFriction;
			}
			else
			{
				BWLog.Info("Could not find drive assist meta data component");
			}
			this.ComputeAlignScaler();
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x00052F54 File Offset: 0x00051354
		private void ComputeAlignScaler()
		{
			Chunk chunk = this.chunk;
			Rigidbody component = chunk.go.GetComponent<Rigidbody>();
			float num = (!(component != null) || component.isKinematic) ? 1f : component.mass;
			base.UpdateConnectedCache();
			float num2 = 0f;
			foreach (Chunk chunk2 in Block.connectedChunks[this])
			{
				if (chunk2 != chunk)
				{
					component = chunk2.go.GetComponent<Rigidbody>();
					if (component != null && !component.isKinematic)
					{
						num2 += component.mass;
					}
				}
			}
			this.alignScaler = 1f;
			if (num2 > 0.25f && num > 0.25f)
			{
				this.alignScaler = (num + num2) / num;
			}
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0005305C File Offset: 0x0005145C
		protected void ReplaceWheelColliders()
		{
			this.newColliders.Clear();
			this.oldColliders.Clear();
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			foreach (Block block in list)
			{
				if (block is BlockAbstractWheel)
				{
					Vector3 vector = block.Scale();
					if (Mathf.Abs(vector.y - vector.z) < 0.01f && vector.y / vector.x < 2.1f)
					{
						MeshCollider component = block.go.GetComponent<MeshCollider>();
						component.enabled = false;
						CapsuleCollider capsuleCollider = block.go.AddComponent<CapsuleCollider>();
						float num = vector.y * 0.5f;
						float height = vector.x + num;
						capsuleCollider.height = height;
						capsuleCollider.radius = num;
						capsuleCollider.direction = 0;
						capsuleCollider.material = new PhysicMaterial
						{
							dynamicFriction = this.dynamicFriction,
							staticFriction = this.staticFriction,
							frictionCombine = PhysicMaterialCombine.Average
						};
						this.newColliders[block] = capsuleCollider;
						this.oldColliders[block] = component;
					}
				}
			}
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x000531D0 File Offset: 0x000515D0
		protected void RestoreWheelColliders()
		{
			foreach (Block key in this.newColliders.Keys)
			{
				UnityEngine.Object.Destroy(this.newColliders[key]);
				this.oldColliders[key].enabled = true;
			}
			this.newColliders.Clear();
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x00053258 File Offset: 0x00051658
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.RestoreWheelColliders();
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x00053268 File Offset: 0x00051668
		private void OffsetRigidbodies(Vector3 localOffset)
		{
			Vector3 b = this.go.transform.TransformDirection(localOffset);
			Rigidbody component = this.chunk.go.GetComponent<Rigidbody>();
			if (component != null && !component.isKinematic)
			{
				Vector3 centerOfMass = component.centerOfMass;
				component.centerOfMass = centerOfMass + b;
			}
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x000532C4 File Offset: 0x000516C4
		public TileResultCode Assist(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float num = eInfo.floatArg * Util.GetFloatArg(args, 0, 1f);
			this.assistApplications += num;
			return TileResultCode.True;
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x000532F4 File Offset: 0x000516F4
		public override void FixedUpdate()
		{
			this.alignInFieldChunkApplications *= this.alignScaler;
			base.FixedUpdate();
			if (this.broken || this.didFix)
			{
				return;
			}
			Vector3 a = -Vector3.up * this.cmOffset;
			if (this.assistApplications != this.appliedAssistApplications)
			{
				float d = this.assistApplications - this.appliedAssistApplications;
				this.OffsetRigidbodies(d * a);
				if (this.appliedAssistApplications == 0f)
				{
					this.ReplaceWheelColliders();
				}
				if (this.assistApplications == 0f)
				{
					this.RestoreWheelColliders();
				}
				this.appliedAssistApplications = this.assistApplications;
			}
			this.assistApplications = 0f;
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x000533B8 File Offset: 0x000517B8
		protected void VisualizeCM()
		{
			Rigidbody component = this.chunk.go.GetComponent<Rigidbody>();
			if (component != null && !component.isKinematic)
			{
				Vector3 worldCenterOfMass = component.worldCenterOfMass;
				Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + Vector3.up * 3f);
			}
		}

		// Token: 0x0400091B RID: 2331
		private float assistApplications;

		// Token: 0x0400091C RID: 2332
		private float appliedAssistApplications;

		// Token: 0x0400091D RID: 2333
		private float cmOffset = 1f;

		// Token: 0x0400091E RID: 2334
		private float dynamicFriction = 0.75f;

		// Token: 0x0400091F RID: 2335
		private float staticFriction = 0.75f;

		// Token: 0x04000920 RID: 2336
		private float alignScaler = 1f;

		// Token: 0x04000921 RID: 2337
		private Dictionary<Block, CapsuleCollider> newColliders = new Dictionary<Block, CapsuleCollider>();

		// Token: 0x04000922 RID: 2338
		private Dictionary<Block, MeshCollider> oldColliders = new Dictionary<Block, MeshCollider>();
	}
}
