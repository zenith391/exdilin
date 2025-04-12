using System;
using UnityEngine;

// Token: 0x0200034E RID: 846
public class BlowingLeavesEffect : WeatherEffect
{
	// Token: 0x060025E7 RID: 9703 RVA: 0x00117913 File Offset: 0x00115D13
	public BlowingLeavesEffect(bool green)
	{
		this.green = green;
		this.loopSfx = "Wind Loop";
	}

	// Token: 0x060025E8 RID: 9704 RVA: 0x0011794E File Offset: 0x00115D4E
	public override void Pause()
	{
		base.Pause();
		if (BlowingLeavesEffect.smallLeavesPs != null)
		{
			BlowingLeavesEffect.smallLeavesPs.Pause();
		}
		if (BlowingLeavesEffect.mediumLeavesPs != null)
		{
			BlowingLeavesEffect.mediumLeavesPs.Pause();
		}
	}

	// Token: 0x060025E9 RID: 9705 RVA: 0x0011798A File Offset: 0x00115D8A
	public override void Resume()
	{
		base.Resume();
		if (BlowingLeavesEffect.smallLeavesPs != null)
		{
			BlowingLeavesEffect.smallLeavesPs.Play();
		}
		if (BlowingLeavesEffect.mediumLeavesPs != null)
		{
			BlowingLeavesEffect.mediumLeavesPs.Play();
		}
	}

	// Token: 0x060025EA RID: 9706 RVA: 0x001179C8 File Offset: 0x00115DC8
	protected override float GetTargetVolume()
	{
		float num = Mathf.Clamp(this.avgLeafSpeed * 0.1f, 0f, 1f);
		return num * base.GetTargetVolume();
	}

