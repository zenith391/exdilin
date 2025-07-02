using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class MeteorShowerEffect : WeatherEffect
{
	private enum MeteorState
	{
		HitSolid,
		HitWater,
		HitNothing
	}

	private class MeteorInfo
	{
		public MeteorState state;

		public Vector3 velocity = Vector3.zero;

		public float hitTime;

		public Transform hitSolidTransform;

		public Vector3 hitSolidLocalPosition;

		public Vector3 startPosition;

		public Vector3 hitWaterPosition;

		public bool inWater;

		public float lifetime;

		public float startLifetime;

		public float time;

		public bool removeMe;

		private float smokeToEmit;

		private GameObject sfxObject;

		private AudioSource sfxAudioSource;

		private bool playedSfx;

		public void Update(float deltaTime)
		{
			lifetime -= deltaTime;
			time += deltaTime;
			if (time > hitTime)
			{
				if (!playedSfx && sfxAudioSource != null)
				{
					sfxAudioSource.volume = 1f;
					sfxAudioSource.Stop();
					if (state == MeteorState.HitSolid)
					{
						sfxAudioSource.PlayOneShot(Sound.GetSfx("Meteor Impact Earth " + Random.Range(1, 5)));
					}
					else
					{
						sfxAudioSource.PlayOneShot(Sound.GetSfx("Meteor Impact Water " + Random.Range(1, 5)));
					}
					playedSfx = true;
				}
				float num = (time - hitTime) / (startLifetime - hitTime);
				if (state == MeteorState.HitWater)
				{
					Vector3 vector = hitWaterPosition + (time - hitTime) * velocity;
					float num2 = 0.5f + Random.value * 0.5f;
					if (bubblePs != null)
					{
						bubblePs.Emit(vector + Random.insideUnitSphere, velocity * 0.01f, 0.2f + Random.value * 0.3f, num2, Color.white);
					}
				}
				else if (state == MeteorState.HitSolid && hitSolidTransform != null && hitSolidTransform.gameObject != null)
				{
					smokeToEmit += 0.3f * (1f - num);
					if (smokeToEmit > 1f)
					{
						smokeToEmit -= 1f;
						Vector3 position = hitSolidTransform.localToWorldMatrix.MultiplyPoint(hitSolidLocalPosition);
						Vector3 vector2 = Vector3.up * (1f - num) * Random.Range(0.3f, 0.4f);
						float num3 = 10f;
						vector2 = Quaternion.Euler(Random.Range(0f - num3, num3), Random.Range(0f - num3, num3), Random.Range(0f - num3, num3)) * vector2;
						if (inWater && bubblePs != null)
						{
							bubblePs.Emit(position, vector2, 0.1f + Random.value * 0.15f, 1f, Color.white);
						}
						else
						{
							ParticleSystem.Particle particle = new ParticleSystem.Particle
							{
								position = position,
								remainingLifetime = 1f,
								startLifetime = 1f,
								velocity = vector2,
								angularVelocity = (1f - num) * Random.Range(-200f, 200f),
								size = Random.Range(1f, 2f),
								color = Color.white,
								rotation = Random.Range(0f, 360f),
								randomSeed = (uint)Random.Range(1, 999999)
							};
							smokePs.Emit(particle);
						}
					}
				}
			}
			else
			{
				if (sfxObject == null && Sound.sfxEnabled)
				{
					sfxObject = new GameObject("Meteor");
					sfxAudioSource = sfxObject.AddComponent<AudioSource>();
					sfxAudioSource.dopplerLevel = 0.2f;
					sfxAudioSource.clip = Sound.GetSfx("Meteor No Impact " + Random.Range(1, 5));
					sfxAudioSource.loop = false;
					sfxAudioSource.volume = 1f;
					Sound.SetWorldAudioSourceParams(sfxAudioSource);
					sfxAudioSource.maxDistance = 120f;
					sfxAudioSource.minDistance = 15f;
					sfxAudioSource.Play();
					if (BlockAbstractWater.CameraWithinAnyWater())
					{
						AudioLowPassFilter audioLowPassFilter = sfxObject.AddComponent<AudioLowPassFilter>();
						audioLowPassFilter.cutoffFrequency = 600f;
					}
				}
				if (sfxObject != null)
				{
					Vector3 position2 = startPosition + velocity * time;
					sfxObject.transform.position = position2;
				}
			}
			if (lifetime < 0f)
			{
				removeMe = true;
				Destroy();
			}
		}

		public void Destroy()
		{
			if (sfxObject != null)
			{
				Object.Destroy(sfxObject, 1f);
			}
		}
	}

	private static GameObject meteorGo;

	private static GameObject smokeGo;

	private static ParticleSystem meteorPs;

	private static ParticleSystem smokePs;

	private static ParticleSystem bubblePs;

	private float meteorsToEmit;

	private static MeteorShowerEffectSettings settings;

	private List<MeteorInfo> meteorInfos = new List<MeteorInfo>();

	private void CreateParticleSystem()
	{
		if (meteorPs == null)
		{
			if (meteorGo == null)
			{
				meteorGo = Object.Instantiate(Resources.Load("Env Effect/Meteor Shower Particle System")) as GameObject;
			}
			settings = meteorGo.GetComponent<MeteorShowerEffectSettings>();
			meteorPs = meteorGo.transform.Find("Meteor Shower Head").gameObject.GetComponent<ParticleSystem>();
		}
		if (smokePs == null)
		{
			if (smokeGo == null)
			{
				smokeGo = Object.Instantiate(Resources.Load("Env Effect/Volcano Particle System")) as GameObject;
			}
			smokePs = smokeGo.transform.Find("Smoke").gameObject.GetComponent<ParticleSystem>();
		}
		Stop();
	}

	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		for (int num = meteorInfos.Count - 1; num >= 0; num--)
		{
			MeteorInfo meteorInfo = meteorInfos[num];
			meteorInfo.Update(deltaTime);
			if (meteorInfo.removeMe)
			{
				meteorInfos.RemoveAt(num);
			}
		}
		if (!(meteorPs != null))
		{
			return;
		}
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		float num2 = -100000000f;
		BlockWater worldOceanBlock = Blocksworld.worldOceanBlock;
		if (worldOceanBlock != null && worldOceanBlock.go.GetComponent<Collider>() != null)
		{
			num2 = worldOceanBlock.go.GetComponent<Collider>().bounds.max.y;
			bubblePs = worldOceanBlock.bubblePs;
		}
		Vector3 vector = Util.ProjectOntoPlane(Blocksworld.cameraForward, Vector3.up);
		float num3 = 40f;
		Vector3 vector2 = cameraPosition + vector * num3 * 0.8f;
		meteorsToEmit += Random.Range(0.1f, 1f) * settings.emitPerFrame * intensityMultiplier;
		if (!(meteorsToEmit > 0f) || !(Random.value < meteorsToEmit))
		{
			return;
		}
		meteorsToEmit -= 1f;
		Vector3 vector3 = Quaternion.Euler(0f, effectAngle, 0f) * Vector3.forward;
		float num4 = Random.Range(settings.speedRandomFrom, settings.speedRandomTo);
		Vector3 velocity = (Vector3.down + vector3 * Random.Range(0.5f, 1.5f)).normalized * num4;
		float num5 = 1f;
		float num6 = Mathf.Max(num3 * 0.3f, num5 * num4 * 0.4f);
		Vector3 vector4 = vector2 + Vector3.up * num6 + Vector3.right * Random.Range(0f - num3, num3) + Vector3.forward * Random.Range(0f - num3, num3) - new Vector3(velocity.x, 0f, velocity.z) * num5 * 0.5f;
		float size = Random.Range(2f, 3f);
		float value = Random.value;
		Color color = value * settings.startColorFrom + (1f - value) * settings.startColorTo;
		float num7 = num4 * num5;
		RaycastHit[] array = Physics.RaycastAll(vector4, velocity.normalized, num7);
		if (array.Length != 0)
		{
			Util.SmartSort(array, vector4);
			bool inWater = vector4.y < num2;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				Collider collider = raycastHit.collider;
				Block block = BWSceneManager.FindBlock(collider.gameObject);
				MeteorState state = MeteorState.HitSolid;
				Vector3 hitSolidLocalPosition = Vector3.zero;
				Transform transform = null;
				bool flag = true;
				bool flag2 = true;
				if (block is BlockAbstractWater blockAbstractWater)
				{
					state = MeteorState.HitWater;
					flag2 = false;
					bubblePs = blockAbstractWater.bubblePs;
					inWater = true;
				}
				else if (!collider.isTrigger && block != null)
				{
					state = MeteorState.HitSolid;
					transform = block.go.transform;
					hitSolidLocalPosition = transform.worldToLocalMatrix.MultiplyPoint(raycastHit.point);
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					float num8 = raycastHit.distance / num4;
					if (flag2)
					{
						float num9 = Mathf.Clamp((raycastHit.distance + 1f) / num7, 0f, 1f);
						num5 *= num9;
					}
					meteorInfos.Add(new MeteorInfo
					{
						state = state,
						hitSolidLocalPosition = hitSolidLocalPosition,
						hitSolidTransform = transform,
						startPosition = vector4,
						hitWaterPosition = raycastHit.point,
						lifetime = 0.5f + num8,
						startLifetime = 0.5f + num8,
						velocity = velocity,
						hitTime = num8,
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
			meteorInfos.Add(new MeteorInfo
			{
				state = MeteorState.HitNothing,
				startPosition = vector4,
				lifetime = num5,
				startLifetime = num5,
				velocity = velocity,
				hitTime = num5 + 1f,
				inWater = false
			});
		}
		ParticleSystem.Particle particle = new ParticleSystem.Particle
		{
			position = vector4,
			velocity = velocity,
			size = size,
			startLifetime = num5,
			remainingLifetime = num5,
			color = new Color(color.r, color.g, color.b, 1f),
			randomSeed = (uint)Random.Range(1, 9999999)
		};
		meteorPs.Emit(particle);
	}

	public override void Start()
	{
		base.Start();
		CreateParticleSystem();
		bubblePs = null;
	}

	public override void Stop()
	{
		if (meteorPs != null)
		{
			meteorPs.Stop();
			meteorPs.Clear();
		}
		if (smokePs != null)
		{
			smokePs.Stop();
			smokePs.Clear();
		}
		foreach (MeteorInfo meteorInfo in meteorInfos)
		{
			meteorInfo.Destroy();
		}
		meteorInfos.Clear();
	}

	public override void Reset()
	{
		base.Reset();
		if (meteorGo != null)
		{
			Object.Destroy(meteorGo);
		}
		meteorGo = null;
		meteorPs = null;
		if (smokeGo != null)
		{
			Object.Destroy(smokeGo);
		}
		smokeGo = null;
		smokePs = null;
	}

	public override void FogChanged()
	{
		base.FogChanged();
		if (smokePs != null)
		{
			Color fogColor = Blocksworld.fogColor;
			Material material = smokePs.GetComponent<Renderer>().material;
			material.SetColor("_FogColor", fogColor);
			float value = Blocksworld.fogStart * Blocksworld.fogMultiplier;
			float value2 = Blocksworld.fogEnd * Blocksworld.fogMultiplier;
			material.SetFloat("_FogStart", value);
			material.SetFloat("_FogEnd", value2);
		}
	}
}
