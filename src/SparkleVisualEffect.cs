using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class SparkleVisualEffect : EmissionVisualEffect
{
	private static GameObject sparkles;

	private static ParticleSystem particles;

	private const int MAX_PARTICLES = 200;

	private Transform transform;

	private Vector3 size;

	private Vector3 localOffset = Vector3.zero;

	private bool singleBlock;

	private bool enabled = true;

	private BlockVfxRange range;

	public SparkleVisualEffect(string name, BlockVfxRange range = BlockVfxRange.BLOCK)
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
		if (sparkles == null)
		{
			sparkles = Object.Instantiate(Resources.Load("VFX/Sparkle Particle System")) as GameObject;
			particles = sparkles.GetComponent<ParticleSystem>();
		}
		switch (range)
		{
		case BlockVfxRange.BLOCK:
			size = base.block.GetEffectSize();
			transform = base.block.goT;
			localOffset = base.block.GetEffectLocalOffset();
			singleBlock = true;
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
			if (particleCount >= 200)
			{
				return;
			}
			float num = 1f - (float)particleCount / 200f;
			float num2 = num * Mathf.Min((size.x * size.y + size.x * size.z + size.y * size.z) * 0.1f, 5f);
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
				if (num3 <= Blocksworld.fogEnd)
				{
					Vector3 vector3 = CalculateRandomHullPosition(block, singleBlock, vector2, theSize);
					if (!(vector3 == Vector3.zero) && (num3 <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector3, Vector3.one))))
					{
						float num4 = Random.Range(0.5f, 1f) * Mathf.Min(timeLength * 2f, 1f);
						float num5 = Random.Range(0.5f, 0.8f);
						float y = Random.Range(-0.1f, 0.2f);
						Vector3 velocity = new Vector3(0f, y, 0f);
						float num6 = num5;
						float lifetime = num4;
						particles.Emit(vector3, velocity, num6, lifetime, _color);
					}
				}
			}
		}
	}
}
