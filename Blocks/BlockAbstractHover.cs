using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000053 RID: 83
	public class BlockAbstractHover : Block
	{
		// Token: 0x060006E1 RID: 1761 RVA: 0x0002AD04 File Offset: 0x00029104
		public BlockAbstractHover(List<List<Tile>> tiles) : base(tiles)
		{
			this.sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
		}

		// Token: 0x060006E2 RID: 1762 RVA: 0x0002AD54 File Offset: 0x00029154
		public override void Play()
		{
			base.Play();
			this.currentVol = 0f;
			this.loopName = "Antigrav Hum";
			this.chunkBlocksSet = null;
		}

		// Token: 0x060006E3 RID: 1763 RVA: 0x0002AD7C File Offset: 0x0002917C
		protected void AddGravityForce(Rigidbody rb, float gravityMultiplier, float mass)
		{
			if (rb != null && !rb.isKinematic)
			{
				Vector3 force = Physics.gravity * mass * gravityMultiplier;
				rb.AddForce(force);
				this.sfxLoopStrength += Mathf.Abs(gravityMultiplier * 0.2f);
			}
		}

		// Token: 0x060006E4 RID: 1764 RVA: 0x0002ADD2 File Offset: 0x000291D2
		protected void UpdateChunkRigidbody()
		{
			if (this.chunkRigidBody == null && !this.didFix)
			{
				this.chunkRigidBody = this.chunk.rb;
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x0002AE04 File Offset: 0x00029204
		private void GatherVaryingMassBlocks()
		{
			base.UpdateConnectedCache();
			List<Block> list = Block.connectedCache[this];
			this.allRigidbodies = new HashSet<Rigidbody>();
			this.totalMassModel = 0f;
			this.varyingMassBlocksChunk.Clear();
			this.varyingMassBlocksModel.Clear();
			foreach (Block block in list)
			{
				Rigidbody component = block.goT.parent.GetComponent<Rigidbody>();
				if (component != null)
				{
					if (block.CanChangeMass())
					{
						List<Block> list2;
						if (this.varyingMassBlocksModel.TryGetValue(component, out list2))
						{
							list2.Add(block);
						}
						else
						{
							list2 = new List<Block>();
							this.varyingMassBlocksModel[component] = list2;
							list2.Add(block);
						}
						if (block.chunk == this.chunk)
						{
							this.varyingMassBlocksChunk.Add(block);
						}
					}
					else if (!this.allRigidbodies.Contains(component))
					{
						this.totalMassModel += component.mass;
						this.allRigidbodies.Add(component);
					}
				}
				if (this.informAboutVaryingGravity)
				{
					block.SetVaryingGravity(true);
				}
			}
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x0002AF60 File Offset: 0x00029360
		public override void Play2()
		{
			base.Play2();
			this.UpdateChunkRigidbody();
			this.GatherVaryingMassBlocks();
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x0002AF74 File Offset: 0x00029374
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.broken)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
				return;
			}
			if (this.vanished)
			{
				return;
			}
			this.UpdateChunkRigidbody();
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x0002AFB4 File Offset: 0x000293B4
		protected void UpdateSFXs()
		{
			bool flag = this.sfxLoopUpdateCounter % 5 == 0;
			if (Sound.sfxEnabled && this.playLoop && !this.vanished)
			{
				float num = Mathf.Clamp(this.sfxLoopStrength * 0.5f, 0f, 0.5f);
				float num2 = (num >= this.currentVol) ? 0.01f : -0.01f;
				this.currentVol = Mathf.Clamp(this.currentVol + num2, 0f, Mathf.Max(this.currentVol, num));
				if (flag)
				{
					float pitch = 0.9f + Mathf.Clamp(this.sfxLoopStrength * 0.05f, 0f, 0.2f);
					this.PlayLoopSound(this.currentVol > 0.01f, base.GetLoopClip(), this.currentVol, null, pitch);
					base.UpdateWithinWaterLPFilter(null);
				}
			}
			else if (flag)
			{
				this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			}
			this.sfxLoopUpdateCounter++;
			this.sfxLoopStrength = 0f;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x0002B0D5 File Offset: 0x000294D5
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
			this.chunkBlocksSet = null;
		}

		// Token: 0x060006EA RID: 1770 RVA: 0x0002B0FD File Offset: 0x000294FD
		public override void Pause()
		{
			this.PlayLoopSound(false, base.GetLoopClip(), 1f, null, 1f);
		}

		// Token: 0x060006EB RID: 1771 RVA: 0x0002B118 File Offset: 0x00029518
		protected float GetVaryingMassOffset(List<Block> list)
		{
			float num = 0f;
			foreach (Block block in list)
			{
				num += block.GetCurrentMassChange();
			}
			return num;
		}

		// Token: 0x060006EC RID: 1772 RVA: 0x0002B178 File Offset: 0x00029578
		public override bool TreatAsVehicleLikeBlock()
		{
			return true;
		}

		// Token: 0x0400050A RID: 1290
		public Quaternion rotation = Quaternion.identity;

		// Token: 0x0400050B RID: 1291
		protected List<Block> varyingMassBlocksChunk = new List<Block>();

		// Token: 0x0400050C RID: 1292
		protected Dictionary<Rigidbody, List<Block>> varyingMassBlocksModel = new Dictionary<Rigidbody, List<Block>>();

		// Token: 0x0400050D RID: 1293
		protected bool informAboutVaryingGravity = true;

		// Token: 0x0400050E RID: 1294
		protected HashSet<Rigidbody> allRigidbodies;

		// Token: 0x0400050F RID: 1295
		protected Rigidbody chunkRigidBody;

		// Token: 0x04000510 RID: 1296
		protected float totalMassModel;

		// Token: 0x04000511 RID: 1297
		protected float sfxLoopStrength;

		// Token: 0x04000512 RID: 1298
		protected float currentVol;

		// Token: 0x04000513 RID: 1299
		protected bool playLoop = true;

		// Token: 0x04000514 RID: 1300
		private int sfxLoopUpdateCounter;

		// Token: 0x04000515 RID: 1301
		private const int SFX_LOOP_UPDATE_INTERVAL = 5;

		// Token: 0x04000516 RID: 1302
		protected HashSet<Block> chunkBlocksSet;
	}
}
