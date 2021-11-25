using System;
using Blocks;
using UnityEngine;

// Token: 0x02000353 RID: 851
public class RainEffect : WeatherEffect
{
	// Token: 0x060025FE RID: 9726 RVA: 0x00118DFA File Offset: 0x001171FA
	public RainEffect()
	{
		this.loopSfx = "Rain Loop";
	}

	// Token: 0x060025FF RID: 9727 RVA: 0x00118E24 File Offset: 0x00117224
	protected override float GetTargetVolume()
	{
		float num = 1f / (1f + 0.02f * this.avgSplashDistance + 2f * this.indoorFraction);
		return num * base.GetTargetVolume();
	}

	// Token: 0x06002600 RID: 9728 RVA: 0x00118E60 File Offset: 0x00117260
	private void CreateParticleSystem()
	{
		if (this.rainPs == null)
		{
			if (this.rainGo == null)
			{
				this.rainGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Rain Particle Systems")) as GameObject);
			}
			this.settings = this.rainGo.GetComponent<RainEffectSettings>();
			this.rainPs = this.rainGo.transform.Find("Rain Particle System").gameObject.GetComponent<ParticleSystem>();
			GameObject gameObject = this.rainGo.transform.Find("Ripple Particle System").gameObject;
			this.ripplePs = gameObject.GetComponent<ParticleSystem>();
			GameObject gameObject2 = this.rainGo.transform.Find("Splash Particle System").gameObject;
			this.splashPs = gameObject2.GetComponent<ParticleSystem>();
		}
		this.ResetRain();
		this.Stop();
	}

	// Token: 0x06002601 RID: 9729 RVA: 0x00118F3C File Offset: 0x0011733C
	public override void Pause()
	{
		base.Pause();
		if (this.rainPs != null)
		{
			this.rainPs.Pause();
		}
		if (this.ripplePs != null)
		{
			this.ripplePs.Pause();
		}
		if (this.splashPs != null)
		{
			this.splashPs.Pause();
		}
	}

	// Token: 0x06002602 RID: 9730 RVA: 0x00118FA4 File Offset: 0x001173A4
	public override void Resume()
	{
		base.Resume();
		if (this.rainPs != null)
		{
			this.rainPs.Play();
		}
		if (this.ripplePs != null)
		{
			this.ripplePs.Play();
		}
		if (this.splashPs != null)
		{
			this.splashPs.Play();
		}
	}

