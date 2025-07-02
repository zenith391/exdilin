using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class VisualEffectWind : EmissionVisualEffect
{
	private static GameObject windLines;

	private static ParticleSystem particles;

	private const int MAX_PARTICLES = 50;

	private Transform transform;

	private Vector3 size;

	private Vector3 localOffset = Vector3.zero;

	private float particlePower;

	private bool enabled = true;

	private bool singleBlock;

	private BlockVfxRange range;

	private const float PARTICLE_SPEED_MODIFIER = 0.25f;

	public VisualEffectWind(string name, BlockVfxRange range = BlockVfxRange.BLOCK)
		: base(name)
	{
		this.range = range;
	}

	public override void Stop()
	{
		if (particles != null)
		{
			particles.Clear();
			if (particles.isPaused)
			{
				particles.Play();
			}
		}
	}

	public override void Pause()
	{
		base.Pause();
		if (particles != null)
		{
			particles.Pause();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (particles != null)
		{
			particles.Play();
		}
	}

	public override void Begin()
	{
		base.Begin();
		if (windLines == null)
		{
			windLines = Object.Instantiate(Resources.Load("VFX/Wind Particle System")) as GameObject;
			particles = windLines.GetComponent<ParticleSystem>();
		}
		switch (range)
		{
		case BlockVfxRange.BLOCK:
			size = base.block.GetEffectSize();
			transform = base.block.goT;
			localOffset = base.block.GetEffectLocalOffset();
			singleBlock = true;
			particlePower = base.block.GetEffectPower() * 0.25f;
			enabled = !TreasureHandler.IsPartOfPickedUpTreasureModel(base.block);
			break;
		case BlockVfxRange.CHUNK:
		case BlockVfxRange.MODEL:
		case BlockVfxRange.GROUP:
		{
			transform = base.block.chunk.go.transform;
			List<Block> list = null;
			if (range == BlockVfxRange.CHUNK)
			{
				list = base.block.chunk.blocks;
			}
			else if (range == BlockVfxRange.GROUP && base.block is BlockGrouped blockGrouped)
			{
				list = blockGrouped.group.GetBlockList();
				localOffset = Vector3.zero;
				for (int i = 0; i < list.Count; i++)
				{
					Block block = list[i];
					Chunk chunk = block.chunk;
					localOffset += transform.worldToLocalMatrix.MultiplyPoint(chunk.go.transform.position);
				}
				localOffset /= (float)list.Count;
			}
			if (range == BlockVfxRange.MODEL || list == null)
			{
				base.block.UpdateConnectedCache();
				list = Block.connectedCache[base.block];
			}
			size = Util.ComputeBoundsCustom(list, (Block b) => new Bounds(b.GetPosition(), b.size)).size;
			break;
		}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (paused)
		{
			return;
		}
		if (HasEnded())
		{
			if (particles.particleCount == 0)
			{
				Destroy();
			}
		}
		else
		{
			if (transform == null || !enabled)
			{
				return;
			}
			int particleCount = particles.particleCount;
			if (particleCount >= 50 || particlePower == 0f)
			{
				return;
			}
			float num = 1f - (float)particleCount / 50f;
			float num2 = num * Mathf.Min((size.x * size.y + size.x * size.z + size.y * size.z) * 0.1f, 15f);
			Vector3 vector = transform.TransformDirection(localOffset);
			while (num2 > 0f && (!(num2 < 1f) || !(Random.value > num2)))
			{
				num2 -= 1f;
				Vector3 theSize = size;
				if (block.isTreasure)
				{
					float treasureModelScale = TreasureHandler.GetTreasureModelScale(block);
					theSize *= 0.5f * Mathf.Min(treasureModelScale, 1f / (treasureModelScale + 0.01f));
				}
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 vector2 = transform.position + vector;
				float num3 = (vector2 - cameraPosition).magnitude - theSize.magnitude;
				Vector3 vector3 = transform.TransformDirection(new Vector3(0f, 0f, particlePower * -0.1f));
				Vector3 vector4 = CalculateRandomHullPosition(block, singleBlock, vector2 + vector3, theSize);
				if (!(vector4 == Vector3.zero) && num3 <= Blocksworld.fogEnd && ((num3 <= 15f && !(vector4 == Vector3.zero)) || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector4, Vector3.one))))
				{
					float num4 = Random.Range(0.5f, 1f) * Mathf.Min(timeLength * 2f, 1f);
					float num5 = Random.Range(0.5f, 1.5f);
					Vector3 velocity = transform.TransformDirection(new Vector3(0f, 0f, particlePower));
					float num6 = num5;
					float lifetime = num4;
					particles.Emit(vector4, velocity, num6, lifetime, _color);
				}
			}
		}
	}
}
