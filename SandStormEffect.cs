using System;
using UnityEngine;

// Token: 0x02000354 RID: 852
public class SandStormEffect : WeatherEffect
{
	// Token: 0x0600260A RID: 9738 RVA: 0x00119898 File Offset: 0x00117C98
	public SandStormEffect()
	{
		this.loopSfx = "Sandstorm Loop";
	}

	// Token: 0x0600260B RID: 9739 RVA: 0x001198C1 File Offset: 0x00117CC1
	public override void Pause()
	{
		base.Pause();
		if (this.dustPs != null)
		{
			this.dustPs.Pause();
		}
		if (this.rubblePs != null)
		{
			this.rubblePs.Pause();
		}
	}

	// Token: 0x0600260C RID: 9740 RVA: 0x00119901 File Offset: 0x00117D01
	public override void Resume()
	{
		base.Resume();
		if (this.dustPs != null)
		{
			this.dustPs.Play();
		}
		if (this.rubblePs != null)
		{
			this.rubblePs.Play();
		}
	}

	// Token: 0x0600260D RID: 9741 RVA: 0x00119944 File Offset: 0x00117D44
	protected override float GetTargetVolume()
	{
		float num = 1f / (1f + 0.5f * this.avgSpawnDiffY);
		return num * base.GetTargetVolume();
	}