	// Token: 0x06002603 RID: 9731 RVA: 0x0011900C File Offset: 0x0011740C
	public override void Update()
	{
		base.Update();
		if (this.rainGo != null && !this.paused)
		{
			if (this.needsAngle && this.rainPs.startSpeed < 20f)
			{
				this.rainPs.startSpeed += Time.deltaTime * 10f;
			}
			float y = this.rainGo.transform.eulerAngles.y;
			if (Mathf.Abs(this.newYAngle - y) > 3f)
			{
				float num = Time.deltaTime * 100f;
				if (y > this.newYAngle)
				{
					num = -num;
				}
				this.rainGo.transform.rotation = Quaternion.Euler(0f, y + num, 0f);
			}
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Vector3 a = Blocksworld.blocksworldCamera.GetTargetPosition() - cameraPosition;
			float magnitude = a.magnitude;
			bool flag = true;
            Bounds bounds = new Bounds();
			if (magnitude > 0.01f)
			{
				Vector3 a2 = a / magnitude;
				float d = Mathf.Min(magnitude * 0.5f, 30f);
				Vector3 vector = cameraPosition + a2 * d;
				GameObject worldOcean = Blocksworld.worldOcean;
				if (worldOcean != null && worldOcean.GetComponent<Collider>() != null)
				{
					bounds = worldOcean.GetComponent<Collider>().bounds;
					vector.y = Mathf.Max(vector.y, bounds.max.y + 10f);
				}
				else if (this.ripplePs.isPlaying)
				{
					flag = false;
				}
				Vector3 a3 = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
				if (a3.sqrMagnitude > 0.001f)
				{
					a3 = a3.normalized;
					vector += a3 * this.indoorFraction * 70f;
				}
				this.rainGo.transform.position = vector;
			}
			this.rainPs.emissionRate = this.intensityMultiplier * this.settings.rainEmissionRate * this.intensityFraction;
			Vector3 a4 = cameraPosition + Blocksworld.cameraForward * 15f + Vector3.up * 20f;
			if (flag)
			{
				a4.y = Mathf.Max(a4.y, bounds.max.y + 1f);
			}
			bool flag2 = false;
			if (a4.y - cameraPosition.y < 40f)
			{
				this.toEmit += this.settings.ripplesPerFrame * this.intensityMultiplier;
				int num2 = 0;
				int num3 = 0;
				while ((float)num3 < this.toEmit)
				{
					num2++;
					Vector3 origin = a4 + Vector3.right * UnityEngine.Random.Range(-15f, 15f) + Vector3.forward * UnityEngine.Random.Range(-15f, 15f);
					RaycastHit raycastHit;
					if (Physics.Raycast(origin, Vector3.down, out raycastHit, 40f))
					{
						flag2 = true;
						Vector3 point = raycastHit.point;
						bool flag3 = Vector3.Dot(raycastHit.normal, Vector3.up) > 0.98f;
						Vector3 vector2 = point + Vector3.up * 0.05f;
						Vector3 velocity = this.settings.splashSpeed * Vector3.Reflect(Vector3.down, raycastHit.normal);
						Vector3 a5 = Vector3.zero;
						Rigidbody rigidbody = raycastHit.rigidbody;
						if (rigidbody != null)
						{
							a5 = rigidbody.velocity;
							Vector3 lhs = point - rigidbody.worldCenterOfMass;
							a5 += Vector3.Cross(lhs, rigidbody.angularVelocity);
						}
						velocity = new Vector3(velocity.x * UnityEngine.Random.Range(0.7f, 1.3f), velocity.y * UnityEngine.Random.Range(0.7f, 1.3f), velocity.z * UnityEngine.Random.Range(0.7f, 1.3f));
						float num4 = UnityEngine.Random.Range(this.settings.splashSizeRandomFrom, this.settings.splashSizeRandomTo);
						float num5 = UnityEngine.Random.Range(this.settings.splashLifetimeRandomFrom, this.settings.splashLifetimeRandomTo);
						Color white = Color.white;
						Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
						BlockAbstractWater blockAbstractWater = block as BlockAbstractWater;
						if (blockAbstractWater != null)
						{
							num5 *= 0.7f;
							num4 *= 0.7f;
							white.a = 0.7f;
						}
						ParticleSystem.Particle particle = default(ParticleSystem.Particle);
						particle.position = vector2;
						particle.velocity = velocity;
						particle.size = num4;
						particle.startLifetime = num5;
						particle.remainingLifetime = num5;
						particle.color = white;
						particle.randomSeed = (uint)UnityEngine.Random.Range(1, 9999999);
						this.splashPs.Emit(particle);
						bool flag4 = flag3 && a5.sqrMagnitude < 2f;
						if (flag4)
						{
							this.ripplePs.Emit(vector2, Vector3.zero, UnityEngine.Random.Range(this.settings.rippleSizeRandomFrom, this.settings.rippleSizeRandomTo), UnityEngine.Random.Range(this.settings.rippleSizeRandomFrom, this.settings.rippleSizeRandomTo), new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.8f, 1f)));
						}
						float magnitude2 = (cameraPosition - vector2).magnitude;
						this.avgSplashDistance = 0.99f * this.avgSplashDistance + 0.01f * magnitude2;
						bool flag5 = Mathf.Abs(cameraPosition.x - point.x) < 5f && Mathf.Abs(cameraPosition.z - point.z) < 5f;
						if (point.y > cameraPosition.y)
						{
							this.intensityFraction = 0.9f * this.intensityFraction;
							if (flag5)
							{
								this.indoorFraction = 0.95f * this.indoorFraction + 0.05f;
							}
						}
						else
						{
							this.intensityFraction = 0.9f * this.intensityFraction + 0.1f;
							if (flag5)
							{
								this.indoorFraction = 0.95f * this.indoorFraction;
							}
						}
					}
					else
					{
						this.intensityFraction = 0.9f * this.intensityFraction + 0.1f;
						this.indoorFraction = 0.95f * this.indoorFraction;
					}
					num3++;
				}
				this.toEmit -= (float)num2;
			}
			if (!flag2)
			{
				this.avgSplashDistance = 0.99f * this.avgSplashDistance + 2f;
			}
		}
	}

	// Token: 0x06002604 RID: 9732 RVA: 0x00119704 File Offset: 0x00117B04
	public override void SetEffectAngle(float angle)
	{
		base.SetEffectAngle(angle);
		this.needsAngle = true;
		if (angle < 0f)
		{
			angle += 360f;
		}
		if (angle == 360f)
		{
			angle = 0f;
		}
		this.newYAngle = angle;
	}

	// Token: 0x06002605 RID: 9733 RVA: 0x00119741 File Offset: 0x00117B41
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
		this.rainPs.Play();
	}

	// Token: 0x06002606 RID: 9734 RVA: 0x0011975C File Offset: 0x00117B5C
	public override void Stop()
	{
		if (this.rainPs != null)
		{
			this.rainPs.Stop();
			this.ripplePs.Stop();
			this.splashPs.Stop();
			this.rainPs.Clear();
			this.ripplePs.Clear();
			this.splashPs.Clear();
			this.rainPs.startSpeed = 0f;
		}
		this.needsAngle = false;
	}

	// Token: 0x06002607 RID: 9735 RVA: 0x001197D4 File Offset: 0x00117BD4
	private void ResetRain()
	{
		this.newYAngle = 0f;
		this.needsAngle = false;
		if (this.rainPs != null)
		{
			this.rainPs.startSpeed = 0f;
		}
		if (this.rainGo != null)
		{
			this.rainGo.transform.rotation = Quaternion.identity;
		}
	}

	// Token: 0x06002608 RID: 9736 RVA: 0x0011983A File Offset: 0x00117C3A
	public override void Reset()
	{
		base.Reset();
		if (this.rainGo != null)
		{
			UnityEngine.Object.Destroy(this.rainGo);
		}
		this.rainGo = null;
		this.rainPs = null;
		this.ripplePs = null;
		this.splashPs = null;
	}

	// Token: 0x06002609 RID: 9737 RVA: 0x0011987A File Offset: 0x00117C7A
	public override float GetFogMultiplier()
	{
		return 1f - Mathf.Min(0.3f, this.intensityMultiplier * 0.5f);
	}

	// Token: 0x0400210C RID: 8460
	private GameObject rainGo;

	// Token: 0x0400210D RID: 8461
	private ParticleSystem rainPs;

	// Token: 0x0400210E RID: 8462
	private ParticleSystem ripplePs;

	// Token: 0x0400210F RID: 8463
	private ParticleSystem splashPs;

	// Token: 0x04002110 RID: 8464
	private bool needsAngle;

	// Token: 0x04002111 RID: 8465
	private float newYAngle;

	// Token: 0x04002112 RID: 8466
	private float indoorFraction;

	// Token: 0x04002113 RID: 8467
	private float intensityFraction = 1f;

	// Token: 0x04002114 RID: 8468
	private const float indoorAlpha = 0.05f;

	// Token: 0x04002115 RID: 8469
	private const float intensityAlpha = 0.1f;

	// Token: 0x04002116 RID: 8470
	private float toEmit;

	// Token: 0x04002117 RID: 8471
	private float avgSplashDistance = 1f;

	// Token: 0x04002118 RID: 8472
	private RainEffectSettings settings;
}
