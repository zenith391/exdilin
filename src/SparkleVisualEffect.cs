using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200033F RID: 831
public class SparkleVisualEffect : EmissionVisualEffect
{
	// Token: 0x06002556 RID: 9558 RVA: 0x001104F3 File Offset: 0x0010E8F3
	public SparkleVisualEffect(string name, BlockVfxRange range = BlockVfxRange.BLOCK) : base(name)
	{
		this.range = range;
	}

	// Token: 0x06002557 RID: 9559 RVA: 0x00110515 File Offset: 0x0010E915
	public override void Stop()
	{
		if (SparkleVisualEffect.particles != null)
		{
			SparkleVisualEffect.particles.Clear();
			if (SparkleVisualEffect.particles.isPaused)
			{
				SparkleVisualEffect.particles.Play();
			}
		}
	}

	// Token: 0x06002558 RID: 9560 RVA: 0x0011054A File Offset: 0x0010E94A
	public override void Pause()
	{
		base.Pause();
		if (SparkleVisualEffect.particles != null)
		{
			SparkleVisualEffect.particles.Pause();
		}
	}

	// Token: 0x06002559 RID: 9561 RVA: 0x0011056C File Offset: 0x0010E96C
	public override void Resume()
	{
		base.Resume();
		if (SparkleVisualEffect.particles != null)
		{
			SparkleVisualEffect.particles.Play();
		}
	}

	// Token: 0x0600255A RID: 9562 RVA: 0x00110590 File Offset: 0x0010E990
	public override void Begin()
	{
		base.Begin();
		if (SparkleVisualEffect.sparkles == null)
		{
			SparkleVisualEffect.sparkles = (UnityEngine.Object.Instantiate(Resources.Load("VFX/Sparkle Particle System")) as GameObject);
			SparkleVisualEffect.particles = SparkleVisualEffect.sparkles.GetComponent<ParticleSystem>();
		}
		switch (this.range)
		{
		case BlockVfxRange.BLOCK:
			this.size = this.block.GetEffectSize();
			this.transform = this.block.goT;
			this.localOffset = this.block.GetEffectLocalOffset();
			this.singleBlock = true;
			this.enabled = !TreasureHandler.IsPartOfPickedUpTreasureModel(this.block);
			break;
		case BlockVfxRange.CHUNK:
		case BlockVfxRange.MODEL:
		case BlockVfxRange.GROUP:
		{
			this.transform = this.block.chunk.go.transform;
			List<Block> list = null;
			if (this.range == BlockVfxRange.CHUNK)
			{
				list = this.block.chunk.blocks;
			}
			else if (this.range == BlockVfxRange.GROUP)
			{
				BlockGrouped blockGrouped = this.block as BlockGrouped;
				if (blockGrouped != null)
				{
					list = blockGrouped.group.GetBlockList();
					this.localOffset = Vector3.zero;
					for (int i = 0; i < list.Count; i++)
					{
						Block block = list[i];
						Chunk chunk = block.chunk;
						Matrix4x4 worldToLocalMatrix = this.transform.worldToLocalMatrix;
						this.localOffset += worldToLocalMatrix.MultiplyPoint(chunk.go.transform.position);
					}
					this.localOffset /= (float)list.Count;
				}
			}
			if (this.range == BlockVfxRange.MODEL || list == null)
			{
				this.block.UpdateConnectedCache();
				list = Block.connectedCache[this.block];
			}
			this.size = Util.ComputeBoundsCustom(list, (Block b) => new Bounds(b.GetPosition(), b.size)).size;
			break;
		}
		}
	}

	// Token: 0x0600255B RID: 9563 RVA: 0x001107A4 File Offset: 0x0010EBA4
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (this.paused)
		{
			return;
		}
		if (base.HasEnded())
		{
			if (SparkleVisualEffect.particles.particleCount == 0)
			{
				this.Destroy();
			}
		}
		else
		{
			if (this.transform == null || !this.enabled)
			{
				return;
			}
			int particleCount = SparkleVisualEffect.particles.particleCount;
			if (particleCount >= 200)
			{
				return;
			}
			float num = 1f - (float)particleCount / 200f;
			float num2 = num * Mathf.Min((this.size.x * this.size.y + this.size.x * this.size.z + this.size.y * this.size.z) * 0.1f, 5f);
			Vector3 b = this.transform.TransformDirection(this.localOffset);
			while (num2 > 0f)
			{
				if (num2 < 1f && UnityEngine.Random.value > num2)
				{
					break;
				}
				num2 -= 1f;
				Vector3 vector = this.size;
				if (this.block.isTreasure)
				{
					float treasureModelScale = TreasureHandler.GetTreasureModelScale(this.block);
					vector *= 0.5f * Mathf.Min(treasureModelScale, 1f / (treasureModelScale + 0.01f));
				}
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 vector2 = this.transform.position + b;
				float num3 = (vector2 - cameraPosition).magnitude - vector.magnitude;
				if (num3 <= Blocksworld.fogEnd)
				{
					Vector3 vector3 = base.CalculateRandomHullPosition(this.block, this.singleBlock, vector2, vector);
					if (!(vector3 == Vector3.zero))
					{
						if (num3 <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector3, Vector3.one)))
						{
							float num4 = UnityEngine.Random.Range(0.5f, 1f) * Mathf.Min(this.timeLength * 2f, 1f);
							float num5 = UnityEngine.Random.Range(0.5f, 0.8f);
							float y = UnityEngine.Random.Range(-0.1f, 0.2f);
							Vector3 velocity = new Vector3(0f, y, 0f);
							float num6 = num5;
							float lifetime = num4;
							SparkleVisualEffect.particles.Emit(vector3, velocity, num6, lifetime, this._color);
						}
					}
				}
			}
		}
	}

	// Token: 0x04001FEB RID: 8171
	private static GameObject sparkles;

	// Token: 0x04001FEC RID: 8172
	private static ParticleSystem particles;

	// Token: 0x04001FED RID: 8173
	private const int MAX_PARTICLES = 200;

	// Token: 0x04001FEE RID: 8174
	private Transform transform;

	// Token: 0x04001FEF RID: 8175
	private Vector3 size;

	// Token: 0x04001FF0 RID: 8176
	private Vector3 localOffset = Vector3.zero;

	// Token: 0x04001FF1 RID: 8177
	private bool singleBlock;

	// Token: 0x04001FF2 RID: 8178
	private bool enabled = true;

	// Token: 0x04001FF3 RID: 8179
	private BlockVfxRange range;
}
