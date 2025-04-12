using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000350 RID: 848
public class MeteorShowerEffect : WeatherEffect
{
	// Token: 0x060025F5 RID: 9717 RVA: 0x0011813C File Offset: 0x0011653C
	private void CreateParticleSystem()
	{
		if (MeteorShowerEffect.meteorPs == null)
		{
			if (MeteorShowerEffect.meteorGo == null)
			{
				MeteorShowerEffect.meteorGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Meteor Shower Particle System")) as GameObject);
			}
			MeteorShowerEffect.settings = MeteorShowerEffect.meteorGo.GetComponent<MeteorShowerEffectSettings>();
			MeteorShowerEffect.meteorPs = MeteorShowerEffect.meteorGo.transform.Find("Meteor Shower Head").gameObject.GetComponent<ParticleSystem>();
		}
		if (MeteorShowerEffect.smokePs == null)
		{
			if (MeteorShowerEffect.smokeGo == null)
			{
				MeteorShowerEffect.smokeGo = (UnityEngine.Object.Instantiate(Resources.Load("Env Effect/Volcano Particle System")) as GameObject);
			}
			MeteorShowerEffect.smokePs = MeteorShowerEffect.smokeGo.transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
		}
		this.Stop();
	}

	// Token: 0x060025F6 RID: 9718 RVA: 0x00118218 File Offset: 0x00116618
	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		for (int i = this.meteorInfos.Count - 1; i >= 0; i--)
		{
			MeteorShowerEffect.MeteorInfo meteorInfo = this.meteorInfos[i];
			meteorInfo.Update(deltaTime);
			if (meteorInfo.removeMe)
			{
				this.meteorInfos.RemoveAt(i);
			}
		}
		if (MeteorShowerEffect.meteorPs != null)
		{
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			float num = -1E+08f;
			BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
			if (worldOceanBlock != null && worldOceanBlock.go.GetComponent<Collider>() != null)
			{
				num = worldOceanBlock.go.GetComponent<Collider>().bounds.max.y;
				MeteorShowerEffect.bubblePs = worldOceanBlock.bubblePs;
			}
			Vector3 a = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
			float num2 = 40f;
			Vector3 a2 = cameraPosition + a * num2 * 0.8f;
			this.meteorsToEmit += UnityEngine.Random.Range(0.1f, 1f) * MeteorShowerEffect.settings.emitPerFrame * this.intensityMultiplier;
			if (this.meteorsToEmit > 0f && UnityEngine.Random.value < this.meteorsToEmit)
			{
				this.meteorsToEmit -= 1f;
				Vector3 a3 = Quaternion.Euler(0f, this.effectAngle, 0f) * Vector3.forward;
				float num3 = UnityEngine.Random.Range(MeteorShowerEffect.settings.speedRandomFrom, MeteorShowerEffect.settings.speedRandomTo);
				Vector3 velocity = (Vector3.down + a3 * UnityEngine.Random.Range(0.5f, 1.5f)).normalized * num3;
				float num4 = 1f;
				float d = Mathf.Max(num2 * 0.3f, num4 * num3 * 0.4f);
				Vector3 vector = a2 + Vector3.up * d + Vector3.right * UnityEngine.Random.Range(-num2, num2) + Vector3.forward * UnityEngine.Random.Range(-num2, num2) - new Vector3(velocity.x, 0f, velocity.z) * num4 * 0.5f;
				float size = UnityEngine.Random.Range(2f, 3f);
				float value = UnityEngine.Random.value;
				Color color = value * MeteorShowerEffect.settings.startColorFrom + (1f - value) * MeteorShowerEffect.settings.startColorTo;
				float num5 = num3 * num4;
				RaycastHit[] array = Physics.RaycastAll(vector, velocity.normalized, num5);
				if (array.Length > 0)
				{
					Util.SmartSort(array, vector);
					bool inWater = vector.y < num;
					foreach (RaycastHit raycastHit in array)
					{
						Collider collider = raycastHit.collider;
						Block block = BWSceneManager.FindBlock(collider.gameObject, false);
						MeteorShowerEffect.MeteorState state = MeteorShowerEffect.MeteorState.HitSolid;
						Vector3 hitSolidLocalPosition = Vector3.zero;
						Transform transform = null;
						bool flag = true;
						bool flag2 = true;
						BlockAbstractWater blockAbstractWater = block as BlockAbstractWater;
						if (blockAbstractWater != null)
						{
							state = MeteorShowerEffect.MeteorState.HitWater;
							flag2 = false;
							MeteorShowerEffect.bubblePs = blockAbstractWater.bubblePs;
							inWater = true;
						}
						else if (!collider.isTrigger && block != null)
						{
							state = MeteorShowerEffect.MeteorState.HitSolid;
							transform = block.go.transform;
							hitSolidLocalPosition = transform.worldToLocalMatrix.MultiplyPoint(raycastHit.point);
						}
						else
						{
							flag = false;
						}
						if (flag)
						{
							float num6 = raycastHit.distance / num3;
							if (flag2)
							{
								float num7 = Mathf.Clamp((raycastHit.distance + 1f) / num5, 0f, 1f);
								num4 *= num7;
							}
							this.meteorInfos.Add(new MeteorShowerEffect.MeteorInfo
							{
								state = state,
								hitSolidLocalPosition = hitSolidLocalPosition,
								hitSolidTransform = transform,
								startPosition = vector,
								hitWaterPosition = raycastHit.point,
								lifetime = 0.5f + num6,
								startLifetime = 0.5f + num6,
								velocity = velocity,
								hitTime = num6,
								inWater = inWater
							});
							if (flag2)
							{
								break;
							}
						}
					}
				}
				else
				{
					this.meteorInfos.Add(new MeteorShowerEffect.MeteorInfo
					{
						state = MeteorShowerEffect.MeteorState.HitNothing,
						startPosition = vector,
						lifetime = num4,
						startLifetime = num4,
						velocity = velocity,
						hitTime = num4 + 1f,
						inWater = false
					});
				}
				ParticleSystem.Particle particle = default(ParticleSystem.Particle);
				particle.position = vector;
				particle.velocity = velocity;
				particle.size = size;
				particle.startLifetime = num4;
				particle.remainingLifetime = num4;
				particle.color = new Color(color.r, color.g, color.b, 1f);
				particle.randomSeed = (uint)UnityEngine.Random.Range(1, 9999999);
				MeteorShowerEffect.meteorPs.Emit(particle);
			}
		}
	}

	// Token: 0x060025F7 RID: 9719 RVA: 0x0011876E File Offset: 0x00116B6E
	public override void Start()
	{
		base.Start();
		this.CreateParticleSystem();
		MeteorShowerEffect.bubblePs = null;
	}

	// Token: 0x060025F8 RID: 9720 RVA: 0x00118784 File Offset: 0x00116B84
	public override void Stop()
	{
		if (MeteorShowerEffect.meteorPs != null)
		{
			MeteorShowerEffect.meteorPs.Stop();
			MeteorShowerEffect.meteorPs.Clear();
		}
		if (MeteorShowerEffect.smokePs != null)
		{
			MeteorShowerEffect.smokePs.Stop();
			MeteorShowerEffect.smokePs.Clear();
		}
		foreach (MeteorShowerEffect.MeteorInfo meteorInfo in this.meteorInfos)
		{
			meteorInfo.Destroy();
		}
		this.meteorInfos.Clear();
	}

	// Token: 0x060025F9 RID: 9721 RVA: 0x00118834 File Offset: 0x00116C34
	public override void Reset()
	{
		base.Reset();
		if (MeteorShowerEffect.meteorGo != null)
		{
			UnityEngine.Object.Destroy(MeteorShowerEffect.meteorGo);
		}
		MeteorShowerEffect.meteorGo = null;
		MeteorShowerEffect.meteorPs = null;
		if (MeteorShowerEffect.smokeGo != null)
		{
			UnityEngine.Object.Destroy(MeteorShowerEffect.smokeGo);
		}
		MeteorShowerEffect.smokeGo = null;
		MeteorShowerEffect.smokePs = null;
	}

	// Token: 0x060025FA RID: 9722 RVA: 0x00118894 File Offset: 0x00116C94
	public override void FogChanged()
	{
		base.FogChanged();
		if (MeteorShowerEffect.smokePs != null)
		{
			Color fogColor = Blocksworld.fogColor;
			Material material = MeteorShowerEffect.smokePs.GetComponent<Renderer>().material;
			material.SetColor("_FogColor", fogColor);
			float value = Blocksworld.fogStart * Blocksworld.fogMultiplier;
			float value2 = Blocksworld.fogEnd * Blocksworld.fogMultiplier;
			material.SetFloat("_FogStart", value);
			material.SetFloat("_FogEnd", value2);
		}
	}

	// Token: 0x040020F0 RID: 8432
	private static GameObject meteorGo;

	// Token: 0x040020F1 RID: 8433
	private static GameObject smokeGo;

	// Token: 0x040020F2 RID: 8434
	private static ParticleSystem meteorPs;

	// Token: 0x040020F3 RID: 8435
	private static ParticleSystem smokePs;

	// Token: 0x040020F4 RID: 8436
	private static ParticleSystem bubblePs;

	// Token: 0x040020F5 RID: 8437
	private float meteorsToEmit;

	// Token: 0x040020F6 RID: 8438
	private static MeteorShowerEffectSettings settings;

	// Token: 0x040020F7 RID: 8439
	private List<MeteorShowerEffect.MeteorInfo> meteorInfos = new List<MeteorShowerEffect.MeteorInfo>();

	// Token: 0x02000351 RID: 849
	private enum MeteorState
	{
		// Token: 0x040020F9 RID: 8441
		HitSolid,
		// Token: 0x040020FA RID: 8442
		HitWater,
		// Token: 0x040020FB RID: 8443
		HitNothing
	}

	// Token: 0x02000352 RID: 850
	private class MeteorInfo
	{
		// Token: 0x060025FC RID: 9724 RVA: 0x0011891C File Offset: 0x00116D1C
		public void Update(float deltaTime)
		{
			this.lifetime -= deltaTime;
			this.time += deltaTime;
			if (this.time > this.hitTime)
			{
				if (!this.playedSfx && this.sfxAudioSource != null)
				{
					this.sfxAudioSource.volume = 1f;
					this.sfxAudioSource.Stop();
					if (this.state == MeteorShowerEffect.MeteorState.HitSolid)
					{
						this.sfxAudioSource.PlayOneShot(Sound.GetSfx("Meteor Impact Earth " + UnityEngine.Random.Range(1, 5)));
					}
					else
					{
						this.sfxAudioSource.PlayOneShot(Sound.GetSfx("Meteor Impact Water " + UnityEngine.Random.Range(1, 5)));
					}
					this.playedSfx = true;
				}
				float num = (this.time - this.hitTime) / (this.startLifetime - this.hitTime);
				if (this.state == MeteorShowerEffect.MeteorState.HitWater)
				{
					Vector3 a = this.hitWaterPosition + (this.time - this.hitTime) * this.velocity;
					float num2 = 0.5f + UnityEngine.Random.value * 0.5f;
					if (MeteorShowerEffect.bubblePs != null)
					{
						MeteorShowerEffect.bubblePs.Emit(a + UnityEngine.Random.insideUnitSphere, this.velocity * 0.01f, 0.2f + UnityEngine.Random.value * 0.3f, num2, Color.white);
					}
				}
				else if (this.state == MeteorShowerEffect.MeteorState.HitSolid && this.hitSolidTransform != null && this.hitSolidTransform.gameObject != null)
				{
					this.smokeToEmit += 0.3f * (1f - num);
					if (this.smokeToEmit > 1f)
					{
						this.smokeToEmit -= 1f;
						Vector3 position = this.hitSolidTransform.localToWorldMatrix.MultiplyPoint(this.hitSolidLocalPosition);
						Vector3 point = Vector3.up * (1f - num) * UnityEngine.Random.Range(0.3f, 0.4f);
						float num3 = 10f;
						point = Quaternion.Euler(UnityEngine.Random.Range(-num3, num3), UnityEngine.Random.Range(-num3, num3), UnityEngine.Random.Range(-num3, num3)) * point;
						if (this.inWater && MeteorShowerEffect.bubblePs != null)
						{
							MeteorShowerEffect.bubblePs.Emit(position, point, 0.1f + UnityEngine.Random.value * 0.15f, 1f, Color.white);
						}
						else
						{
							ParticleSystem.Particle particle = new ParticleSystem.Particle
							{
								position = position,
								remainingLifetime = 1f,
								startLifetime = 1f,
								velocity = point,
								angularVelocity = (1f - num) * UnityEngine.Random.Range(-200f, 200f),
								size = UnityEngine.Random.Range(1f, 2f),
								color = Color.white,
								rotation = UnityEngine.Random.Range(0f, 360f),
								randomSeed = (uint)UnityEngine.Random.Range(1, 999999)
							};
							MeteorShowerEffect.smokePs.Emit(particle);
						}
					}
				}
			}
			else
			{
				if (this.sfxObject == null && Sound.sfxEnabled)
				{
					this.sfxObject = new GameObject("Meteor");
					this.sfxAudioSource = this.sfxObject.AddComponent<AudioSource>();
					this.sfxAudioSource.dopplerLevel = 0.2f;
					this.sfxAudioSource.clip = Sound.GetSfx("Meteor No Impact " + UnityEngine.Random.Range(1, 5));
					this.sfxAudioSource.loop = false;
					this.sfxAudioSource.volume = 1f;
					Sound.SetWorldAudioSourceParams(this.sfxAudioSource, 5f, 150f, AudioRolloffMode.Logarithmic);
					this.sfxAudioSource.maxDistance = 120f;
					this.sfxAudioSource.minDistance = 15f;
					this.sfxAudioSource.Play();
					if (BlockAbstractWater.CameraWithinAnyWater())
					{
						AudioLowPassFilter audioLowPassFilter = this.sfxObject.AddComponent<AudioLowPassFilter>();
						audioLowPassFilter.cutoffFrequency = 600f;
					}
				}
				if (this.sfxObject != null)
				{
					Vector3 position2 = this.startPosition + this.velocity * this.time;
					this.sfxObject.transform.position = position2;
				}
			}
			if (this.lifetime < 0f)
			{
				this.removeMe = true;
				this.Destroy();
			}
		}

		// Token: 0x060025FD RID: 9725 RVA: 0x00118DD7 File Offset: 0x001171D7
		public void Destroy()
		{
			if (this.sfxObject != null)
			{
				UnityEngine.Object.Destroy(this.sfxObject, 1f);
			}
		}

		// Token: 0x040020FC RID: 8444
		public MeteorShowerEffect.MeteorState state;

		// Token: 0x040020FD RID: 8445
		public Vector3 velocity = Vector3.zero;

		// Token: 0x040020FE RID: 8446
		public float hitTime;

		// Token: 0x040020FF RID: 8447
		public Transform hitSolidTransform;

		// Token: 0x04002100 RID: 8448
		public Vector3 hitSolidLocalPosition;

		// Token: 0x04002101 RID: 8449
		public Vector3 startPosition;

		// Token: 0x04002102 RID: 8450
		public Vector3 hitWaterPosition;

		// Token: 0x04002103 RID: 8451
		public bool inWater;

		// Token: 0x04002104 RID: 8452
		public float lifetime;

		// Token: 0x04002105 RID: 8453
		public float startLifetime;

		// Token: 0x04002106 RID: 8454
		public float time;

		// Token: 0x04002107 RID: 8455
		public bool removeMe;

		// Token: 0x04002108 RID: 8456
		private float smokeToEmit;

		// Token: 0x04002109 RID: 8457
		private GameObject sfxObject;

		// Token: 0x0400210A RID: 8458
		private AudioSource sfxAudioSource;

		// Token: 0x0400210B RID: 8459
		private bool playedSfx;
	}
}