	// Token: 0x060025EB RID: 9707 RVA: 0x001179F9 File Offset: 0x00115DF9
	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		this.velDirectionVec = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
	}

	// Token: 0x060025EC RID: 9708 RVA: 0x00117A24 File Offset: 0x00115E24
	private void CreateParticleSystem()
	{
		if (this.leavesGo == null)
		{
			this.leavesGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Blowing Leaves Particle System")) as GameObject);
		}
		if (this.leavesGo != null)
		{
			BlowingLeavesEffect.settings = this.leavesGo.GetComponent<BlowingLeavesEffectSettings>();
			if (BlowingLeavesEffect.smallLeavesPs == null)
			{
				BlowingLeavesEffect.smallLeavesPs = this.leavesGo.transform.Find("Small Leaves").gameObject.GetComponent<ParticleSystem>();
			}
			if (BlowingLeavesEffect.mediumLeavesPs == null)
			{
				BlowingLeavesEffect.mediumLeavesPs = this.leavesGo.transform.Find("Medium Leaves").gameObject.GetComponent<ParticleSystem>();
			}
		}
		this.Stop();
	}

	// Token: 0x060025ED RID: 9709 RVA: 0x00117AEC File Offset: 0x00115EEC
	public override void Update()
	{
		base.Update();
		if (BlowingLeavesEffect.smallLeavesPs != null && !this.paused)
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
				this.smallToEmit += BlowingLeavesEffect.settings.smallEmitPerFrame * this.intensityMultiplier;
				this.mediumToEmit += BlowingLeavesEffect.settings.mediumEmitPerFrame * this.intensityMultiplier;
				Vector3 a3 = a / magnitude;
				float d = Mathf.Min(magnitude * 0.5f, 30f);
				Vector3 vector = this.camVelocity * 0.5f;
				if (vector.magnitude > 15f)
				{
					vector = vector.normalized * 15f;
				}
				Vector3 centerPos = cameraPosition + a3 * d + vector;
				int num = 0;
				int num2 = 0;
				while ((float)num2 < this.mediumToEmit)
				{
					num++;
					this.EmitLeaf(cameraPosition, centerPos, BlowingLeavesEffect.mediumLeavesPs, BlowingLeavesEffect.settings.mediumSizeMultiplier, vector);
					num2++;
				}
				this.mediumToEmit -= (float)num;
				int num3 = 0;
				int num4 = 0;
				while ((float)num4 < this.smallToEmit)
				{
					num3++;
					this.EmitLeaf(cameraPosition, centerPos, BlowingLeavesEffect.smallLeavesPs, BlowingLeavesEffect.settings.smallSizeMultiplier, vector);
					num4++;
				}
				this.smallToEmit -= (float)num3;
			}
		}
	}

	// Token: 0x060025EE RID: 9710 RVA: 0x00117CEC File Offset: 0x001160EC
	private void EmitLeaf(Vector3 camPos, Vector3 centerPos, ParticleSystem ps, float sizeMultiplier, Vector3 camVelOffset)
	{
		float num = UnityEngine.Random.Range(BlowingLeavesEffect.settings.lifetimeRandomFrom, BlowingLeavesEffect.settings.lifetimeRandomTo);
		float size = UnityEngine.Random.Range(BlowingLeavesEffect.settings.sizeRandomFrom, BlowingLeavesEffect.settings.sizeRandomTo) * sizeMultiplier;
		Vector3 vector = centerPos + new Vector3((float)UnityEngine.Random.Range(-30, 30), centerPos.y + 30f, (float)UnityEngine.Random.Range(-30, 30));
		float num2 = centerPos.y - 5f;
		Vector3 vector2 = vector;
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, Vector3.down, out raycastHit, 40f))
		{
			Vector3 point = raycastHit.point;
			num2 = point.y;
			vector2.y = point.y + UnityEngine.Random.Range(0.1f, 10f);
		}
		else
		{
			vector2.y = centerPos.y + UnityEngine.Random.Range(-5f, 10f);
		}
		float num3 = this.CalculateAlphaMultiplier(camPos, vector2 - camVelOffset);
		if (num3 > 0.01f)
		{
			Vector3 a = this.velDirectionVec + 0.4f * UnityEngine.Random.onUnitSphere;
			a.y *= 0.5f;
			if (a.sqrMagnitude > 0.0001f)
			{
				a.Normalize();
			}
			float num4 = (this.intensityMultiplier * BlowingLeavesEffect.settings.speedMultiplierPerIntensity + BlowingLeavesEffect.settings.speedMultiplierPerIntensityBias) * UnityEngine.Random.Range(BlowingLeavesEffect.settings.speedRandomFrom, BlowingLeavesEffect.settings.speedRandomTo) * (vector2.y - num2 + 0.5f);
			num4 = Mathf.Min(num4, 5f);
			this.avgLeafSpeed = 0.99f * this.avgLeafSpeed + 0.01f * num4;
			bool flag = Mathf.Abs(num2 - vector2.y) < 2f;
			if (flag)
			{
				Vector3 a2 = vector2 + a * num4 * num;
				Vector3 origin = a2 + Vector3.up * 5f;
				RaycastHit raycastHit2;
				if (Physics.Raycast(origin, Vector3.down, out raycastHit2, 6f))
				{
					a = (raycastHit2.point - vector2).normalized;
				}
			}
			ParticleSystem.Particle particle = default(ParticleSystem.Particle);
			particle.position = vector2;
			particle.velocity = a * num4;
			particle.size = size;
			particle.startLifetime = num;
			particle.remainingLifetime = num;
			Color c;
			if (this.green)
			{
				c = new Color(UnityEngine.Random.Range(0f, 0.1f), UnityEngine.Random.Range(0.5f, 1f), 0f, num3 * UnityEngine.Random.Range(0.95f, 1f));
			}
			else
			{
				c = new Color(UnityEngine.Random.Range(0.6f, 1f), UnityEngine.Random.Range(0.2f, 0.8f), 0f, num3 * UnityEngine.Random.Range(0.95f, 1f));
			}
			particle.color = c;
			particle.angularVelocity = UnityEngine.Random.Range(-num4 * 800f, num4 * 800f);
			particle.rotation = UnityEngine.Random.Range(0f, 360f);
			ps.Emit(particle);
		}
	}

	// Token: 0x060025EF RID: 9711 RVA: 0x00118038 File Offset: 0x00116438
	private float CalculateAlphaMultiplier(Vector3 camPos, Vector3 spawnPoint)
	{
		float result = 1f;
		float num = 40f;
		float magnitude = (camPos - spawnPoint).magnitude;
		if (magnitude > num)
		{
			result = Mathf.Max(0f, 1f - (magnitude - num) / 20f);
		}
		return result;
	}

	// Token: 0x060025F0 RID: 9712 RVA: 0x00118083 File Offset: 0x00116483
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
	}

	// Token: 0x060025F1 RID: 9713 RVA: 0x00118094 File Offset: 0x00116494
	public override void Stop()
	{
		if (BlowingLeavesEffect.smallLeavesPs != null)
		{
			BlowingLeavesEffect.smallLeavesPs.Stop();
			BlowingLeavesEffect.smallLeavesPs.Clear();
		}
		if (BlowingLeavesEffect.mediumLeavesPs != null)
		{
			BlowingLeavesEffect.mediumLeavesPs.Stop();
			BlowingLeavesEffect.mediumLeavesPs.Clear();
		}
	}

	// Token: 0x060025F2 RID: 9714 RVA: 0x001180E9 File Offset: 0x001164E9
	public override void Reset()
	{
		base.Reset();
		if (this.leavesGo != null)
		{
			UnityEngine.Object.Destroy(this.leavesGo);
		}
		this.leavesGo = null;
		BlowingLeavesEffect.smallLeavesPs = null;
		BlowingLeavesEffect.mediumLeavesPs = null;
	}

	// Token: 0x040020E5 RID: 8421
	private static ParticleSystem smallLeavesPs;

	// Token: 0x040020E6 RID: 8422
	private static ParticleSystem mediumLeavesPs;

	// Token: 0x040020E7 RID: 8423
	private Vector3 prevCamPos = Vector3.zero;

	// Token: 0x040020E8 RID: 8424
	private Vector3 camVelocity = Vector3.zero;

	// Token: 0x040020E9 RID: 8425
	private float smallToEmit;

	// Token: 0x040020EA RID: 8426
	private float mediumToEmit;

	// Token: 0x040020EB RID: 8427
	private static BlowingLeavesEffectSettings settings;

	// Token: 0x040020EC RID: 8428
	private Vector3 velDirectionVec = Vector3.forward;

	// Token: 0x040020ED RID: 8429
	private GameObject leavesGo;

	// Token: 0x040020EE RID: 8430
	private bool green;

	// Token: 0x040020EF RID: 8431
	private float avgLeafSpeed;
}