	// Token: 0x0600260E RID: 9742 RVA: 0x00119972 File Offset: 0x00117D72
	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		this.sandGo.transform.rotation = Quaternion.Euler(0f, angle, 0f);
	}

	// Token: 0x0600260F RID: 9743 RVA: 0x0011999C File Offset: 0x00117D9C
	private void CreateParticleSystem()
	{
		if (this.sandGo == null)
		{
			this.sandGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Sand Storm Particle Systems")) as GameObject);
		}
		if (this.sandGo != null)
		{
			this.settings = this.sandGo.GetComponent<SandStormEffectSettings>();
			this.dustPs = this.sandGo.transform.Find("Sand Storm Dust Particle System").gameObject.GetComponent<ParticleSystem>();
			this.rubblePs = this.sandGo.transform.Find("Sand Storm Rubble Particle System").gameObject.GetComponent<ParticleSystem>();
		}
		this.Stop();
	}

	// Token: 0x06002610 RID: 9744 RVA: 0x00119A48 File Offset: 0x00117E48
	public override void FogChanged()
	{
		if (this.sandGo != null)
		{
			Color fogColor = Blocksworld.fogColor;
			Material material = this.dustPs.GetComponent<Renderer>().material;
			Material material2 = this.rubblePs.GetComponent<Renderer>().material;
			material.SetColor("_FogColor", fogColor);
			material2.SetColor("_FogColor", fogColor);
			float value = Blocksworld.fogStart * Blocksworld.fogMultiplier;
			float value2 = Blocksworld.fogEnd * Blocksworld.fogMultiplier;
			material.SetFloat("_FogStart", value);
			material.SetFloat("_FogEnd", value2);
			material2.SetFloat("_FogStart", value);
			material2.SetFloat("_FogEnd", value2);
		}
	}

	// Token: 0x06002611 RID: 9745 RVA: 0x00119AF4 File Offset: 0x00117EF4
	public override void Update()
	{
		base.Update();
		if (this.sandGo != null && !this.paused)
		{
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Vector3 a = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
			float magnitude = a.magnitude;
			if (magnitude > 0.01f)
			{
				Vector3 a2 = cameraPosition - this.prevCamPos;
				float smoothDeltaTime = Time.smoothDeltaTime;
				if (smoothDeltaTime > 0.001f)
				{
					this.camVelocity = 0.9f * this.camVelocity + 0.1f * (a2 / smoothDeltaTime);
					this.camVelocity.y = 0f;
				}
				this.prevCamPos = cameraPosition;
				this.dustToEmit += (UnityEngine.Random.Range(this.settings.dustEmitRandomFrom, this.settings.dustEmitRandomTo) + this.settings.dustEmitBias) * this.intensityMultiplier;
				this.rubbleToEmit += (UnityEngine.Random.Range(this.settings.rubbleEmitRandomFrom, this.settings.rubbleEmitRandomTo) + this.settings.rubbleEmitBias) * this.intensityMultiplier;
				Vector3 a3 = a / magnitude;
				float d = Mathf.Min(magnitude * 0.5f, 30f);
				Vector3 b = this.camVelocity * 0.5f;
				if (b.magnitude > 15f)
				{
					b = b.normalized * 15f;
				}
				Vector3 centerPos = cameraPosition + a3 * d + b;
				int num = 0;
				int num2 = 0;
				while ((float)num2 < this.rubbleToEmit)
				{
					num++;
					float num3 = Mathf.Min(1.2f, this.intensityMultiplier * this.settings.rubbleSpeedFractionPerIntensity + this.settings.rubbleSpeedFractionBias) * UnityEngine.Random.Range(this.settings.rubbleSpeedRandomFrom, this.settings.rubbleSpeedRandomTo);
					Vector3 a4 = this.CalculateEmitReferencePoint(centerPos, num3, 1f);
					Vector3 vector = a4 + this.sandGo.transform.right * UnityEngine.Random.Range(-30f, 30f) + Vector3.up * UnityEngine.Random.Range(0f, 3f);
					float num4 = UnityEngine.Random.Range(this.settings.rubbleLifetimeRandomFrom, this.settings.rubbleLifetimeRandomTo);
					float size = UnityEngine.Random.Range(this.settings.rubbleSizeRandomFrom, this.settings.rubbleSizeRandomTo);
					float y = vector.y;
					this.GetGoodSpawnPoint(size, num3, ref vector, ref num4);
					float magnitude2 = (vector - cameraPosition).magnitude;
					if (magnitude2 < 200f && num4 > 0.4f)
					{
						float num5 = vector.y - y;
						float d2 = 0f;
						if (num5 <= 5f)
						{
							d2 = num3 * UnityEngine.Random.Range(0.1f, 0.3f);
						}
						float value = UnityEngine.Random.value;
						Color color = value * this.settings.rubbleStartColorFrom + (1f - value) * this.settings.rubbleStartColorTo;
						Vector3 velocity = this.sandGo.transform.forward * num3 + Vector3.up * d2;
						ParticleSystem.Particle particle = default(ParticleSystem.Particle);
						particle.position = vector;
						particle.velocity = velocity;
						particle.size = size;
						particle.startLifetime = num4;
						particle.remainingLifetime = num4;
						particle.color = new Color(color.r, color.g, color.b, UnityEngine.Random.Range(this.settings.rubbleAlphaRandomFrom, this.settings.rubbleAlphaRandomTo));
						particle.randomSeed = (uint)UnityEngine.Random.Range(1, 9999999);
						this.rubblePs.Emit(particle);
					}
					num2++;
				}
				this.rubbleToEmit -= (float)num;
				int num6 = 0;
				int num7 = 0;
				while ((float)num7 < this.dustToEmit)
				{
					num6++;
					float num8 = Mathf.Min(1.2f, this.intensityMultiplier * this.settings.dustSpeedFractionPerIntensity + this.settings.dustSpeedFractionBias) * UnityEngine.Random.Range(this.settings.dustSpeedRandomFrom, this.settings.dustSpeedRandomTo);
					Vector3 a5 = this.CalculateEmitReferencePoint(centerPos, num8, 0.5f);
					Vector3 vector2 = a5 + this.sandGo.transform.right * UnityEngine.Random.Range(-30f, 30f) + Vector3.up * UnityEngine.Random.Range(0f, 3f);
					float num9 = UnityEngine.Random.Range(this.settings.dustLifetimeRandomFrom, this.settings.dustLifetimeRandomTo);
					float size2 = UnityEngine.Random.Range(this.settings.dustSizeRandomFrom, this.settings.dustSizeRandomTo);
					float y2 = vector2.y;
					this.GetGoodSpawnPoint(size2, num8, ref vector2, ref num9);
					this.avgSpawnDiffY = 0.9f * this.avgSpawnDiffY + 0.1f * Mathf.Abs(vector2.y - y2);
					float magnitude3 = (vector2 - cameraPosition).magnitude;
					if (magnitude3 < 200f && num9 > 0.4f)
					{
						ParticleSystem.Particle particle2 = default(ParticleSystem.Particle);
						particle2.position = vector2;
						particle2.velocity = this.sandGo.transform.forward * num8;
						particle2.rotation = UnityEngine.Random.Range(0f, 360f);
						particle2.size = size2;
						particle2.remainingLifetime = num9;
						particle2.startLifetime = num9;
						particle2.randomSeed = (uint)UnityEngine.Random.Range(1, 9999999);
						float value2 = UnityEngine.Random.value;
						Color color2 = value2 * this.settings.dustStartColorFrom + (1f - value2) * this.settings.dustStartColorTo;
						particle2.color = new Color(color2.r, color2.g, color2.b, UnityEngine.Random.Range(this.settings.dustAlphaRandomFrom, this.settings.dustAlphaRandomTo));
						this.dustPs.Emit(particle2);
					}
					num7++;
				}
				this.dustToEmit -= (float)num6;
			}
		}
	}

	// Token: 0x06002612 RID: 9746 RVA: 0x0011A184 File Offset: 0x00118584
	private Vector3 CalculateEmitReferencePoint(Vector3 centerPos, float speed, float offsetMultiplier = 0.5f)
	{
		Vector3 result = centerPos - this.sandGo.transform.forward * speed * offsetMultiplier;
		GameObject worldOcean = Blocksworld.worldOcean;
		if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
		{
			Bounds bounds = worldOcean.GetComponent<Collider>().bounds;
			result.y = Mathf.Clamp(result.y, bounds.max.y + 5f, bounds.max.y + 10000f);
		}
		return result;
	}

	// Token: 0x06002613 RID: 9747 RVA: 0x0011A224 File Offset: 0x00118624
	private void GetGoodSpawnPoint(float size, float speed, ref Vector3 spawnPoint, ref float lifetime)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(spawnPoint + Vector3.up * 30f, -Vector3.up, out raycastHit, 45f))
		{
			Vector3 point = raycastHit.point;
			point.y = Mathf.Max(point.y + size * 0.5f, spawnPoint.y);
			spawnPoint = point;
			if (Physics.Raycast(spawnPoint, this.sandGo.transform.forward, out raycastHit, 45f))
			{
				lifetime = raycastHit.distance / speed;
			}
		}
	}

	// Token: 0x06002614 RID: 9748 RVA: 0x0011A2C9 File Offset: 0x001186C9
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
	}

	// Token: 0x06002615 RID: 9749 RVA: 0x0011A2D8 File Offset: 0x001186D8
	public override void Stop()
	{
		if (this.dustPs != null)
		{
			this.dustPs.Stop();
			this.dustPs.Clear();
		}
		if (this.rubblePs != null)
		{
			this.rubblePs.Stop();
			this.rubblePs.Clear();
		}
	}

	// Token: 0x06002616 RID: 9750 RVA: 0x0011A333 File Offset: 0x00118733
	public override void Reset()
	{
		base.Reset();
		if (this.sandGo != null)
		{
			UnityEngine.Object.Destroy(this.sandGo);
		}
		this.sandGo = null;
		this.dustPs = null;
		this.rubblePs = null;
	}

	// Token: 0x06002617 RID: 9751 RVA: 0x0011A36C File Offset: 0x0011876C
	public override float GetFogMultiplier()
	{
		return 1f - Mathf.Min(0.85f, 0.7f * this.intensityMultiplier);
	}

	// Token: 0x04002119 RID: 8473
	private GameObject sandGo;

	// Token: 0x0400211A RID: 8474
	private ParticleSystem dustPs;

	// Token: 0x0400211B RID: 8475
	private ParticleSystem rubblePs;

	// Token: 0x0400211C RID: 8476
	private Vector3 prevCamPos = Vector3.zero;

	// Token: 0x0400211D RID: 8477
	private Vector3 camVelocity = Vector3.zero;

	// Token: 0x0400211E RID: 8478
	private float dustToEmit;

	// Token: 0x0400211F RID: 8479
	private float rubbleToEmit;

	// Token: 0x04002120 RID: 8480
	private float avgSpawnDiffY;

	// Token: 0x04002121 RID: 8481
	private SandStormEffectSettings settings;
}
