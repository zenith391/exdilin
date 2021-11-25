using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000342 RID: 834
public class VisualEffectWind : EmissionVisualEffect
{
	// Token: 0x06002574 RID: 9588 RVA: 0x00110A49 File Offset: 0x0010EE49
	public VisualEffectWind(string name, BlockVfxRange range = BlockVfxRange.BLOCK) : base(name)
	{
		this.range = range;
	}

	// Token: 0x06002575 RID: 9589 RVA: 0x00110A6B File Offset: 0x0010EE6B
	public override void Stop()
	{
		if (VisualEffectWind.particles != null)
		{
			VisualEffectWind.particles.Clear();
			if (VisualEffectWind.particles.isPaused)
			{
				VisualEffectWind.particles.Play();
			}
		}
	}

	// Token: 0x06002576 RID: 9590 RVA: 0x00110AA0 File Offset: 0x0010EEA0
	public override void Pause()
	{
		base.Pause();
		if (VisualEffectWind.particles != null)
		{
			VisualEffectWind.particles.Pause();
		}
	}

	// Token: 0x06002577 RID: 9591 RVA: 0x00110AC2 File Offset: 0x0010EEC2
	public override void Resume()
	{
		base.Resume();
		if (VisualEffectWind.particles != null)
		{
			VisualEffectWind.particles.Play();
		}
	}

	// Token: 0x06002578 RID: 9592 RVA: 0x00110AE4 File Offset: 0x0010EEE4
	public override void Begin()
	{
		base.Begin();
		if (VisualEffectWind.windLines == null)
		{
			VisualEffectWind.windLines = (UnityEngine.Object.Instantiate(Resources.Load("VFX/Wind Particle System")) as GameObject);
			VisualEffectWind.particles = VisualEffectWind.windLines.GetComponent<ParticleSystem>();
		}
		switch (this.range)
		{
		case BlockVfxRange.BLOCK:
			this.size = this.block.GetEffectSize();
			this.transform = this.block.goT;
			this.localOffset = this.block.GetEffectLocalOffset();
			this.singleBlock = true;
			this.particlePower = this.block.GetEffectPower() * 0.25f;
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

	// Token: 0x06002579 RID: 9593 RVA: 0x00110D0C File Offset: 0x0010F10C
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (this.paused)
		{
			return;
		}
		if (base.HasEnded())
		{
			if (VisualEffectWind.particles.particleCount == 0)
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
			int particleCount = VisualEffectWind.particles.particleCount;
			if (particleCount >= 50 || this.particlePower == 0f)
			{
				return;
			}
			float num = 1f - (float)particleCount / 50f;
			float num2 = num * Mathf.Min((this.size.x * this.size.y + this.size.x * this.size.z + this.size.y * this.size.z) * 0.1f, 15f);
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
				Vector3 a = this.transform.position + b;
				float num3 = (a - cameraPosition).magnitude - vector.magnitude;
				Vector3 b2 = this.transform.TransformDirection(new Vector3(0f, 0f, this.particlePower * -0.1f));
				Vector3 vector2 = base.CalculateRandomHullPosition(this.block, this.singleBlock, a + b2, vector);
				if (!(vector2 == Vector3.zero))
				{
					if (num3 <= Blocksworld.fogEnd)
					{
						if ((num3 <= 15f && !(vector2 == Vector3.zero)) || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector2, Vector3.one)))
						{
							float num4 = UnityEngine.Random.Range(0.5f, 1f) * Mathf.Min(this.timeLength * 2f, 1f);
							float num5 = UnityEngine.Random.Range(0.5f, 1.5f);
							Vector3 velocity = this.transform.TransformDirection(new Vector3(0f, 0f, this.particlePower));
							float num6 = num5;
							float lifetime = num4;
							VisualEffectWind.particles.Emit(vector2, velocity, num6, lifetime, this._color);
						}
					}
				}
			}
		}
	}

	// Token: 0x04002005 RID: 8197
	private static GameObject windLines;

	// Token: 0x04002006 RID: 8198
	private static ParticleSystem particles;

	// Token: 0x04002007 RID: 8199
	private const int MAX_PARTICLES = 50;

	// Token: 0x04002008 RID: 8200
	private Transform transform;

	// Token: 0x04002009 RID: 8201
	private Vector3 size;

	// Token: 0x0400200A RID: 8202
	private Vector3 localOffset = Vector3.zero;

	// Token: 0x0400200B RID: 8203
	private float particlePower;

	// Token: 0x0400200C RID: 8204
	private bool enabled = true;

	// Token: 0x0400200D RID: 8205
	private bool singleBlock;

	// Token: 0x0400200E RID: 8206
	private BlockVfxRange range;

	// Token: 0x0400200F RID: 8207
	private const float PARTICLE_SPEED_MODIFIER = 0.25f;
}
