using System;
using UnityEngine;

// Token: 0x0200033E RID: 830
public class SpaceDustVisualEffect : EmissionVisualEffect
{
	// Token: 0x06002550 RID: 9552 RVA: 0x0011017B File Offset: 0x0010E57B
	public SpaceDustVisualEffect(string name) : base(name)
	{
	}

	// Token: 0x06002551 RID: 9553 RVA: 0x00110196 File Offset: 0x0010E596
	public override void Stop()
	{
		if (SpaceDustVisualEffect.particles != null)
		{
			SpaceDustVisualEffect.particles.Clear();
			if (SpaceDustVisualEffect.particles.isPaused)
			{
				SpaceDustVisualEffect.particles.Play();
			}
		}
	}

	// Token: 0x06002552 RID: 9554 RVA: 0x001101CB File Offset: 0x0010E5CB
	public override void Pause()
	{
		base.Pause();
		if (SpaceDustVisualEffect.particles != null)
		{
			SpaceDustVisualEffect.particles.Pause();
		}
	}

	// Token: 0x06002553 RID: 9555 RVA: 0x001101ED File Offset: 0x0010E5ED
	public override void Resume()
	{
		base.Resume();
		if (SpaceDustVisualEffect.particles != null)
		{
			SpaceDustVisualEffect.particles.Play();
		}
	}

	// Token: 0x06002554 RID: 9556 RVA: 0x00110210 File Offset: 0x0010E610
	public override void Begin()
	{
		base.Begin();
		if (SpaceDustVisualEffect.dust == null)
		{
			SpaceDustVisualEffect.dust = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Space Dust Particle System")) as GameObject);
			SpaceDustVisualEffect.particles = SpaceDustVisualEffect.dust.GetComponent<ParticleSystem>();
		}
		this.paused = false;
		this.size = this.block.GetEffectSize();
		this.transform = this.block.goT;
		this.localOffset = this.block.GetEffectLocalOffset();
		this.timeLength = 100f;
	}

	// Token: 0x06002555 RID: 9557 RVA: 0x001102A0 File Offset: 0x0010E6A0
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (this.paused)
		{
			return;
		}
		if (base.HasEnded())
		{
			if (SpaceDustVisualEffect.particles.particleCount == 0)
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
			int particleCount = SpaceDustVisualEffect.particles.particleCount;
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
				Vector3 cameraPosition = Blocksworld.cameraPosition;
				Vector3 a = this.transform.position + b;
				float num3 = (a - cameraPosition).magnitude - vector.magnitude;
				if (num3 <= Blocksworld.fogEnd)
				{
					Vector3 vector2 = a + 20f * UnityEngine.Random.Range(0.6f, 1f) * vector.magnitude * UnityEngine.Random.onUnitSphere;
					if (!(vector2 == Vector3.zero))
					{
						if (num3 <= 15f || GeometryUtility.TestPlanesAABB(Blocksworld.frustumPlanes, new Bounds(vector2, Vector3.one)))
						{
							float num4 = UnityEngine.Random.Range(0.2f, 0.4f);
							float y = UnityEngine.Random.Range(-0.001f, 0.001f);
							Vector3 velocity = new Vector3(0f, y, 0f);
							float num5 = num4;
							float lifetime = 10f;
							Color white = Color.white;
							white.a = 0.6f;
							SpaceDustVisualEffect.particles.Emit(vector2, velocity, num5, lifetime, white);
						}
					}
				}
			}
		}
	}

	// Token: 0x04001FE4 RID: 8164
	private static GameObject dust;

	// Token: 0x04001FE5 RID: 8165
	private static ParticleSystem particles;

	// Token: 0x04001FE6 RID: 8166
	private const int MAX_PARTICLES = 200;

	// Token: 0x04001FE7 RID: 8167
	private Transform transform;

	// Token: 0x04001FE8 RID: 8168
	private Vector3 size;

	// Token: 0x04001FE9 RID: 8169
	private Vector3 localOffset = Vector3.zero;

	// Token: 0x04001FEA RID: 8170
	private bool enabled = true;
}
